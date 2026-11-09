namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// A player's cumulative stats at one sample time (the dump samples every 180 seconds, plus the final state).
/// Stored as a TimescaleDB hypertable partitioned on <see cref="StartTime"/>.
/// </summary>
public class MatchPlayerStat
{
    public long MatchId { get; set; }
    public DateTime StartTime { get; set; }
    public int PlayerSlot { get; set; }

    /// <summary>
    /// Seconds into the match this sample was taken.
    /// </summary>
    public int TimeStampS { get; set; }

    public int NetWorth { get; set; }
    public int GoldPlayer { get; set; }
    public int GoldPlayerOrbs { get; set; }
    public int GoldLaneCreepOrbs { get; set; }
    public int GoldNeutralCreepOrbs { get; set; }
    public int GoldBoss { get; set; }
    public int GoldBossOrb { get; set; }
    public int GoldTreasure { get; set; }
    public int GoldDenied { get; set; }
    public int GoldDeathLoss { get; set; }
    public int GoldLaneCreep { get; set; }
    public int GoldNeutralCreep { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int CreepKills { get; set; }
    public int NeutralKills { get; set; }
    public int PossibleCreeps { get; set; }
    public int CreepDamage { get; set; }
    public int PlayerDamage { get; set; }
    public int NeutralDamage { get; set; }
    public int BossDamage { get; set; }
    public int Denies { get; set; }
    public int PlayerHealing { get; set; }
    public int AbilityPoints { get; set; }
    public int SelfHealing { get; set; }
    public int PlayerBarriering { get; set; }
    public int TeammateHealing { get; set; }
    public int TeammateBarriering { get; set; }
    public int SelfDamage { get; set; }
    public int BulletKills { get; set; }
    public int MeleeKills { get; set; }
    public int AbilityKills { get; set; }
    public int HeadshotKills { get; set; }
    public int PlayerDamageTaken { get; set; }
    public int MaxHealth { get; set; }
    public int WeaponPower { get; set; }
    public int TechPower { get; set; }
    public int ShotsHit { get; set; }
    public int ShotsMissed { get; set; }
    public int DamageAbsorbed { get; set; }
    public int AbsorptionProvided { get; set; }
    public int HeroBulletsHit { get; set; }
    public int HeroBulletsHitCrit { get; set; }
    public int HealPrevented { get; set; }
    public int HealLost { get; set; }
    public int DamageMitigated { get; set; }
    public int Level { get; set; }
}
