# 会话代码更新全量追溯（2026-02-28）

- 生成时间：2026-02-28 14:55:45 +08:00
- 追溯范围：本次会话从“创建项目”到“切换 MySQL 5.7.44 + EF Core ORM”全部代码变更
- 说明：时间基于终端执行记录与会话顺序整理，精度为分钟级（部分为区间/估算）

## 1. 时间线（逐次更新）

1. 2026-02-28 13:49
- 变更：初始化项目骨架
- 位置：`/backend`（`dotnet new webapi`）
- 说明：创建后端项目基础结构。

2. 2026-02-28 13:49-13:50
- 变更：移除模板 OpenAPI 依赖，改写 Program 为业务 API
- 文件：`backend/GameLibrary.Api.csproj`、`backend/Program.cs`
- 说明：删除 `Microsoft.AspNetCore.OpenApi`，开始接入 Steam/Epic 同步接口。

3. 2026-02-28 13:50-13:52
- 变更：新增核心模型与服务层（第一版）
- 新增文件：
  - `backend/Models/GamePlatform.cs`
  - `backend/Models/OwnedGame.cs`
  - `backend/Models/SyncRequests.cs`
  - `backend/Models/LibraryResponse.cs`
  - `backend/Services/IGameLibraryStore.cs`
  - `backend/Services/SyncResult.cs`
  - `backend/Services/SteamOwnedGamesClient.cs`
  - `backend/Services/EpicLibraryClient.cs`
  - `backend/Services/DuplicateDetector.cs`
- 说明：实现平台枚举、库存模型、请求模型、重复检测、Steam/Epic 拉取客户端。

4. 2026-02-28 13:52
- 变更：配置文件补充 Steam 配置项
- 文件：`backend/appsettings.json`、`backend/appsettings.Development.json`
- 说明：增加 `Steam:ApiKey` 字段。

5. 2026-02-28 13:52
- 变更：更新 HTTP 示例文件
- 文件：`backend/GameLibrary.Api.http`
- 说明：示例请求由模板 weather 改为 `/api/health`。

6. 2026-02-28 13:53
- 变更：后端首次编译验证通过
- 说明：`dotnet build`（当时无外部包依赖）成功。

7. 2026-02-28 13:53-13:57
- 变更：创建前端 Vue3+TS 项目结构（手工 Vite）
- 新增文件：
  - `frontend/package.json`
  - `frontend/tsconfig.json`
  - `frontend/tsconfig.app.json`
  - `frontend/tsconfig.node.json`
  - `frontend/vite.config.ts`
  - `frontend/index.html`
  - `frontend/src/main.ts`
  - `frontend/src/env.d.ts`
  - `frontend/src/types.ts`
  - `frontend/src/api.ts`
  - `frontend/src/App.vue`
  - `frontend/src/style.css`
- 说明：实现 Steam/Epic 同步表单、库存列表、重复预警。

8. 2026-02-28 13:57
- 变更：补充工程文件
- 新增文件：`/.gitignore`、`frontend/.env.example`、`/README.md`
- 说明：加入运行说明与 Epic Token 获取步骤。

9. 2026-02-28 13:58-14:02
- 变更：存储从内存改为文件持久化（中间版本）
- 新增：`backend/Services/FileBackedGameLibraryStore.cs`
- 删除：`backend/Services/InMemoryGameLibraryStore.cs`
- 调整：`Program.cs`、`.gitignore`、`README.md`
- 说明：支持重启后保留库存。

10. 2026-02-28 14:02
- 变更：移除 `UseHttpsRedirection`
- 文件：`backend/Program.cs`
- 说明：避免本地调试时 HTTP 调用提前结束问题。

11. 2026-02-28 14:03-14:05
- 变更：前端依赖与类型检查修正
- 调整：`frontend/package.json`（补 `@types/node`）
- 调整：`backend/Services/EpicLibraryClient.cs`（允许空库返回成功）
- 说明：修复构建/类型检查阻塞。

12. 2026-02-28 14:07
- 变更：项目重命名
- 目录：`steam-epic-library-manager` -> `multi-platform-game-library-manager`
- 说明：项目名改为平台可扩展。

13. 2026-02-28 14:08-14:15
- 变更：后端改造为 MySQL 持久化（手写 SQL 版本）
- 新增：
  - `backend/Services/MySqlGameLibraryStore.cs`
  - `backend/Models/SavedAccount.cs`
  - `backend/Services/TitleNormalizer.cs`
  - `backend/sql/001_init_schema.sql`
  - `backend/sql/002_backend_queries.sql`
