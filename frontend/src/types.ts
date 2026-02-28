export type Platform = string;

export interface OwnedGame {
  externalId: string;
  title: string;
  platform: Platform;
  accountName: string;
  syncedAtUtc: string;
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
