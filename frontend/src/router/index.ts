import type { NavigationGuardNext, RouteLocationNormalized } from "vue-router";
import { createRouter, createWebHistory } from "vue-router";
import MainLayout from "../layouts/MainLayout.vue";
import LoginPage from "../pages/LoginPage.vue";
import AccountManagementPage from "../pages/AccountManagementPage.vue";
import HomePage from "../pages/HomePage.vue";
import InventoryPage from "../pages/InventoryPage.vue";
import { useAuthStore } from "../stores/authStore";
import { pinia } from "../stores/pinia";

let initialized = false;

/**
 * 路由鉴权前先初始化认证态，避免刷新后误判未登录。
 */
async function ensureAuthInitialized(): Promise<void> {
  if (initialized) {
    return;
  }

  const authStore = useAuthStore(pinia);
  await authStore.initializeAuthState();
  initialized = true;
}

/**
 * 统一路由守卫：
 * 1. 需要鉴权的页面未登录则跳转登录页；
 * 2. 已登录访问登录页则跳转主页。
 */
async function authGuard(
  to: RouteLocationNormalized,
  _from: RouteLocationNormalized,
  next: NavigationGuardNext
): Promise<void> {
  await ensureAuthInitialized();
  const authStore = useAuthStore(pinia);
  const requiresAuth = to.meta.requiresAuth !== false;

  if (requiresAuth && !authStore.isAuthenticated) {
    next({ name: "login" });
    return;
  }

  if (to.name === "login" && authStore.isAuthenticated) {
    next({ name: "home" });
    return;
  }

  next();
}

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      redirect: "/app/home"
    },
    {
      path: "/login",
      name: "login",
      component: LoginPage,
      meta: {
        requiresAuth: false,
        title: "登录"
      }
    },
    {
      path: "/app",
      component: MainLayout,
      meta: { requiresAuth: true },
      children: [
        {
          path: "home",
          name: "home",
          component: HomePage,
          meta: {
            title: "主页",
            subtitle: "查看库存概览与重复购买预警。"
          }
        },
        {
          path: "accounts",
          name: "accounts",
          component: AccountManagementPage,
          meta: {
            title: "平台账号管理",
            subtitle: "维护平台账号，并通过凭证拉取库存。"
          }
        },
        {
          path: "inventory",
          name: "inventory",
          component: InventoryPage,
          meta: {
            title: "详细库存信息",
            subtitle: "按平台、账号、游戏维度查看全部库存记录。"
          }
        }
      ]
    }
  ]
});

router.beforeEach(authGuard);
