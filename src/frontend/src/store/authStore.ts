import { create } from "zustand";
import { persist } from "zustand/middleware";
import { api } from "../services/api";
import { tokenStore } from "../services/tokenStore";
import type { AuthResponse, User } from "../types";

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => Promise<void>;
}

function applyAuth(set: (partial: Partial<AuthState>) => void, res: AuthResponse) {
  tokenStore.set(res.accessToken, res.refreshToken);
  set({ user: res.user, isAuthenticated: true });
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,

      async login(email, password) {
        const { data } = await api.post<AuthResponse>("/auth/login", { email, password });
        applyAuth(set, data);
      },

      async register(email, password, displayName) {
        const { data } = await api.post<AuthResponse>("/auth/register", {
          email,
          password,
          displayName,
        });
        applyAuth(set, data);
      },

      async logout() {
        const refreshToken = tokenStore.getRefresh();
        try {
          if (refreshToken) {
            await api.post("/auth/logout", JSON.stringify(refreshToken), {
              headers: { "Content-Type": "application/json" },
            });
          }
        } catch {
          // Best-effort; clear local state regardless.
        }
        tokenStore.clear();
        set({ user: null, isAuthenticated: false });
      },
    }),
    {
      name: "fc.auth",
      // Only persist the user profile; tokens live in tokenStore.
      partialize: (state) => ({ user: state.user, isAuthenticated: state.isAuthenticated }),
    },
  ),
);
