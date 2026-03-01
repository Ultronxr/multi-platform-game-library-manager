# 开发状态（DEV_STATE）

> 最后更新：2026-03-01 18:04:18 +08:00

## 当前开发进度

- 已完成：后端分层重构，将 `Auth/Sync/Library` 业务逻辑下沉到 Service 层，Controller 仅保留路由与返回。
- 已完成：前端引入 `Pinia`，新增 `authStore` 与 `libraryStore`，`App.vue` 改为页面编排层。
- 已完成：类型定义迁移到 `frontend/src/types/gameLibrary.ts`，删除旧 `frontend/src/types.ts`。
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
- 已完成：已保存账号支持一键重拉库存、编辑账号信息与删除账号（后端接口 + 前端操作按钮联动）。
- 已完成：前端改造为 Ant Design Vue 组件化界面，统一表单、表格、弹窗与反馈交互风格。
- 已完成：游戏库存支持多维筛选（游戏名、平台、账号名、账号ID），支持模糊匹配与平台下拉过滤。
- 已完成：库存列表切换为后端分页，前端改为按页加载与后端筛选参数联动。
- 已完成：游戏库存表格固定高度，分页大小切换不再触发页面整体高度跳变。
- 已完成：进一步加高库存表格固定高度，提升单屏可见记录数与浏览连续性。
- 已完成：厂商库存 API 请求增加 DEBUG 级完整响应日志输出（含状态码与响应体）。
- 已完成：按用户要求回滚上一轮日志级别改动，数据库 SQL 与请求流水日志恢复到回滚前配置（`Microsoft.*` 回到 `Info`，移除自定义请求 Debug 中间件与 EF Core Debug 降级）。
- 已完成：按用户要求仅使用 `nlog.config` 调整日志分层：`Microsoft.EntityFrameworkCore.*`、`Microsoft.AspNetCore.*`、`System.Net.Http.HttpClient.*` 下调到 `Debug` 观察层级，`Microsoft.*` 收敛为 `Warn+`，避免干扰业务 `Info` 日志。
- 已完成：按用户要求彻底关闭 `Microsoft.AspNetCore.*` 与 `Microsoft.EntityFrameworkCore.*` 日志（通过 NLog `Null` 目标黑洞规则，无代码侵入）。
- 已完成：修复 Epic 库存游戏名称解析优先级，优先使用 `sandboxName`（顶层 + `metadata` + `catalogItem`）作为游戏名称来源。
- 已完成：修复 Epic 库存分页拉取，按 `responseMetadata.nextCursor` + `stateToken` 循环抓取全部页，并加入游标防死循环与记录去重。
- 已完成：Epic 库存新增 `appName` 持久化并返回前端；库存统计和分页改为“同平台+同账号+同游戏名”聚合主记录，前端表格支持展开查看组内全部原始条目。
- 已完成：前端界面改造为经典后台模板，认证后采用左侧菜单（主页/平台账号管理/详细库存信息）与右侧内容区分栏展示。
- 已完成：前端重构为“独立登录页 + 主布局 + 子路由页面 + 页面内组件化”，不再将全部逻辑堆叠在 `App.vue`。
- 已完成：补齐 `vue-router` 依赖并通过前端构建校验（`npm exec -- vue-tsc -b tsconfig.json`、提权 `npm run build`）。
- 已完成：账号编辑提交支持清空 `externalAccountId`（空值提交为 `null`），避免历史 ID 无法移除。

## 遗留 Bug

- 暂未发现本轮重构引入的功能性回归。
- 暂未发现新增 SQL 语法错误（未执行数据库实际导入验证）。
- `002_backend_queries.sql` 为参考查询脚本，当前未在 CI 中做执行级校验。
- 当前未补充针对“目录迁移后的 import 路径”自动化回归测试，仅依赖编译与类型检查。
- Swagger 访问控制当前仅通过中间件顺序保证；如后续有更细粒度安全需求需补充白名单策略。
- 部分 API 字段名仍保留历史 `*Utc` 后缀（兼容前端类型），当前仅调整为 UTC+8 展示语义。
- 历史 UTC 数据迁移 SQL 已提供，但尚未在目标数据库环境执行验证。
- 账号编辑当前为内联单行编辑，暂未提供字段级权限与更细粒度审计日志。
- 当前环境执行 `vite build` 仍可能遇到 `spawn EPERM`，需在提权权限下执行。
- 前端筛选当前需点击“应用筛选”触发请求，尚未增加输入防抖自动查询。
- 若厂商接口返回体过大，Debug 日志体积增长较快，需关注日志轮转与磁盘占用。
- 当前环境 `dotnet build` 受 NuGet SSL 凭证问题影响（`NU1301`），本轮无法完成后端编译校验。
- 本次变更新增 `owned_games.epic_app_name` 字段，历史数据库需执行升级 SQL（见 `backend/sql/002_backend_queries.sql` 第 10 节）后再运行同步。

## 未完成 TODO

- 若后续继续深化重构，可将 `ServiceOperationResult` 与统一错误映射进一步抽到基础层，减少 Controller 重复返回模板。
- 可补充自动化测试（后端 xUnit + 前端 Vitest）覆盖登录、同步与 401 会话失效流程。
- 可在数据库环境执行一次 `001_init_schema.sql` 冒烟导入，确认 COMMENT 与字符集在目标 MySQL 版本下表现一致。
- 可增加 SQL lint 或迁移工具（如 Flyway/Liquibase）来保证后续 DDL 规范持续生效。
- 可增加 ESLint + 路径别名规则（例如 `@/services`、`@/types`）避免后续目录迁移时出现相对路径脆弱性。
- 可在维护窗口执行并验证历史数据迁移 SQL（仅执行一次），执行前建议全库备份。
- 可为账号重拉/删除操作增加二次确认弹窗组件与操作审计记录表。
- 可在具备构建权限的环境补跑 `vite build` 产物验证，并补充 UI 视觉回归截图。
- 可在前端增加筛选条件输入防抖与 URL 查询参数持久化，便于分享筛选结果。
- 可在网络/证书可用环境重跑 `dotnet build backend/GameLibrary.Api.csproj`，完成当前工作区构建状态确认。
- 可针对前端打包产物进行分包优化（当前 `index` chunk 超过 500KB 告警）。

## 下一步实现思路

- 如用户要求继续推进质量改造，建议按以下顺序执行：
  1. 为 Service 层新增单元测试，覆盖鉴权/同步错误分支；
  2. 为前端 Store 行为新增测试，验证登录恢复与 401 退登；
  3. 在不改 API 契约前提下统一错误码与错误文案常量；
  4. 继续按日维护 `CHANGELOG` 与 `DEV_STATE`。
