import { Routes, Route, Navigate } from "react-router-dom";
import { DashboardLayout } from "./components/layout/DashboardLayout";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { LoginPage } from "./features/auth/LoginPage";
import { RegisterPage } from "./features/auth/RegisterPage";
import { ForgotPasswordPage } from "./features/auth/ForgotPasswordPage";
import { ResetPasswordPage } from "./features/auth/ResetPasswordPage";
import { DashboardPage } from "./features/analytics/DashboardPage";
import { DecksPage } from "./features/decks/DecksPage";
import { DeckCardsPage } from "./features/cards/DeckCardsPage";
import { StudyPage } from "./features/study/StudyPage";
import { GoalsPage } from "./features/goals/GoalsPage";
import { RemindersPage } from "./features/reminders/RemindersPage";
import { AnalyticsPage } from "./features/analytics/AnalyticsPage";
import { AiGenerationPage } from "./features/ai/AiGenerationPage";
import { SettingsPage } from "./features/auth/SettingsPage";

const titles: Record<string, string> = {
  "/": "Dashboard",
  "/decks": "Decks",
  "/study": "Study",
  "/goals": "Goals",
  "/reminders": "Reminders",
  "/analytics": "Analytics",
  "/ai": "AI Generation",
  "/settings": "Settings",
};

function withLayout(path: string, page: React.ReactNode) {
  return <DashboardLayout title={titles[path]}>{page}</DashboardLayout>;
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />

      <Route element={<ProtectedRoute />}>
        <Route path="/" element={withLayout("/", <DashboardPage />)} />
        <Route path="/decks" element={withLayout("/decks", <DecksPage />)} />
        <Route path="/decks/:deckId" element={withLayout("/decks", <DeckCardsPage />)} />
        <Route path="/study" element={withLayout("/study", <StudyPage />)} />
        <Route path="/goals" element={withLayout("/goals", <GoalsPage />)} />
        <Route path="/reminders" element={withLayout("/reminders", <RemindersPage />)} />
        <Route path="/analytics" element={withLayout("/analytics", <AnalyticsPage />)} />
        <Route path="/ai" element={withLayout("/ai", <AiGenerationPage />)} />
        <Route path="/settings" element={withLayout("/settings", <SettingsPage />)} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
