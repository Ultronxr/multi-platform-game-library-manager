<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { storeToRefs } from "pinia";
import { message } from "ant-design-vue";
import {
  DeleteOutlined,
  EditOutlined,
  FireOutlined,
  ReloadOutlined,
  SyncOutlined,
  UserOutlined
} from "@ant-design/icons-vue";
import type { LibraryGameListItem, SavedAccount } from "./types/gameLibrary";
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
const {
  library,
  accounts,
  loading,
  pagedGames,
  gamesLoading,
  gamesTotalCount,
  gamesQuery,
  syncingSteam,
  syncingEpic,
  hasDuplicates
} = storeToRefs(libraryStore);

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

const accountEditModalOpen = ref(false);
const editingAccountId = ref<number | null>(null);
const accountEditForm = reactive({
  accountName: "",
  externalAccountId: "",
  credentialValue: ""
});
const inventoryFilters = reactive({
  gameTitle: "",
  platform: undefined as string | undefined,
  accountName: "",
  accountExternalId: ""
});

const editingAccount = computed(() =>
  accounts.value.find((account) => account.id === editingAccountId.value) ?? null
);
const editingAccountLoading = computed(() =>
  editingAccountId.value !== null && libraryStore.isAccountActionLoading(editingAccountId.value)
);

const accountTableColumns = [
  { title: "平台", dataIndex: "platform", key: "platform", width: 90 },
  { title: "账号名称", dataIndex: "accountName", key: "accountName", width: 160 },
  { title: "平台账号ID", dataIndex: "externalAccountId", key: "externalAccountId", width: 170 },
  { title: "凭证类型", dataIndex: "credentialType", key: "credentialType", width: 150 },
  { title: "凭证预览", dataIndex: "credentialPreview", key: "credentialPreview", width: 180 },
  { title: "上次同步 (UTC+8)", dataIndex: "lastSyncedAtUtc", key: "lastSyncedAtUtc", width: 170 },
  { title: "操作", key: "actions", width: 260, fixed: "right" as const }
];

const gameTableColumns = [
  { title: "游戏", dataIndex: "title", key: "title", ellipsis: true },
  { title: "平台", dataIndex: "platform", key: "platform", width: 100 },
  { title: "账号", dataIndex: "accountName", key: "accountName", width: 160 },
  { title: "账号ID", dataIndex: "accountExternalId", key: "accountExternalId", width: 180 },
  { title: "同步时间 (UTC+8)", dataIndex: "syncedAtUtc", key: "syncedAtUtc", width: 170 }
];
const gamePlatformOptions = computed(() => {
  const options = new Set<string>();
  for (const account of accounts.value) {
    options.add(account.platform);
  }

  return [...options]
    .sort((a, b) => a.localeCompare(b))
    .map((platform) => ({ label: platform, value: platform }));
});
const gameTablePagination = computed(() => ({
  current: gamesQuery.value.pageNumber,
  pageSize: gamesQuery.value.pageSize,
  total: gamesTotalCount.value,
  showSizeChanger: true,
  showTotal: (total: number) => `共 ${total} 条`
}));
const inventoryTableScroll = {
  x: 900,
  y: 680
};

/**
 * 退出登录并清理页面数据。
 */
function logout(): void {
  authStore.logout();
  libraryStore.resetLibraryData();
}

/**
 * 手动刷新库存与账号列表。
 */
