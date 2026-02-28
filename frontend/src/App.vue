<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import {
  ApiError,
  bootstrapAdmin,
  clearAccessToken,
  fetchAccounts,
  fetchBootstrapStatus,
  fetchCurrentUser,
  fetchLibrary,
  getAccessToken,
  login,
  syncEpic,
  syncSteam
} from "./api";
import type { CurrentUserResponse, LibraryResponse, SavedAccount } from "./types";

const currentUser = ref<CurrentUserResponse | null>(null);
const authLoading = ref(true);
const authenticating = ref(false);
const bootstrapping = ref(false);
const bootstrapEnabled = ref(false);

const library = ref<LibraryResponse | null>(null);
const accounts = ref<SavedAccount[]>([]);
const loading = ref(false);
const syncingSteam = ref(false);
const syncingEpic = ref(false);
const errorMessage = ref("");

const loginForm = reactive({
  username: "",
  password: ""
});

const bootstrapForm = reactive({
  setupToken: "",
  username: "",
  password: ""
});

const steamForm = reactive({
  steamId: "",
  apiKey: "",
  accountName: ""
});

const epicForm = reactive({
  accessToken: "",
  accountName: ""
});

const hasDuplicates = computed(() => (library.value?.duplicateGroups ?? 0) > 0);
const isAuthenticated = computed(() => currentUser.value !== null);

/**
 * 重置登录后可见的库存相关状态，避免用户切换时出现脏数据残留。
 */
function resetLibraryData(): void {
  library.value = null;
  accounts.value = [];
}

/**
 * 清空本地会话并回到未登录状态。
 * @param message 需要展示给用户的提示信息。
 */
function logout(message = ""): void {
  clearAccessToken();
  currentUser.value = null;
  resetLibraryData();
  errorMessage.value = message;
}

/**
 * 统一处理接口异常，并在 401 场景下按需强制登出。
 * @param error 接口错误对象。
 * @param invalidateSessionOnUnauthorized 是否在 401 时使会话失效。
 */
function handleApiError(error: unknown, invalidateSessionOnUnauthorized = false): void {
  if (invalidateSessionOnUnauthorized && error instanceof ApiError && error.status === 401) {
    logout("登录已失效，请重新登录。");
    return;
  }

  errorMessage.value = (error as Error).message;
}

/**
 * 加载游戏库存数据。
 */
async function loadLibrary(): Promise<void> {
  if (!isAuthenticated.value) {
    return;
  }

  loading.value = true;
  try {
    library.value = await fetchLibrary();
  } catch (error) {
    handleApiError(error, true);
  } finally {
    loading.value = false;
  }
}

/**
 * 加载已保存账号数据。
 */
async function loadAccounts(): Promise<void> {
  if (!isAuthenticated.value) {
    return;
  }

  try {
    accounts.value = await fetchAccounts();
  } catch (error) {
    handleApiError(error, true);
  }
}

/**
 * 并行加载所有受保护数据，提高页面恢复速度。
 */
async function loadProtectedData(): Promise<void> {
  await Promise.all([loadLibrary(), loadAccounts()]);
}

/**
 * 刷新后端引导状态，决定是否展示初始化管理员入口。
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
 * 启动时恢复认证上下文：
 * 1. 先获取 bootstrap 状态；
 * 2. 若存在本地 token，则向后端校验并拉取数据。
 */
async function initializeAuthState(): Promise<void> {
  authLoading.value = true;
  errorMessage.value = "";
  await refreshBootstrapStatus();

  const token = getAccessToken();
  if (!token) {
    authLoading.value = false;
    return;
  }

  try {
    // 启动时优先校验本地令牌，避免前端误以为处于已登录状态。
    currentUser.value = await fetchCurrentUser();
    await loadProtectedData();
  } catch (error) {
    handleApiError(error, true);
  } finally {
    authLoading.value = false;
  }
}

/**
 * 提交登录表单并加载受保护数据。
 */
