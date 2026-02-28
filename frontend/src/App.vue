<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { fetchAccounts, fetchLibrary, syncEpic, syncSteam } from "./api";
import type { LibraryResponse, SavedAccount } from "./types";

const library = ref<LibraryResponse | null>(null);
const accounts = ref<SavedAccount[]>([]);
const loading = ref(false);
const syncingSteam = ref(false);
const syncingEpic = ref(false);
const errorMessage = ref("");

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

async function loadLibrary(): Promise<void> {
  loading.value = true;
  errorMessage.value = "";
  try {
    library.value = await fetchLibrary();
  } catch (error) {
    errorMessage.value = (error as Error).message;
  } finally {
    loading.value = false;
  }
}

async function loadAccounts(): Promise<void> {
  try {
    accounts.value = await fetchAccounts();
  } catch (error) {
    errorMessage.value = (error as Error).message;
  }
}

async function onSyncSteam(): Promise<void> {
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
    await Promise.all([loadLibrary(), loadAccounts()]);
  } catch (error) {
    errorMessage.value = (error as Error).message;
  } finally {
    syncingSteam.value = false;
  }
}

async function onSyncEpic(): Promise<void> {
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
    await Promise.all([loadLibrary(), loadAccounts()]);
  } catch (error) {
    errorMessage.value = (error as Error).message;
  } finally {
    syncingEpic.value = false;
  }
}

onMounted(async () => {
  await Promise.all([loadLibrary(), loadAccounts()]);
});
</script>

<template>
  <main class="page">
    <section class="hero">
      <p class="eyebrow">Multi-Platform</p>
      <h1>跨平台已拥有游戏管理器</h1>
      <p class="subtitle">当前支持 Steam、Epic，后续可扩展更多平台。</p>
    </section>

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

    <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

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
  </main>
</template>
