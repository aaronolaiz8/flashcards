import { api } from "../../services/api";
import type { CreateGoalInput, Goal } from "../../types";

export const goalsApi = {
  async list(): Promise<Goal[]> {
    const { data } = await api.get<Goal[]>("/goals");
    return data;
  },
  async create(input: CreateGoalInput): Promise<Goal> {
    const { data } = await api.post<Goal>("/goals", input);
    return data;
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/goals/${id}`);
  },
};
