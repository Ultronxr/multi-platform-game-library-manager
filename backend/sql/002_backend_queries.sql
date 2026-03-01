-- ------------------------------------------------------------
-- MySQL 5.7.44 后端常用参考 SQL（仅 DML/查询，不包含建表 DDL）
-- ------------------------------------------------------------
SET time_zone = '+08:00';

-- 1) 平台账号与凭证信息 Upsert
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
  CURRENT_TIMESTAMP(6)
)
ON DUPLICATE KEY UPDATE
  id = LAST_INSERT_ID(id),
  external_account_id = VALUES(external_account_id),
  credential_type = VALUES(credential_type),
  credential_value = VALUES(credential_value),
  last_synced_at = VALUES(last_synced_at),
  updated_at = CURRENT_TIMESTAMP(6);

-- 2) 删除账号历史库存（全量刷新前清空）
DELETE FROM owned_games
WHERE account_id = @accountId;

-- 3) 插入单条已拥有游戏记录
INSERT INTO owned_games (
  account_id,
  platform,
  account_name,
  external_game_id,
  title,
  epic_app_name,
  normalized_title,
  synced_at
)
VALUES (
  @accountId,
  @platform,
  @accountName,
  @externalGameId,
  @title,
  @epicAppName,
  @normalizedTitle,
  @syncedAt
);

-- 4) 查询全部游戏库存
SELECT
  platform,
  account_name,
  external_game_id,
  title,
  epic_app_name,
  synced_at
FROM owned_games
ORDER BY title ASC;

-- 5) 查询已保存平台账号
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

-- 6) 检查是否仍可执行 bootstrap-admin
SELECT COUNT(1) AS user_count
FROM app_users;

-- 7) 按用户名查询可登录用户
SELECT
  id,
  username,
  password_hash,
  role,
  is_active,
  created_at,
  updated_at,
  last_login_at
FROM app_users
WHERE username = @username
LIMIT 1;

-- 8) 新增管理员/普通用户
INSERT INTO app_users (
  username,
  password_hash,
  role,
  is_active
)
VALUES (
  @username,
  @passwordHash,
  @role,
  1
);

-- 9) 更新用户最近登录时间
UPDATE app_users
SET
  last_login_at = CURRENT_TIMESTAMP(6),
  updated_at = CURRENT_TIMESTAMP(6)
WHERE id = @userId;

-- 10) （可选且仅执行一次）为历史库补充 Epic appName 字段
SET @has_epic_app_name := (
  SELECT COUNT(1)
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'owned_games'
    AND COLUMN_NAME = 'epic_app_name'
);
SET @alter_sql := IF(
  @has_epic_app_name = 0,
  'ALTER TABLE owned_games ADD COLUMN epic_app_name VARCHAR(256) NULL COMMENT ''Epic 应用标识（仅 Epic 数据有值）'' AFTER title',
  'SELECT ''owned_games.epic_app_name already exists'' AS message'
);
PREPARE epic_app_stmt FROM @alter_sql;
EXECUTE epic_app_stmt;
DEALLOCATE PREPARE epic_app_stmt;

-- 11) （可选且仅执行一次）历史 UTC 数据迁移为 UTC+8
-- 注意：以下语句仅用于历史数据一次性迁移，重复执行会导致时间再次 +8 小时。
START TRANSACTION;

UPDATE app_users
SET
  created_at = DATE_ADD(created_at, INTERVAL 8 HOUR),
  updated_at = DATE_ADD(updated_at, INTERVAL 8 HOUR),
  last_login_at = CASE
    WHEN last_login_at IS NULL THEN NULL
    ELSE DATE_ADD(last_login_at, INTERVAL 8 HOUR)
  END;

UPDATE platform_accounts
SET
  created_at = DATE_ADD(created_at, INTERVAL 8 HOUR),
  updated_at = DATE_ADD(updated_at, INTERVAL 8 HOUR),
  last_synced_at = CASE
    WHEN last_synced_at IS NULL THEN NULL
    ELSE DATE_ADD(last_synced_at, INTERVAL 8 HOUR)
  END;

UPDATE owned_games
SET
  synced_at = DATE_ADD(synced_at, INTERVAL 8 HOUR),
  created_at = DATE_ADD(created_at, INTERVAL 8 HOUR);

COMMIT;
