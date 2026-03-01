<script setup lang="ts">
import { reactive } from "vue";

interface LoginCredentials {
  username: string;
  password: string;
}

interface LoginFormCardProps {
  loading: boolean;
}

const props = defineProps<LoginFormCardProps>();

/**
 * 登录表单提交事件。
 * `submit`：提交用户名和密码。
 */
const emit = defineEmits<{
  (event: "submit", payload: LoginCredentials): void;
}>();

const loginForm = reactive<LoginCredentials>({
  username: "",
  password: ""
});

/**
 * 提交登录参数。
 */
function onSubmit(): void {
  emit("submit", {
    username: loginForm.username.trim(),
    password: loginForm.password
  });
}
</script>

<template>
  <a-card title="用户登录" :bordered="false" class="surface-card">
    <a-form layout="vertical" :model="loginForm" @finish="onSubmit">
      <a-form-item label="用户名">
        <a-input v-model:value="loginForm.username" placeholder="admin" />
      </a-form-item>
      <a-form-item label="密码">
        <a-input-password v-model:value="loginForm.password" placeholder="请输入密码" />
      </a-form-item>
      <a-button type="primary" html-type="submit" block :loading="props.loading">
        {{ props.loading ? "登录中..." : "登录" }}
      </a-button>
    </a-form>
  </a-card>
</template>
