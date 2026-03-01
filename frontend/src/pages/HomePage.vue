<script setup lang="ts">
import { onMounted } from "vue";
import { storeToRefs } from "pinia";
import DuplicateWarningPanel from "../components/home/DuplicateWarningPanel.vue";
import SummaryMetrics from "../components/home/SummaryMetrics.vue";
import { useLibraryStore } from "../stores/libraryStore";

const libraryStore = useLibraryStore();
const { library, loading } = storeToRefs(libraryStore);

/**
 * 主页手动刷新：同步刷新库存摘要、账号列表和分页数据。
 */
async function refreshDashboard(): Promise<void> {
  await libraryStore.loadProtectedData();
}

onMounted(async () => {
  if (library.value === null) {
    await libraryStore.loadLibrary();
  }
});
</script>

<template>
  <section class="content-grid">
    <SummaryMetrics
      :total-games="library?.totalGames ?? 0"
      :duplicate-groups="library?.duplicateGroups ?? 0"
      :loading="loading"
      @refresh="refreshDashboard"
    />

    <DuplicateWarningPanel :duplicates="library?.duplicates ?? []" />
  </section>
</template>
