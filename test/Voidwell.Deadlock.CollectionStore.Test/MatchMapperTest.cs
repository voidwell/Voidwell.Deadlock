using System.Text.Json;
using Voidwell.Deadlock.Collections;
using Voidwell.Deadlock.Collections.Models;
using Voidwell.Deadlock.Data.Models;
using Xunit;

namespace Voidwell.Deadlock.CollectionStore.Test;

public class MatchMapperTest
{
    // Trimmed from a real /v1/matches/metadata response, including fields the app does not read (gold_source_*).
    private const string _sampleMatch = """
        {
          "match_id": 133243570,
          "start_time": "2026-11-07 22:07:08",
          "winning_team": "Team1",
          "duration_s": 605,
          "match_outcome": "TeamWin",
          "match_mode": "Unranked",
          "game_mode": "Normal",
          "average_badge_team0": 0,
          "average_badge_team1": 0,
          "average_badge": null,
          "not_scored": false,
          "banned_hero_ids": [3, 7],
          "objectives_mask_team0": 65509,
          "objectives_mask_team1": 65533,
          "is_high_skill_range_parties": false,
          "low_pri_pool": false,
          "new_player_pool": false,
          "game_mode_version": 2,
          "players": [
            {
              "account_id": 167491517,
              "hero_id": 13,
              "player_slot": 7,
              "team": "Team1",
              "kills": 4, "deaths": 0, "assists": 1,
              "net_worth": 10661, "last_hits": 73, "denies": 0,
              "ability_points": 12, "assigned_lane": 6, "player_level": 16,
              "abandon_match_time_s": 0,
              "mvp_rank": 3,
              "player_tracked_stats": {},
              "player_match_outcome": "Win",
              "hero_xp": 0,
              "won": true,
              "items": [
                { "game_time_s": 21, "item_id": 1080948381, "upgrade_id": 0, "sold_time_s": 0, "flags": 0, "imbued_ability_id": 0, "upgrade_info": 65536, "net_worth_at_buy": 2628 },
                { "game_time_s": 95, "item_id": 4294967295, "upgrade_id": 1, "sold_time_s": 404, "flags": 1, "imbued_ability_id": 0, "upgrade_info": 65537, "net_worth_at_buy": 13000 }
              ],
              "stats": [
                { "time_stamp_s": 180, "net_worth": 13000, "kills": 3, "player_damage": 4090, "level": 17, "gold_source_players_kills": 3 },
                { "time_stamp_s": 360, "net_worth": 21000, "kills": 4, "player_damage": 9000, "level": 22, "gold_source_players_kills": 4 }
              ]
            },
            {
              "account_id": 12948240,
              "hero_id": 15,
              "player_slot": 2,
              "team": "Team0",
              "kills": 1, "deaths": 5, "assists": 2,
              "net_worth": 8000, "last_hits": 40, "denies": 1,
              "ability_points": 10, "assigned_lane": 1, "player_level": 14,
              "abandon_match_time_s": 0,
              "player_match_outcome": "Loss",
              "items": [],
              "stats": []
            }
          ]
        }
        """;

    private static MatchMetadataModel Deserialize(string json)
    {
        return JsonSerializer.Deserialize<MatchMetadataModel>(json, AssetsClient.JsonOptions)!;
    }

    [Fact]
    public void Map_MapsMatchFields()
    {
        var record = MatchMapper.Map(Deserialize(_sampleMatch));

        var match = record.Match;
        Assert.Equal(133243570, match.MatchId);
        Assert.Equal(new DateTime(2026, 11, 7, 22, 7, 8, DateTimeKind.Utc), match.StartTime);
        Assert.Equal(DateTimeKind.Utc, match.StartTime.Kind);
        Assert.Equal(605, match.DurationS);
        Assert.Equal(MatchMode.Unranked, match.MatchMode);
        Assert.Equal(Team.Team1, match.WinningTeam);
        Assert.Equal(0, match.AverageBadgeTeam0);
        Assert.Equal(0, match.AverageBadgeTeam1);
        Assert.Null(match.AverageBadge);
        Assert.Equal([3, 7], match.BannedHeroIds);
        Assert.Equal(65509, match.ObjectivesMaskTeam0);
        Assert.Equal(65533, match.ObjectivesMaskTeam1);
        Assert.Equal(2, match.GameModeVersion);
        Assert.False(match.NotScored);
    }

    [Fact]
    public void Map_MapsPlayers()
    {
        var record = MatchMapper.Map(Deserialize(_sampleMatch));

        Assert.Equal(2, record.Players.Count);

        var winner = record.Players.Single(a => a.PlayerSlot == 7);
        Assert.Equal(167491517, winner.AccountId);
        Assert.Equal(13, winner.HeroId);
        Assert.Equal(Team.Team1, winner.Team);
        Assert.True(winner.Won);
        Assert.Equal(PlayerMatchOutcome.Win, winner.PlayerMatchOutcome);
        Assert.Equal(10661, winner.NetWorth);
        Assert.Equal(3, winner.MvpRank);
        Assert.Equal(record.Match.StartTime, winner.StartTime);
        Assert.Equal(record.Match.DurationS, winner.DurationS);
        Assert.Equal(MatchMode.Unranked, winner.MatchMode);
    }

    [Fact]
    public void Map_DerivesWonFromTeamWhenFlagIsMissing()
    {
        var record = MatchMapper.Map(Deserialize(_sampleMatch));

        var loser = record.Players.Single(a => a.PlayerSlot == 2);
        Assert.Equal(Team.Team0, loser.Team);
        Assert.False(loser.Won);
        Assert.Null(loser.HeroBuildId);
        Assert.Null(loser.Party);
    }

    [Fact]
    public void Map_MapsItemsInPurchaseOrderAndKeepsLargeIds()
    {
        var record = MatchMapper.Map(Deserialize(_sampleMatch));

        Assert.Equal(2, record.Items.Count);
        Assert.Equal([0, 1], record.Items.Select(a => a.Seq));
        Assert.All(record.Items, a => Assert.Equal(7, a.PlayerSlot));

        // Item ids are unsigned 32-bit and must not be capped.
        Assert.Equal(4294967295, record.Items[1].ItemId);
        Assert.Equal(404, record.Items[1].SoldTimeS);
    }

    [Fact]
    public void Map_MapsStatSamples()
    {
        var record = MatchMapper.Map(Deserialize(_sampleMatch));

        Assert.Equal(2, record.Stats.Count);
        Assert.Equal([180, 360], record.Stats.Select(a => a.TimeStampS));

        var first = record.Stats[0];
        Assert.Equal(13000, first.NetWorth);
        Assert.Equal(3, first.Kills);
        Assert.Equal(4090, first.PlayerDamage);
        Assert.Equal(17, first.Level);
        Assert.Equal(record.Match.MatchId, first.MatchId);
    }

    [Fact]
    public void Map_WithoutPlayers_ReturnsMatchOnly()
    {
        var record = MatchMapper.Map(Deserialize("""{ "match_id": 5, "start_time": "2026-01-02 03:04:05", "winning_team": "Team0", "match_mode": "Ranked", "duration_s": 10 }"""));

        Assert.Empty(record.Players);
        Assert.Empty(record.Items);
        Assert.Empty(record.Stats);
        Assert.Empty(record.Match.BannedHeroIds);
        Assert.Equal(MatchMode.Ranked, record.Match.MatchMode);
    }
}
