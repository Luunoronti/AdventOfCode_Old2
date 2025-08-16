namespace TermGlass;


internal sealed partial class MainLoop
{
    private void DrawRulers()
    {
        int W = _t.Width, H = _t.Height;
        if (W < 10 || H < 5) return;

        var bg = _cfg.RulerBgColor;
        var a = _cfg.RulerBgAlpha;
        var lw = Math.Max(1, _cfg.LeftRulerWidth);

        var opaqueMode = !_buf.AlphaBlendEnabled;

        // background of rulers
        for (var x = 0; x < W; x++)
        {
            if (opaqueMode)
                _buf.TrySet(x, 0, new Cell(' ', Rgb.White, bg));
            else
                _buf.BlendBgAndFg(x, 0, bg, a, bg, a);
        }

        for (var x = 0; x < Math.Min(lw, W); x++)
        {
            for (var y = 0; y < H; y++)
            {
                if (opaqueMode)
                    _buf.TrySet(x, y, new Cell(' ', Rgb.White, bg));
                else
                    _buf.BlendBgAndFg(x, y, bg, a, bg, a);
            }
        }

        // labels
        for (var sx = lw; sx < W; sx++)
        {
            var (wx, _) = _vp.ScreenToWorld(sx, 1);
            if (sx % 10 == 0)
            {
                var label = ((int)Math.Round(wx)).ToString();
                Renderer.PutTextKeepBg(_buf, sx, 0, label, Rgb.White);
            }
        }

        for (var sy = 1; sy < H - 1; sy++)
        {
            var (_, wy) = _vp.ScreenToWorld(lw, sy);
            if (sy % 2 == 0)
            {
                var s = ((int)Math.Round(wy)).ToString();
                s = s.Length <= 3 ? s.PadLeft(3) : s[^3..];
                Renderer.PutTextKeepBg(_buf, 0, sy, s, Rgb.White);
            }
        }

        // mouse highlight on rulers
        var msx = _input.MouseX.Clamp(0, W - 1);
        var msy = _input.MouseY.Clamp(0, H - 1);
        var ha = _cfg.RulerHighlightAlpha;
        var hi = new Rgb(80, 140, 240);

        if (opaqueMode)
        {
            _buf.TrySet(msx, 0, new Cell(_buf[msx, 0].Ch, hi, bg));
            _buf.TrySet(0, msy, new Cell(_buf[0, msy].Ch, hi, bg));
        }
        else
        {
            _buf.BlendBgAndFg(msx, 0, hi, ha, hi, ha);
            _buf.BlendBgAndFg(0, msy, hi, ha, hi, ha);
        }
    }

    private void DrawStatusBar()
    {
        int W = _t.Width, H = _t.Height;
        if (W < 10 || H < 3) return;

        for (var x = 0; x < W; x++)
            _buf.Set(x, H - 1, new Cell(' ', Rgb.Black, Rgb.Gray));

        var (ix, iy) = _vp.WorldCellUnderScreen(_input.MouseX, _input.MouseY);
        var autoInfo = _cfg.AutoPlay ? $"{_cfg.AutoStepPerSecond:F1}/s | FPS {_fps:F1}" : "off";

        var baseText = $" {_cfg.ColorMode} | {_cfg.Layers} | Zoom {_vp.Zoom:F2} | Auto {autoInfo} | Cell {ix}, {iy}";
        var line = $"F1 Help | {baseText}";
        if (!string.IsNullOrWhiteSpace(_statusFromFrame))
        {
            var room = Math.Max(0, W - line.Length - 3);
            if (room > 0)
            {
                var extra = _statusFromFrame!.Length > room ? _statusFromFrame.Substring(0, room) : _statusFromFrame;
                line = $"{line} | {extra.PadLeft(room)}";
            }
        }
        var status = line.PadRight(W);
        Renderer.PutText(_buf, 0, H - 1, status[..Math.Min(W, status.Length)], Rgb.Black, Rgb.Gray);
    }

    private void DrawTooltipIfAny()
    {
        if (!_tooltipEnabled) return;
        var provider = _tooltipFromFrame ?? _tooltip;
        if (provider == null) return;
        if (!_cfg.Layers.HasFlag(UiLayers.Overlays)) return;

        // skip tooltip if cursor is over a window
        if (Window.All().Any(w =>
            w.Visible &&
            _input.MouseX >= w.X &&
            _input.MouseX < w.X + w.W &&
            _input.MouseY >= w.Y &&
            _input.MouseY < w.Y + w.H)) return;


        var (ix, iy) = _vp.WorldCellUnderScreen(_input.MouseX, _input.MouseY);
        var text = provider(ix, iy);
        if (string.IsNullOrEmpty(text)) return;

        // initial position near cursor
        var sx = Math.Clamp(_input.MouseX + 2, 0, _t.Width - 1);
        var sy = Math.Clamp(_input.MouseY + 1, 0, _t.Height - 2);

        // estimate tooltip size (similar to Renderer.DrawTooltipBox)
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var maxLen = 0;
        foreach (var ln in lines) maxLen = Math.Max(maxLen, ln?.Length ?? 0);
        var w = Math.Clamp(maxLen + 2, 6, _t.Width);
        var h = Math.Min(lines.Length, Math.Max(1, _t.Height - 1));

        if (sx + w >= _t.Width) sx = Math.Max(0, _t.Width - w - 1);
        if (sy + lines.Length >= _t.Height - 1)
            sy = Math.Max(0, _input.MouseY - lines.Length - 1);

        Renderer.DrawTooltipBox(_buf, sx, sy, lines, _cfg.TooltipBgAlpha, _cfg.TooltipBorderAlpha);
    }

    private void ToggleHelpWindow()
    {
        if (_helpWin == null)
        {
            var w = Math.Min(64, _t.Width - 6);
            var h = Math.Min(18, _t.Height - 6);
            var x = (_t.Width - w) / 2;
            var y = (_t.Height - h) / 2;

            _helpWin = Window.Create(
                x: x, y: y, w: w, h: h,
                bg: new Rgb(20, 20, 24), bgAlpha: 220,
                z: 100,
                content: (buf, self) =>
                {
                    Renderer.PutTextKeepBg(buf, self.X + 2, self.Y, "[ Help ]", new Rgb(255, 230, 120));

                    int ln = self.Y + 2;
                    int lx = self.X + 2;

                    void L(string s) { Renderer.PutTextKeepBg(buf, lx, ln++, s, new Rgb(230, 230, 230)); }

                    L("Esc/Ctrl+Q : quit");
                    L("= / -       : zoom in/out (also mouse wheel)");
                    L("0           : reset zoom");
                    L("LMB drag    : pan (PPM also if terminal allows)");
                    L("Arrows/WASD : pan");
                    L("Space       : step");
                    L("F5/F6/F7/F8 : layers (all/map/ui/overlays)");
                    L("C           : toggle color mode (TrueColor/16)");
                    L("T           : toggle tooltip");

                    ln++;
                    Renderer.PutTextKeepBg(buf, lx, ln, "Press F1 to hide this window.", new Rgb(200, 220, 255));
                }
            );

            _helpWin.ShowCloseButton = true;
            _helpWin.OnClose = w =>
            {
                _helpWin = null;
                _input.Dirty = true;
            };

            _helpWin.BorderColorActive = new Rgb(255, 200, 80);
        }
        else
        {
            _helpWin.Visible = !_helpWin.Visible;
        }
    }
}
