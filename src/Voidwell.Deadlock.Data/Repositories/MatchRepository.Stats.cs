using Npgsql;
using NpgsqlTypes;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.Repositories;

public partial class MatchRepository
{
    private const string _statCopyCommand = "COPY match_player_stat (match_id, start_time, player_slot, time_stamp_s, net_worth, gold_player, gold_player_orbs, gold_lane_creep_orbs, gold_neutral_creep_orbs, gold_boss, gold_boss_orb, gold_treasure, gold_denied, gold_death_loss, gold_lane_creep, gold_neutral_creep, kills, deaths, assists, creep_kills, neutral_kills, possible_creeps, creep_damage, player_damage, neutral_damage, boss_damage, denies, player_healing, ability_points, self_healing, player_barriering, teammate_healing, teammate_barriering, self_damage, bullet_kills, melee_kills, ability_kills, headshot_kills, player_damage_taken, max_health, weapon_power, tech_power, shots_hit, shots_missed, damage_absorbed, absorption_provided, hero_bullets_hit, hero_bullets_hit_crit, heal_prevented, heal_lost, damage_mitigated, level) FROM STDIN (FORMAT BINARY)";

    private static async Task WriteStatAsync(NpgsqlBinaryImporter writer, MatchPlayerStat stat, CancellationToken cancellationToken)
    {
        await writer.StartRowAsync(cancellationToken);
        await writer.WriteAsync(stat.MatchId, NpgsqlDbType.Bigint, cancellationToken);
        await writer.WriteAsync(stat.StartTime, NpgsqlDbType.TimestampTz, cancellationToken);
        await writer.WriteAsync(stat.PlayerSlot, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.TimeStampS, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.NetWorth, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldPlayer, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldPlayerOrbs, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldLaneCreepOrbs, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldNeutralCreepOrbs, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldBoss, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldBossOrb, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldTreasure, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldDenied, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldDeathLoss, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldLaneCreep, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.GoldNeutralCreep, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.Kills, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.Deaths, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.Assists, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.CreepKills, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.NeutralKills, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.PossibleCreeps, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.CreepDamage, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.PlayerDamage, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.NeutralDamage, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.BossDamage, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.Denies, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.PlayerHealing, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.AbilityPoints, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.SelfHealing, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.PlayerBarriering, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.TeammateHealing, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.TeammateBarriering, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.SelfDamage, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.BulletKills, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.MeleeKills, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.AbilityKills, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.HeadshotKills, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.PlayerDamageTaken, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.MaxHealth, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.WeaponPower, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.TechPower, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.ShotsHit, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.ShotsMissed, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.DamageAbsorbed, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.AbsorptionProvided, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.HeroBulletsHit, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.HeroBulletsHitCrit, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.HealPrevented, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.HealLost, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.DamageMitigated, NpgsqlDbType.Integer, cancellationToken);
        await writer.WriteAsync(stat.Level, NpgsqlDbType.Integer, cancellationToken);
    }
}
