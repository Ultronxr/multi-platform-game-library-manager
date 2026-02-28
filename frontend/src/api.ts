import type { EpicSyncRequest, LibraryResponse, SavedAccount, SteamSyncRequest } from "./types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5119";

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {})
    }
  });

  if (!response.ok) {
    const errorPayload = (await response.json().catch(() => null)) as { message?: string } | null;
    const message = errorPayload?.message ?? `请求失败: ${response.status}`;
    throw new Error(message);
  }

  return (await response.json()) as T;
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