async function refreshLibrary(): Promise<void> {
  await libraryStore.loadProtectedData();
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
 * 打开账号编辑弹窗并回填当前值。
 * @param account 已保存账号行数据。
 */
function openEditAccount(account: SavedAccount): void {
  editingAccountId.value = account.id;
  accountEditForm.accountName = account.accountName;
  accountEditForm.externalAccountId = account.externalAccountId ?? "";
  accountEditForm.credentialValue = "";
  accountEditModalOpen.value = true;
}

/**
 * 关闭账号编辑弹窗并清空临时表单。
 */
function closeEditAccountModal(): void {
  accountEditModalOpen.value = false;
  editingAccountId.value = null;
  accountEditForm.accountName = "";
  accountEditForm.externalAccountId = "";
  accountEditForm.credentialValue = "";
}

/**
 * 提交账号编辑。
 */
async function submitEditAccount(): Promise<void> {
  if (editingAccountId.value === null) {
    return;
  }

  const success = await libraryStore.updateSavedAccount(editingAccountId.value, {
    accountName: accountEditForm.accountName,
    externalAccountId: accountEditForm.externalAccountId,
    credentialValue: accountEditForm.credentialValue || undefined
  });
  if (!success) {
    return;
  }

  message.success("账号信息已更新");
  closeEditAccountModal();
}

/**
 * 一键重拉指定账号库存。
 * @param account 已保存账号。
 */
async function onResyncAccount(account: SavedAccount): Promise<void> {
  const success = await libraryStore.resyncSavedAccount(account.id);
  if (success) {
    message.success(`已完成 ${account.accountName} 的库存重拉`);
  }
}

/**
 * 删除指定账号及其库存数据。
 * @param account 已保存账号。
 */
async function onDeleteAccount(account: SavedAccount): Promise<void> {
  const success = await libraryStore.deleteSavedAccount(account.id);
  if (!success) {
    return;
  }

  message.success(`已删除账号 ${account.accountName}`);
  if (editingAccountId.value === account.id) {
    closeEditAccountModal();
  }
}

/**
 * 生成库存行主键，避免同名游戏冲突。
 * @param game 游戏记录。
 */
function gameRowKey(game: LibraryGameListItem): string {
  return `${game.platform}-${game.accountName}-${game.externalId}`;
}

/**
 * 按当前筛选条件加载库存分页数据。
 * @param pageNumber 目标页码。
 * @param pageSize 每页条数。
 */
async function applyInventoryFilters(
  pageNumber = gamesQuery.value.pageNumber,
  pageSize = gamesQuery.value.pageSize
): Promise<void> {
  await libraryStore.loadLibraryGamesPage({
    pageNumber,
    pageSize,
    gameTitle: inventoryFilters.gameTitle.trim() || undefined,
    platform: inventoryFilters.platform,
    accountName: inventoryFilters.accountName.trim() || undefined,
    accountExternalId: inventoryFilters.accountExternalId.trim() || undefined
  });
}

/**
 * 重置库存筛选条件并回到第一页。
 */
async function clearInventoryFilters(): Promise<void> {
  inventoryFilters.gameTitle = "";
  inventoryFilters.platform = undefined;
  inventoryFilters.accountName = "";
  inventoryFilters.accountExternalId = "";
  await applyInventoryFilters(1, gamesQuery.value.pageSize);
}

/**
 * 库存表格分页回调（后端分页）。
 * @param pagination 分页参数。
 */
async function onGameTableChange(pagination: { current?: number; pageSize?: number }): Promise<void> {
  await applyInventoryFilters(
    pagination.current ?? 1,
    pagination.pageSize ?? gamesQuery.value.pageSize
  );
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
  <main class="app-shell">
    <section class="hero">
      <div>
        <p class="hero-tag">Multi-Platform Game Hub</p>
        <h1>跨平台已拥有游戏管理器</h1>
        <p>统一管理 Steam / Epic 库存，快速发现重复购买。</p>
      </div>
      <a-space v-if="isAuthenticated">
        <a-tag color="blue">
          <UserOutlined />
          {{ currentUser?.username }}（{{ currentUser?.role }}）
        </a-tag>
        <a-button @click="logout">退出登录</a-button>
      </a-space>
    </section>

    <a-alert v-if="errorMessage" :message="errorMessage" type="error" show-icon />

    <a-card v-if="authLoading" class="loading-card">
      <a-spin tip="正在校验登录状态..." />
    </a-card>

    <template v-else-if="!isAuthenticated">
      <section class="grid two">
        <a-card title="用户登录" :bordered="false" class="surface-card">
          <a-form layout="vertical" :model="loginForm" @finish="onLogin">
            <a-form-item label="用户名">
              <a-input v-model:value="loginForm.username" placeholder="admin" />
            </a-form-item>
            <a-form-item label="密码">
              <a-input-password v-model:value="loginForm.password" placeholder="请输入密码" />
            </a-form-item>
            <a-button type="primary" html-type="submit" block :loading="authenticating">
              {{ authenticating ? "登录中..." : "登录" }}
            </a-button>
          </a-form>
        </a-card>

        <a-card
          v-if="bootstrapEnabled"
          title="初始化管理员（仅首次）"
          :bordered="false"
          class="surface-card"
        >
          <a-form layout="vertical" :model="bootstrapForm" @finish="onBootstrapAdmin">
            <a-form-item label="Setup Token">
              <a-input v-model:value="bootstrapForm.setupToken" placeholder="服务端配置的 BootstrapToken" />
            </a-form-item>
            <a-form-item label="管理员用户名">
              <a-input v-model:value="bootstrapForm.username" placeholder="admin" />
            </a-form-item>
            <a-form-item label="管理员密码">
              <a-input-password v-model:value="bootstrapForm.password" placeholder="至少8位" />
            </a-form-item>
            <a-button type="primary" html-type="submit" block :loading="bootstrapping">
              {{ bootstrapping ? "初始化中..." : "创建并登录管理员" }}
            </a-button>
          </a-form>
        </a-card>
      </section>
    </template>

    <template v-else>
      <section class="grid two">
        <a-card title="同步 Steam 库存" :bordered="false" class="surface-card">
          <a-form layout="vertical" :model="steamForm" @finish="onSyncSteam">
            <a-form-item label="SteamID">
              <a-input v-model:value="steamForm.steamId" placeholder="7656119..." />
            </a-form-item>
            <a-form-item label="账号别名（可选）">
              <a-input v-model:value="steamForm.accountName" placeholder="Main Steam" />
            </a-form-item>
            <a-form-item label="API Key（可选，后端已配置可不填）">
              <a-input v-model:value="steamForm.apiKey" placeholder="Steam Web API Key" />
            </a-form-item>
            <a-button type="primary" html-type="submit" block :loading="syncingSteam">
              <template #icon><SyncOutlined /></template>
              {{ syncingSteam ? "同步中..." : "同步 Steam" }}
            </a-button>
          </a-form>
        </a-card>

        <a-card title="同步 Epic 库存" :bordered="false" class="surface-card">
          <a-form layout="vertical" :model="epicForm" @finish="onSyncEpic">
            <a-form-item label="Access Token">
              <a-textarea
                v-model:value="epicForm.accessToken"
                :auto-size="{ minRows: 4, maxRows: 6 }"
                placeholder="登录 Epic 后获得的 Bearer Token"
              />
            </a-form-item>
            <a-form-item label="账号别名（可选）">
              <a-input v-model:value="epicForm.accountName" placeholder="Main Epic" />
            </a-form-item>
            <a-button type="primary" html-type="submit" block :loading="syncingEpic">
              <template #icon><SyncOutlined /></template>
              {{ syncingEpic ? "同步中..." : "同步 Epic" }}
            </a-button>
          </a-form>
        </a-card>
      </section>

      <section class="grid three">
        <a-card class="metric-card" :bordered="false">
          <a-statistic title="总游戏数" :value="library?.totalGames ?? 0" />
        </a-card>
        <a-card class="metric-card" :bordered="false">
          <a-statistic title="跨平台重复组" :value="library?.duplicateGroups ?? 0">
            <template #prefix><FireOutlined /></template>
          </a-statistic>
        </a-card>
        <a-card class="metric-card action-card" :bordered="false">
          <a-button type="primary" size="large" :loading="loading" @click="refreshLibrary">
            <template #icon><ReloadOutlined /></template>
            {{ loading ? "刷新中..." : "刷新库存" }}
          </a-button>
        </a-card>
      </section>

      <a-card title="已保存账号与登录信息（掩码）" :bordered="false" class="surface-card">
        <a-table
          :columns="accountTableColumns"
          :data-source="accounts"
          :scroll="{ x: 1200 }"
          :pagination="false"
          size="middle"
          row-key="id"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'externalAccountId'">
              {{ record.externalAccountId || "-" }}
            </template>
            <template v-else-if="column.key === 'credentialPreview'">
              <a-typography-text code>{{ record.credentialPreview }}</a-typography-text>
            </template>
            <template v-else-if="column.key === 'lastSyncedAtUtc'">
              {{ record.lastSyncedAtUtc || "-" }}
            </template>
            <template v-else-if="column.key === 'actions'">
              <a-space wrap>
                <a-button
                  size="small"
                  type="primary"
                  :loading="libraryStore.isAccountActionLoading(record.id)"
                  @click="onResyncAccount(record)"
                >
                  <template #icon><ReloadOutlined /></template>
                  重拉库存
                </a-button>
                <a-button
                  size="small"
                  :disabled="libraryStore.isAccountActionLoading(record.id)"
                  @click="openEditAccount(record)"
                >
                  <template #icon><EditOutlined /></template>
                  修改
                </a-button>
                <a-popconfirm
                  title="确认删除该账号及其关联库存吗？"
                  ok-text="删除"
                  cancel-text="取消"
                  @confirm="onDeleteAccount(record)"
                >
                  <a-button
                    size="small"
                    danger
                    :disabled="libraryStore.isAccountActionLoading(record.id)"
                  >
                    <template #icon><DeleteOutlined /></template>
                    删除
                  </a-button>
                </a-popconfirm>
              </a-space>
            </template>
          </template>
        </a-table>
      </a-card>

      <a-card v-if="hasDuplicates" title="重复购买预警" :bordered="false" class="surface-card">
        <a-collapse ghost>
          <a-collapse-panel
            v-for="group in library?.duplicates ?? []"
            :key="group.normalizedTitle"
            :header="group.games[0]?.title || group.normalizedTitle"
          >
            <a-list :data-source="group.games" size="small">
              <template #renderItem="{ item }">
                <a-list-item>
                  <a-space>
                    <a-tag color="geekblue">{{ item.platform }}</a-tag>
                    <span>{{ item.accountName }}</span>
                    <a-typography-text code>{{ item.externalId }}</a-typography-text>
                  </a-space>
                </a-list-item>
              </template>
            </a-list>
          </a-collapse-panel>
        </a-collapse>
      </a-card>

      <a-card title="全部库存" :bordered="false" class="surface-card">
        <div class="inventory-filter-grid">
          <a-input
            v-model:value="inventoryFilters.gameTitle"
            allow-clear
            placeholder="游戏名称（模糊）"
          />
          <a-select
            v-model:value="inventoryFilters.platform"
            :options="gamePlatformOptions"
            allow-clear
            placeholder="平台（下拉）"
          />
          <a-input
            v-model:value="inventoryFilters.accountName"
            allow-clear
            placeholder="账号名称（模糊）"
          />
          <a-input
            v-model:value="inventoryFilters.accountExternalId"
            allow-clear
            placeholder="账号ID（模糊）"
          />
        </div>
        <div class="inventory-filter-meta">
          <a-tag color="processing">
            筛选结果：{{ pagedGames.length }} / {{ gamesTotalCount }}
          </a-tag>
          <a-space>
            <a-button type="primary" @click="applyInventoryFilters(1, gamesQuery.pageSize)">
              应用筛选
            </a-button>
            <a-button @click="clearInventoryFilters">重置筛选</a-button>
          </a-space>
        </div>
        <a-table
          class="inventory-table-fixed"
          :columns="gameTableColumns"
          :data-source="pagedGames"
          :loading="gamesLoading"
          :pagination="gameTablePagination"
          :scroll="inventoryTableScroll"
          size="middle"
          :row-key="gameRowKey"
          @change="onGameTableChange"
        />
      </a-card>
    </template>

    <a-modal
      v-model:open="accountEditModalOpen"
      title="编辑已保存账号"
      ok-text="保存修改"
      cancel-text="取消"
      :confirm-loading="editingAccountLoading"
      @ok="submitEditAccount"
      @cancel="closeEditAccountModal"
    >
      <a-form layout="vertical" :model="accountEditForm">
        <a-form-item label="平台">
          <a-input :value="editingAccount?.platform ?? '-'" disabled />
        </a-form-item>
        <a-form-item label="账号名称">
          <a-input v-model:value="accountEditForm.accountName" placeholder="请输入账号名称" />
        </a-form-item>
        <a-form-item label="平台账号ID">
          <a-input
            v-model:value="accountEditForm.externalAccountId"
            placeholder="Steam 建议填写 SteamID；Epic 可留空"
          />
        </a-form-item>
        <a-form-item label="新凭证（留空表示不修改）">
          <a-input-password v-model:value="accountEditForm.credentialValue" placeholder="请输入新凭证" />
        </a-form-item>
      </a-form>
      <a-alert
        type="warning"
        show-icon
        :message="`提示：${editingAccount?.platform === 'Steam' ? 'Steam 账号必须保留平台账号ID。' : '如不需要更新凭证，请保持凭证输入框为空。'}`"
      />
    </a-modal>
  </main>
</template>
