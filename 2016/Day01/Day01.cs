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
}
