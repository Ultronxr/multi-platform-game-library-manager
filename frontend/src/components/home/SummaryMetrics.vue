<script setup lang="ts">
import { FireOutlined, ReloadOutlined } from "@ant-design/icons-vue";

interface SummaryMetricsProps {
  totalGames: number;
  duplicateGroups: number;
  loading: boolean;
}

const props = defineProps<SummaryMetricsProps>();

/**
 * 概览刷新事件。
 * `refresh`：刷新库存摘要和重复组数据。
 */
const emit = defineEmits<{
  (event: "refresh"): void;
}>();
</script>

<template>
  <section class="grid three">
    <a-card class="metric-card" :bordered="false">
      <a-statistic title="总游戏数" :value="props.totalGames" />
    </a-card>
    <a-card class="metric-card" :bordered="false">
      <a-statistic title="重复购买组" :value="props.duplicateGroups">
        <template #prefix><FireOutlined /></template>
      </a-statistic>
    </a-card>
    <a-card class="metric-card action-card" :bordered="false">
      <a-button type="primary" size="large" :loading="props.loading" @click="emit('refresh')">
        <template #icon><ReloadOutlined /></template>
        {{ props.loading ? "刷新中..." : "刷新库存" }}
      </a-button>
    </a-card>
  </section>
</template>
