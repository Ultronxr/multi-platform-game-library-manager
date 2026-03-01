import { computed, ref } from "vue";
import { defineStore } from "pinia";
import {
  ApiError,
  bootstrapAdmin,
  clearAccessToken,
  fetchBootstrapStatus,
  fetchCurrentUser,
  getAccessToken,
  login
} from "../services/gameLibraryApi";
import type { CurrentUserResponse } from "../types/gameLibrary";

/**
 * 认证状态管理：
 * - 维护当前用户与登录流程状态；
 * - 统一处理鉴权相关错误与会话失效。
 */
export const useAuthStore = defineStore("auth", () => {
  const currentUser = ref<CurrentUserResponse | null>(null);
  const authLoading = ref(true);
  const authenticating = ref(false);
  const bootstrapping = ref(false);
  const bootstrapEnabled = ref(false);
  const errorMessage = ref("");

  const isAuthenticated = computed(() => currentUser.value !== null);

  /**
   * 设置全局错误提示。
   * @param message 需要展示给用户的信息。
   */
  function setError(message: string): void {
    errorMessage.value = message;
  }

  /**
   * 清理错误提示。
   */
  function clearError(): void {
    errorMessage.value = "";
  }

  /**
   * 退出登录并清空会话状态。
   * @param message 可选提示信息。
   */
  function logout(message = ""): void {
    clearAccessToken();
    currentUser.value = null;
    errorMessage.value = message;
  }

  /**
   * 统一处理认证相关接口异常。
   * @param error 异常对象。
   * @param invalidateSessionOnUnauthorized 是否在 401 时强制登出。
   */
  function handleApiError(error: unknown, invalidateSessionOnUnauthorized = false): void {
    if (invalidateSessionOnUnauthorized && error instanceof ApiError && error.status === 401) {
      logout("登录已失效，请重新登录。");
      return;
    }

    errorMessage.value = error instanceof Error ? error.message : "请求失败，请稍后重试。";
  }

  /**
   * 刷新引导状态，决定是否展示“初始化管理员”入口。
   */
  async function refreshBootstrapStatus(): Promise<void> {
    try {
      const status = await fetchBootstrapStatus();
      bootstrapEnabled.value = status.bootstrapEnabled;
    } catch {
      bootstrapEnabled.value = false;
    }
  }

  /**
   * 启动时恢复认证状态。
   * @returns 若本地 token 校验通过返回 true，否则 false。
   */
  async function initializeAuthState(): Promise<boolean> {
    authLoading.value = true;
    clearError();
    await refreshBootstrapStatus();

    const token = getAccessToken();
    if (!token) {
      authLoading.value = false;
      return false;
    }

    try {
      currentUser.value = await fetchCurrentUser();
      return true;
    } catch (error) {
      handleApiError(error, true);
      return false;
    } finally {
      authLoading.value = false;
    }
  }

  /**
   * 用户名密码登录。
   * @param username 用户名。
   * @param password 密码。
   * @returns 登录成功返回 true。
   */
  async function loginWithCredentials(username: string, password: string): Promise<boolean> {
    if (!username.trim() || !password) {
      setError("请输入用户名和密码。");
      return false;
    }

    authenticating.value = true;
    clearError();
    try {
      await login({
        username: username.trim(),
        password
      });

      currentUser.value = await fetchCurrentUser();
      return true;
    } catch (error) {
      handleApiError(error);
      return false;
    } finally {
      authenticating.value = false;
    }
  }

  /**
   * 首次初始化管理员并自动登录。
   * @param setupToken 服务端配置的初始化口令。
   * @param username 管理员用户名。
   * @param password 管理员密码。
   * @returns 初始化并登录成功返回 true。
   */
  async function bootstrapAdminAndLogin(
    setupToken: string,
    username: string,
    password: string
  ): Promise<boolean> {
    if (!setupToken.trim() || !username.trim() || !password) {
      setError("请填写初始化管理员所需的全部字段。");
      return false;
    }

    bootstrapping.value = true;
    clearError();

    try {
      await bootstrapAdmin({
        setupToken: setupToken.trim(),
        username: username.trim(),
        password
      });

      await refreshBootstrapStatus();
      return await loginWithCredentials(username, password);
    } catch (error) {
      handleApiError(error);
      return false;
    } finally {
      bootstrapping.value = false;
    }
  }

  return {
    currentUser,
    authLoading,
    authenticating,
    bootstrapping,
    bootstrapEnabled,
    errorMessage,
    isAuthenticated,
    setError,
    clearError,
    logout,
    initializeAuthState,
    refreshBootstrapStatus,
    loginWithCredentials,
    bootstrapAdminAndLogin
  };
});
