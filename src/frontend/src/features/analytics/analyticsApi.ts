import { api } from "../../services/api";
import type { DashboardOverview } from "../../types";

export const analyticsApi = {
  async overview(): Promise<DashboardOverview> {
    const { data } = await api.get<DashboardOverview>("/analytics/overview");
    return data;
  },
};
