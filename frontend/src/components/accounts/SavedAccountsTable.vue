<script setup lang="ts">
import { DeleteOutlined, EditOutlined, ReloadOutlined } from "@ant-design/icons-vue";
import type { SavedAccount } from "../../types/gameLibrary";

interface SavedAccountsTableProps {
  accounts: SavedAccount[];
  isAccountActionLoading: (accountId: number) => boolean;
}

const props = defineProps<SavedAccountsTableProps>();

/**
 * 账号表格行事件。
 * `resync`：重拉库存；
 * `edit`：打开编辑弹窗；
 * `delete`：删除账号。
 */
const emit = defineEmits<{
  (event: "resync", account: SavedAccount): void;
  (event: "edit", account: SavedAccount): void;
  (event: "delete", account: SavedAccount): void;
}>();

const accountTableColumns = [
  { title: "平台", dataIndex: "platform", key: "platform", width: 90 },
  { title: "账号名称", dataIndex: "accountName", key: "accountName", width: 170 },
  { title: "平台账号ID", dataIndex: "externalAccountId", key: "externalAccountId", width: 180 },
  { title: "凭证类型", dataIndex: "credentialType", key: "credentialType", width: 150 },
  { title: "凭证预览", dataIndex: "credentialPreview", key: "credentialPreview", width: 180 },
  { title: "上次同步 (UTC+8)", dataIndex: "lastSyncedAtUtc", key: "lastSyncedAtUtc", width: 170 },
  { title: "操作", key: "actions", width: 260, fixed: "right" as const }
];
</script>

<template>
  <a-table
    :columns="accountTableColumns"
    :data-source="props.accounts"
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
            :loading="props.isAccountActionLoading(record.id)"
            @click="emit('resync', record)"
          >
            <template #icon><ReloadOutlined /></template>
            重拉库存
          </a-button>
          <a-button
            size="small"
            :disabled="props.isAccountActionLoading(record.id)"
            @click="emit('edit', record)"
          >
            <template #icon><EditOutlined /></template>
            修改
          </a-button>
          <a-popconfirm
            title="确认删除该账号及其关联库存吗？"
            ok-text="删除"
            cancel-text="取消"
            @confirm="emit('delete', record)"
          >
            <a-button size="small" danger :disabled="props.isAccountActionLoading(record.id)">
              <template #icon><DeleteOutlined /></template>
              删除
            </a-button>
          </a-popconfirm>
        </a-space>
      </template>
    </template>
  </a-table>
</template>
