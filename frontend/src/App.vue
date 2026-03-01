<script setup lang="ts">
import { onMounted, reactive } from "vue";
import { storeToRefs } from "pinia";
import { useAuthStore } from "./stores/authStore";
import { useLibraryStore } from "./stores/libraryStore";

const authStore = useAuthStore();
const libraryStore = useLibraryStore();

const {
  currentUser,
  authLoading,
  authenticating,
  bootstrapping,
  bootstrapEnabled,
  errorMessage,
  isAuthenticated
} = storeToRefs(authStore);

const { library, accounts, loading, syncingSteam, syncingEpic, hasDuplicates } = storeToRefs(libraryStore);

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

/**
 * 退出登录并清理页面数据。
 */
function logout(): void {
  authStore.logout();
  libraryStore.resetLibraryData();
}

/**
 * 手动刷新库存列表。
 */
async function refreshLibrary(): Promise<void> {
  await libraryStore.loadLibrary();
}

/**
 * 登录后加载受保护数据。
 */
async function onLogin(): Promise<void> {
  const success = await authStore.loginWithCredentials(loginForm.username, loginForm.password);
  if (!success) {
    return;
  }

  await libraryStore.loadProtectedData();
}

/**
 * 首次初始化管理员并自动登录。
 */
async function onBootstrapAdmin(): Promise<void> {
  const success = await authStore.bootstrapAdminAndLogin(
    bootstrapForm.setupToken,
    bootstrapForm.username,
    bootstrapForm.password
  );

  if (!success) {
    return;
  }

  loginForm.username = bootstrapForm.username.trim();
  loginForm.password = bootstrapForm.password;
  bootstrapForm.setupToken = "";
  await libraryStore.loadProtectedData();
}

/**
 * 提交 Steam 同步请求。
 */
async function onSyncSteam(): Promise<void> {
  await libraryStore.syncSteam({
    steamId: steamForm.steamId,
    apiKey: steamForm.apiKey,
    accountName: steamForm.accountName
  });
}

/**
 * 提交 Epic 同步请求。
 */
async function onSyncEpic(): Promise<void> {
  await libraryStore.syncEpic({
    accessToken: epicForm.accessToken,
    accountName: epicForm.accountName
  });
}

/**
 * 生命周期钩子：恢复认证状态并尝试加载库存数据。
 */
onMounted(async () => {
  const restored = await authStore.initializeAuthState();
  if (!restored) {
    return;
  }

  await libraryStore.loadProtectedData();
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
        <button class="ghost" :disabled="loading" @click="refreshLibrary">
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
              <th>上次同步 (UTC+8)</th>
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
              <th>同步时间 (UTC+8)</th>
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