async function onLogin(): Promise<void> {
  if (!loginForm.username.trim() || !loginForm.password) {
    errorMessage.value = "请输入用户名和密码。";
    return;
  }

  authenticating.value = true;
  errorMessage.value = "";
  try {
    await login({
      username: loginForm.username.trim(),
      password: loginForm.password
    });

    currentUser.value = await fetchCurrentUser();
    await loadProtectedData();
  } catch (error) {
    handleApiError(error);
  } finally {
    authenticating.value = false;
  }
}

/**
 * 首次初始化管理员后自动登录，减少重复输入操作。
 */
async function onBootstrapAdmin(): Promise<void> {
  if (!bootstrapForm.setupToken.trim() || !bootstrapForm.username.trim() || !bootstrapForm.password) {
    errorMessage.value = "请填写初始化管理员所需的全部字段。";
    return;
  }

  bootstrapping.value = true;
  errorMessage.value = "";

  try {
    await bootstrapAdmin({
      setupToken: bootstrapForm.setupToken.trim(),
      username: bootstrapForm.username.trim(),
      password: bootstrapForm.password
    });

    loginForm.username = bootstrapForm.username.trim();
    loginForm.password = bootstrapForm.password;
    bootstrapForm.setupToken = "";
    await refreshBootstrapStatus();
    await onLogin();
  } catch (error) {
    handleApiError(error);
  } finally {
    bootstrapping.value = false;
  }
}

/**
 * 提交 Steam 同步请求并刷新展示数据。
 */
async function onSyncSteam(): Promise<void> {
  if (!isAuthenticated.value) {
    errorMessage.value = "请先登录。";
    return;
  }

  if (!steamForm.steamId.trim()) {
    errorMessage.value = "请填写 SteamID。";
    return;
  }

  syncingSteam.value = true;
  errorMessage.value = "";
  try {
    await syncSteam({
      steamId: steamForm.steamId.trim(),
      apiKey: steamForm.apiKey.trim() || undefined,
      accountName: steamForm.accountName.trim() || undefined
    });
    await loadProtectedData();
  } catch (error) {
    handleApiError(error, true);
  } finally {
    syncingSteam.value = false;
  }
}

/**
 * 提交 Epic 同步请求并刷新展示数据。
 */
async function onSyncEpic(): Promise<void> {
  if (!isAuthenticated.value) {
    errorMessage.value = "请先登录。";
    return;
  }

  if (!epicForm.accessToken.trim()) {
    errorMessage.value = "请填写 Epic Access Token。";
    return;
  }

  syncingEpic.value = true;
  errorMessage.value = "";
  try {
    await syncEpic({
      accessToken: epicForm.accessToken.trim(),
      accountName: epicForm.accountName.trim() || undefined
    });
    await loadProtectedData();
  } catch (error) {
    handleApiError(error, true);
  } finally {
    syncingEpic.value = false;
  }
}

/**
 * 生命周期钩子：组件挂载后初始化认证状态与页面数据。
 */
onMounted(async () => {
  await initializeAuthState();
});
</script>

