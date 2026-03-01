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

9. 2026-03-01 09:54:35 +08:00
- 变更：按开发规范统一后端异步方法命名
- 调整：
  - `backend/Controllers/AuthController.cs`
  - `backend/Controllers/LibraryController.cs`
  - `backend/Controllers/SyncController.cs`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：将控制器中的 `public async` 方法统一改为 `*Async` 后缀（如 `LoginAsync`、`GetLibraryAsync`、`SyncSteamAsync`），不改变路由与业务行为；后端构建与前端类型检查通过。

10. 2026-03-01 10:00:04 +08:00
- 变更：按开发规范清洗文件命名与目录结构
- 调整：
  - `backend/Services/Auth/AuthOptions.cs`
  - `backend/Services/Auth/IAuthService.cs`
  - `backend/Services/Auth/AuthService.cs`
  - `backend/Services/Auth/JwtTokenService.cs`
  - `backend/Services/Auth/PasswordHashService.cs`
  - `backend/Services/Library/DuplicateDetector.cs`
  - `backend/Services/Library/EfCoreGameLibraryStore.cs`
  - `backend/Services/Library/IGameLibraryStore.cs`
  - `backend/Services/Library/ILibraryQueryService.cs`
  - `backend/Services/Library/LibraryQueryService.cs`
  - `backend/Services/Library/TitleNormalizer.cs`
  - `backend/Services/Sync/EpicLibraryClient.cs`
  - `backend/Services/Sync/SteamOwnedGamesClient.cs`
  - `backend/Services/Sync/SyncResult.cs`
  - `backend/Services/Sync/ISyncService.cs`
  - `backend/Services/Sync/SyncService.cs`
  - `backend/Services/Common/ServiceOperationResult.cs`
  - `frontend/src/services/gameLibraryApi.ts`
  - `frontend/src/stores/authStore.ts`
  - `frontend/src/stores/libraryStore.ts`
  - `frontend/src/types/gameLibrary.ts`
  - `ARCHITECTURE.md`
  - `API_CONTRACTS.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 删除：
  - `frontend/src/api.ts`
  - `frontend/src/types/api.ts`
- 说明：后端 `Services` 按 `Auth/Library/Sync/Common` 分层归档，前端 API 入口迁移到 `services` 目录且类型文件命名统一，完成引用修复并通过后端构建与前端类型检查。

11. 2026-03-01 10:13:51 +08:00
- 变更：后端将 Swagger 排除在登录鉴权之外
- 调整：
  - `backend/Program.cs`
  - `API_CONTRACTS.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：将 Swagger 分支中间件前置到 `UseAuthentication/UseAuthorization` 之前，确保 `/swagger` 路由无需登录访问；业务接口鉴权逻辑保持不变。

12. 2026-03-01 10:33:00 +08:00
- 变更：统一所有 API 时间展示为 UTC+8 字符串格式并移除 UTC 时间戳展示
- 新增：
  - `backend/Services/Common/Utc8DateTimeJsonConverter.cs`
- 调整：
  - `backend/Program.cs`
  - `backend/Controllers/HealthController.cs`
  - `frontend/src/App.vue`
  - `API_CONTRACTS.md`
  - `ARCHITECTURE.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：后端注册全局 `DateTime`/`DateTime?` JSON 转换器，统一返回 `UTC+8` 且格式为 `yyyy-MM-dd HH:mm:ss`；健康检查字段由 `utc` 调整为 `time`；前端表头从 `(UTC)` 改为 `(UTC+8)`，契约文档与架构文档同步更新。

13. 2026-03-01 10:42:26 +08:00
- 变更：数据库持久化时间改为 UTC+8，SQL 脚本同步调整
- 调整：
  - `backend/Services/Common/Utc8DateTimeJsonConverter.cs`
  - `backend/Services/Auth/AuthService.cs`
  - `backend/Services/Library/EfCoreGameLibraryStore.cs`
  - `backend/Data/Entities/AppUserEntity.cs`
  - `backend/Data/Entities/PlatformAccountEntity.cs`
  - `backend/Data/Entities/OwnedGameEntity.cs`
  - `backend/sql/001_init_schema.sql`
  - `backend/sql/002_backend_queries.sql`
  - `API_CONTRACTS.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：后端写库时间统一使用 `UTC+8`，并修正时间转换器对 `DateTimeKind.Unspecified` 的处理，避免数据库 `DATETIME` 二次偏移；SQL 脚本显式设置 `time_zone = '+08:00'`，并将 `UTC_TIMESTAMP(6)` 改为 `CURRENT_TIMESTAMP(6)`。

