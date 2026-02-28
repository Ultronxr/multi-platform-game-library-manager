import type {
  AuthLoginResponse,
  BootstrapAdminRequest,
  BootstrapStatusResponse,
  CurrentUserResponse,
  EpicSyncRequest,
  LibraryResponse,
  LoginRequest,
  SavedAccount,
  SteamSyncRequest
} from "./types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5119";
const ACCESS_TOKEN_STORAGE_KEY = "game_library_access_token";

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

export function getAccessToken(): string {
  return localStorage.getItem(ACCESS_TOKEN_STORAGE_KEY) ?? "";
}

export function setAccessToken(token: string): void {
  localStorage.setItem(ACCESS_TOKEN_STORAGE_KEY, token);
}

export function clearAccessToken(): void {
  localStorage.removeItem(ACCESS_TOKEN_STORAGE_KEY);
}

interface ApiRequestInit extends RequestInit {
  skipAuth?: boolean;
}

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

export async function fetchBootstrapStatus(): Promise<BootstrapStatusResponse> {
  return request<BootstrapStatusResponse>("/api/auth/bootstrap-status", { skipAuth: true });
}

export async function bootstrapAdmin(payload: BootstrapAdminRequest): Promise<void> {
  await request<{ message: string }>("/api/auth/bootstrap-admin", {
    method: "POST",
    body: JSON.stringify(payload),
    skipAuth: true
  });
}

export async function login(payload: LoginRequest): Promise<AuthLoginResponse> {
  const response = await request<AuthLoginResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify(payload),
    skipAuth: true
  });

  setAccessToken(response.accessToken);
  return response;
}

export async function fetchCurrentUser(): Promise<CurrentUserResponse> {
  return request<CurrentUserResponse>("/api/auth/me");
}

export async function fetchLibrary(): Promise<LibraryResponse> {
  return request<LibraryResponse>("/api/library");
}

export async function fetchAccounts(): Promise<SavedAccount[]> {
  return request<SavedAccount[]>("/api/accounts");
}

export async function syncSteam(payload: SteamSyncRequest): Promise<void> {
  await request<{ syncedCount: number }>("/api/sync/steam", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}

export async function syncEpic(payload: EpicSyncRequest): Promise<void> {
  await request<{ syncedCount: number }>("/api/sync/epic", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}