<template>
  <main class="page">
    <section class="hero">
      <p class="eyebrow">Multi-Platform</p>
      <h1>跨平台已拥有游戏管理器</h1>
      <p class="subtitle">当前支持 Steam、Epic，后续可扩展更多平台。</p>
      <div v-if="isAuthenticated" class="hero-user">
        <span>当前用户：{{ currentUser?.username }}（{{ currentUser?.role }}）</span>
        <button class="ghost" @click="logout()">退出登录</button>
      </div>
    </section>

    <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

    <section v-if="authLoading" class="panel full">
      <h2>正在校验登录状态...</h2>
    </section>

    <template v-else-if="!isAuthenticated">
      <section class="panel auth-panel">
        <h2>用户登录</h2>
        <label>
          用户名
          <input v-model="loginForm.username" placeholder="admin" />
        </label>
        <label>
          密码
          <input v-model="loginForm.password" type="password" placeholder="请输入密码" />
        </label>
        <button :disabled="authenticating" @click="onLogin">
          {{ authenticating ? "登录中..." : "登录" }}
        </button>
      </section>

      <section v-if="bootstrapEnabled" class="panel auth-panel">
        <h2>初始化管理员（仅首次）</h2>
        <label>
          Setup Token
          <input v-model="bootstrapForm.setupToken" placeholder="服务端配置的 BootstrapToken" />
        </label>
        <label>
          管理员用户名
          <input v-model="bootstrapForm.username" placeholder="admin" />
        </label>
        <label>
          管理员密码
          <input v-model="bootstrapForm.password" type="password" placeholder="至少8位" />
        </label>
        <button :disabled="bootstrapping" @click="onBootstrapAdmin">
          {{ bootstrapping ? "初始化中..." : "创建并登录管理员" }}
        </button>
      </section>
    </template>

    <template v-else>
      <section class="panel-grid">
        <article class="panel">
          <h2>同步 Steam 库存</h2>
          <label>
            SteamID
            <input v-model="steamForm.steamId" placeholder="7656119..." />
          </label>
          <label>
            账号别名（可选）
            <input v-model="steamForm.accountName" placeholder="Main Steam" />
          </label>
          <label>
            API Key（可选，后端已配置可不填）
            <input v-model="steamForm.apiKey" placeholder="Steam Web API Key" />
          </label>
          <button :disabled="syncingSteam" @click="onSyncSteam">
            {{ syncingSteam ? "同步中..." : "同步 Steam" }}
          </button>
        </article>

        <article class="panel">
          <h2>同步 Epic 库存</h2>
          <label>
            Access Token
            <textarea
              v-model="epicForm.accessToken"
              rows="4"
              placeholder="登录 Epic 后获得的 Bearer Token"
            />
          </label>
          <label>
            账号别名（可选）
            <input v-model="epicForm.accountName" placeholder="Main Epic" />
          </label>
          <button :disabled="syncingEpic" @click="onSyncEpic">
            {{ syncingEpic ? "同步中..." : "同步 Epic" }}
          </button>
        </article>
      </section>

      <section class="summary">
        <div class="metric">
          <span>总游戏数</span>
          <strong>{{ library?.totalGames ?? 0 }}</strong>
        </div>
        <div class="metric warning">
          <span>跨平台重复组</span>
          <strong>{{ library?.duplicateGroups ?? 0 }}</strong>
        </div>
        <button class="ghost" :disabled="loading" @click="loadLibrary">
          {{ loading ? "刷新中..." : "刷新库存" }}
        </button>
      </section>

      <section class="panel full">
        <h2>已保存账号与登录信息（掩码）</h2>
        <table>
          <thead>
            <tr>
              <th>平台</th>
              <th>账号名称</th>
              <th>平台账号ID</th>
              <th>凭证类型</th>
              <th>凭证预览</th>
              <th>上次同步 (UTC)</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="account in accounts" :key="account.id">
              <td>{{ account.platform }}</td>
              <td>{{ account.accountName }}</td>
              <td>{{ account.externalAccountId || "-" }}</td>
              <td>{{ account.credentialType }}</td>
              <td><code>{{ account.credentialPreview }}</code></td>
              <td>{{ account.lastSyncedAtUtc || "-" }}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <section v-if="hasDuplicates" class="panel duplicates">
        <h2>重复购买预警</h2>
        <div v-for="group in library?.duplicates ?? []" :key="group.normalizedTitle" class="dup-group">
          <h3>{{ group.games[0]?.title }}</h3>
          <ul>
            <li v-for="game in group.games" :key="`${game.platform}-${game.accountName}-${game.externalId}`">
              <strong>{{ game.platform }}</strong>
              <span>{{ game.accountName }}</span>
              <code>{{ game.externalId }}</code>
            </li>
          </ul>
        </div>
      </section>

      <section class="panel full">
        <h2>全部库存</h2>
        <table>
          <thead>
            <tr>
              <th>游戏</th>
              <th>平台</th>
              <th>账号</th>
              <th>同步时间 (UTC)</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="game in library?.games ?? []" :key="`${game.platform}-${game.accountName}-${game.externalId}`">
              <td>{{ game.title }}</td>
              <td>{{ game.platform }}</td>
              <td>{{ game.accountName }}</td>
              <td>{{ game.syncedAtUtc }}</td>
            </tr>
          </tbody>
        </table>
      </section>
    </template>
  </main>
</template>
