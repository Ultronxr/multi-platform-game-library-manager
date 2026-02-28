# 变更记录（2026-03-01）

- 文件名规则：`CHANGELOG_{yyyy-MM-dd}.md`
- 说明：本文件仅记录 `2026-03-01` 当日变更。

## 1. 时间线（逐次更新）

1. 2026-03-01 00:18:23 +08:00
- 变更：按 `AGENTS.md` 要求补齐项目整理文档
- 新增：
  - `ARCHITECTURE.md`
  - `API_CONTRACTS.md`
- 调整：
  - `trace-logs/CHANGELOG_2026-02-28.md`
- 说明：新增项目架构树形说明与 API 契约文档，明确文件职责、接口路径、请求响应模型、鉴权与错误码约定，作为后续结构查阅与前后端联调基线。

2. 2026-03-01 01:11:33 +08:00
- 变更：按编码规范执行代码注释与文档化整理（不改变业务行为）
- 调整：
  - `backend/Controllers/AuthController.cs`
  - `backend/Controllers/HealthController.cs`
  - `backend/Controllers/LibraryController.cs`
  - `backend/Controllers/SyncController.cs`
  - `backend/Data/GameLibraryDbContext.cs`
  - `backend/Data/Entities/AppUserEntity.cs`
  - `backend/Data/Entities/OwnedGameEntity.cs`
  - `backend/Data/Entities/PlatformAccountEntity.cs`
  - `backend/Models/AuthModels.cs`
  - `backend/Models/GamePlatform.cs`
  - `backend/Models/LibraryResponse.cs`
  - `backend/Models/OwnedGame.cs`
  - `backend/Models/SavedAccount.cs`
  - `backend/Models/SyncRequests.cs`
  - `backend/Services/AuthOptions.cs`
  - `backend/Services/DuplicateDetector.cs`
  - `backend/Services/EfCoreGameLibraryStore.cs`
  - `backend/Services/EpicLibraryClient.cs`
  - `backend/Services/IGameLibraryStore.cs`
  - `backend/Services/JwtTokenService.cs`
  - `backend/Services/PasswordHashService.cs`
  - `backend/Services/SteamOwnedGamesClient.cs`
  - `backend/Services/SyncResult.cs`
  - `backend/Services/TitleNormalizer.cs`
  - `frontend/src/App.vue`
  - `trace-logs/CHANGELOG_2026-02-28.md`
- 说明：为后端公开类/接口/方法补齐 XML `summary` 注释，在核心边界逻辑增加中文注释；为前端核心流程与生命周期钩子补齐 JSDoc，统一代码可读性与维护规范。

3. 2026-03-01 01:20:09 +08:00
- 变更：执行第二轮代码规范整理与环境验证
- 调整：
  - `backend/Data/Entities/AppUserEntity.cs`
  - `backend/Data/Entities/OwnedGameEntity.cs`
  - `backend/Data/Entities/PlatformAccountEntity.cs`
  - `frontend/src/api.ts`
  - `trace-logs/CHANGELOG_2026-02-28.md`
- 说明：补齐实体属性级 XML 注释与前端 API 访问层 JSDoc；安装并验证 `.NET SDK 9.0.311` 后完成后端 `net9.0` 构建验证。

4. 2026-03-01 01:39:28 +08:00
- 变更：按 `AGENTS.md` 执行新一轮项目整理校准
- 调整：
  - `ARCHITECTURE.md`
  - `API_CONTRACTS.md`
  - `trace-logs/CHANGELOG_2026-02-28.md`
- 说明：校准 `trace-logs` 实际文件名映射（`CHANGELOG_2026-02-28.md`），刷新架构文档与 API 契约文档更新时间戳，并补充本次整理追溯记录。

5. 2026-03-01 01:45:37 +08:00
- 变更：根据 `AGENTS.md` 重新整理 CHANGELOG 结构
- 调整：
  - `trace-logs/CHANGELOG_2026-02-28.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
  - `ARCHITECTURE.md`
- 说明：按“按天拆分”规则将 2026-03-01 条目从 `CHANGELOG_2026-02-28.md` 迁移到当日日志，并补建开发状态文档以满足协作规范。

6. 2026-03-01 02:06:14 +08:00
- 变更：按代码规范执行全项目分层重构（后端 Service 化 + 前端 Pinia 化）
- 新增：
  - `backend/Services/AuthService.cs`
  - `backend/Services/IAuthService.cs`
  - `backend/Services/ISyncService.cs`
  - `backend/Services/SyncService.cs`
  - `backend/Services/ILibraryQueryService.cs`
  - `backend/Services/LibraryQueryService.cs`
  - `backend/Services/ServiceOperationResult.cs`
  - `frontend/src/stores/authStore.ts`
  - `frontend/src/stores/libraryStore.ts`
  - `frontend/src/types/api.ts`
- 调整：
  - `backend/Controllers/AuthController.cs`
  - `backend/Controllers/LibraryController.cs`
  - `backend/Controllers/SyncController.cs`
  - `backend/Models/AuthModels.cs`
  - `backend/Program.cs`
  - `frontend/src/App.vue`
  - `frontend/src/api.ts`
  - `frontend/src/main.ts`
  - `frontend/package.json`
  - `frontend/package-lock.json`
  - `ARCHITECTURE.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 删除：
  - `frontend/src/types.ts`
- 说明：将 Controller 中业务逻辑下沉到 Service 层，统一服务结果模型；前端引入 Pinia 管理认证与库存状态，页面组件仅负责交互编排；构建与类型检查均通过。

7. 2026-03-01 02:10:56 +08:00
- 变更：补齐数据库建表字段注释
- 调整：
  - `backend/sql/001_init_schema.sql`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：对 `app_users`、`platform_accounts`、`owned_games` 三张表的每个字段补充 `COMMENT`，不改变字段类型与约束。

8. 2026-03-01 02:18:35 +08:00
- 变更：根据数据库规范重整 SQL 文件
- 调整：
  - `backend/sql/001_init_schema.sql`
  - `backend/sql/002_backend_queries.sql`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：统一 SQL 注释与结构化分段风格；`001_init_schema.sql` 在保留字段级 `COMMENT` 基础上补充表级 `COMMENT`，`002_backend_queries.sql` 明确为参考 DML/查询脚本并统一中文说明。
