# 开发状态（DEV_STATE）

> 最后更新：2026-03-01 10:44:00 +08:00

## 当前开发进度

- 已完成：后端分层重构，将 `Auth/Sync/Library` 业务逻辑下沉到 Service 层，Controller 仅保留路由与返回。
- 已完成：前端引入 `Pinia`，新增 `authStore` 与 `libraryStore`，`App.vue` 改为页面编排层。
- 已完成：类型定义迁移到 `frontend/src/types/api.ts`，删除旧 `frontend/src/types.ts`。
- 已完成：构建校验通过（`dotnet build backend/GameLibrary.Api.csproj`、`npm exec -- vue-tsc -b tsconfig.json`）。
- 已完成：同步更新 `ARCHITECTURE.md` 与 `CHANGELOG_2026-03-01.md`。
- 已完成：`backend/sql/001_init_schema.sql` 的建表语句为每个字段补充 `COMMENT` 注释，满足 SQL 规范要求。
- 已完成：重整 `backend/sql/001_init_schema.sql` 与 `backend/sql/002_backend_queries.sql` 的注释风格与分段结构，统一为中文语义说明。
- 已完成：后端控制器全部 `public async` 方法统一为 `*Async` 命名，满足异步方法命名规范。
- 已完成：文件与目录结构清洗；后端 `Services` 分为 `Auth/Library/Sync/Common`，前端 API 与类型文件归位并统一命名。
- 已完成：后端 Swagger 路由排除登录鉴权，文档页可匿名访问。
- 已完成：全局时间响应统一为 `UTC+8` 字符串格式 `yyyy-MM-dd HH:mm:ss`，并移除前端 `(UTC)` 展示。
- 已完成：数据库持久化时间改为 `UTC+8`；写库逻辑、SQL 默认时间函数与字段注释已同步调整。
- 已完成：补充“历史 UTC 数据一次性迁移为 UTC+8”的事务 SQL 脚本（参考 `backend/sql/002_backend_queries.sql`）。

## 遗留 Bug

- 暂未发现本轮重构引入的功能性回归。
- 暂未发现新增 SQL 语法错误（未执行数据库实际导入验证）。
- `002_backend_queries.sql` 为参考查询脚本，当前未在 CI 中做执行级校验。
- 当前未补充针对“目录迁移后的 import 路径”自动化回归测试，仅依赖编译与类型检查。
- Swagger 访问控制当前仅通过中间件顺序保证；如后续有更细粒度安全需求需补充白名单策略。
- 部分 API 字段名仍保留历史 `*Utc` 后缀（兼容前端类型），当前仅调整为 UTC+8 展示语义。
- 历史 UTC 数据迁移 SQL 已提供，但尚未在目标数据库环境执行验证。

## 未完成 TODO

- 若后续继续深化重构，可将 `ServiceOperationResult` 与统一错误映射进一步抽到基础层，减少 Controller 重复返回模板。
- 可补充自动化测试（后端 xUnit + 前端 Vitest）覆盖登录、同步与 401 会话失效流程。
- 可在数据库环境执行一次 `001_init_schema.sql` 冒烟导入，确认 COMMENT 与字符集在目标 MySQL 版本下表现一致。
- 可增加 SQL lint 或迁移工具（如 Flyway/Liquibase）来保证后续 DDL 规范持续生效。
- 可增加 ESLint + 路径别名规则（例如 `@/services`、`@/types`）避免后续目录迁移时出现相对路径脆弱性。
- 可在维护窗口执行并验证历史数据迁移 SQL（仅执行一次），执行前建议全库备份。

## 下一步实现思路

- 如用户要求继续推进质量改造，建议按以下顺序执行：
  1. 为 Service 层新增单元测试，覆盖鉴权/同步错误分支；
  2. 为前端 Store 行为新增测试，验证登录恢复与 401 退登；
  3. 在不改 API 契约前提下统一错误码与错误文案常量；
  4. 继续按日维护 `CHANGELOG` 与 `DEV_STATE`。
