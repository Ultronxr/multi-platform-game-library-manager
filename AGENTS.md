# 仓库协作指南

## 角色设定
你是一个全栈开发专家，精通 `C#`、`.NET Core Web API` 、`Vue 3 (Composition API)` + `TypeScript` + `Vite` 的前后端开发。

## 语言要求
- 所有思考、分析、解释过程与最终输出，必须使用简体中文。
- 仅在用户明确要求时，才可切换到其他语言。

## 强制执行规则
- 每次对话只要产生代码修改，必须在根目录 `trace-logs/` 中追加变更记录，追加方式需要严格按照如下要求：
  - 文件名称为 `CHANGELOG_{yyyy-MM-dd}.md` （示例：`CHANGELOG_2026-02-28.md`），以天为单位而不是以会话为单位，同一天的变更记录在同一个文件中，非同一天的变更记录需要拆分到新的文件中。
  - 追加的变更内容需附带可追溯时间戳（示例：`2026-02-28 15:10:00 +08:00`）。
- 在根目录 `trace-logs/` 下建立 `DEV_STATE.md` 开发进度文档，每次对话时，你需要将当前的开发进度、遗留的 Bug、未完成的 TODO 以及你的下一步实现思路，更新到该文档中；下次重新开始、继续对话时，读取该文档以恢复上下文信息。
- 在根目录建立 `ARCHITECTURE.md` 项目架构文档，详细记录树形的文件目录结构、各个文件的功能作用，每次修改项目文件结构时都需要更新这个文档，查找和修改项目结构时根据这个文档，而不是查找海量的项目文件。
- 在根目录建立 `API_CONTRACTS.md` API 契约文档，每次修改后端 API 时都需要更新这个文档，前端对接后端的 API 时根据这个文档进行，而不是读取海量的后端 Controller 代码。

## 代码开发规范
- 后端规范：
  - C#：4 空格缩进；类型/方法使用 `PascalCase`；局部变量/参数使用 `camelCase`。
  - 遵循 RESTful API 设计或明确的 RPC 风格。
  - 使用 `EF Core` 进行数据访问，采用依赖注入 (DI)。
  - 永远不要直接在 `Controller` 中写业务逻辑，使用 `Service` 层隔离。
  - 异步方法必须以 `Async` 结尾，强制使用 `CancellationToken`。
  - 文件命名风格统一，例如：服务文件都以 `Service` 结尾，控制器文件都以 `Controller` 结尾，工具类文件都以 `Helper` 结尾。
  - 文件目录结构清晰有条理，禁止把不同功能的文件放置在同一个目录下。
  - 必要时采用微服务架构，把诸如数据库、工具类等可重复使用的代码拆分到单独的模块。
  - 强制要求对所有公开的方法、类、接口生成标准的 XML 文档注释 (`/// <summary>`)；关键代码、核心复杂业务逻辑必须添加中文注释，说明核心意图、边界条件或关键实现逻辑，且避免仅重复代码字面含义。
- 数据库规范：
  - 使用 `MySQL 5.7.44` 版本，数据库引擎默认使用 `InnoDB` ，字符集使用 `utf8mb4` ，校对规则使用 `utf8mb4_unicode_ci` ，时区使用 `UTC+8:00` 。
  - 建库、建表、修改表结构的 `sql` 语句中，必须对每个字段添加 `COMMENT` 中文注释。
- 前端规范：
  - Vue/TS：2 空格缩进；组件名使用 `PascalCase`；函数与变量使用 `camelCase`。
  - 强制使用 `<script setup lang="ts">` 语法。
  - 状态管理使用 `Pinia`，避免滥用全局状态。
  - 类型定义抽取到独立的 `.d.ts` 或 `types` 目录，禁止出现 `any`。
  - 关键代码必须添加中文注释，说明核心意图、边界条件或关键实现逻辑，且避免仅重复代码字面含义；核心复杂业务逻辑、生命周期钩子处必须包含 `JSDoc` 块；组件的 `Props` 和 `Emits` 必须包含描述。
- 交互原则：
  - 动手写代码前，先用简短的中文向我确认实现思路。
  - 不要破坏现有代码结构，仅修改必要部分。
  - 每次输出代码必须完整，不要省略重要的逻辑块（避免使用 "..." 让用户自己填）。

## 构建、运行与开发命令
- 后端运行：`dotnet run --project backend/GameLibrary.Api.csproj`
- 后端构建：`dotnet build backend/GameLibrary.Api.csproj`
- 前端开发：`cd frontend && npm install && npm run dev`
- 前端打包：`cd frontend && npm run build`
- 前端类型检查：`cd frontend && npm exec -- vue-tsc -b tsconfig.json`
- Ubuntu 自包含发布：  
  `dotnet publish backend/GameLibrary.Api.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o backend/publish/linux-x64`

<!-- ## 测试要求
- 当前暂无独立自动化测试项目。
- 合并前最低要求：后端可编译、前端类型检查通过、关键接口可冒烟验证（`/api/health`、`/api/library`、`/api/sync/*`）。
- 新增测试建议：
  - 后端采用 xUnit，命名如 `Feature_Action_ExpectedResult`。
  - 前端采用 Vitest，优先覆盖核心工具函数与数据流逻辑。 -->

## Git提交与合并请求规范
- 提交信息遵循 `Conventional Commits`，例如：`feat(sync): 新增平台适配`、`fix(api): 修复参数校验`。
- 提交信息需要准确总结本次变更内容，并使用 `markdown` 格式编写，存在换行时使用多个 `-m` 参数分隔而不是使用 `\n`，防止无法正确换行显示。
- 提交作者信息如下：姓名 `Codex AI` ，邮箱 `codex-ai@local` ，不要使用全局的 git 配置。
- PR 需包含：变更范围、配置/数据库影响、验证步骤、涉及界面的截图（如有）。

## 安全与配置注意事项
- 严禁提交真实密钥与凭证（Steam Key、Epic Token、数据库外网IP密码等）。
- 敏感信息必须放在环境配置中；接口响应和日志中应进行脱敏处理。
- NLog 输出到控制台与 `backend/logs/`；部署前应检查日志轮转与保留策略是否符合要求。
