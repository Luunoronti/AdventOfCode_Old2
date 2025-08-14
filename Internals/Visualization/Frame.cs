namespace Visualization;

// Rysowanie w koordach świata vs. ekranu
public sealed class Frame
{
    private readonly Terminal _t;
    private readonly Viewport _vp;
    private readonly CellBuffer _buf;
    internal readonly InputState Input;
    internal readonly VizConfig Cfg;

    internal Frame(Terminal t, Viewport vp, CellBuffer buf, InputState input, VizConfig cfg)
    {
        _t = t; _vp = vp; _buf = buf; Input = input; Cfg = cfg;
    }

    // Transformacje
    public (double wx, double wy) ScreenToWorld(int sx, int sy) => _vp.ScreenToWorld(sx, sy);
    public (int sx, int sy) WorldToScreen(double wx, double wy) => _vp.WorldToScreen(wx, wy);

    // Rysowanie mapy świata przez samplowanie okna widoku
    public void DrawWorld(IWorldSource world) => Renderer.DrawWorld(_buf, _vp, world, Cfg.Layers.HasFlag(UiLayers.Map));

    // (Overlays) — zostawiamy interfejs do rysowania z zewnątrz
    public void DrawRectWorld(double x, double y, double w, double h, char ch, Rgb fg, Rgb bg)
        => Renderer.DrawRectWorld(_buf, _vp, x, y, w, h, ch, fg, bg, Cfg.Layers.HasFlag(UiLayers.Overlays));

    public void DrawCircleWorld(double cx, double cy, double r, char ch, Rgb fg, Rgb bg)
        => Renderer.DrawCircleWorld(_buf, _vp, cx, cy, r, ch, fg, bg, Cfg.Layers.HasFlag(UiLayers.Overlays));

    public void DrawTextScreen(int sx, int sy, string text, Rgb fg, Rgb bg)
        => Renderer.DrawTextScreen(_buf, sx, sy, text, fg, bg, Cfg.Layers.HasFlag(UiLayers.Overlays));
}