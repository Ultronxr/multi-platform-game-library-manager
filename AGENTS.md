# 仓库协作指南

## 语言要求
- 所有思考、分析、解释过程与最终输出，必须使用简体中文。
- 仅在用户明确要求时，才可切换到其他语言。

## 强制执行规则
- 每次对话只要产生代码修改，必须在根目录 `trace-logs/` 中追加变更记录，并附带可追溯时间戳（示例：`2026-02-28 15:10:00 +08:00`）。

## 项目结构与模块划分
- `backend/`：ASP.NET Core Web API（`net9.0`），接口在 `Controllers/`，EF Core 数据模型在 `Data/`，业务模型在 `Models/`，服务实现在 `Services/`。
- `backend/sql/`：数据库初始化与参考 SQL 脚本。
- `backend/scripts/`：跨平台自包含发布脚本（Linux/Windows）。
- `frontend/`：Vue3 + TypeScript 前端，源码位于 `frontend/src/`。
- `trace-logs/`：会话级变更追溯日志。

## 构建、运行与开发命令
- 后端运行：`dotnet run --project backend/GameLibrary.Api.csproj`
- 后端构建：`dotnet build backend/GameLibrary.Api.csproj`
- 前端开发：`cd frontend && npm install && npm run dev`
- 前端打包：`cd frontend && npm run build`
- 前端类型检查：`cd frontend && npm exec -- vue-tsc -b tsconfig.json`
- Ubuntu 自包含发布：  
  `dotnet publish backend/GameLibrary.Api.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o backend/publish/linux-x64`

## 代码风格与命名规范
- C#：4 空格缩进；类型/方法使用 `PascalCase`；局部变量/参数使用 `camelCase`；异步方法以 `Async` 结尾。
- Vue/TS：2 空格缩进；组件名使用 `PascalCase`；函数与变量使用 `camelCase`。
- 关键代码必须添加中文注释，说明核心意图、边界条件或关键实现逻辑；避免仅重复代码字面含义。
- 控制器保持轻量，业务与持久化逻辑放入 `Services/`。

## 测试要求
- 当前暂无独立自动化测试项目。
- 合并前最低要求：后端可编译、前端类型检查通过、关键接口可冒烟验证（`/api/health`、`/api/library`、`/api/sync/*`）。
- 新增测试建议：
  - 后端采用 xUnit，命名如 `Feature_Action_ExpectedResult`。
  - 前端采用 Vitest，优先覆盖核心工具函数与数据流逻辑。

## 提交与合并请求规范
- 提交信息遵循 `Conventional Commits`，例如：`feat(sync): 新增平台适配`、`fix(api): 修复参数校验`。
- 提交信息需要准确总结本次变更内容，并使用 `markdown` 格式编写。
- PR 需包含：变更范围、配置/数据库影响、验证步骤、涉及界面的截图（如有）。

## 安全与配置注意事项
- 严禁提交真实密钥与凭证（Steam Key、Epic Token、数据库密码等）。
- 敏感信息必须放在环境配置中；接口响应和日志中应进行脱敏处理。
- NLog 输出到控制台与 `backend/logs/`；部署前应检查日志轮转与保留策略是否符合要求。