- 删除：`backend/Services/FileBackedGameLibraryStore.cs`
- 调整：`backend/Program.cs`、`backend/GameLibrary.Api.csproj`、`README.md`、`.gitignore`
- 说明：账号、登录信息、库存全部入库，新增 `/api/accounts`。

14. 2026-02-28 14:15-14:18
- 变更：前端联动账号数据展示
- 调整：`frontend/src/types.ts`、`frontend/src/api.ts`、`frontend/src/App.vue`
- 说明：展示“已保存账号与凭证掩码”。

15. 2026-02-28 14:18
- 变更：清理误生成的 JS/tsbuildinfo 并加忽略规则
- 调整：`.gitignore`、`frontend/tsconfig.app.json`、`frontend/tsconfig.node.json`
- 说明：保留纯 TS/Vue 源文件，避免构建中间产物污染。

16. 2026-02-28 14:25
- 变更：后端接口从 Program 抽离为 Controller
- 新增：
  - `backend/Controllers/HealthController.cs`
  - `backend/Controllers/LibraryController.cs`
  - `backend/Controllers/SyncController.cs`
- 调整：`backend/Program.cs`
- 说明：`Program` 只负责注册与管道，API 路由改由 Controller 承担。

17. 2026-02-28 14:45-14:53
- 变更：后端从手写 SQL 切换为 EF Core ORM（当前版本）
- 新增：
  - `backend/Data/GameLibraryDbContext.cs`
  - `backend/Data/Entities/PlatformAccountEntity.cs`
  - `backend/Data/Entities/OwnedGameEntity.cs`
  - `backend/Services/EfCoreGameLibraryStore.cs`
- 删除：`backend/Services/MySqlGameLibraryStore.cs`
- 调整：`backend/GameLibrary.Api.csproj`、`backend/Program.cs`、`README.md`
- 说明：改用 `EF Core + Pomelo`，连接版本固定到 `MySQL 5.7.44`。

18. 2026-02-28 14:53-14:55
- 变更：SQL 与文档版本说明同步
- 调整：`backend/sql/001_init_schema.sql`、`backend/sql/002_backend_queries.sql`、`README.md`
- 说明：明确 MySQL 5.7.44 与 ORM 化现状。

19. 2026-02-28 14:56-15:01
- 变更：后端支持 Ubuntu 部署场景的跨平台自包含发布
- 新增：
  - `backend/scripts/publish-self-contained.sh`
  - `backend/scripts/publish-self-contained.ps1`
- 调整：`README.md`、`.gitignore`
- 说明：新增多运行时自包含单文件发布脚本（默认 `linux-x64/linux-arm64/win-x64`），可在无 .NET 运行时环境下部署。

20. 2026-02-28 15:01-15:03
- 变更：接入 NLog（控制台 + 文件）
- 新增：`backend/nlog.config`
- 调整：`backend/GameLibrary.Api.csproj`、`backend/Program.cs`、`backend/Controllers/SyncController.cs`、`README.md`
- 说明：启用 NLog Host 集成、异常落日志、同步接口关键流程日志，日志输出到控制台与 `backend/logs`。

21. 2026-02-28 15:49:14 +08:00
- 变更：将 `AGENTS.md` 全文重写为简体中文版本
- 调整：`AGENTS.md`
- 说明：统一文档语言为简体中文，保留并强化强制规则、构建命令、提交规范与安全要求。

## 2. 当前最终状态（截至本文件生成）

- 项目根目录：`multi-platform-game-library-manager`
- 后端 API 组织方式：Controller 模式
- 数据访问方式：EF Core ORM
- 数据库目标版本：MySQL 5.7.44
- 发布方式：支持跨平台自包含单文件发布（优先 Ubuntu `linux-x64`）
- 日志方案：NLog（控制台 + 文件滚动）
- 覆盖数据类型：
  - 平台账号信息
  - 登录凭证信息（返回时掩码）
  - 游戏库存数据

## 3. 关键最终文件索引

- 入口：`backend/Program.cs`
- 控制器：`backend/Controllers/*.cs`
- ORM 上下文：`backend/Data/GameLibraryDbContext.cs`
- ORM 实体：`backend/Data/Entities/*.cs`
- 存储服务：`backend/Services/EfCoreGameLibraryStore.cs`
- SQL 文件：`backend/sql/001_init_schema.sql`、`backend/sql/002_backend_queries.sql`
- 项目说明：`README.md`

## 4. 备注

- 本追溯文件记录“代码更新”本身，不包含每次命令失败重试的全部终端细节。
- 若需要，我可以继续生成第二份“命令执行审计日志（含失败原因与恢复动作）”。
