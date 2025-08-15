using TermGlass;

namespace Year2016;

class Day01
{
    public string Part1(PartInput Input)
    {
        Traveller traveller = new(CardinalDirection.North) { };
        foreach (var part in Input.FullString.SplitTrim(','))
        {
            traveller.CardinalDirection = part[0] switch 
            {
                'R' => traveller.CardinalDirection.Right,
                'L' => traveller.CardinalDirection.Left,
                _ => traveller.CardinalDirection,
            };

            if (int.TryParse(part[1..], out var steps))
                traveller.Walk(steps, null);
        }
        return traveller.Location.ManhattanDistance(new(0, 0)).ToString();
    }
    public string Part2(PartInput Input)
    {
        Part2_Viz(Input);

        Traveller traveller = new(CardinalDirection.North) { StoreVisitedLocations = true };
        foreach (var part in Input.FullString.SplitTrim(','))
        {
            traveller.CardinalDirection = part[0] switch
            {
                'R' => traveller.CardinalDirection.Right,
                'L' => traveller.CardinalDirection.Left,
                _ => traveller.CardinalDirection,
            };

            if (!int.TryParse(part[1..], out var steps))
                break;

            if (traveller.Walk(steps, null, (t) => t.GetVisitedCount(t.Location) <= 1) == WalkResult.Cancelled)
                break;
        }

        return traveller.Location.ManhattanDistance(new(0, 0)).ToString();
    }




    // Interactive visualization for Part2 (you can do the same for Part1)
    public string Part2_Viz(PartInput input)
    {
        // Parse input once
        var tokens = input.FullString.Split(',').Select(s => s.Trim()).ToArray();

        // Traveller from your lib
        var traveller = new Traveller(CardinalDirection.North) { StoreVisitedLocations = true };

        // State for stepping
        int instrIndex = 0;
        int stepsLeftInInstr = 0;
        HashSet<(int x, int y)> visited = new() { (0, 0) };
        bool done = false;

        // Parse current instruction (turn + total steps)
        void LoadNextInstruction()
        {
            if (instrIndex >= tokens.Length) { done = true; return; }
            var part = tokens[instrIndex++];
            traveller.CardinalDirection = part[0] switch
            {
                'R' => traveller.CardinalDirection.Right,
                'L' => traveller.CardinalDirection.Left,
                _ => traveller.CardinalDirection,
            };
            stepsLeftInInstr = int.TryParse(part[1..], out var s) ? s : 0;
        }
        LoadNextInstruction();

        // Run visualizer until finished
        Visualizer.Run(
            new VizConfig
            {
                ColorMode = ColorMode.TrueColor,
                Layers = UiLayers.All,
                AutoPlay = false,            // press Space to step; set true for autoplay
                AutoStepPerSecond = 10.0,    // if you toggle autoplay (1/2/3 keys)
            },
            draw: frame =>
            {
                // Advance one step when requested
                if (!done && (frame.Input.StepRequested || frame.Cfg.AutoPlay))
                {
                    if (stepsLeftInInstr <= 0)
                        LoadNextInstruction();

                    if (!done && stepsLeftInInstr > 0)
                    {
                        // Walk exactly one step; stop early if revisiting for Part2 semantics
                        var result = traveller.Walk(
                            1,
                            null,
                            t => t.GetVisitedCount(t.Location) <= 1
                        );
                        stepsLeftInInstr--;

                        visited.Add(((int)traveller.Location.X, (int)traveller.Location.Y));

                        if (result == WalkResult.Cancelled)
                            done = true;
                        else if (stepsLeftInInstr == 0 && instrIndex >= tokens.Length)
                            done = true;

                        // Force redraw (MainLoop also handles dirty, this helps responsiveness)
                        frame.Input.Dirty = true;
                    }
                }

                // Draw visited cells as overlays (sparse map)
                foreach (var (x, y) in visited)
                    frame.DrawRectWorld(x, y, 1, 1, '.', new Rgb(230, 230, 230), new Rgb(40, 40, 40));

                // Draw current traveller position
                frame.DrawRectWorld(traveller.Location.X, traveller.Location.Y, 1, 1, '@', new Rgb(255, 230, 120), new Rgb(60, 60, 60));

                // Draw origin for reference
                frame.DrawRectWorld(0, 0, 1, 1, 'X', new Rgb(200, 80, 80), new Rgb(30, 30, 30));

                // Optionally: final answer on completion
                if (done)
                {
                    var dist = traveller.Location.ManhattanDistance(new(0, 0));
                    frame.DrawTextScreen(4, 2, $"Done. Manhattan distance: {dist}", Rgb.Black, Rgb.Yellow);
                }
            },
            info: (ix, iy) =>
            {
                // Tooltip near cursor (index is world cell)
                var dist = Math.Abs(traveller.Location.X) + Math.Abs(traveller.Location.Y);
                var hereVisited = visited.Contains((ix, iy));
                return $"({ix},{iy})\nVisited: {hereVisited}\nTraveller: ({traveller.Location.X},{traveller.Location.Y})\nDist: {dist}";
            }
        );

        // Return final answer (driver reflection can treat this like Part2)
        return traveller.Location.ManhattanDistance(new(0, 0)).ToString();
    }
}
