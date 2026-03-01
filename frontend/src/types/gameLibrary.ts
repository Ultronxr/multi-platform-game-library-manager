export type Platform = string;

export interface OwnedGame {
  externalId: string;
  title: string;
  platform: Platform;
  accountName: string;
  syncedAtUtc: string;
  epicAppName?: string | null;
}

export interface LibraryGameGroupItem {
  externalId: string;
  epicAppName?: string | null;
  syncedAtUtc: string;
}

export interface LibraryGameListItem {
  groupKey: string;
  title: string;
  platform: Platform;
  accountName: string;
  accountExternalId?: string | null;
  syncedAtUtc: string;
  groupItemCount: number;
  epicAppName?: string | null;
  groupItems: LibraryGameGroupItem[];
}

export interface DuplicateGroup {
  normalizedTitle: string;
  games: OwnedGame[];
}

export interface LibraryResponse {
  totalGames: number;
  duplicateGroups: number;
  games: OwnedGame[];
  duplicates: DuplicateGroup[];
}

export interface LibraryGamesQuery {
  gameTitle?: string;
  platform?: string;
  accountName?: string;
  accountExternalId?: string;
  pageNumber: number;
  pageSize: number;
}

export interface InventoryFilters {
  gameTitle: string;
  platform?: string;
  accountName: string;
  accountExternalId: string;
}

export interface LibraryGamesPageResponse {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  items: LibraryGameListItem[];
}

export interface SavedAccount {
  id: number;
  platform: Platform;
  accountName: string;
  externalAccountId?: string | null;
  credentialType: string;
  credentialPreview: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  lastSyncedAtUtc?: string | null;
}

export interface SteamSyncRequest {
  steamId: string;
  apiKey?: string;
  accountName?: string;
}

export interface EpicSyncRequest {
  accessToken: string;
  accountName?: string;
}

export interface UpdateSavedAccountRequest {
  accountName?: string;
  externalAccountId?: string | null;
  credentialValue?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface BootstrapAdminRequest {
  setupToken: string;
  username: string;
  password: string;
}

export interface CreateUserRequest {
  username: string;
  password: string;
  role?: "admin" | "user";
}

export interface AuthLoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  username: string;
  role: string;
}

export interface CurrentUserResponse {
  username: string;
  role: string;
}

export interface BootstrapStatusResponse {
  hasAnyUser: boolean;
  bootstrapEnabled: boolean;
}
