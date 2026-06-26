import { api } from "../../services/api";

export const authApi = {
  async forgotPassword(email: string): Promise<void> {
    await api.post("/auth/forgot-password", { email });
  },

  async resetPassword(token: string, newPassword: string): Promise<void> {
    await api.post("/auth/reset-password", { token, newPassword });
  },
};