14. 2026-03-01 10:44:00 +08:00
- 变更：补充历史 UTC 数据迁移 SQL 与契约文档时间戳
- 调整：
  - `backend/sql/002_backend_queries.sql`
  - `API_CONTRACTS.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：在 SQL 参考脚本新增“仅执行一次”的历史时间字段整体 `+8` 小时迁移事务，避免存量数据与新入库 UTC+8 规则混用。

15. 2026-03-01 11:18:44 +08:00
- 变更：新增已保存账号的一键重拉、修改、删除能力（前后端联动）
- 新增：
  - `backend/Models/AccountManagementModels.cs`
  - `backend/Services/Library/IAccountManagementService.cs`
  - `backend/Services/Library/AccountManagementService.cs`
- 调整：
  - `backend/Controllers/LibraryController.cs`
  - `backend/Program.cs`
  - `frontend/src/services/gameLibraryApi.ts`
  - `frontend/src/stores/libraryStore.ts`
  - `frontend/src/types/gameLibrary.ts`
  - `frontend/src/App.vue`
  - `frontend/src/style.css`
  - `API_CONTRACTS.md`
  - `ARCHITECTURE.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：后端新增 `/api/accounts/{id}/resync|PUT|DELETE` 接口，支持使用已保存凭证重拉库存、编辑账号信息（含凭证）与删除账号（级联删除库存）；前端在账号表格新增“重拉库存/修改/删除”操作列并接入 Pinia 状态流。

16. 2026-03-01 11:36:27 +08:00
- 变更：前端表单/表格改造为 Ant Design Vue 组件并完成视觉重构
- 调整：
  - `frontend/package.json`
  - `frontend/package-lock.json`
  - `frontend/src/main.ts`
  - `frontend/src/App.vue`
  - `frontend/src/style.css`
  - `ARCHITECTURE.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：引入 `ant-design-vue` 与 `@ant-design/icons-vue`，将登录/初始化/同步表单、账号与库存表格重写为 Ant 组件（`Form`/`Table`/`Card`/`Modal` 等），保留原有业务交互并增强操作反馈；前端类型检查通过，受环境限制 `vite build` 因 `spawn EPERM` 未能执行。

17. 2026-03-01 11:46:07 +08:00
- 变更：新增游戏库存多维度筛选能力（游戏名/平台/账号名/账号ID）
- 调整：
  - `frontend/src/App.vue`
  - `frontend/src/style.css`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：在“全部库存”区域新增筛选栏，采用“文本模糊查询 + 平台下拉选择”组合；筛选维度包含游戏名称、平台、账号名称、账号ID，并显示筛选结果计数与一键重置。

18. 2026-03-01 11:57:14 +08:00
- 变更：库存列表切换为后端分页（替代前端分页）
- 新增：
  - `backend/Models/LibraryGamePageModels.cs`
- 调整：
  - `backend/Controllers/LibraryController.cs`
  - `backend/Services/Library/ILibraryQueryService.cs`
  - `backend/Services/Library/LibraryQueryService.cs`
  - `frontend/src/services/gameLibraryApi.ts`
  - `frontend/src/stores/libraryStore.ts`
  - `frontend/src/types/gameLibrary.ts`
  - `frontend/src/App.vue`
  - `API_CONTRACTS.md`
  - `ARCHITECTURE.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：新增 `/api/library/games` 后端分页接口，并在 `/api/library` 增加 `includeGames` 控制摘要返回；前端库存表改为服务端分页与筛选参数联动，避免全量数据在前端分页。

