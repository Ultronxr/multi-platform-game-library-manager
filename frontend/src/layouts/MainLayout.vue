<script setup lang="ts">
import { computed, h, onMounted, ref, watch } from "vue";
import { storeToRefs } from "pinia";
import type { MenuProps } from "ant-design-vue";
import { RouterView, useRoute, useRouter } from "vue-router";
import {
  DatabaseOutlined,
  HomeOutlined,
  LogoutOutlined,
  TeamOutlined,
  UserOutlined
} from "@ant-design/icons-vue";
import { useAuthStore } from "../stores/authStore";
import { useLibraryStore } from "../stores/libraryStore";

type ProtectedRouteName = "home" | "accounts" | "inventory";

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const libraryStore = useLibraryStore();
const { currentUser, errorMessage, isAuthenticated } = storeToRefs(authStore);

const protectedDataInitialized = ref(false);
const loadingProtectedData = ref(false);

const menuItems: MenuProps["items"] = [
  { key: "home", icon: h(HomeOutlined), label: "主页" },
  { key: "accounts", icon: h(TeamOutlined), label: "平台账号管理" },
  { key: "inventory", icon: h(DatabaseOutlined), label: "详细库存信息" }
];

const selectedMenuKeys = computed(() => [String(route.name ?? "home")]);
const pageTitle = computed(() => String(route.meta.title ?? "主页"));
const pageSubtitle = computed(() => String(route.meta.subtitle ?? ""));

/**
 * 首次进入管理后台时并行加载库存和账号数据。
 */
async function initializeProtectedData(): Promise<void> {
  if (protectedDataInitialized.value || !isAuthenticated.value) {
    return;
  }

  loadingProtectedData.value = true;
  try {
    await libraryStore.loadProtectedData();
    protectedDataInitialized.value = true;
  } finally {
    loadingProtectedData.value = false;
  }
}

/**
 * 左侧菜单切换路由。
 * @param event Menu 点击事件。
 */
function onMenuClick(event: { key: string }): void {
  router.push({ name: event.key as ProtectedRouteName });
}

/**
 * 退出登录并清理受保护数据。
 */
function logout(): void {
  authStore.logout();
  libraryStore.resetLibraryData();
  protectedDataInitialized.value = false;
  router.replace({ name: "login" });
}

watch(
  () => isAuthenticated.value,
  (authenticated) => {
    if (!authenticated) {
      router.replace({ name: "login" });
    }
  }
);

onMounted(async () => {
  await initializeProtectedData();
});
</script>

<template>
  <main class="app-shell">
    <a-layout class="admin-layout">
      <a-layout-sider
        class="admin-sider"
        :width="244"
        :breakpoint="'lg'"
        :collapsed-width="0"
        theme="light"
      >
        <div class="admin-brand">
          <p class="hero-tag">Multi-Platform Game Hub</p>
          <h2>游戏库存后台</h2>
        </div>
        <a-menu :selected-keys="selectedMenuKeys" mode="inline" :items="menuItems" @click="onMenuClick" />
      </a-layout-sider>

      <a-layout class="admin-main-layout">
        <a-layout-header class="admin-header">
          <div>
            <h1 class="admin-header-title">{{ pageTitle }}</h1>
            <p class="admin-header-subtitle">{{ pageSubtitle }}</p>
          </div>
          <a-space>
            <a-tag color="blue">
              <UserOutlined />
              {{ currentUser?.username }}（{{ currentUser?.role }}）
            </a-tag>
            <a-button @click="logout">
              <template #icon><LogoutOutlined /></template>
              退出登录
            </a-button>
          </a-space>
        </a-layout-header>

        <a-layout-content class="admin-content">
          <a-alert v-if="errorMessage" class="page-alert" :message="errorMessage" type="error" show-icon />
          <a-card v-if="loadingProtectedData" class="loading-card">
            <a-spin tip="正在加载库存数据..." />
          </a-card>
          <RouterView v-else />
        </a-layout-content>
      </a-layout>
    </a-layout>
  </main>
</template>
