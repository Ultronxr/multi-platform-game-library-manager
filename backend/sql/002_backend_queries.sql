-- ------------------------------------------------------------
-- SQL reference for MySQL 5.7.44 schema/data operations
-- ------------------------------------------------------------

-- 1) Upsert account + credential
INSERT INTO platform_accounts (
  platform,
  account_name,
  external_account_id,
  credential_type,
  credential_value,
  last_synced_at
)
VALUES (
  @platform,
  @accountName,
  @externalAccountId,
  @credentialType,
  @credentialValue,
  UTC_TIMESTAMP(6)
)
ON DUPLICATE KEY UPDATE
  id = LAST_INSERT_ID(id),
  external_account_id = VALUES(external_account_id),
  credential_type = VALUES(credential_type),
  credential_value = VALUES(credential_value),
  last_synced_at = VALUES(last_synced_at),
  updated_at = UTC_TIMESTAMP(6);

-- 2) Delete existing inventory for an account (full refresh)
DELETE FROM owned_games
WHERE account_id = @accountId;

-- 3) Insert one owned game row
INSERT INTO owned_games (
  account_id,
  platform,
  account_name,
  external_game_id,
  title,
  normalized_title,
  synced_at
)
VALUES (
  @accountId,
  @platform,
  @accountName,
  @externalGameId,
  @title,
  @normalizedTitle,
  @syncedAt
);

-- 4) Query all games
SELECT
  platform,
  account_name,
  external_game_id,
  title,
  synced_at
FROM owned_games
ORDER BY title ASC;

-- 5) Query all saved platform accounts
SELECT
  id,
  platform,
  account_name,
  external_account_id,
  credential_type,
  credential_value,
  created_at,
  updated_at,
  last_synced_at
FROM platform_accounts
ORDER BY platform ASC, account_name ASC;
