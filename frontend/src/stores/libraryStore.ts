import { computed, ref } from "vue";
import { defineStore } from "pinia";
import {
  ApiError,
  deleteSavedAccount as deleteSavedAccountApi,
  fetchAccounts,
  fetchLibraryGamesPage,
  fetchLibrarySummary,
  resyncSavedAccount as resyncSavedAccountApi,
  syncEpic as syncEpicApi,
  syncSteam as syncSteamApi,
  updateSavedAccount as updateSavedAccountApi
} from "../services/gameLibraryApi";
import type {
  EpicSyncRequest,
  LibraryGameListItem,
  LibraryGamesQuery,
  LibraryResponse,
  SavedAccount,
  SteamSyncRequest,
  UpdateSavedAccountRequest
} from "../types/gameLibrary";
import { useAuthStore } from "./authStore";

/**
 * 库存状态管理：
 * - 管理库存与账号列表；
 * - 封装 Steam/Epic 同步与数据刷新流程。
 */
export const useLibraryStore = defineStore("library", () => {
  const library = ref<LibraryResponse | null>(null);
  const accounts = ref<SavedAccount[]>([]);
  const loading = ref(false);
  const pagedGames = ref<LibraryGameListItem[]>([]);
  const gamesLoading = ref(false);
  const gamesTotalCount = ref(0);
  const gamesQuery = ref<LibraryGamesQuery>({
    pageNumber: 1,
    pageSize: 20
  });
  const syncingSteam = ref(false);
  const syncingEpic = ref(false);
  const accountActionLoadingIds = ref<number[]>([]);

  const hasDuplicates = computed(() => (library.value?.duplicateGroups ?? 0) > 0);

  /**
   * 清空库存与账号数据。
   */
  function resetLibraryData(): void {
    library.value = null;
    accounts.value = [];
    pagedGames.value = [];
    gamesTotalCount.value = 0;
    gamesQuery.value = {
      pageNumber: 1,
      pageSize: 20
    };
  }

  /**
   * 统一处理库存模块错误。
   * @param error 异常对象。
   */
  function handleApiError(error: unknown): void {
    const authStore = useAuthStore();
    if (error instanceof ApiError && error.status === 401) {
      authStore.logout("登录已失效，请重新登录。");
      resetLibraryData();
      return;
    }

    authStore.setError(error instanceof Error ? error.message : "请求失败，请稍后重试。");
  }

  /**
   * 标记账号操作加载状态，避免重复提交。
   * @param accountId 账号主键。
   * @param loadingState 是否处于加载中。
   */
  function setAccountActionLoading(accountId: number, loadingState: boolean): void {
    if (loadingState) {
      if (!accountActionLoadingIds.value.includes(accountId)) {
        accountActionLoadingIds.value = [...accountActionLoadingIds.value, accountId];
      }
      return;
    }

    accountActionLoadingIds.value = accountActionLoadingIds.value.filter((id) => id !== accountId);
  }

  /**
   * 判断账号行是否正在执行操作。
   * @param accountId 账号主键。
   */
  function isAccountActionLoading(accountId: number): boolean {
    return accountActionLoadingIds.value.includes(accountId);
  }

  /**
   * 加载库存列表。
   */
  async function loadLibrary(): Promise<void> {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
      return;
    }

    loading.value = true;
    try {
      library.value = await fetchLibrarySummary(false);
    } catch (error) {
      handleApiError(error);
    } finally {
      loading.value = false;
    }
  }

  /**
   * 分页加载库存明细。
   * @param query 分页与筛选参数（可选，未传时使用上次查询条件）。
   */
  async function loadLibraryGamesPage(query?: Partial<LibraryGamesQuery>): Promise<void> {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
      return;
    }

    gamesQuery.value = {
      ...gamesQuery.value,
      ...query
    };

    gamesLoading.value = true;
    try {
      const page = await fetchLibraryGamesPage(gamesQuery.value);
      pagedGames.value = page.items;
      gamesTotalCount.value = page.totalCount;
      gamesQuery.value = {
        ...gamesQuery.value,
        pageNumber: page.pageNumber,
        pageSize: page.pageSize
      };
    } catch (error) {
      handleApiError(error);
    } finally {
      gamesLoading.value = false;
    }
  }

  /**
   * 加载已保存账号列表。
   */
  async function loadAccounts(): Promise<void> {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
      return;
    }

    try {
      accounts.value = await fetchAccounts();
    } catch (error) {
      handleApiError(error);
    }
  }

  /**
   * 并行刷新库存与账号数据。
   */
  async function loadProtectedData(): Promise<void> {
    await Promise.all([loadLibrary(), loadAccounts(), loadLibraryGamesPage()]);
  }

  /**
   * 同步 Steam 库存并刷新页面数据。
   * @param payload 同步请求参数。
   */
  async function syncSteam(payload: SteamSyncRequest): Promise<void> {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
      authStore.setError("请先登录。");
      return;
    }

    if (!payload.steamId.trim()) {
      authStore.setError("请填写 SteamID。");
      return;
    }

    syncingSteam.value = true;
    authStore.clearError();
    try {
      await syncSteamApi({
        steamId: payload.steamId.trim(),
        apiKey: payload.apiKey?.trim() || undefined,
        accountName: payload.accountName?.trim() || undefined
      });
      await loadProtectedData();
    } catch (error) {
      handleApiError(error);
    } finally {
      syncingSteam.value = false;
    }
  }

  /**
   * 同步 Epic 库存并刷新页面数据。
   * @param payload 同步请求参数。
   */
  async function syncEpic(payload: EpicSyncRequest): Promise<void> {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
      authStore.setError("请先登录。");
      return;
    }

    if (!payload.accessToken.trim()) {
      authStore.setError("请填写 Epic Access Token。");
      return;
    }

    syncingEpic.value = true;
    authStore.clearError();
    try {
      await syncEpicApi({
        accessToken: payload.accessToken.trim(),
        accountName: payload.accountName?.trim() || undefined
      });
      await loadProtectedData();
    } catch (error) {
      handleApiError(error);
    } finally {
      syncingEpic.value = false;
    }
  }

  /**
   * 使用已保存凭证重新拉取指定账号库存。
   * @param accountId 账号主键。
   */
  async function resyncSavedAccount(accountId: number): Promise<boolean> {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
      authStore.setError("请先登录。");
      return false;
    }

    setAccountActionLoading(accountId, true);
    authStore.clearError();
    try {
      await resyncSavedAccountApi(accountId);
      await loadProtectedData();
      return true;
    } catch (error) {
      handleApiError(error);
      return false;
    } finally {
      setAccountActionLoading(accountId, false);
    }
  }

  /**
   * 更新已保存账号基础信息或凭证。
   * @param accountId 账号主键。
   * @param payload 更新参数。
   */
  async function updateSavedAccount(accountId: number, payload: UpdateSavedAccountRequest): Promise<boolean> {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
      authStore.setError("请先登录。");
      return false;
    }

    setAccountActionLoading(accountId, true);
    authStore.clearError();
    try {
      await updateSavedAccountApi(accountId, payload);
      await loadProtectedData();
      return true;
    } catch (error) {
      handleApiError(error);
      return false;
    } finally {
      setAccountActionLoading(accountId, false);
    }
  }

  /**
   * 删除已保存账号及其关联库存。
   * @param accountId 账号主键。
   */
  async function deleteSavedAccount(accountId: number): Promise<boolean> {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
      authStore.setError("请先登录。");
      return false;
    }

    setAccountActionLoading(accountId, true);
    authStore.clearError();
    try {
      await deleteSavedAccountApi(accountId);
      await loadProtectedData();
      return true;
    } catch (error) {
      handleApiError(error);
      return false;
    } finally {
      setAccountActionLoading(accountId, false);
    }
  }

  return {
    library,
    accounts,
    loading,
    pagedGames,
    gamesLoading,
    gamesTotalCount,
    gamesQuery,
    syncingSteam,
    syncingEpic,
    accountActionLoadingIds,
    hasDuplicates,
    resetLibraryData,
    loadLibrary,
    loadLibraryGamesPage,
    loadAccounts,
    loadProtectedData,
    syncSteam,
    syncEpic,
    isAccountActionLoading,
    resyncSavedAccount,
    updateSavedAccount,
    deleteSavedAccount
  };
});
