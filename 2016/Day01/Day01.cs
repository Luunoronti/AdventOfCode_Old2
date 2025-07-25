using System.Linq;

namespace Year_2016;

class Day01
{
    public string Part1(PartInput Input)
    {
        Traveller traveller = new(CardinalDirection.North) { };
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
        Traveller traveller = new(CardinalDirection.North) { StoreVisitedLocations = true };
        foreach (var part in Input.FullString.SplitTrim(','))
        {
            traveller.CardinalDirection = part[0] == 'R' ? traveller.CardinalDirection.Right : part[0] == 'L' ? traveller.CardinalDirection.Left : traveller.CardinalDirection;
            if (!int.TryParse(part[1..], out var steps) ||
                traveller.Walk(steps, null, (t) =>
            {
                return t.VisitedLocations[t.Location] <= 1;
            }) == WalkResult.Cancelled)
                break;
        }

        return traveller.Location.ManhattanDistance(new(0, 0)).ToString();
    }
}
