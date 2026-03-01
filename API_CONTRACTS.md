# API 契约文档

> 更新时间：2026-03-01 11:18:44 +08:00
>
> 服务端项目：`backend/GameLibrary.Api.csproj`

## 1. 全局约定

- Base URL：`http://localhost:5119`（默认本地开发地址）
- 前缀：所有业务接口均在 `/api` 下
- 请求/响应格式：`application/json`
- 时间字段：统一返回 `UTC+8` 时间字符串，格式 `yyyy-MM-dd HH:mm:ss`（不返回 ISO 8601 `Z` 时间戳）
- 说明：部分字段名历史上保留 `Utc` 后缀，但字段值以 `UTC+8` 格式化字符串返回
- 持久化约定：数据库 `DATETIME` 字段统一按 `UTC+8` 写入与读取（`backend/sql/*.sql` 已显式设置 `time_zone = '+08:00'`）
- 鉴权方式：`Authorization: Bearer <accessToken>`
- 默认授权策略：除 `[AllowAnonymous]` 接口外，均要求登录；`/swagger` 文档路由不要求登录

## 2. 通用错误响应

多数业务错误返回结构如下：

```json
{
  "message": "错误描述"
}
```

常见状态码：

- `400 BadRequest`：参数校验不通过/外部平台同步失败
- `401 Unauthorized`：未登录或凭据无效
- `403 Forbidden`：角色权限不足（如非 admin 调用管理员接口）
- `404 NotFound`：资源不存在（如账号 ID 无效）
- `409 Conflict`：资源冲突（如用户名已存在）

## 3. 枚举与模型约定

### 3.1 平台枚举

- `platform`: `Steam` | `Epic`
- 后端启用了 `JsonStringEnumConverter`，枚举以字符串形式返回

### 3.2 核心响应模型

`AuthLoginResponse`

```json
{
  "accessToken": "string",
  "expiresAtUtc": "2026-03-01 08:00:00",
  "username": "string",
  "role": "admin"
}
```

`LibraryResponse`

```json
{
  "totalGames": 0,
  "duplicateGroups": 0,
  "games": [
    {
      "externalId": "string",
      "title": "string",
      "platform": "Steam",
      "accountName": "string",
      "syncedAtUtc": "2026-03-01 08:00:00"
    }
  ],
  "duplicates": [
    {
      "normalizedTitle": "string",
      "games": []
    }
  ]
}
```

## 4. 接口明细

## 4.1 健康检查

### GET `/api/health`

- 鉴权：否（匿名）
- 说明：服务健康状态探针

`200 OK`

```json
{
  "status": "ok",
  "time": "2026-03-01 08:00:00"
}
```

## 4.2 认证模块（`/api/auth`）

### GET `/api/auth/bootstrap-status`

- 鉴权：否（匿名）
- 说明：查询是否允许初始化管理员

`200 OK`

```json
{
  "hasAnyUser": false,
  "bootstrapEnabled": true
}
```

### POST `/api/auth/bootstrap-admin`

- 鉴权：否（匿名）
- 说明：仅在系统尚无用户且配置了 `Auth:BootstrapToken` 时可用

请求体：

```json
{
  "setupToken": "string",
  "username": "string",
  "password": "string"
}
```

校验规则：

- `setupToken` 必须与服务端配置一致
- `username` 长度 `3-64`
- `password` 最少 `8` 位

`200 OK`

```json
{
  "message": "Bootstrap admin created successfully."
}
```

错误码：

- `400`：未配置 BootstrapToken / 参数不合法 / 系统已有用户
- `401`：`setupToken` 不匹配

### POST `/api/auth/login`

- 鉴权：否（匿名）
- 说明：用户名密码登录并签发 JWT

请求体：

```json
{
  "username": "string",
  "password": "string"
}
```

`200 OK`：返回 `AuthLoginResponse`

错误码：

- `400`：用户名或密码为空
- `401`：用户名不存在、用户禁用、或密码错误

### POST `/api/auth/users`

- 鉴权：是（仅 `admin`）
- 说明：创建新用户

请求体：

```json
{
  "username": "string",
  "password": "string",
  "role": "admin"
}
```

字段说明：

- `role` 可选，默认 `user`
- `role` 仅允许：`admin` / `user`

`200 OK`

```json
{
  "message": "User created."
}
```

错误码：

- `400`：参数不合法或角色非法
- `401`：未登录
- `403`：非 admin
- `409`：用户名已存在

### GET `/api/auth/me`

- 鉴权：是（任意登录用户）
- 说明：返回当前 token 对应用户信息

`200 OK`

```json
{
  "username": "string",
  "role": "admin"
}
```

## 4.3 同步模块（`/api/sync`）

