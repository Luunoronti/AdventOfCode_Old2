using TermGlass;

namespace AdventOfCode.Internals;

// Ambient gateway you can call from ANYWHERE (AoC parts, helpers, etc.)
public static class Viz
{
    // Ambient session (null when viz is off)
    static readonly AsyncLocal<VizSession?> _current = new();
    internal static VizSession? Current => _current.Value;

    // Safe no-op helpers. You can sprinkle these in puzzle code freely.
    public static void Step(string? label = null) => _current.Value?.Gate(label);
    public static void Draw(Action<Frame> draw) => _current.Value?.EnqueueDraw(draw);
    public static void Tooltip(Func<int, int, string?> tooltip) => _current.Value?.SetTooltip(tooltip);
    public static void Status(string text) => _current.Value?.SetStatus(text);

    // Host helpers for runners
    internal static IDisposable Push(VizSession session)
    {
        var prev = _current.Value;
        _current.Value = session;
        return new Popper(() => _current.Value = prev);
    }

    sealed class Popper : IDisposable
    {
        readonly Action _pop;
        public Popper(Action pop) => _pop = pop;
        public void Dispose() => _pop();
    }
}
