-- ------------------------------------------------------------
-- Multi Platform Game Library
-- MySQL 5.7.44 schema initialization
-- ------------------------------------------------------------

CREATE DATABASE IF NOT EXISTS game_library_hub
  DEFAULT CHARACTER SET utf8mb4
  DEFAULT COLLATE utf8mb4_unicode_ci;

USE game_library_hub;

CREATE TABLE IF NOT EXISTS app_users (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  username VARCHAR(64) NOT NULL,
  password_hash VARCHAR(512) NOT NULL,
  role VARCHAR(32) NOT NULL DEFAULT 'user',
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  last_login_at DATETIME(6) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uk_app_users_username (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS platform_accounts (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  platform VARCHAR(32) NOT NULL,
  account_name VARCHAR(128) NOT NULL,
  external_account_id VARCHAR(128) NULL,
  credential_type VARCHAR(64) NOT NULL,
  credential_value TEXT NOT NULL,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  last_synced_at DATETIME(6) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uk_platform_account_name (platform, account_name),
  KEY idx_platform_external_account (platform, external_account_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS owned_games (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  account_id BIGINT UNSIGNED NOT NULL,
  platform VARCHAR(32) NOT NULL,
  account_name VARCHAR(128) NOT NULL,
  external_game_id VARCHAR(128) NOT NULL,
  title VARCHAR(512) NOT NULL,
  normalized_title VARCHAR(512) NOT NULL,
  synced_at DATETIME(6) NOT NULL,
  created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (id),
  UNIQUE KEY uk_account_external_game (account_id, external_game_id),
  KEY idx_normalized_title (normalized_title),
  KEY idx_platform_account (platform, account_name),
  CONSTRAINT fk_owned_games_account
    FOREIGN KEY (account_id) REFERENCES platform_accounts(id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
