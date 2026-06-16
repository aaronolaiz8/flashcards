// Token persistence shared between the axios interceptors and the auth store.
// Kept separate from the store to avoid an import cycle (api <-> authStore).

const ACCESS_KEY = "fc.accessToken";
const REFRESH_KEY = "fc.refreshToken";

export const tokenStore = {
  getAccess: () => localStorage.getItem(ACCESS_KEY),
  getRefresh: () => localStorage.getItem(REFRESH_KEY),
  set(access: string, refresh: string) {
    localStorage.setItem(ACCESS_KEY, access);
    localStorage.setItem(REFRESH_KEY, refresh);
  },
  clear() {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};
