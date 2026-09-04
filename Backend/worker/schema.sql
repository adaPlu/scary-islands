CREATE TABLE IF NOT EXISTS runs (
  id TEXT PRIMARY KEY,
  player_id TEXT NOT NULL,
  island TEXT NOT NULL,
  duration_ms INTEGER NOT NULL CHECK(duration_ms > 0),
  escaped INTEGER NOT NULL CHECK(escaped IN (0,1)),
  created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS runs_leaderboard ON runs(island, escaped, duration_ms);

-- Redemption codes are intentionally never stored as plaintext.
-- Only SHA-256 hashes of normalized codes are kept in D1 so valid codes are
-- not exposed by the Unity client or this public repository.
CREATE TABLE IF NOT EXISTS promo_codes (
  id TEXT PRIMARY KEY,
  code_hash TEXT NOT NULL UNIQUE,
  reward_type TEXT NOT NULL,
  reward_value INTEGER NOT NULL DEFAULT 0,
  max_redemptions INTEGER,
  redemption_count INTEGER NOT NULL DEFAULT 0,
  active INTEGER NOT NULL DEFAULT 1 CHECK(active IN (0,1)),
  expires_at TEXT,
  created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS promo_redemptions (
  id TEXT PRIMARY KEY,
  promo_code_id TEXT NOT NULL,
  player_id TEXT NOT NULL,
  redeemed_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE(promo_code_id, player_id),
  FOREIGN KEY(promo_code_id) REFERENCES promo_codes(id)
);

CREATE INDEX IF NOT EXISTS promo_codes_hash ON promo_codes(code_hash);
CREATE INDEX IF NOT EXISTS promo_redemptions_player ON promo_redemptions(player_id);
