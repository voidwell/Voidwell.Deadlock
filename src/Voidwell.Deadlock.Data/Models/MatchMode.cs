namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// Values match the dump's match_mode enum.
/// </summary>
public enum MatchMode : short
{
    Invalid = 0,
    Unranked = 1,
    PrivateLobby = 2,
    CoopBot = 3,
    Ranked = 4,
    ServerTest = 5,
    Tutorial = 6,
    HeroLabs = 7,
    NewPlayerPlacement = 8
}
