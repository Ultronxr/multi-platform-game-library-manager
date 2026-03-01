import { computed, ref } from "vue";
import { defineStore } from "pinia";
import {
  ApiError,
  deleteSavedAccount as deleteSavedAccountApi,
  fetchAccounts,
  fetchLibrary,
  resyncSavedAccount as resyncSavedAccountApi,
  syncEpic as syncEpicApi,
  syncSteam as syncSteamApi,
  updateSavedAccount as updateSavedAccountApi
} from "../services/gameLibraryApi";
import type {
  EpicSyncRequest,
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
      library.value = await fetchLibrary();
    } catch (error) {
      handleApiError(error);
    } finally {
      loading.value = false;
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
    await Promise.all([loadLibrary(), loadAccounts()]);
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
    syncingSteam,
    syncingEpic,
    accountActionLoadingIds,
    hasDuplicates,
    resetLibraryData,
    loadLibrary,
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
