<script setup lang="ts">
import { onMounted } from "vue";
import { storeToRefs } from "pinia";
import { useRouter } from "vue-router";
import BootstrapAdminCard from "../components/auth/BootstrapAdminCard.vue";
import LoginFormCard from "../components/auth/LoginFormCard.vue";
import { useAuthStore } from "../stores/authStore";

interface LoginPayload {
  username: string;
  password: string;
}

interface BootstrapPayload {
  setupToken: string;
  username: string;
  password: string;
}

const router = useRouter();
const authStore = useAuthStore();
const { authLoading, authenticating, bootstrapping, bootstrapEnabled, errorMessage } = storeToRefs(authStore);

/**
 * 登录成功后跳转到后台主页。
 * @param payload 登录参数。
 */
async function handleLogin(payload: LoginPayload): Promise<void> {
  const success = await authStore.loginWithCredentials(payload.username, payload.password);
  if (!success) {
    return;
  }

  await router.replace({ name: "home" });
}

/**
 * 初始化管理员并自动登录后进入后台主页。
 * @param payload 初始化管理员参数。
 */
async function handleBootstrap(payload: BootstrapPayload): Promise<void> {
  const success = await authStore.bootstrapAdminAndLogin(
    payload.setupToken,
    payload.username,
    payload.password
  );
  if (!success) {
    return;
  }

  await router.replace({ name: "home" });
}

onMounted(async () => {
  await authStore.refreshBootstrapStatus();
});
</script>

<template>
  <main class="login-shell">
    <section class="auth-shell">
      <section class="hero">
        <div>
          <p class="hero-tag">Multi-Platform Game Hub</p>
          <h1>跨平台已拥有游戏管理器</h1>
          <p>登录后统一管理 Steam / Epic 库存并识别重复购买。</p>
        </div>
      </section>

      <a-alert v-if="errorMessage" :message="errorMessage" type="error" show-icon />

      <a-card v-if="authLoading" class="loading-card">
        <a-spin tip="正在校验登录状态..." />
      </a-card>

      <section v-else class="grid two">
        <LoginFormCard :loading="authenticating" @submit="handleLogin" />
        <BootstrapAdminCard
          v-if="bootstrapEnabled"
          :loading="bootstrapping"
          @submit="handleBootstrap"
        />
      </section>
    </section>
  </main>
</template>
