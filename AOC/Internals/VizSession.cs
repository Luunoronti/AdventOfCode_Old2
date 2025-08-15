using System.Collections.Concurrent;
using TermGlass;

namespace AdventOfCode.Internals;

// A live viz session that the Visualizer drives
public sealed class VizSession
{
    // Queue of draw actions for the next frame
    readonly ConcurrentQueue<Action<Frame>> _pendingDraws = new();

    // step gate: blocks algorithm when manual stepping is active
    readonly ManualResetEventSlim _stepGate = new(initialState: true);

    volatile string _status = string.Empty;
    volatile Func<int, int, string?>? _tooltip;

    // externally controlled by the Visualizer loop:
    public bool AutoPlay { get; set; } = true;
    public int FrameDelayMs { get; set; } = 0;   // throttle if desired

    internal void EnqueueDraw(Action<Frame> draw) => _pendingDraws.Enqueue(draw);
    internal void SetTooltip(Func<int, int, string?> f) => _tooltip = f;
    internal void SetStatus(string s) => _status = s;

    // Called by AoC code to synchronize with stepping
    internal void Gate(string? label)
    {
        // optional: record the label to a timeline buffer (omitted for brevity)
        _stepGate.Wait();
    }

    // Visualizer main loop: drain draw queue into the current frame
    internal void RenderInto(Frame frame)
    {
        while (_pendingDraws.TryDequeue(out var draw))
            draw(frame);

        frame.StatusText = _status;
        frame.TooltipProvider = _tooltip;
    }

    // Controlled by input handling in Visualizer.Run(...)
    internal void SingleStepPulse()
    {
        if (AutoPlay) return;
        // open gate just for one step
        _stepGate.Set();
        _stepGate.Reset();
    }

    internal void SetManual()
    {
        AutoPlay = false; _stepGate.Reset();
    }
    internal void SetAuto()
    {
        AutoPlay = true; _stepGate.Set();
    }
}