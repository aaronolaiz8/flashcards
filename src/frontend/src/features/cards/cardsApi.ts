import { api } from "../../services/api";
import type { Card } from "../../types";

export interface CardInput {
  front: string;
  back: string;
  tags?: string[];
}

export const cardsApi = {
  async list(deckId: number): Promise<Card[]> {
    const { data } = await api.get<Card[]>(`/decks/${deckId}/cards`);
    return data;
  },
  async create(deckId: number, input: CardInput): Promise<Card> {
    const { data } = await api.post<Card>(`/decks/${deckId}/cards`, input);
    return data;
  },
  async update(deckId: number, cardId: number, input: CardInput): Promise<Card> {
    const { data } = await api.put<Card>(`/decks/${deckId}/cards/${cardId}`, input);
    return data;
  },
  async remove(deckId: number, cardId: number): Promise<void> {
    await api.delete(`/decks/${deckId}/cards/${cardId}`);
  },
  async bulkCreate(deckId: number, cards: CardInput[]): Promise<Card[]> {
    const { data } = await api.post<Card[]>(`/decks/${deckId}/cards/bulk`, cards);
    return data;
  },
};
