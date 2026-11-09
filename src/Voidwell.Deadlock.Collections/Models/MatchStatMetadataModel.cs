namespace Voidwell.Deadlock.Collections.Models;

/// <summary>
/// One cumulative stat sample for a player. The API also returns gold_source_* breakdowns, which are not read.
/// </summary>
public class MatchStatMetadataModel
{
    public long TimeStampS { get; set; }

    public long NetWorth { get; set; }
    public long GoldPlayer { get; set; }
    public long GoldPlayerOrbs { get; set; }
    public long GoldLaneCreepOrbs { get; set; }
    public long GoldNeutralCreepOrbs { get; set; }
    public long GoldBoss { get; set; }
    public long GoldBossOrb { get; set; }
    public long GoldTreasure { get; set; }
    public long GoldDenied { get; set; }
    public long GoldDeathLoss { get; set; }
    public long GoldLaneCreep { get; set; }
    public long GoldNeutralCreep { get; set; }
    public long Kills { get; set; }
    public long Deaths { get; set; }
    public long Assists { get; set; }
    public long CreepKills { get; set; }
    public long NeutralKills { get; set; }
    public long PossibleCreeps { get; set; }
    public long CreepDamage { get; set; }
    public long PlayerDamage { get; set; }
    public long NeutralDamage { get; set; }
    public long BossDamage { get; set; }
    public long Denies { get; set; }
    public long PlayerHealing { get; set; }
    public long AbilityPoints { get; set; }
    public long SelfHealing { get; set; }
    public long PlayerBarriering { get; set; }
    public long TeammateHealing { get; set; }
    public long TeammateBarriering { get; set; }
    public long SelfDamage { get; set; }
    public long BulletKills { get; set; }
    public long MeleeKills { get; set; }
    public long AbilityKills { get; set; }
    public long HeadshotKills { get; set; }
    public long PlayerDamageTaken { get; set; }
    public long MaxHealth { get; set; }
    public long WeaponPower { get; set; }
    public long TechPower { get; set; }
    public long ShotsHit { get; set; }
    public long ShotsMissed { get; set; }
    public long DamageAbsorbed { get; set; }
    public long AbsorptionProvided { get; set; }
    public long HeroBulletsHit { get; set; }
    public long HeroBulletsHitCrit { get; set; }
    public long HealPrevented { get; set; }
    public long HealLost { get; set; }
    public long DamageMitigated { get; set; }
    public long Level { get; set; }
}
