import type {
  AuthLoginResponse,
  BootstrapAdminRequest,
  BootstrapStatusResponse,
  CurrentUserResponse,
  EpicSyncRequest,
  LibraryGamesPageResponse,
  LibraryGamesQuery,
  LibraryResponse,
  LoginRequest,
  SavedAccount,
  SteamSyncRequest,
  UpdateSavedAccountRequest
} from "../types/gameLibrary";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5119";
const ACCESS_TOKEN_STORAGE_KEY = "game_library_access_token";

/**
 * 统一 API 异常对象，包含 HTTP 状态码。
 */
export class ApiError extends Error {
  status: number;

  /**
   * @param status HTTP 状态码。
   * @param message 错误描述。
   */
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

/**
 * 读取本地缓存的访问令牌。
 */
export function getAccessToken(): string {
  return localStorage.getItem(ACCESS_TOKEN_STORAGE_KEY) ?? "";
}

/**
 * 持久化访问令牌。
 * @param token JWT 访问令牌。
 */
export function setAccessToken(token: string): void {
  localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, token);
}

/**
 * 清理本地访问令牌。
 */
export function clearAccessToken(): void {
  localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);
}

interface ApiRequestInit extends RequestInit {
  /**
   * 是否跳过自动附加 Authorization 头。
   */
  skipAuth?: boolean;
}

/**
 * 通用请求封装：
 * 1. 自动追加 JSON Content-Type；
 * 2. 默认追加 Bearer Token；
 * 3. 非 2xx 响应统一抛出 ApiError。
 * @param url 相对路径。
 * @param init 请求参数。
 */
async function request<T>(url: string, init?: ApiRequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  if (init?.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (!init?.skipAuth) {
    const accessToken = getAccessToken();
    if (accessToken) {
      // 所有业务请求自动附带 JWT，避免遗漏鉴权头。
      headers.set("Authorization", `Bearer ${accessToken}`);
    }
  }

  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...init,
    headers
  });

  if (!response.ok) {
    const errorPayload = (await response.json().catch(() => null)) as { message?: string } | null;
    const message = errorPayload?.message ?? `请求失败: ${response.status}`;
    throw new ApiError(response.status, message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get("Content-Type") ?? "";
  if (!contentType.includes("application/json")) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

/**
 * 获取管理员初始化开关状态。
 */
export async function fetchBootstrapStatus(): Promise<BootstrapStatusResponse> {
  return request<BootstrapStatusResponse>("/api/auth/bootstrap-status", { skipAuth: true });
}

/**
 * 初始化首个管理员账号。
 * @param payload 初始化参数。
 */
export async function bootstrapAdmin(payload: BootstrapAdminRequest): Promise<void> {
  await request<{ message: string }>("/api/auth/bootstrap-admin", {
    method: "POST",
    body: JSON.stringify(payload),
    skipAuth: true
  });
}

/**
 * 登录并自动缓存 accessToken。
 * @param payload 登录参数。
 */
export async function login(payload: LoginRequest): Promise<AuthLoginResponse> {
  const response = await request<AuthLoginResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify(payload),
    skipAuth: true
  });

  setAccessToken(response.accessToken);
  return response;
}

/**
 * 获取当前登录用户信息。
 */
export async function fetchCurrentUser(): Promise<CurrentUserResponse> {
  return request<CurrentUserResponse>("/api/auth/me");
}

/**
 * 获取游戏库存与重复组信息。
 */
export async function fetchLibrary(): Promise<LibraryResponse> {
  return request<LibraryResponse>("/api/library");
}

/**
 * 获取库存聚合信息。
 * @param includeGames 是否返回全部库存明细。
 */
export async function fetchLibrarySummary(includeGames: boolean): Promise<LibraryResponse> {
  return request<LibraryResponse>(`/api/library?includeGames=${includeGames ? "true" : "false"}`);
}

/**
 * 分页查询库存明细。
 * @param query 分页与筛选参数。
 */
export async function fetchLibraryGamesPage(query: LibraryGamesQuery): Promise<LibraryGamesPageResponse> {
  const params = new URLSearchParams();
  params.set("pageNumber", String(query.pageNumber));
  params.set("pageSize", String(query.pageSize));

  if (query.gameTitle) {
    params.set("gameTitle", query.gameTitle);
  }

  if (query.platform) {
    params.set("platform", query.platform);
  }

  if (query.accountName) {
    params.set("accountName", query.accountName);
  }

  if (query.accountExternalId) {
    params.set("accountExternalId", query.accountExternalId);
  }

  return request<LibraryGamesPageResponse>(`/api/library/games?${params.toString()}`);
}

/**
 * 获取已保存的平台账号列表。
 */
export async function fetchAccounts(): Promise<SavedAccount[]> {
  return request<SavedAccount[]>("/api/accounts");
}

/**
 * 使用已保存凭证重新拉取指定账号库存。
 * @param accountId 账号主键。
 */
export async function resyncSavedAccount(accountId: number): Promise<void> {
  await request<{ syncedCount: number }>(`/api/accounts/${accountId}/resync`, {
    method: "POST"
  });
}

/**
 * 更新已保存账号信息。
 * @param accountId 账号主键。
 * @param payload 更新参数。
 */
export async function updateSavedAccount(
  accountId: number,
  payload: UpdateSavedAccountRequest
): Promise<void> {
  await request<{ message: string }>(`/api/accounts/${accountId}`, {
    method: "PUT",
    body: JSON.stringify(payload)
  });
}

/**
 * 删除已保存账号及其关联库存。
 * @param accountId 账号主键。
 */
export async function deleteSavedAccount(accountId: number): Promise<void> {
  await request<{ message: string }>(`/api/accounts/${accountId}`, {
    method: "DELETE"
  });
}

/**
 * 触发 Steam 库存同步。
 * @param payload 同步参数。
 */
export async function syncSteam(payload: SteamSyncRequest): Promise<void> {
  await request<{ syncedCount: number }>("/api/sync/steam", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}

/**
 * 触发 Epic 库存同步。
 * @param payload 同步参数。
 */
export async function syncEpic(payload: EpicSyncRequest): Promise<void> {
  await request<{ syncedCount: number }>("/api/sync/epic", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}
