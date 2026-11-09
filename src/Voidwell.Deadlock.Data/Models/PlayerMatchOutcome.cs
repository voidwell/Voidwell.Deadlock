namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Values match the dump's player_match_outcome enum.
/// </summary>
public enum PlayerMatchOutcome : short
{
    Invalid = 0,
    Win = 1,
    Loss = 2,
    Penalized = 3,
    PenalizedParty = 4,
    NotScored = 5
}
