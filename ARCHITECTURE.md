# 项目架构文档

> 更新时间：2026-03-01 18:01:12 +08:00

## 1. 总览

- 项目名称：`multi-platform-game-library-manager`
- 架构形态：前后端分离
- 后端：`ASP.NET Core Web API (.NET 9)` + `EF Core (MySQL 5.7.44)`
- 前端：`Vue 3 + TypeScript + Vite + Ant Design Vue`
- 核心能力：账号鉴权、Steam/Epic 库存同步、跨平台重复游戏识别

## 2. 目录树与职责

```text
multi-platform-game-library-manager/
├─ .git/                               # Git 元数据（版本管理内部目录）
├─ .gitignore                          # Git 忽略规则
├─ AGENTS.md                           # 仓库协作与编码规范
├─ API_CONTRACTS.md                    # 后端 API 契约文档（本次整理新增）
├─ ARCHITECTURE.md                     # 项目架构文档（本次整理新增）
├─ README.md                           # 项目使用说明与部署指南
├─ backend/                            # ASP.NET Core Web API
│  ├─ .idea/                           # IDE 项目配置（非业务源码）
│  │  └─ .idea.backend.dir             # IDE 目录配置
│  ├─ bin/                             # 构建产物目录（非业务源码）
│  ├─ obj/                             # 编译中间产物目录（非业务源码）
│  ├─ Controllers/                     # API 控制器层（只负责入参校验与路由编排）
│  │  ├─ AuthController.cs             # 登录、初始化管理员、用户管理、当前用户信息
│  │  ├─ HealthController.cs           # 健康检查接口
│  │  ├─ LibraryController.cs          # 库存与账号查询接口
│  │  └─ SyncController.cs             # Steam/Epic 同步入口
│  ├─ Data/                            # EF Core 数据访问层
│  │  ├─ GameLibraryDbContext.cs       # DbContext 与实体映射、索引、约束配置
│  │  └─ Entities/
│  │     ├─ AppUserEntity.cs           # 应用用户实体（鉴权用户表）
│  │     ├─ OwnedGameEntity.cs         # 已拥有游戏实体
│  │     └─ PlatformAccountEntity.cs   # 平台账号实体
│  ├─ Models/                          # 业务模型与 DTO
│  │  ├─ AccountManagementModels.cs    # 已保存账号编辑请求模型
│  │  ├─ AuthModels.cs                 # 认证请求/响应模型
│  │  ├─ GamePlatform.cs               # 平台枚举（Steam/Epic）
│  │  ├─ LibraryGamePageModels.cs      # 库存分页查询请求与响应模型
│  │  ├─ LibraryResponse.cs            # 库存聚合响应模型与重复组模型
│  │  ├─ OwnedGame.cs                  # 已拥有游戏模型
│  │  ├─ SavedAccount.cs               # 已保存账号展示模型
│  │  └─ SyncRequests.cs               # 同步请求模型
│  ├─ Properties/
│  │  └─ launchSettings.json           # 本地启动配置
│  ├─ scripts/                         # 发布脚本
│  │  ├─ publish-self-contained.ps1    # PowerShell 自包含发布脚本
│  │  └─ publish-self-contained.sh     # Bash 自包含发布脚本
│  ├─ Services/                        # 业务服务层与外部平台客户端
│  │  ├─ Auth/                         # 认证与用户管理服务
│  │  │  ├─ AuthOptions.cs             # 鉴权配置对象
│  │  │  ├─ AuthService.cs             # 认证业务实现（登录、初始化管理员、创建用户）
│  │  │  ├─ IAuthService.cs            # 认证业务接口抽象
│  │  │  ├─ JwtTokenService.cs         # JWT 访问令牌生成服务
│  │  │  └─ PasswordHashService.cs     # 密码哈希与校验服务
│  │  ├─ Common/
│  │  │  ├─ ServiceOperationResult.cs  # 服务层标准结果模型
│  │  │  └─ Utc8DateTimeJsonConverter.cs # 全局 UTC+8 时间序列化/反序列化转换器
│  │  ├─ Library/                      # 库存查询与持久化服务
│  │  │  ├─ AccountManagementService.cs # 已保存账号重拉、修改、删除服务实现
│  │  │  ├─ DuplicateDetector.cs       # 跨平台重复游戏检测服务
│  │  │  ├─ EfCoreGameLibraryStore.cs  # 基于 EF Core 的库存存储实现
│  │  │  ├─ IAccountManagementService.cs # 已保存账号管理服务接口抽象
│  │  │  ├─ IGameLibraryStore.cs       # 库存存储接口抽象
│  │  │  ├─ ILibraryQueryService.cs    # 库存查询服务接口抽象
│  │  │  ├─ LibraryQueryService.cs     # 库存查询实现（聚合库存与重复检测）
│  │  │  └─ TitleNormalizer.cs         # 游戏名归一化工具
│  │  └─ Sync/                         # 平台同步服务
│  │     ├─ EpicLibraryClient.cs       # Epic 库存拉取客户端
│  │     ├─ ISyncService.cs            # 平台同步服务接口抽象
│  │     ├─ SteamOwnedGamesClient.cs   # Steam 库存拉取客户端
│  │     ├─ SyncResult.cs              # 平台拉取结果模型
│  │     └─ SyncService.cs             # 平台同步业务实现（Steam/Epic）
│  ├─ sql/
│  │  ├─ 001_init_schema.sql           # 数据库初始化脚本
│  │  └─ 002_backend_queries.sql       # 常用查询与排障 SQL
│  ├─ appsettings.Development.json     # 开发环境配置
│  ├─ appsettings.json                 # 基础运行配置
│  ├─ GameLibrary.Api.csproj           # 后端项目文件与依赖声明
│  ├─ GameLibrary.Api.http             # 接口调试样例
│  ├─ nlog.config                      # NLog 日志配置
│  └─ Program.cs                       # 后端应用入口与依赖注册
├─ frontend/                           # Vue 3 前端工程
│  ├─ node_modules/                    # 前端依赖目录（非业务源码）
│  ├─ src/                             # 前端源码
│  │  ├─ App.vue                       # 根组件，仅承载路由出口
│  │  ├─ components/                   # 页面内可复用业务组件
│  │  │  ├─ accounts/
│  │  │  │  ├─ AccountEditModal.vue   # 账号编辑弹窗组件
│  │  │  │  ├─ SavedAccountsTable.vue # 已保存账号表格组件
│  │  │  │  └─ SyncPanels.vue         # Steam/Epic 同步面板组件
│  │  │  ├─ auth/
│  │  │  │  ├─ BootstrapAdminCard.vue # 初始化管理员表单卡片
│  │  │  │  └─ LoginFormCard.vue      # 登录表单卡片
│  │  │  ├─ home/
│  │  │  │  ├─ DuplicateWarningPanel.vue # 重复购买预警组件
│  │  │  │  └─ SummaryMetrics.vue     # 首页统计卡片组件
│  │  │  └─ inventory/
│  │  │     ├─ InventoryFilterPanel.vue # 库存筛选栏组件
│  │  │     └─ InventoryTable.vue      # 库存表格（含展开行）组件
│  │  ├─ env.d.ts                      # Vite 环境变量类型声明
│  │  ├─ layouts/
│  │  │  └─ MainLayout.vue             # 登录后后台主布局（左侧菜单+右侧内容）
│  │  ├─ main.ts                       # 前端入口与挂载
│  │  ├─ pages/                        # 路由页面
│  │  │  ├─ AccountManagementPage.vue  # 平台账号管理页
│  │  │  ├─ HomePage.vue               # 主页（统计+重复预警）
│  │  │  ├─ InventoryPage.vue          # 详细库存信息页
│  │  │  └─ LoginPage.vue              # 登录页
│  │  ├─ router/
│  │  │  └─ index.ts                   # 路由定义与鉴权守卫
│  │  ├─ services/
│  │  │  └─ gameLibraryApi.ts          # API 访问封装与鉴权头管理
│  │  ├─ stores/
│  │  │  ├─ authStore.ts               # 认证状态管理（Pinia）
│  │  │  ├─ libraryStore.ts            # 库存状态管理（Pinia）
│  │  │  └─ pinia.ts                   # 全局 Pinia 实例
│  │  ├─ style.css                     # 全局样式
│  │  └─ types/
│  │     └─ gameLibrary.ts             # 前端业务类型定义
│  ├─ .env.example                     # 前端环境变量示例
│  ├─ index.html                       # Vite 页面模板
│  ├─ package-lock.json                # npm 锁文件
│  ├─ package.json                     # 前端依赖与脚本
│  ├─ tsconfig.app.json                # 前端应用 TS 配置
│  ├─ tsconfig.json                    # TS 根配置
│  ├─ tsconfig.node.json               # Node 侧 TS 配置
│  └─ vite.config.ts                   # Vite 构建与开发配置
└─ trace-logs/                         # 变更追溯日志目录
   ├─ CHANGELOG_2026-02-28.md          # 2026-02-28 当日变更记录
   ├─ CHANGELOG_2026-03-01.md          # 2026-03-01 当日变更记录
   └─ DEV_STATE.md                     # 当前开发进度、遗留问题与下一步计划
```

## 3. 分层关系说明

- `Controllers`：处理 HTTP 协议层、参数校验、调用服务层，不承载核心业务。
- `Services`：封装核心业务与外部平台交互，提供可复用能力。
- `Data`：负责 EF Core 实体映射与数据库访问。
- `Models`：用于 API 输入输出和领域数据传递。
- `frontend/src/router/index.ts`：集中定义前端路由与鉴权守卫，统一处理登录态跳转。
- `frontend/src/layouts/MainLayout.vue`：后台主框架，负责左侧菜单、顶部用户信息和子页面容器。
- `frontend/src/pages`：页面级编排层，负责组织页面组件并触发 Store 行为。
- `frontend/src/components`：业务组件层，拆分登录、同步、统计、表格等可复用 UI 单元。
- `frontend/src/services/gameLibraryApi.ts`：作为前端接口访问出口，集中处理 token、错误与基础请求逻辑。
- `frontend/src/stores`：基于 Pinia 管理认证态与库存态，避免页面组件承载复杂状态流程。

## 4. 维护规则

- 每次新增/删除/移动项目文件时，必须同步更新本文档。
- 前端联调后端接口时，应优先参考 `API_CONTRACTS.md`，避免直接遍历 Controller 源码。
