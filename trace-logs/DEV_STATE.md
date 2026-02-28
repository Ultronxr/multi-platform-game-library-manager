# 开发状态（DEV_STATE）

> 最后更新：2026-03-01 02:18:35 +08:00

## 当前开发进度

- 已完成：后端分层重构，将 `Auth/Sync/Library` 业务逻辑下沉到 Service 层，Controller 仅保留路由与返回。
- 已完成：前端引入 `Pinia`，新增 `authStore` 与 `libraryStore`，`App.vue` 改为页面编排层。
- 已完成：类型定义迁移到 `frontend/src/types/api.ts`，删除旧 `frontend/src/types.ts`。
- 已完成：构建校验通过（`dotnet build backend/GameLibrary.Api.csproj`、`npm exec -- vue-tsc -b tsconfig.json`）。
- 已完成：同步更新 `ARCHITECTURE.md` 与 `CHANGELOG_2026-03-01.md`。
- 已完成：`backend/sql/001_init_schema.sql` 的建表语句为每个字段补充 `COMMENT` 注释，满足 SQL 规范要求。
- 已完成：重整 `backend/sql/001_init_schema.sql` 与 `backend/sql/002_backend_queries.sql` 的注释风格与分段结构，统一为中文语义说明。

## 遗留 Bug

- 暂未发现本轮重构引入的功能性回归。
- 暂未发现新增 SQL 语法错误（未执行数据库实际导入验证）。
- `002_backend_queries.sql` 为参考查询脚本，当前未在 CI 中做执行级校验。

## 未完成 TODO

- 若后续继续深化重构，可将 `ServiceOperationResult` 与统一错误映射进一步抽到基础层，减少 Controller 重复返回模板。
- 可补充自动化测试（后端 xUnit + 前端 Vitest）覆盖登录、同步与 401 会话失效流程。
- 可在数据库环境执行一次 `001_init_schema.sql` 冒烟导入，确认 COMMENT 与字符集在目标 MySQL 版本下表现一致。
- 可增加 SQL lint 或迁移工具（如 Flyway/Liquibase）来保证后续 DDL 规范持续生效。

## 下一步实现思路

- 如用户要求继续推进质量改造，建议按以下顺序执行：
  1. 为 Service 层新增单元测试，覆盖鉴权/同步错误分支；
  2. 为前端 Store 行为新增测试，验证登录恢复与 401 退登；
  3. 在不改 API 契约前提下统一错误码与错误文案常量；
  4. 继续按日维护 `CHANGELOG` 与 `DEV_STATE`。
