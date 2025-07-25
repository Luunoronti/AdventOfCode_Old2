
namespace AdventOfCode.Runtime;

public class Traveller
{
    public Traveller() : this(new Location(0, 0), CardinalDirection.North) { }
    public Traveller(CardinalDirection StartDirection) : this(new Location(0, 0), StartDirection) { }
    public Traveller(Location StartLocation) : this(StartLocation, CardinalDirection.North) { }
    public Traveller(Location StartLocation, CardinalDirection StartDirection)
    {
        this.StartLocation = StartLocation;
        this.StartDirection = StartDirection;

        Location = StartLocation;
        CardinalDirection = StartDirection;

        VisitedLocations[Location] = 1;
    }
    public Location Location { get; set; }
    public CardinalDirection CardinalDirection { get; set; }

    public Location StartLocation { get; set; }
    public CardinalDirection StartDirection { get; set; }

    public bool StoreVisitedLocations { get; set; }

    public Dictionary<Location, int> VisitedLocations { get; } = [];


    /// <summary>
    /// Walk n steps in the direction specified in CardinalDirection.
    /// </summary>
    /// <param name="steps">The steps to take</param>
    /// <param name="CanStepPred">A handler that is called before step is taken.</param>
    /// <param name="TookStepPred">A handler that is called after the step is taken.</param>
    public WalkResult Walk(int steps = 1, Func<Traveller, StepPred> CanStepPred = null, Func<Traveller, bool> TookStepPred = null)
    {
        // no callbacks, just take n steps in a given direction
        if (!StoreVisitedLocations && CanStepPred == null && TookStepPred == null)
        {
            Location += CardinalDirection.Direction * steps;
            return WalkResult.Completed;
        }

        // slower method, call our handlers with each step
        for (int s = 0; s < steps; ++s)
        {
            // we must update direction at each step because our callbacks could have changed it
            var dir = CardinalDirection.Direction;

            var p = CanStepPred?.Invoke(this) ?? StepPred.Continue;
            if (p == StepPred.Cancel) return WalkResult.Cancelled;
            if (p == StepPred.TryAgain)
            {
                --s;
                continue;
            }

            Location += dir;
            
            if (StoreVisitedLocations)
                VisitedLocations[Location] = VisitedLocations.TryGetValue(Location, out var visited) ? visited + 1 : 1;

            if (!(TookStepPred?.Invoke(this) ?? true))
                return WalkResult.Cancelled;
        }
        return WalkResult.Completed;
    }
}
