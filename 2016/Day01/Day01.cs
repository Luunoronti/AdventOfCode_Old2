namespace Year_2016;

class Day01
{
    public string Part1(PartInput Input)
    {
        Traveller traveller = new(CardinalDirection.North);
        foreach (var part in Input.FullString.SplitTrim(','))
        {
            traveller.CardinalDirection = part[0] == 'R' ? traveller.CardinalDirection.Right : part[0] == 'L' ? traveller.CardinalDirection.Left : traveller.CardinalDirection;
            if (int.TryParse(part[1..], out var steps))
                traveller.Walk(steps, null);
        }
        return traveller.Location.ManhattanDistance(new(0, 0)).ToString();
    }
    public string Part2(PartInput Input)
    {
        Traveller traveller = new(CardinalDirection.North);
        List<Location> visitedLocations = [];
        foreach (var part in Input.FullString.SplitTrim(','))
        {
            traveller.CardinalDirection = part[0] == 'R' ? traveller.CardinalDirection.Right : part[0] == 'L' ? traveller.CardinalDirection.Left : traveller.CardinalDirection;
            if (!int.TryParse(part[1..], out var steps) ||
                traveller.Walk(steps, (t) =>
            {
                if (visitedLocations.Contains(t.Location))
                    return StepPred.Cancel;
                visitedLocations.Add(t.Location);
                return StepPred.Continue;
            }) == WalkResult.Cancelled)
                break;
        }

        return traveller.Location.ManhattanDistance(new(0, 0)).ToString();
    }
}