19. 2026-03-01 12:04:07 +08:00
- 变更：固定游戏库存表格高度，避免分页尺寸变更引起窗口高度跳变
- 调整：
  - `frontend/src/App.vue`
  - `frontend/src/style.css`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：为库存表格设置固定滚动高度（桌面与移动端分段高度），分页条数切换时仅表格内部滚动区域变化，不再导致页面整体高度频繁变化。

20. 2026-03-01 12:27:05 +08:00
- 变更：进一步加高游戏库存表格固定高度
- 调整：
  - `frontend/src/App.vue`
  - `frontend/src/style.css`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：将库存表格固定滚动高度从 `520` 提升至 `680`，并同步提升平板/手机断点高度（`560`/`460`），减少可视区域不足导致的频繁滚动。

21. 2026-03-01 12:32:53 +08:00
- 变更：厂商库存 API 响应增加 DEBUG 级完整日志输出
- 调整：
  - `backend/Services/Sync/SteamOwnedGamesClient.cs`
  - `backend/Services/Sync/EpicLibraryClient.cs`
  - `backend/nlog.config`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：Steam/Epic 库存拉取请求在成功、失败与特殊状态码下均记录 `状态码 + 原始响应体` 到 DEBUG 日志；异常场景补充 DEBUG 异常日志；NLog 增加两个客户端 logger 的 Debug 级别输出规则，便于精确追踪返回结果。

22. 2026-03-01 12:44:40 +08:00
- 变更：将数据库 SQL 日志与前端请求触发的后端请求日志统一下调到 DEBUG
- 调整：
  - `backend/Program.cs`
  - `backend/nlog.config`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：新增自定义 HTTP 请求 Debug 中间件（输出方法、路径、状态码与耗时）；EF Core 将 `CommandExecuting/CommandExecuted/CommandError` 事件级别降为 Debug；NLog 将 `Microsoft.*` 总规则提高到 `Warn`，避免框架层 `INFO` 噪声混淆业务日志。

23. 2026-03-01 12:50:50 +08:00
- 变更：回滚上一轮“SQL/请求日志下调到 DEBUG”的改动
- 调整：
  - `backend/Program.cs`
  - `backend/nlog.config`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：移除自定义 HTTP 请求 Debug 中间件与 EF Core `ConfigureWarnings` Debug 降级配置；NLog 恢复为 `Microsoft.*` `Info` 级别输出，保留既有厂商同步客户端 Debug 日志规则。

24. 2026-03-01 13:07:18 +08:00
- 变更：仅通过 `nlog.config` 下调非业务日志到 DEBUG 观察层级
- 调整：
  - `backend/nlog.config`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：不改任何后端代码，仅在 NLog 规则中将 `Microsoft.EntityFrameworkCore.*`、`Microsoft.AspNetCore.*`、`System.Net.Http.HttpClient.*` 的非业务日志限制在 `Debug` 范围；`Microsoft.*` 统一收敛到 `Warn+`，业务日志继续走 `Info+`。

25. 2026-03-01 13:27:58 +08:00
- 变更：彻底关闭 `Microsoft.AspNetCore` 与 `EF Core` 日志
- 调整：
  - `backend/nlog.config`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：在 NLog 新增 `Null` 目标 `blackhole`，并将 `Microsoft.EntityFrameworkCore.*` 与 `Microsoft.AspNetCore.*` 全量路由到 `blackhole`（`minLevel=Trace` + `final=true`），实现这两类日志完全静默。

26. 2026-03-01 13:52:15 +08:00
- 变更：修复 Epic 库存游戏名称解析优先级
- 调整：
  - `backend/Services/Sync/EpicLibraryClient.cs`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：`ResolveTitle` 增加 `sandboxName` 最高优先级（同时覆盖顶层、`metadata`、`catalogItem` 三个层级），确保 Epic 返回记录中优先使用 `sandboxName` 作为游戏名称。

27. 2026-03-01 16:12:26 +08:00
- 变更：修复 Epic 库存接口分页拉取不完整问题
- 调整：
  - `backend/Services/Sync/EpicLibraryClient.cs`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：`GetOwnedGamesAsync` 改为按 `responseMetadata.nextCursor` 循环翻页拉取，并传递 `stateToken`；新增游标防死循环保护与 `externalId` 去重，避免只取首屏导致库存缺失。

