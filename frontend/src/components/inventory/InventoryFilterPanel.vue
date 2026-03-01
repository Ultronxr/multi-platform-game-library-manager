<script setup lang="ts">
import { computed } from "vue";
import type { InventoryFilters } from "../../types/gameLibrary";

interface FilterOption {
  label: string;
  value: string;
}

interface InventoryFilterPanelProps {
  modelValue: InventoryFilters;
  platformOptions: FilterOption[];
  filteredCount: number;
  totalCount: number;
  loading: boolean;
}

const props = defineProps<InventoryFilterPanelProps>();

/**
 * 筛选面板事件。
 * `update:modelValue`：同步筛选条件；
 * `apply`：应用筛选；
 * `reset`：重置筛选。
 */
const emit = defineEmits<{
  (event: "update:modelValue", value: InventoryFilters): void;
  (event: "apply"): void;
  (event: "reset"): void;
}>();

const gameTitle = computed({
  get: () => props.modelValue.gameTitle,
  set: (value: string) => emit("update:modelValue", { ...props.modelValue, gameTitle: value })
});

const platform = computed({
  get: () => props.modelValue.platform,
  set: (value: string | undefined) => emit("update:modelValue", { ...props.modelValue, platform: value })
});

const accountName = computed({
  get: () => props.modelValue.accountName,
  set: (value: string) => emit("update:modelValue", { ...props.modelValue, accountName: value })
});

const accountExternalId = computed({
  get: () => props.modelValue.accountExternalId,
  set: (value: string) => emit("update:modelValue", { ...props.modelValue, accountExternalId: value })
});

/**
 * 重置筛选条件。
 */
function onReset(): void {
  emit("update:modelValue", {
    gameTitle: "",
    platform: undefined,
    accountName: "",
    accountExternalId: ""
  });
  emit("reset");
}
</script>

<template>
  <div class="inventory-filter-grid">
    <a-input v-model:value="gameTitle" allow-clear placeholder="游戏名称（模糊）" />
    <a-select v-model:value="platform" :options="props.platformOptions" allow-clear placeholder="平台（下拉）" />
    <a-input v-model:value="accountName" allow-clear placeholder="账号名称（模糊）" />
    <a-input v-model:value="accountExternalId" allow-clear placeholder="账号ID（模糊）" />
  </div>
  <div class="inventory-filter-meta">
    <a-tag color="processing">筛选结果：{{ props.filteredCount }} / {{ props.totalCount }}</a-tag>
    <a-space>
      <a-button type="primary" :loading="props.loading" @click="emit('apply')">应用筛选</a-button>
      <a-button :disabled="props.loading" @click="onReset">重置筛选</a-button>
    </a-space>
  </div>
</template>
