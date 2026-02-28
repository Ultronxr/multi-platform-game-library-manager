-- ------------------------------------------------------------
-- MySQL 5.7.44 后端常用参考 SQL（仅 DML/查询，不包含建表 DDL）
-- ------------------------------------------------------------

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
  UTC_TIMESTAMP(6)
)
ON DUPLICATE KEY UPDATE
  id = LAST_INSERT_ID(id),
  external_account_id = VALUES(external_account_id),
  credential_type = VALUES(credential_type),
  credential_value = VALUES(credential_value),
  last_synced_at = VALUES(last_synced_at),
  updated_at = UTC_TIMESTAMP(6);

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

-- 4) 查询全部游戏库存
SELECT
  platform,
  account_name,
  external_game_id,
  title,
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
  last_login_at = UTC_TIMESTAMP(6),
  updated_at = UTC_TIMESTAMP(6)
WHERE id = @userId;
