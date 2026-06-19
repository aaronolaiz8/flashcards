import { useEffect, useState } from "react";
import { Plus, Target, Flame, Trash2 } from "lucide-react";
import { Button } from "../../components/ui/Button";
import { ConfirmDialog } from "../../components/ui/ConfirmDialog";
import { apiErrorMessage } from "../../services/api";
import { decksApi } from "../decks/decksApi";
import type { CreateGoalInput, DeckSummary, Goal } from "../../types";
import { goalsApi } from "./goalsApi";
import { GoalFormDialog } from "./GoalFormDialog";

export function GoalsPage() {
  const [goals, setGoals] = useState<Goal[]>([]);
  const [decks, setDecks] = useState<DeckSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [deleting, setDeleting] = useState<Goal | null>(null);
  const [deleteBusy, setDeleteBusy] = useState(false);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [g, d] = await Promise.all([goalsApi.list(), decksApi.list()]);
      setGoals(g);
      setDecks(d);
    } catch (err) {
      setError(apiErrorMessage(err, "Could not load goals"));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function handleCreate(input: CreateGoalInput) {
    await goalsApi.create(input);
    await load();
  }

  async function confirmDelete() {
    if (!deleting) return;
    setDeleteBusy(true);
    try {
      await goalsApi.remove(deleting.id);
      setDeleting(null);
      await load();
    } catch (err) {
      setError(apiErrorMessage(err, "Could not delete goal"));
    } finally {
      setDeleteBusy(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <p className="text-sm text-text-muted">
          {goals.length} {goals.length === 1 ? "goal" : "goals"}
        </p>
        <Button onClick={() => setFormOpen(true)} className="gap-2">
          <Plus className="h-4 w-4" /> New goal
        </Button>
      </div>

      {error && <p className="rounded-lg bg-danger-100 px-4 py-3 text-sm text-danger-600">{error}</p>}

      {loading ? (
        <p className="text-sm text-text-muted">Loading goals…</p>
      ) : goals.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-xl bg-surface-card py-16 text-center shadow-sm">
          <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-success-100 text-success-600">
            <Target className="h-6 w-6" />
          </div>
          <p className="font-medium text-text-heading">No goals yet</p>
          <p className="mb-4 mt-1 text-sm text-text-muted">Set a daily review target to build a study streak.</p>
          <Button onClick={() => setFormOpen(true)} className="gap-2">
            <Plus className="h-4 w-4" /> New goal
          </Button>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {goals.map((goal) => {
            const pct = Math.round(goal.progressPct);
            return (
              <div key={goal.id} className="rounded-xl bg-surface-card p-5 shadow-sm">
                <div className="flex items-start justify-between">
                  <div className="min-w-0">
                    <p className="truncate font-medium text-text-heading">{goal.label ?? "Daily review goal"}</p>
                    <p className="mt-0.5 text-xs text-text-muted">
                      {goal.decks.length === 0
                        ? "All decks"
                        : goal.decks.map((d) => d.deckTitle).join(", ")}
                    </p>
                  </div>
                  <button
                    onClick={() => setDeleting(goal)}
                    className="rounded-md p-2 text-text-muted hover:bg-danger-100 hover:text-danger-600"
                    aria-label="Delete goal"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>

                <div className="mt-4 flex items-center justify-between text-sm">
                  <span className="text-text-muted">
                    <span className="font-semibold text-text-heading">{goal.reviewsToday}</span> / {goal.dailyReviewBudget} today
                  </span>
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-warning-100 px-2.5 py-1 text-xs font-medium text-warning-600">
                    <Flame className="h-3.5 w-3.5" /> {goal.currentStreak}-day streak
                  </span>
                </div>

                <div className="mt-2 h-2.5 w-full overflow-hidden rounded-full bg-surface">
                  <div className="h-full rounded-full bg-brand-600 transition-all" style={{ width: `${pct}%` }} />
                </div>
              </div>
            );
          })}
        </div>
      )}

      <GoalFormDialog open={formOpen} onOpenChange={setFormOpen} decks={decks} onSubmit={handleCreate} />

      <ConfirmDialog
        open={Boolean(deleting)}
        onOpenChange={(o) => !o && setDeleting(null)}
        title="Delete goal"
        message={`Delete "${deleting?.label ?? "this goal"}"? This cannot be undone.`}
        confirmLabel="Delete"
        busy={deleteBusy}
        onConfirm={confirmDelete}
      />
    </div>
  );
}
