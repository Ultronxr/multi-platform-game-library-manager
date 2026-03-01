<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { storeToRefs } from "pinia";
import InventoryFilterPanel from "../components/inventory/InventoryFilterPanel.vue";
import InventoryTable from "../components/inventory/InventoryTable.vue";
import { useLibraryStore } from "../stores/libraryStore";
import type { InventoryFilters, LibraryGamesQuery } from "../types/gameLibrary";

const libraryStore = useLibraryStore();
const { pagedGames, gamesLoading, gamesTotalCount, gamesQuery, accounts } = storeToRefs(libraryStore);

const filters = ref<InventoryFilters>({
  gameTitle: "",
  platform: undefined,
  accountName: "",
  accountExternalId: ""
});

const platformOptions = computed(() => {
  const options = new Set<string>();
  for (const account of accounts.value) {
    options.add(account.platform);
  }

  return [...options]
    .sort((a, b) => a.localeCompare(b))
    .map((platform) => ({ label: platform, value: platform }));
});

/**
 * 将当前筛选条件转换为后端分页查询参数。
 */
function buildQuery(pageNumber: number, pageSize: number): Partial<LibraryGamesQuery> {
  return {
    pageNumber,
    pageSize,
    gameTitle: filters.value.gameTitle.trim() || undefined,
    platform: filters.value.platform,
    accountName: filters.value.accountName.trim() || undefined,
    accountExternalId: filters.value.accountExternalId.trim() || undefined
  };
}

/**
 * 按当前筛选条件加载库存数据。
 * @param pageNumber 页码。
 * @param pageSize 每页条数。
 */
async function applyFilters(pageNumber: number, pageSize: number): Promise<void> {
  await libraryStore.loadLibraryGamesPage(buildQuery(pageNumber, pageSize));
}

/**
 * 应用筛选并回到第一页。
 */
async function handleApplyFilters(): Promise<void> {
  await applyFilters(1, gamesQuery.value.pageSize);
}

/**
 * 重置筛选并回到第一页。
 */
async function handleResetFilters(): Promise<void> {
  await applyFilters(1, gamesQuery.value.pageSize);
}

/**
 * 处理分页变化（服务端分页）。
 * @param payload 分页参数。
 */
async function handleTablePageChange(payload: { current: number; pageSize: number }): Promise<void> {
  await applyFilters(payload.current, payload.pageSize);
}

onMounted(async () => {
  if (pagedGames.value.length === 0) {
    await applyFilters(gamesQuery.value.pageNumber, gamesQuery.value.pageSize);
  }
});
</script>

<template>
  <section class="content-grid">
    <a-card title="全部库存" :bordered="false" class="surface-card">
      <InventoryFilterPanel
        v-model="filters"
        :platform-options="platformOptions"
        :filtered-count="pagedGames.length"
        :total-count="gamesTotalCount"
        :loading="gamesLoading"
        @apply="handleApplyFilters"
        @reset="handleResetFilters"
      />
      <InventoryTable
        :games="pagedGames"
        :loading="gamesLoading"
        :page-number="gamesQuery.pageNumber"
        :page-size="gamesQuery.pageSize"
        :total-count="gamesTotalCount"
        @change-page="handleTablePageChange"
      />
    </a-card>
  </section>
</template>
