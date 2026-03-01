<script setup lang="ts">
import { reactive } from "vue";
import { SyncOutlined } from "@ant-design/icons-vue";
import type { EpicSyncRequest, SteamSyncRequest } from "../../types/gameLibrary";

interface SyncPanelsProps {
  syncingSteam: boolean;
  syncingEpic: boolean;
}

const props = defineProps<SyncPanelsProps>();

/**
 * 同步面板事件。
 * `sync-steam`：触发 Steam 库存同步；
 * `sync-epic`：触发 Epic 库存同步。
 */
const emit = defineEmits<{
  (event: "sync-steam", payload: SteamSyncRequest): void;
  (event: "sync-epic", payload: EpicSyncRequest): void;
}>();

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
 * 提交 Steam 同步参数。
 */
function submitSteam(): void {
  emit("sync-steam", {
    steamId: steamForm.steamId,
    apiKey: steamForm.apiKey || undefined,
    accountName: steamForm.accountName || undefined
  });
}

/**
 * 提交 Epic 同步参数。
 */
function submitEpic(): void {
  emit("sync-epic", {
    accessToken: epicForm.accessToken,
    accountName: epicForm.accountName || undefined
  });
}
</script>

<template>
  <section class="grid two">
    <a-card title="同步 Steam 库存" :bordered="false" class="surface-card">
      <a-form layout="vertical" :model="steamForm" @finish="submitSteam">
        <a-form-item label="SteamID">
          <a-input v-model:value="steamForm.steamId" placeholder="7656119..." />
        </a-form-item>
        <a-form-item label="账号别名（可选）">
          <a-input v-model:value="steamForm.accountName" placeholder="Main Steam" />
        </a-form-item>
        <a-form-item label="API Key（可选，后端已配置可不填）">
          <a-input v-model:value="steamForm.apiKey" placeholder="Steam Web API Key" />
        </a-form-item>
        <a-button type="primary" html-type="submit" block :loading="props.syncingSteam">
          <template #icon><SyncOutlined /></template>
          {{ props.syncingSteam ? "同步中..." : "同步 Steam" }}
        </a-button>
      </a-form>
    </a-card>

    <a-card title="同步 Epic 库存" :bordered="false" class="surface-card">
      <a-form layout="vertical" :model="epicForm" @finish="submitEpic">
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
        <a-button type="primary" html-type="submit" block :loading="props.syncingEpic">
          <template #icon><SyncOutlined /></template>
          {{ props.syncingEpic ? "同步中..." : "同步 Epic" }}
        </a-button>
      </a-form>
    </a-card>
  </section>
</template>
