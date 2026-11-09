using Voidwell.Deadlock.Collections.Models;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.CollectionStore;

public static partial class MatchMapper
{
    private static MatchPlayerStat MapStat(Match match, int playerSlot, MatchStatMetadataModel source)
    {
        return new MatchPlayerStat
        {
            MatchId = match.MatchId,
            StartTime = match.StartTime,
            PlayerSlot = playerSlot,
            TimeStampS = Clamp(source.TimeStampS),
            NetWorth = Clamp(source.NetWorth),
            GoldPlayer = Clamp(source.GoldPlayer),
            GoldPlayerOrbs = Clamp(source.GoldPlayerOrbs),
            GoldLaneCreepOrbs = Clamp(source.GoldLaneCreepOrbs),
            GoldNeutralCreepOrbs = Clamp(source.GoldNeutralCreepOrbs),
            GoldBoss = Clamp(source.GoldBoss),
            GoldBossOrb = Clamp(source.GoldBossOrb),
            GoldTreasure = Clamp(source.GoldTreasure),
            GoldDenied = Clamp(source.GoldDenied),
            GoldDeathLoss = Clamp(source.GoldDeathLoss),
            GoldLaneCreep = Clamp(source.GoldLaneCreep),
            GoldNeutralCreep = Clamp(source.GoldNeutralCreep),
            Kills = Clamp(source.Kills),
            Deaths = Clamp(source.Deaths),
            Assists = Clamp(source.Assists),
            CreepKills = Clamp(source.CreepKills),
            NeutralKills = Clamp(source.NeutralKills),
            PossibleCreeps = Clamp(source.PossibleCreeps),
            CreepDamage = Clamp(source.CreepDamage),
            PlayerDamage = Clamp(source.PlayerDamage),
            NeutralDamage = Clamp(source.NeutralDamage),
            BossDamage = Clamp(source.BossDamage),
            Denies = Clamp(source.Denies),
            PlayerHealing = Clamp(source.PlayerHealing),
            AbilityPoints = Clamp(source.AbilityPoints),
            SelfHealing = Clamp(source.SelfHealing),
            PlayerBarriering = Clamp(source.PlayerBarriering),
            TeammateHealing = Clamp(source.TeammateHealing),
            TeammateBarriering = Clamp(source.TeammateBarriering),
            SelfDamage = Clamp(source.SelfDamage),
            BulletKills = Clamp(source.BulletKills),
            MeleeKills = Clamp(source.MeleeKills),
            AbilityKills = Clamp(source.AbilityKills),
            HeadshotKills = Clamp(source.HeadshotKills),
            PlayerDamageTaken = Clamp(source.PlayerDamageTaken),
            MaxHealth = Clamp(source.MaxHealth),
            WeaponPower = Clamp(source.WeaponPower),
            TechPower = Clamp(source.TechPower),
            ShotsHit = Clamp(source.ShotsHit),
            ShotsMissed = Clamp(source.ShotsMissed),
            DamageAbsorbed = Clamp(source.DamageAbsorbed),
            AbsorptionProvided = Clamp(source.AbsorptionProvided),
            HeroBulletsHit = Clamp(source.HeroBulletsHit),
            HeroBulletsHitCrit = Clamp(source.HeroBulletsHitCrit),
            HealPrevented = Clamp(source.HealPrevented),
            HealLost = Clamp(source.HealLost),
            DamageMitigated = Clamp(source.DamageMitigated),
            Level = Clamp(source.Level)
        };
    }
}
