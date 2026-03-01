<script setup lang="ts">
import { reactive } from "vue";

interface BootstrapPayload {
  setupToken: string;
  username: string;
  password: string;
}

interface BootstrapAdminCardProps {
  loading: boolean;
}

const props = defineProps<BootstrapAdminCardProps>();

/**
 * 初始化管理员提交事件。
 * `submit`：提交 setupToken 和管理员账号凭证。
 */
const emit = defineEmits<{
  (event: "submit", payload: BootstrapPayload): void;
}>();

const bootstrapForm = reactive<BootstrapPayload>({
  setupToken: "",
  username: "",
  password: ""
});

/**
 * 提交初始化管理员请求参数。
 */
function onSubmit(): void {
  emit("submit", {
    setupToken: bootstrapForm.setupToken.trim(),
    username: bootstrapForm.username.trim(),
    password: bootstrapForm.password
  });
}
</script>

<template>
  <a-card title="初始化管理员（仅首次）" :bordered="false" class="surface-card">
    <a-form layout="vertical" :model="bootstrapForm" @finish="onSubmit">
      <a-form-item label="Setup Token">
        <a-input v-model:value="bootstrapForm.setupToken" placeholder="服务端配置的 BootstrapToken" />
      </a-form-item>
      <a-form-item label="管理员用户名">
        <a-input v-model:value="bootstrapForm.username" placeholder="admin" />
      </a-form-item>
      <a-form-item label="管理员密码">
        <a-input-password v-model:value="bootstrapForm.password" placeholder="至少 8 位" />
      </a-form-item>
      <a-button type="primary" html-type="submit" block :loading="props.loading">
        {{ props.loading ? "初始化中..." : "创建并登录管理员" }}
      </a-button>
    </a-form>
  </a-card>
</template>
