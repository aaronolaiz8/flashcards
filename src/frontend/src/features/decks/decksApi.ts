import { api } from "../../services/api";
import type { DeckDetail, DeckSummary } from "../../types";

export interface DeckInput {
  title: string;
  description?: string | null;
  tags?: string[];
  visibility?: string;
}

export const decksApi = {
  async list(): Promise<DeckSummary[]> {
    const { data } = await api.get<DeckSummary[]>("/decks");
    return data;
  },
  async get(id: number): Promise<DeckDetail> {
    const { data } = await api.get<DeckDetail>(`/decks/${id}`);
    return data;
  },
  async create(input: DeckInput): Promise<DeckDetail> {
    const { data } = await api.post<DeckDetail>("/decks", input);
    return data;
  },
  async update(id: number, input: DeckInput): Promise<DeckDetail> {
    const { data } = await api.put<DeckDetail>(`/decks/${id}`, input);
    return data;
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/decks/${id}`);
  },
};
