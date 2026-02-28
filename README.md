# Multi-Platform Game Library Manager（前后端分离）

用于统一管理多个游戏平台（当前已支持 Steam、Epic）的已拥有游戏，识别重复拥有，避免重复购买。  
项目命名与结构已按“可扩展更多平台”设计。

## 技术栈

- 前端：Vue 3 + TypeScript + Vite
- 后端：ASP.NET Core Web API (.NET 10)
- 数据库：MySQL 5.7.44（5.7 最新子版本）
- ORM：EF Core + Pomelo.EntityFrameworkCore.MySql
- 日志：NLog（控制台 + 文件）

## 平台接入策略

- Steam：使用官方公开接口 `IPlayerService/GetOwnedGames`。
- Epic：无公开“个人库存”开放 API，使用“用户登录后获取 Access Token，再调用 Epic 官方库服务接口”方式同步。
- 后续新增平台可按同样模式扩展（新增客户端 + 新同步接口 + DB 写入）。

## 目录结构

```text
multi-platform-game-library-manager/
  backend/
    sql/                  # MySQL SQL 文件
  frontend/
```

## 数据库初始化

1. 先创建并初始化数据库（`backend/sql/001_init_schema.sql`）。
2. `backend/sql/002_backend_queries.sql` 保存了关键 SQL（建模/排障参考）。

示例（MySQL CLI）：

```powershell
mysql -u root -p < backend/sql/001_init_schema.sql
```

## 后端配置与启动

1. 编辑 `backend/appsettings.json`：
   - `ConnectionStrings:GameLibraryMySql`
   - `Steam:ApiKey`（可选，也可每次请求携带）
2. 启动后端：

```powershell
dotnet run --project backend/GameLibrary.Api.csproj
```

默认地址见 `backend/Properties/launchSettings.json`（通常是 `http://localhost:5119`）。

## 后端日志（NLog）

- 配置文件：`backend/nlog.config`
- 输出目标：
  - 控制台
  - 文件：`backend/logs/<yyyy-MM-dd>.log`
- 日志按天滚动，最多保留 30 份归档。

## 后端跨平台打包（自包含）

目标：尽量不依赖服务器预装 `.NET Runtime`。  
采用 `self-contained + single-file` 发布，适合 Ubuntu 直接运行。

### Ubuntu（推荐）

在项目 `backend` 目录执行：

```bash
dotnet publish ./GameLibrary.Api.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:PublishTrimmed=false \
  -p:InvariantGlobalization=true \
  -o ./publish/linux-x64
```

生成后在 Ubuntu 上执行（示例）：

```bash
chmod +x ./GameLibrary.Api
./GameLibrary.Api
```

### 一键打多平台包

- Bash：`backend/scripts/publish-self-contained.sh`
- PowerShell：`backend/scripts/publish-self-contained.ps1`

默认运行时：`linux-x64`、`linux-arm64`、`win-x64`。

## 前端启动

```powershell
cd frontend
npm install
copy .env.example .env
npm run dev
```

`.env` 示例：`VITE_API_BASE_URL=http://localhost:5119`

## 主要接口

- `POST /api/sync/steam`
  - body: `{ "steamId": "...", "apiKey": "...", "accountName": "..." }`
- `POST /api/sync/epic`
  - body: `{ "accessToken": "...", "accountName": "..." }`
- `GET /api/library`
  - 返回所有库存 + 跨平台重复组
- `GET /api/accounts`
  - 返回已保存的平台账号与登录信息（凭证为掩码展示）

## Epic Token 获取（登录后）

1. 浏览器登录 Epic。
2. 打开开发者工具 `Network`。
3. 找到 `library-service.live.use1a.on.epicgames.com` 的请求。
4. 复制 `Authorization: Bearer <token>` 里的 token。

## 存储说明

- 已填写的账号信息、登录信息、库存数据全部保存到 MySQL。
- 后端通过 EF Core ORM 访问数据库，不在业务代码中执行手写 SQL。
- 账号凭证目前按明文写库（接口返回时做掩码）。生产环境建议改为应用层加密或 KMS 托管密钥。
