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

// --- Dashboard overview ---

export interface DueForecastPoint {
  date: string;
  count: number;
}

export interface DeckUsagePoint {
  deckId: number;
  title: string;
  reviews: number;
}

export interface DailyReviewsPoint {
  date: string;
  count: number;
}

export interface DashboardGoal {
  id: number;
  label: string | null;
  dailyReviewTarget: number;
  reviewsToday: number;
  currentStreak: number;
  progressPct: number;
}

export interface DashboardOverview {
  totalDecks: number;
  totalCards: number;
  cardsDueToday: number;
  reviewsToday: number;
  currentStreak: number;
  longestStreak: number;
  activeGoals: number;
  nextDeckId: number | null;
  nextDeckTitle: string | null;
  dueForecast: DueForecastPoint[];
  deckUsage: DeckUsagePoint[];
  reviewsByDay: DailyReviewsPoint[];
  goals: DashboardGoal[];
  isEmpty: boolean;
}

// --- Goals ---

export interface GoalDeckRef {
  deckId: number;
  deckTitle: string;
  cardCount: number;
}

export interface Goal {
  id: number;
  label: string | null;
  deadlineDate: string;
  masteryThresholdPct: number;
  recallTargetPct: number;
  dailyNewCardBudget: number;
  dailyReviewBudget: number;
  status: string;
  decks: GoalDeckRef[];
  createdAt: string;
  reviewsToday: number;
  currentStreak: number;
  progressPct: number;
}

export interface CreateGoalInput {
  label?: string | null;
  dailyReviewTarget: number;
  deckIds?: number[];
  deadlineDate?: string | null;
}

// --- AI generation ---

export interface AiSettings {
  provider: string | null; // "Anthropic" | "OpenAI" | null
  model: string | null;
  isConfigured: boolean;
}

export interface GeneratedCard {
  front: string;
  back: string;
}
