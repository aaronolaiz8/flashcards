import { useEffect, useState } from "react";
import { Dialog } from "../../components/ui/Dialog";
import { TextField } from "../../components/ui/TextField";
import { Button } from "../../components/ui/Button";
import { cn } from "../../lib/cn";
import { apiErrorMessage } from "../../services/api";
import type { CreateGoalInput, DeckSummary } from "../../types";

const PRESETS = [10, 20, 30];

export function GoalFormDialog({
  open,
  onOpenChange,
  decks,
  onSubmit,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  decks: DeckSummary[];
  onSubmit: (input: CreateGoalInput) => Promise<void>;
}) {
  const [label, setLabel] = useState("");
  const [target, setTarget] = useState(20);
  const [deckIds, setDeckIds] = useState<number[]>([]);
  const [serverError, setServerError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (open) {
      setLabel("");
      setTarget(20);
      setDeckIds([]);
      setServerError(null);
      setBusy(false);
    }
  }, [open]);

  function toggleDeck(id: number) {
    setDeckIds((prev) => (prev.includes(id) ? prev.filter((d) => d !== id) : [...prev, id]));
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    if (target < 1) {
      setServerError("Enter at least 1 card.");
      return;
    }
    setBusy(true);
    setServerError(null);
    try {
      await onSubmit({
        label: label.trim() || null,
        dailyReviewTarget: target,
        deckIds: deckIds.length > 0 ? deckIds : undefined,
      });
      onOpenChange(false);
    } catch (err) {
      setServerError(apiErrorMessage(err, "Could not create goal"));
    } finally {
      setBusy(false);
    }
  }

  return (
    <Dialog
      open={open}
      onOpenChange={onOpenChange}
      title="New goal"
      description="Choose how many cards to study each day. Do it every day to build a streak."
    >
      <form onSubmit={submit} className="space-y-4">
        <TextField label="Name (optional)" value={label} onChange={(e) => setLabel(e.target.value)} placeholder="Finish Spanish basics" />

        <div>
          <span className="mb-1.5 block text-sm font-medium text-gray-700">
            How many cards do you want to study each day?
          </span>
          <div className="flex items-center gap-2">
            <input
              type="number"
              min={1}
              value={target}
              onChange={(e) => setTarget(Number(e.target.value))}
              className="w-20 rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none transition-colors focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
            />
            <span className="text-sm text-text-muted">cards / day</span>
          </div>
          <div className="mt-2 flex gap-2">
            {PRESETS.map((n) => (
              <button
                key={n}
                type="button"
                onClick={() => setTarget(n)}
                className={cn(
                  "rounded-full border px-3.5 py-1 text-sm transition-colors",
                  target === n
                    ? "border-brand-600 bg-brand-50 text-brand-700"
                    : "border-border-soft text-gray-600 hover:bg-surface",
                )}
              >
                {n}
              </button>
            ))}
          </div>
          <p className="mt-2 text-xs text-text-muted">Hit this number each day to keep your streak alive.</p>
        </div>

        <div>
          <span className="mb-1.5 block text-sm font-medium text-gray-700">Decks (optional)</span>
          {decks.length === 0 ? (
            <p className="text-sm text-text-muted">No decks yet — this goal will count cards from all your decks.</p>
          ) : (
            <div className="max-h-40 space-y-1 overflow-y-auto rounded-lg border border-border-soft p-2">
              {decks.map((d) => (
                <label key={d.id} className="flex cursor-pointer items-center gap-2 rounded-md px-2 py-1.5 text-sm hover:bg-surface">
                  <input
                    type="checkbox"
                    checked={deckIds.includes(d.id)}
                    onChange={() => toggleDeck(d.id)}
                    className="h-4 w-4 accent-brand-600"
                  />
                  <span className="truncate">{d.title}</span>
                </label>
              ))}
            </div>
          )}
          <p className="mt-1 text-xs text-text-muted">Leave empty to count cards from all your decks.</p>
        </div>

        {serverError && <p className="text-sm text-danger-600">{serverError}</p>}

        <div className="flex justify-end gap-3 pt-2">
          <Button type="button" variant="secondary" onClick={() => onOpenChange(false)} disabled={busy}>
            Cancel
          </Button>
          <Button type="submit" disabled={busy}>
            {busy ? "Creating…" : "Create goal"}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