28. 2026-03-01 16:58:50 +08:00
- 变更：支持 Epic 同游戏多条目分组展示与统计口径修正
- 调整：
  - `backend/Data/Entities/OwnedGameEntity.cs`
  - `backend/Data/GameLibraryDbContext.cs`
  - `backend/Models/OwnedGame.cs`
  - `backend/Models/LibraryGamePageModels.cs`
  - `backend/Services/Sync/EpicLibraryClient.cs`
  - `backend/Services/Library/EfCoreGameLibraryStore.cs`
  - `backend/Services/Library/LibraryQueryService.cs`
  - `backend/sql/001_init_schema.sql`
  - `backend/sql/002_backend_queries.sql`
  - `frontend/src/types/gameLibrary.ts`
  - `frontend/src/App.vue`
  - `API_CONTRACTS.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：新增并持久化 Epic `appName`（数据库列 `owned_games.epic_app_name`）；库存统计与分页改为按“同平台 + 同账号 + 同游戏名”聚合主记录，保留组内所有原始条目供展开查看；前端库存表改为可展开行展示同组明细，满足“标记同游戏多个项而非去重删除”的业务要求。

29. 2026-03-01 17:27:07 +08:00
- 变更：前端改造为后台管理模板（左侧菜单 + 右侧内容区）
- 调整：
  - `frontend/src/App.vue`
  - `frontend/src/style.css`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：认证后页面重构为左右分栏布局，左侧菜单新增“主页 / 平台账号管理 / 详细库存信息”三个板块；主页展示统计与重复预警，账号管理展示同步与账号信息，详细库存展示全量库存表格。

30. 2026-03-01 18:01:12 +08:00
- 变更：前端结构重构为“独立登录页 + 主布局多路由 + 页面组件化”
- 新增：
  - `frontend/src/stores/pinia.ts`
  - `frontend/src/router/index.ts`
  - `frontend/src/layouts/MainLayout.vue`
  - `frontend/src/pages/LoginPage.vue`
  - `frontend/src/pages/HomePage.vue`
  - `frontend/src/pages/AccountManagementPage.vue`
  - `frontend/src/pages/InventoryPage.vue`
  - `frontend/src/components/auth/LoginFormCard.vue`
  - `frontend/src/components/auth/BootstrapAdminCard.vue`
  - `frontend/src/components/home/SummaryMetrics.vue`
  - `frontend/src/components/home/DuplicateWarningPanel.vue`
  - `frontend/src/components/accounts/SyncPanels.vue`
  - `frontend/src/components/accounts/SavedAccountsTable.vue`
  - `frontend/src/components/accounts/AccountEditModal.vue`
  - `frontend/src/components/inventory/InventoryFilterPanel.vue`
  - `frontend/src/components/inventory/InventoryTable.vue`
- 调整：
  - `frontend/package.json`
  - `frontend/package-lock.json`
  - `frontend/src/App.vue`
  - `frontend/src/main.ts`
  - `frontend/src/style.css`
  - `frontend/src/types/gameLibrary.ts`
  - `ARCHITECTURE.md`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：新增 `vue-router` 并将原单页逻辑拆分到路由页面与业务组件；`/login` 作为独立登录页，登录后进入 `/app` 主布局，左侧菜单切换“主页/平台账号管理/详细库存信息”子路由；完成 `vue-tsc` 与提权 `vite build` 构建验证。

31. 2026-03-01 18:04:18 +08:00
- 变更：补充账号编辑边界处理并完成最终构建复验
- 调整：
  - `frontend/src/components/accounts/AccountEditModal.vue`
  - `trace-logs/CHANGELOG_2026-03-01.md`
  - `trace-logs/DEV_STATE.md`
- 说明：账号编辑提交时，`externalAccountId` 为空将明确提交 `null` 以支持清空；复跑前端 `vue-tsc` 与提权 `vite build` 均通过。
