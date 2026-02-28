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
