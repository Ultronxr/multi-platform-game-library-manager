<script setup lang="ts">
import { computed, reactive, watch } from "vue";
import type { SavedAccount, UpdateSavedAccountRequest } from "../../types/gameLibrary";

interface AccountEditModalProps {
  open: boolean;
  account: SavedAccount | null;
  loading: boolean;
}

const props = defineProps<AccountEditModalProps>();

/**
 * 编辑弹窗事件。
 * `update:open`：同步弹窗开关；
 * `submit`：提交账号编辑请求。
 */
const emit = defineEmits<{
  (event: "update:open", value: boolean): void;
  (event: "submit", payload: UpdateSavedAccountRequest): void;
}>();

const accountEditForm = reactive({
  accountName: "",
  externalAccountId: "",
  credentialValue: ""
});

const modalOpen = computed({
  get: () => props.open,
  set: (value: boolean) => emit("update:open", value)
});

const modalTip = computed(() =>
  props.account?.platform === "Steam"
    ? "提示：Steam 账号必须保留平台账号ID。"
    : "提示：如不需要更新凭证，请保持凭证输入框为空。"
);

watch(
  () => [props.open, props.account] as const,
  ([open, account]) => {
    if (!open || account === null) {
      accountEditForm.accountName = "";
      accountEditForm.externalAccountId = "";
      accountEditForm.credentialValue = "";
      return;
    }

    accountEditForm.accountName = account.accountName;
    accountEditForm.externalAccountId = account.externalAccountId ?? "";
    accountEditForm.credentialValue = "";
  },
  { immediate: true }
);

/**
 * 提交账号编辑请求。
 */
function handleSubmit(): void {
  const normalizedExternalAccountId = accountEditForm.externalAccountId.trim();

  emit("submit", {
    accountName: accountEditForm.accountName.trim() || undefined,
    externalAccountId: normalizedExternalAccountId === "" ? null : normalizedExternalAccountId,
    credentialValue: accountEditForm.credentialValue || undefined
  });
}
</script>

<template>
  <a-modal
    v-model:open="modalOpen"
    title="编辑已保存账号"
    ok-text="保存修改"
    cancel-text="取消"
    :confirm-loading="props.loading"
    @ok="handleSubmit"
  >
    <a-form layout="vertical" :model="accountEditForm">
      <a-form-item label="平台">
        <a-input :value="props.account?.platform ?? '-'" disabled />
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
        <a-input-password
          v-model:value="accountEditForm.credentialValue"
          placeholder="请输入新凭证"
        />
      </a-form-item>
    </a-form>
    <a-alert type="warning" show-icon :message="modalTip" />
  </a-modal>
</template>
