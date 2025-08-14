namespace Visualization;

// =================== Core loop & rendering ===================

internal sealed class MainLoop
{
    private const int MaxKeyEventsPerFrame = 64;

    private readonly Terminal _t;
    private readonly VizConfig _cfg;
    private readonly Action<Frame> _draw;
    private readonly Viewport _vp;
    private readonly CellBuffer _buf;
    private readonly InputState _input = new();
    private readonly Stopwatch _sw = new();
    private double _accum = 0;
    private InputReader _inputReader;

    public MainLoop(Terminal t, VizConfig cfg, Action<Frame> draw)
    {
        _t = t; _cfg = cfg; _draw = draw;
        _vp = new Viewport();
        _buf = new CellBuffer(_t.Width, _t.Height);
        _vp.AttachTerminal(_t);
    }

    public void Run()
    {
        _t.EnterAltScreen();
        _t.HideCursor();
        _t.EnableMouse(true);
        _t.Clear();

        _vp.CenterOn(0, 0);
        _vp.SetZoom(1.0);

        _inputReader = new InputReader(_input);
        _inputReader.Start();

        _sw.Start();
        bool running = true;

        while (running)
        {
            if (_t.TryRefreshSize())
            {
                _buf.Resize(_t.Width, _t.Height);
                _t.Clear();
                _t.EnableMouse(true);
                _input.OnResize();          // ustawia _dragLastX/Y = MouseX/Y
                _inputReader?.RequestReset(); // zrywa ewentualną niedomkniętą sekwencję ESC
                _input.Dirty = true;
            }

            // ODBIÓR KLAWISZY — limit na ramkę:
            int consumed = 0;
            while (consumed < MaxKeyEventsPerFrame && _input.TryDequeueKey(out var ke))
            {
                consumed++;
                _input.LastKey = ke.Key;
                _input.Ctrl = ke.Ctrl; _input.Shift = ke.Shift; _input.Alt = ke.Alt;

                // Quit: Ctrl+Q
                if (ke.Ctrl && ke.Key == ConsoleKey.Q) { running = false; break; }
                HandleKeys();
            }

            // MYSZ (drag/wheel) — jeśli coś zrobiło zmianę, _input.Dirty już = true
            HandleMouse();

            // Autoplay (może ustawić StepRequested, a przez to też zbrudzić)
            if (_cfg.AutoPlay)
            {
                _accum += _sw.Elapsed.TotalSeconds;
                _sw.Restart();
                double stepEvery = 1.0 / Math.Max(0.0001, _cfg.AutoStepPerSecond);
                while (_accum >= stepEvery)
                {
                    _accum -= stepEvery;
                    _input.StepRequested = true;
                    _input.Dirty = true;
                }
            }
            else
            {
                _sw.Restart();
            }

            // RENDER TYLKO GDY DIRTY lub po resize
            if (_input.Dirty)
            {
                _input.Dirty = false;

                _buf.Fill(new Cell(' ', Rgb.White, Rgb.Black));

                var frame = new Frame(_t, _vp, _buf, _input, _cfg);
                _draw(frame);

                if (_cfg.Layers.HasFlag(UiLayers.Rulers)) DrawRulers();
                if (_cfg.Layers.HasFlag(UiLayers.StatusBar)) DrawStatusBar();

                _t.Draw(_buf);

                _input.StepRequested = false;
                _input.ConsumedMouseMove();
            }
            else
            {
                // nic się nie zmieniło → mikro-drzemka
                Thread.Sleep(1);
            }

            // FPS throttle (opcjonalny — ale zostawmy, by nie grzać CPU)
            if (_cfg.TargetFps > 0)
            {
                int targetMs = 1000 / _cfg.TargetFps;
                Thread.Sleep(Math.Max(0, targetMs - (int)_sw.ElapsedMilliseconds));
            }
        }

        _inputReader?.Stop();
        _t.EnableMouse(false);
        _t.ShowCursor();
        _t.ExitAltScreen();
    }

    // =================== Sterowanie klawiaturą ===================

    private void HandleKeys()
    {
        var k = _input.LastKey;
        bool ctrl = _input.Ctrl;

        switch (k)
        {
            case ConsoleKey.Spacebar:
                _input.StepRequested = true;
                break;

            case ConsoleKey.Add:
            case ConsoleKey.OemPlus:
                if (ctrl) ZoomAtCursor(1.25);
                else Pan(0, -1);
                break;

            case ConsoleKey.Subtract:
            case ConsoleKey.OemMinus:
                if (ctrl) ZoomAtCursor(1 / 1.25);
                else Pan(0, +1);
                break;

            case ConsoleKey.LeftArrow: Pan(-1, 0); break;
            case ConsoleKey.RightArrow: Pan(+1, 0); break;
            case ConsoleKey.UpArrow: Pan(0, -1); break;
            case ConsoleKey.DownArrow: Pan(0, +1); break;

            case ConsoleKey.W: Pan(0, -1); break;
            case ConsoleKey.S: Pan(0, +1); break;
            case ConsoleKey.A: Pan(-1, 0); break;
            case ConsoleKey.D: Pan(+1, 0); break;

            case ConsoleKey.D0: // reset zoomu do 1.0 wokół środka ekranu
                {
                    int sx = _t.Width / 2;
                    int sy = _t.Height / 2;
                    _vp.ResetZoomAroundScreenPoint(sx, sy, 1.0);
                    _input.Dirty = true;
                    break;
                }

            // Autoplay szybko:
            case ConsoleKey.D1: _cfg.AutoPlay = false; break;
            case ConsoleKey.D2: _cfg.AutoPlay = true; _cfg.AutoStepPerSecond = 5; break;
            case ConsoleKey.D3: _cfg.AutoPlay = true; _cfg.AutoStepPerSecond = 30; break;

            // PRESETY WARSTW (F5–F8):
            case ConsoleKey.F5: _cfg.Layers = UiLayers.All; break;                    // wszystko
            case ConsoleKey.F6: _cfg.Layers = UiLayers.Map; break;                    // tylko mapa
            case ConsoleKey.F7: _cfg.Layers = UiLayers.Rulers | UiLayers.StatusBar; break; // tylko UI bez mapy
            case ConsoleKey.F8: _cfg.Layers ^= UiLayers.Overlays; break;              // toggle overlays

            // Precyzyjna zmiana szybkości:
            case ConsoleKey.Oem4: _cfg.AutoStepPerSecond = Math.Max(0.2, _cfg.AutoStepPerSecond / 1.25); break;
            case ConsoleKey.Oem6: _cfg.AutoStepPerSecond *= 1.25; break;
        }
    }

