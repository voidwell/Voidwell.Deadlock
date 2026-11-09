-- Run once after the initial import has finished loading history.
-- Compresses chunks older than 7 days on a schedule. Compressing while a bulk load is still writing to old chunks is slow.
SELECT add_compression_policy('match', INTERVAL '7 days', if_not_exists => true);
SELECT add_compression_policy('match_player', INTERVAL '7 days', if_not_exists => true);
SELECT add_compression_policy('match_player_item', INTERVAL '7 days', if_not_exists => true);
SELECT add_compression_policy('match_player_stat', INTERVAL '7 days', if_not_exists => true);
