<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { storeToRefs } from "pinia";
import { message } from "ant-design-vue";
import AccountEditModal from "../components/accounts/AccountEditModal.vue";
import SavedAccountsTable from "../components/accounts/SavedAccountsTable.vue";
import SyncPanels from "../components/accounts/SyncPanels.vue";
import { useLibraryStore } from "../stores/libraryStore";
import type { SavedAccount, UpdateSavedAccountRequest } from "../types/gameLibrary";

const libraryStore = useLibraryStore();
const { accounts, syncingSteam, syncingEpic } = storeToRefs(libraryStore);

const accountEditModalOpen = ref(false);
const editingAccountId = ref<number | null>(null);

const editingAccount = computed(
  () => accounts.value.find((account) => account.id === editingAccountId.value) ?? null
);
const editingAccountLoading = computed(
  () => editingAccountId.value !== null && libraryStore.isAccountActionLoading(editingAccountId.value)
);

/**
 * 打开账号编辑弹窗并回填当前值。
 * @param account 已保存账号。
 */
function openEditAccount(account: SavedAccount): void {
  editingAccountId.value = account.id;
  accountEditModalOpen.value = true;
}

/**
 * 关闭账号编辑弹窗并清理当前编辑状态。
 */
function closeEditAccountModal(): void {
  accountEditModalOpen.value = false;
  editingAccountId.value = null;
}

/**
 * 提交 Steam 同步请求。
 */
async function handleSyncSteam(payload: { steamId: string; apiKey?: string; accountName?: string }): Promise<void> {
  await libraryStore.syncSteam(payload);
}

/**
 * 提交 Epic 同步请求。
 */
async function handleSyncEpic(payload: { accessToken: string; accountName?: string }): Promise<void> {
  await libraryStore.syncEpic(payload);
}

/**
 * 提交账号编辑请求。
 * @param payload 编辑参数。
 */
async function submitEditAccount(payload: UpdateSavedAccountRequest): Promise<void> {
  if (editingAccountId.value === null) {
    return;
  }

  const success = await libraryStore.updateSavedAccount(editingAccountId.value, payload);
  if (!success) {
    return;
  }

  message.success("账号信息已更新");
  closeEditAccountModal();
}

/**
 * 使用已保存凭证重拉指定账号库存。
 * @param account 已保存账号。
 */
async function resyncAccount(account: SavedAccount): Promise<void> {
  const success = await libraryStore.resyncSavedAccount(account.id);
  if (success) {
    message.success(`已完成 ${account.accountName} 的库存重拉`);
  }
}

/**
 * 删除指定账号及其关联库存。
 * @param account 已保存账号。
 */
async function deleteAccount(account: SavedAccount): Promise<void> {
  const success = await libraryStore.deleteSavedAccount(account.id);
  if (!success) {
    return;
  }

  message.success(`已删除账号 ${account.accountName}`);
  if (editingAccountId.value === account.id) {
    closeEditAccountModal();
  }
}

onMounted(async () => {
  if (accounts.value.length === 0) {
    await libraryStore.loadAccounts();
  }
});
</script>

<template>
  <section class="content-grid">
    <SyncPanels
      :syncing-steam="syncingSteam"
      :syncing-epic="syncingEpic"
      @sync-steam="handleSyncSteam"
      @sync-epic="handleSyncEpic"
    />

    <a-card title="已保存账号与登录信息（掩码）" :bordered="false" class="surface-card">
      <SavedAccountsTable
        :accounts="accounts"
        :is-account-action-loading="libraryStore.isAccountActionLoading"
        @resync="resyncAccount"
        @edit="openEditAccount"
        @delete="deleteAccount"
      />
    </a-card>
  </section>

  <AccountEditModal
    v-model:open="accountEditModalOpen"
    :account="editingAccount"
    :loading="editingAccountLoading"
    @submit="submitEditAccount"
  />
</template>
