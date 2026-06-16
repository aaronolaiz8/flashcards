export interface User {
  id: number;
  email: string;
  displayName: string;
  role: string;
  emailVerified: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface DeckSummary {
  id: number;
  title: string;
  description: string | null;
  tags: string[];
  visibility: string;
  cardCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface DeckDetail extends DeckSummary {
  forkedFromDeckId: number | null;
}

export interface Card {
  id: number;
  deckId: number;
  front: string;
  back: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}
