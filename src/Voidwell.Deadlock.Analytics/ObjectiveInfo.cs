namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// What each of the 16 objective bits in a match stands for, in the order the game's data lists them.
/// </summary>
public static class ObjectiveInfo
{
    public static (string Name, string Type, int? Lane) Describe(int objective)
    {
        return objective switch
        {
            0 => ("Core", "core", null),
            >= 1 and <= 4 => ($"Tier 1, lane {objective}", "tier1", objective),
            >= 5 and <= 8 => ($"Tier 2, lane {objective - 4}", "tier2", objective - 4),
            9 => ("Titan", "titan", null),
            10 or 11 => ($"Titan shield generator {objective - 9}", "shieldGenerator", null),
            >= 12 and <= 15 => ($"Barracks boss, lane {objective - 11}", "barracks", objective - 11),
            _ => ($"Objective {objective}", "unknown", null)
        };
    }
}
