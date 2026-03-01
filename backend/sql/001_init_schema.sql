-- ------------------------------------------------------------
-- 跨平台游戏库管理系统
-- MySQL 5.7.44 初始化建库建表脚本
-- ------------------------------------------------------------

CREATE DATABASE IF NOT EXISTS game_library_hub
  DEFAULT CHARACTER SET utf8mb4
  DEFAULT COLLATE utf8mb4_unicode_ci;

USE game_library_hub;
SET time_zone = '+08:00';

CREATE TABLE IF NOT EXISTS app_users (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '用户主键 ID',
  username VARCHAR(64) NOT NULL COMMENT '用户名（全局唯一）',
  password_hash VARCHAR(512) NOT NULL COMMENT '密码哈希值',
  role VARCHAR(32) NOT NULL DEFAULT 'user' COMMENT '用户角色：admin 或 user',
  is_active TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用（1 启用，0 禁用）',
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间（UTC+8）',
  updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT '更新时间（UTC+8）',
  last_login_at DATETIME(6) NULL COMMENT '最近登录时间（UTC+8）',
  PRIMARY KEY (id),
  UNIQUE KEY uk_app_users_username (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='应用用户表';

CREATE TABLE IF NOT EXISTS platform_accounts (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '平台账号主键 ID',
  platform VARCHAR(32) NOT NULL COMMENT '游戏平台标识（Steam/Epic）',
  account_name VARCHAR(128) NOT NULL COMMENT '账号显示名称',
  external_account_id VARCHAR(128) NULL COMMENT '平台侧账号 ID',
  credential_type VARCHAR(64) NOT NULL COMMENT '凭证类型（如 steam_api_key）',
  credential_value TEXT NOT NULL COMMENT '凭证原始值',
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '创建时间（UTC+8）',
  updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT '更新时间（UTC+8）',
  last_synced_at DATETIME(6) NULL COMMENT '最近同步时间（UTC+8）',
  PRIMARY KEY (id),
  UNIQUE KEY uk_platform_account_name (platform, account_name),
  KEY idx_platform_external_account (platform, external_account_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='平台账号与凭证信息表';

CREATE TABLE IF NOT EXISTS owned_games (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '游戏记录主键 ID',
  account_id BIGINT UNSIGNED NOT NULL COMMENT '所属平台账号 ID（关联 platform_accounts.id）',
  platform VARCHAR(32) NOT NULL COMMENT '游戏平台标识（Steam/Epic）',
  account_name VARCHAR(128) NOT NULL COMMENT '同步时账号名称快照',
  external_game_id VARCHAR(128) NOT NULL COMMENT '平台侧游戏 ID',
  title VARCHAR(512) NOT NULL COMMENT '游戏原始标题',
  normalized_title VARCHAR(512) NOT NULL COMMENT '归一化标题（用于重复检测）',
  synced_at DATETIME(6) NOT NULL COMMENT '本次同步时间（UTC+8）',
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT '记录创建时间（UTC+8）',
  PRIMARY KEY (id),
  UNIQUE KEY uk_account_external_game (account_id, external_game_id),
  KEY idx_normalized_title (normalized_title),
  KEY idx_platform_account (platform, account_name),
  CONSTRAINT fk_owned_games_account
    FOREIGN KEY (account_id) REFERENCES platform_accounts(id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='已拥有游戏库存表';
