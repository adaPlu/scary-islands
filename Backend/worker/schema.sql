CREATE TABLE IF NOT EXISTS runs (id TEXT PRIMARY KEY, player_id TEXT NOT NULL, island TEXT NOT NULL, duration_ms INTEGER NOT NULL CHECK(duration_ms > 0), escaped INTEGER NOT NULL CHECK(escaped IN (0,1)), created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE INDEX IF NOT EXISTS runs_leaderboard ON runs(island, escaped, duration_ms);

