import { api } from "../../services/api";

export interface SessionCard {
  cardId: number;
  front: string;
  back: string;
  state: string;
  nextReviewDate: string | null;
}

export interface Session {
  id: number;
  deckId: number;
  mode: string;
  startedAt: string;
  cards: SessionCard[];
  paceInfo: unknown | null;
}

export interface ReviewResult {
  cardId: number;
  newState: string;
  nextReviewDate: string | null;
  retrievability: number | null;
}

export interface StartSessionInput {
  deckId: number;
  mode: "Spaced" | "Free";
  newCardsLimit?: number;
  cardCountCap?: number;
  shuffle?: boolean;
}

export const studyApi = {
  async start(input: StartSessionInput): Promise<Session> {
    const { data } = await api.post<Session>("/study/sessions", input);
    return data;
  },
  async review(
    sessionId: number,
    cardId: number,
    rating: number,
    responseTimeMs?: number,
  ): Promise<ReviewResult> {
    const { data } = await api.post<ReviewResult>(`/study/sessions/${sessionId}/review`, {
      cardId,
      rating,
      responseTimeMs,
    });
    return data;
  },
  async end(sessionId: number): Promise<void> {
    await api.post(`/study/sessions/${sessionId}/end`, {});
  },
};
