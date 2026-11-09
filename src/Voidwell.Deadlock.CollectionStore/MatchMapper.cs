using System.Globalization;
using Voidwell.Deadlock.Collections.Models;
using Voidwell.Deadlock.Data.Models;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.CollectionStore;

/// <summary>
/// Maps an API match to the rows stored for it. The stat sample mapping lives in the generated MatchMapper.Stats.cs.
/// </summary>
public static partial class MatchMapper
{
    private const string _timeFormat = "yyyy-MM-dd HH:mm:ss";

    public static MatchRecord Map(MatchMetadataModel source)
    {
        var startTime = ParseTime(source.StartTime) ?? throw new InvalidOperationException($"Match {source.MatchId} has no start time.");
        var winningTeam = ParseEnum<Team>(source.WinningTeam);
        var matchMode = ParseEnum<MatchMode>(source.MatchMode);

        var match = new Match
        {
            MatchId = source.MatchId,
            StartTime = startTime,
            DurationS = source.DurationS,
            MatchMode = matchMode,
            WinningTeam = winningTeam,
            AverageBadgeTeam0 = source.AverageBadgeTeam0,
            AverageBadgeTeam1 = source.AverageBadgeTeam1,
            AverageBadge = source.AverageBadge,
            BannedHeroIds = source.BannedHeroIds ?? [],
            ObjectivesMaskTeam0 = source.ObjectivesMaskTeam0 ?? 0,
            ObjectivesMaskTeam1 = source.ObjectivesMaskTeam1 ?? 0,
            IsHighSkillRangeParties = source.IsHighSkillRangeParties,
            LowPriPool = source.LowPriPool,
            NewPlayerPool = source.NewPlayerPool,
            NotScored = source.NotScored,
            GameModeVersion = source.GameModeVersion
        };

        var record = new MatchRecord { Match = match };

        foreach (var player in source.Players ?? [])
        {
            record.Players.Add(MapPlayer(match, player));

            var seq = 0;
            foreach (var item in player.Items ?? [])
            {
                record.Items.Add(new MatchPlayerItem
                {
                    MatchId = match.MatchId,
                    StartTime = match.StartTime,
                    PlayerSlot = player.PlayerSlot,
                    Seq = seq++,
                    ItemId = item.ItemId,
                    UpgradeId = item.UpgradeId,
                    GameTimeS = Clamp(item.GameTimeS),
                    SoldTimeS = Clamp(item.SoldTimeS),
                    Flags = Clamp(item.Flags),
                    ImbuedAbilityId = item.ImbuedAbilityId
                });
            }

            foreach (var stat in player.Stats ?? [])
            {
                record.Stats.Add(MapStat(match, player.PlayerSlot, stat));
            }
        }

        return record;
    }

    private static MatchPlayer MapPlayer(Match match, MatchPlayerMetadataModel source)
    {
        var team = ParseEnum<Team>(source.Team);

        return new MatchPlayer
        {
            MatchId = match.MatchId,
            StartTime = match.StartTime,
            PlayerSlot = source.PlayerSlot,
            AccountId = source.AccountId,
            Team = team,
            HeroId = source.HeroId,

            // Older responses omit the won flag, but it is always whether the player's team won.
            Won = source.Won ?? (team == match.WinningTeam),
            PlayerMatchOutcome = ParseEnum<PlayerMatchOutcome>(source.PlayerMatchOutcome),
            Kills = Clamp(source.Kills),
            Deaths = Clamp(source.Deaths),
            Assists = Clamp(source.Assists),
            NetWorth = Clamp(source.NetWorth),
            LastHits = Clamp(source.LastHits),
            Denies = Clamp(source.Denies),
            AbilityPoints = Clamp(source.AbilityPoints),
            PlayerLevel = Clamp(source.PlayerLevel),
            HeroXp = Clamp(source.HeroXp ?? 0),
            MvpRank = source.MvpRank,
            AbandonMatchTimeS = Clamp(source.AbandonMatchTimeS),
            Party = source.Party,
            AssignedLane = Clamp(source.AssignedLane),
            HeroBuildId = source.HeroBuildId,
            PregameHeroId = source.PregameHeroId,
            MatchMode = match.MatchMode,
            DurationS = match.DurationS,
            AverageBadge = match.AverageBadge
        };
    }

    /// <summary>
    /// The API sends unsigned 32-bit values; the database stores 32-bit signed integers, so out-of-range values are capped.
    /// </summary>
    private static int Clamp(long value)
    {
        return (int)Math.Clamp(value, int.MinValue, int.MaxValue);
    }

    private static DateTime? ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.ParseExact(value, _timeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }

    private static T ParseEnum<T>(string? value)
        where T : struct, Enum
    {
        return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : default;
    }
}