    // =================== Obsługa myszy ===================

    private void HandleMouse()
    {
        int wheel = _input.ConsumeWheel();
        if (wheel != 0)
        {
            double factor = Math.Pow(1.1, wheel);
            ZoomAtCursor(factor);
            _input.Dirty = true;
        }

        if (_input.MouseLeftDragging || _input.MouseRightDragging)
        {
            var (dx, dy) = _input.ConsumeDragDelta();
            double mul = _cfg.PanSpeed / _vp.Zoom;   // zawsze proporcjonalnie do zoomu
            if (dx != 0 || dy != 0)
            {
                _vp.Offset(-dx * mul, -dy * mul);
                _input.Dirty = true;
            }
        }
    }

    private void ZoomAtCursor(double factor)
    {
        int sx = _input.MouseX.Clamp(0, _t.Width - 1);
        int sy = _input.MouseY.Clamp(0, _t.Height - 1);
        var (wx, wy) = _vp.ScreenToWorld(sx, sy);
        _vp.ZoomAround(wx, wy, factor, minZoom: 0.1, maxZoom: 40.0);
    }

    private void Pan(int dx, int dy)
    {
        var (wWorld, hWorld) = _vp.VisibleWorldSize();
        double stepX = wWorld * _cfg.PanKeyStepFrac;
        double stepY = hWorld * _cfg.PanKeyStepFrac;
        _vp.Offset(dx * stepX, dy * stepY);
        _input.Dirty = true;
    }

    // =================== UI: Rulers + Status ===================

    private void DrawRulers()
    {
        int W = _t.Width, H = _t.Height;
        if (W < 10 || H < 5) return;

        // tła
        for (int x = 0; x < W; x++)
            _buf.Set(x, 0, new Cell(' ', Rgb.Black, Rgb.Gray));    // top
        for (int y = 0; y < H; y++)
            _buf.Set(0, y, new Cell(' ', Rgb.Black, Rgb.Gray));    // left “belka” bazowa

        // napisy na top ruler
        for (int sx = 4; sx < W; sx++)
        {
            var (wx, _) = _vp.ScreenToWorld(sx, 1);
            if (sx % 10 == 0)
            {
                var label = ((int)Math.Round(wx)).ToString();
                Renderer.PutText(_buf, sx, 0, label, Rgb.Black, Rgb.Gray);
            }
        }

        // napisy na left ruler (3-znakowe)
        for (int sy = 1; sy < H - 1; sy++)
        {
            var (_, wy) = _vp.ScreenToWorld(4, sy);
            if (sy % 2 == 0)
            {
                var s = ((int)Math.Round(wy)).ToString();
                s = s.Length <= 3 ? s.PadLeft(3) : s[^3..];
                Renderer.PutText(_buf, 0, sy, s, Rgb.Black, Rgb.Gray);
            }
        }

        // === HIGHLIGHT pozycji myszy ===
        int msx = _input.MouseX.Clamp(0, W - 1);
        int msy = _input.MouseY.Clamp(0, H - 1);
        var hi = _cfg.RulerHighlight;

        // top: podświetl pojedynczą komórkę nad kursorem
        {
            var c = _buf[msx, 0];
            _buf.Set(msx, 0, new Cell(c.Ch, c.Fg, hi));
        }

        // left: podświetl cały rząd rulera (kolumny 0..3) na wysokości kursora
        {
            var c = _buf[0, msy];
            _buf.Set(0, msy, new Cell(c.Ch, c.Fg, hi));
        }
    }
    private void DrawStatusBar()
    {
        int W = _t.Width, H = _t.Height;
        if (W < 10 || H < 3) return;

        for (int x = 0; x < W; x++)
            _buf.Set(x, H - 1, new Cell(' ', Rgb.Black, Rgb.Gray));

        // komórka świata pod kursorem (z korektą dla zoom<1)
        var (ix, iy) = _vp.WorldCellUnderScreen(_input.MouseX, _input.MouseY);

        string shortcuts = " Keys: Ctrl+Q quit | =/- zoom | LMB drag pan | Arrows/WASD pan | Space step | 0 reset zoom | F5 all | F6 map | F7 ui | F8 overlays ";
        string info = $" Zoom={_vp.Zoom:F2}  Mouse=({ix},{iy})  Auto={(_cfg.AutoPlay ? $"{_cfg.AutoStepPerSecond:F1}/s" : "off")}  Layers={_cfg.Layers} ";
        string status = (shortcuts + "| " + info).PadRight(W);

        Renderer.PutText(_buf, 0, H - 1, status[..Math.Min(W, status.Length)], Rgb.Black, Rgb.Gray);
    }
}