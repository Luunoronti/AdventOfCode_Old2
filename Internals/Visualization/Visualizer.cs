namespace Visualization;


public static class Visualizer
{
    public static void Run(VizConfig cfg, Action<Frame> draw)
    {
        using var term = new Terminal(cfg.ColorMode);
        var loop = new MainLoop(term, cfg, draw);
        loop.Run();
    }
}