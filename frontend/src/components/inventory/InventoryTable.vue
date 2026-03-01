<script setup lang="ts">
import { computed } from "vue";
import type { LibraryGameGroupItem, LibraryGameListItem } from "../../types/gameLibrary";

interface InventoryTableProps {
  games: LibraryGameListItem[];
  loading: boolean;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

interface TablePaginationChange {
  current: number;
  pageSize: number;
}

const props = defineProps<InventoryTableProps>();

/**
 * 库存表格事件。
 * `change-page`：触发后端分页请求。
 */
const emit = defineEmits<{
  (event: "change-page", payload: TablePaginationChange): void;
}>();

const gameTableColumns = [
  { title: "游戏", dataIndex: "title", key: "title", ellipsis: true },
  { title: "平台", dataIndex: "platform", key: "platform", width: 100 },
  { title: "账号", dataIndex: "accountName", key: "accountName", width: 160 },
  { title: "Epic AppName", dataIndex: "epicAppName", key: "epicAppName", width: 220 },
  { title: "同组条目数", dataIndex: "groupItemCount", key: "groupItemCount", width: 120 },
  { title: "账号ID", dataIndex: "accountExternalId", key: "accountExternalId", width: 180 },
  { title: "同步时间 (UTC+8)", dataIndex: "syncedAtUtc", key: "syncedAtUtc", width: 170 }
];

const gameGroupItemColumns = [
  { title: "条目 ExternalID", dataIndex: "externalId", key: "externalId", width: 260 },
  { title: "Epic AppName", dataIndex: "epicAppName", key: "epicAppName", width: 220 },
  { title: "同步时间 (UTC+8)", dataIndex: "syncedAtUtc", key: "syncedAtUtc", width: 170 }
];

const tablePagination = computed(() => ({
  current: props.pageNumber,
  pageSize: props.pageSize,
  total: props.totalCount,
  showSizeChanger: true,
  showTotal: (total: number) => `共 ${total} 条`
}));

const tableScroll = {
  x: 1150,
  y: 680
};

const tableExpandable = {
  rowExpandable: (record: LibraryGameListItem) => record.groupItemCount > 1
};

/**
 * 库存表格分页变更，回传给父页面触发后端查询。
 * @param pagination Ant Table 分页参数。
 */
function onTableChange(pagination: { current?: number; pageSize?: number }): void {
  emit("change-page", {
    current: pagination.current ?? props.pageNumber,
    pageSize: pagination.pageSize ?? props.pageSize
  });
}

/**
 * 主表行主键。
 * @param game 聚合后的库存主记录。
 */
function gameRowKey(game: LibraryGameListItem): string {
  return game.groupKey;
}

/**
 * 展开明细子表行主键。
 * @param item 分组明细记录。
 */
function groupItemRowKey(item: LibraryGameGroupItem): string {
  return item.externalId;
}
</script>

<template>
  <a-table
    class="inventory-table-fixed"
    :columns="gameTableColumns"
    :data-source="props.games"
    :loading="props.loading"
    :pagination="tablePagination"
    :scroll="tableScroll"
    :expandable="tableExpandable"
    size="middle"
    :row-key="gameRowKey"
    @change="onTableChange"
  >
    <template #bodyCell="{ column, record }">
      <template v-if="column.key === 'epicAppName'">
        {{ record.epicAppName || "-" }}
      </template>
      <template v-else-if="column.key === 'groupItemCount'">
        <a-tag :color="record.groupItemCount > 1 ? 'gold' : 'blue'">
          {{ record.groupItemCount }}
        </a-tag>
      </template>
    </template>
    <template #expandedRowRender="{ record }">
      <a-table
        :columns="gameGroupItemColumns"
        :data-source="record.groupItems"
        :pagination="false"
        :row-key="groupItemRowKey"
        size="small"
      >
        <template #bodyCell="{ column, record: detail }">
          <template v-if="column.key === 'epicAppName'">
            {{ detail.epicAppName || "-" }}
          </template>
        </template>
      </a-table>
    </template>
  </a-table>
</template>
