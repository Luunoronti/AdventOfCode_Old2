﻿using System.Diagnostics;

namespace TermGlass;

internal sealed partial class MainLoop
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
    private InputReader? _inputReader;
    private readonly TooltipProvider? _tooltip;
    private bool _tooltipEnabled = true;

    private string? _statusFromFrame;
    private TooltipProvider? _tooltipFromFrame;

    private int _frameCounter = 0;
    private double _fps = 0.0;
    private DateTime _fpsLastTime = DateTime.UtcNow;

    private Window? _helpWin;

    public MainLoop(Terminal t, VizConfig cfg, Action<Frame> draw, TooltipProvider? tooltip = null)
    {
        _t = t; _cfg = cfg; _draw = draw;
        _tooltip = tooltip;
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

        _vp.SetZoom(1.0);

        if (_cfg.CenterAtZero)
        {
            // classic behavior: place world (0,0) at screen center
            _vp.CenterOn(0, 0);
        }
        else
        {
            // map world (0,0) to the top-left MAP cell (just right/below rulers)
            // Viewport uses +4 (x) and +1 (y) internally, so compensate with LeftRulerWidth
            var lw = Math.Max(1, _cfg.LeftRulerWidth);
            var targetOriginX = 4 - lw; // ensures WorldToScreen(0,*) → sx = lw
            var targetOriginY = 0.0;    // ensures WorldToScreen(*,0) → sy = 1
            _vp.Offset(targetOriginX - _vp.OffsetX, targetOriginY - _vp.OffsetY);
        }


        _inputReader = new InputReader(_input);
        _inputReader.Start();

        _sw.Start();
        var running = true;
        _input.Dirty = true;

        while (running)
        {
            OnTerminalResize();
            running = ConsumeKeyboardInput(running);
            HandleMouse();

            if (Window.HandleMouse(_input, _t.Width, _t.Height))
            {
                _input.Dirty = true;
            }

            // Continuous render when autoplay
            if (_cfg.AutoPlay && _cfg.ContinuousRenderWhenAutoPlay)
            {
                _input.Dirty = true;
            }

            // Autoplay
            if (_cfg.AutoPlay)
            {
                _accum += _sw.Elapsed.TotalSeconds;
                _sw.Restart();
                var stepEvery = 1.0 / Math.Max(0.0001, _cfg.AutoStepPerSecond);
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

            // Render only when dirty
            if (_input.Dirty)
            {
                Draw();
                _input.StepRequested = false;
                _input.ConsumedMouseMove();
            }
            else
            {
                Thread.Sleep(1);
            }

            // FPS throttle
            if (_cfg.TargetFps > 0)
            {
                var targetMs = 1000 / _cfg.TargetFps;
                Thread.Sleep(Math.Max(0, targetMs - (int)_sw.ElapsedMilliseconds));
            }
        }

        _inputReader?.Stop();
        _t.EnableMouse(false);
        _t.ShowCursor();
        _t.ExitAltScreen();
    }

    private void OnTerminalResize()
    {
        if (_t.TryRefreshSize())
        {
            _buf.Resize(_t.Width, _t.Height);
            _t.Clear();
            _t.EnableMouse(true);
            _input.OnResize();
            _inputReader?.RequestReset();
            _input.Dirty = true;

            // clamp all windows to new screen size (TODO if you add helper)
            foreach (var w in Window.All()) { /* clamp here if needed */ }
        }
    }

    private void Pan(int dx, int dy)
    {
        var (wWorld, hWorld) = _vp.VisibleWorldSize();
        var stepX = wWorld * _cfg.PanKeyStepFrac;
        var stepY = hWorld * _cfg.PanKeyStepFrac;
        _vp.Offset(dx * stepX, dy * stepY);
        _input.Dirty = true;
    }

}
