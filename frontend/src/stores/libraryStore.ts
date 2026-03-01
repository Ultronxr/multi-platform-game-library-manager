import { computed, ref } from "vue";
import { defineStore } from "pinia";
import {
  ApiError,
  fetchAccounts,
  fetchLibrary,
  syncEpic as syncEpicApi,
  syncSteam as syncSteamApi
} from "../services/gameLibraryApi";
import type { EpicSyncRequest, LibraryResponse, SavedAccount, SteamSyncRequest } from "../types/gameLibrary";
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

  return {
    library,
    accounts,
    loading,
    syncingSteam,
    syncingEpic,
    hasDuplicates,
    resetLibraryData,
    loadLibrary,
    loadAccounts,
    loadProtectedData,
    syncSteam,
    syncEpic
  };
});