### POST `/api/sync/steam`

- 鉴权：是
- 说明：同步 Steam 已拥有游戏并落库

请求体：

```json
{
  "steamId": "7656119...",
  "apiKey": "string",
  "accountName": "Main Steam"
}
```

字段规则：

- `steamId` 必填
- `apiKey` 可选；未传时回退到服务端 `Steam:ApiKey`
- `accountName` 可选；默认值为 `steamId`

`200 OK`

```json
{
  "syncedCount": 123
}
```

错误码：

- `400`：缺少 API Key、缺少 steamId、Steam 接口调用失败
- `401`：未登录或 token 失效

### POST `/api/sync/epic`

- 鉴权：是
- 说明：使用 Epic Access Token 拉取库存并落库

请求体：

```json
{
  "accessToken": "string",
  "accountName": "Main Epic"
}
```

字段规则：

- `accessToken` 必填
- `accountName` 可选；默认值为 `EpicAccount`

`200 OK`

```json
{
  "syncedCount": 42
}
```

错误码：

- `400`：缺少 accessToken 或 Epic 接口调用失败
- `401`：未登录或 token 失效

## 4.4 库存与账号查询（`/api`）

### GET `/api/library`

- 鉴权：是
- 说明：返回全部库存及跨平台重复组
- 排序：`games` 按 `title` 升序（不区分大小写）

`200 OK`：返回 `LibraryResponse`

错误码：

- `401`：未登录或 token 失效

### GET `/api/accounts`

- 鉴权：是
- 说明：返回已保存的平台账号信息
- 安全：`credentialPreview` 为掩码字符串，不返回明文凭证

`200 OK`

```json
[
  {
    "id": 1,
    "platform": "Steam",
    "accountName": "Main Steam",
    "externalAccountId": "7656119...",
    "credentialType": "steam_api_key",
    "credentialPreview": "ABCD****WXYZ",
    "createdAtUtc": "2026-03-01 08:00:00",
    "updatedAtUtc": "2026-03-01 08:00:00",
    "lastSyncedAtUtc": "2026-03-01 08:00:00"
  }
]
```

错误码：

- `401`：未登录或 token 失效

### POST `/api/accounts/{accountId}/resync`

- 鉴权：是
- 说明：使用数据库已保存的账号凭证，一键重新拉取指定账号库存

`200 OK`

```json
{
  "syncedCount": 123
}
```

错误码：

- `400`：账号缺少必需凭证（如 Steam 缺少 SteamId 或 API Key）
- `401`：未登录或 token 失效
- `404`：账号不存在

### PUT `/api/accounts/{accountId}`

- 鉴权：是
- 说明：修改指定账号信息（账号名、平台账号 ID、凭证）

请求体：

```json
{
  "accountName": "Main Steam",
  "externalAccountId": "7656119...",
  "credentialValue": "new-credential"
}
```

字段规则：

- `accountName` 可选；传空白时保持原值
- `externalAccountId` 可选；传 `""` 时会清空（Steam 账号不允许最终为空）
- `credentialValue` 可选；不传表示不修改，传空白会报错

`200 OK`

```json
{
  "message": "Account updated."
}
```

错误码：

- `400`：字段不合法（如凭证空白、Steam 缺少 SteamId）
- `401`：未登录或 token 失效
- `404`：账号不存在
- `409`：同平台下账号名冲突

### DELETE `/api/accounts/{accountId}`

- 鉴权：是
- 说明：删除指定账号及其关联库存记录（级联删除）

`200 OK`

```json
{
  "message": "Account deleted."
}
```

错误码：

- `401`：未登录或 token 失效
- `404`：账号不存在

## 5. 前端对接映射

`frontend/src/services/gameLibraryApi.ts` 与后端路径映射如下：

- `fetchBootstrapStatus` -> `GET /api/auth/bootstrap-status`
- `bootstrapAdmin` -> `POST /api/auth/bootstrap-admin`
- `login` -> `POST /api/auth/login`
- `fetchCurrentUser` -> `GET /api/auth/me`
- `fetchLibrary` -> `GET /api/library`
- `fetchAccounts` -> `GET /api/accounts`
- `resyncSavedAccount` -> `POST /api/accounts/{accountId}/resync`
- `updateSavedAccount` -> `PUT /api/accounts/{accountId}`
- `deleteSavedAccount` -> `DELETE /api/accounts/{accountId}`
- `syncSteam` -> `POST /api/sync/steam`
- `syncEpic` -> `POST /api/sync/epic`

## 6. 维护要求

- 每次后端新增/修改接口（路径、入参、响应、鉴权、错误码）后，必须同步更新本文档。
- 前端联调和测试应优先以本文档为准，避免通过阅读 Controller 代码猜测契约。
