import { useEffect, useState } from "react";
import { Dialog } from "../../components/ui/Dialog";
import { TextField } from "../../components/ui/TextField";
import { Button } from "../../components/ui/Button";
import { apiErrorMessage } from "../../services/api";
import type { CreateGoalInput, DeckSummary } from "../../types";

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
      setServerError("Daily target must be at least 1.");
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
      description="Set a daily review target. Hit it each day to build a streak."
    >
      <form onSubmit={submit} className="space-y-4">
        <TextField label="Name (optional)" value={label} onChange={(e) => setLabel(e.target.value)} placeholder="Finish Spanish basics" />
        <TextField
          label="Daily review target"
          type="number"
          min={1}
          value={target}
          onChange={(e) => setTarget(Number(e.target.value))}
        />

        <div>
          <span className="mb-1.5 block text-sm font-medium text-gray-700">Decks (optional)</span>
          {decks.length === 0 ? (
            <p className="text-sm text-text-muted">No decks yet — the goal will count reviews across all decks.</p>
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
          <p className="mt-1 text-xs text-text-muted">Leave empty to count reviews across all decks.</p>
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
