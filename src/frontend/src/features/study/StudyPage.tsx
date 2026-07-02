import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { GraduationCap, RotateCcw, Check, Pencil } from "lucide-react";
import { Button } from "../../components/ui/Button";
import { Dialog } from "../../components/ui/Dialog";
import { ConfirmDialog } from "../../components/ui/ConfirmDialog";
import { apiErrorMessage } from "../../services/api";
import { decksApi } from "../decks/decksApi";
import { cardsApi } from "../cards/cardsApi";
import { analyticsApi } from "../analytics/analyticsApi";
import type { DeckSummary } from "../../types";
import { studyApi, type Session, type SessionCard } from "./studyApi";

type Phase = "picker" | "running" | "done";

const cardCount = (n: number) => `${n} card${n === 1 ? "" : "s"}`;

const ratings = [
  { value: 1, label: "Again", className: "bg-danger-500 hover:bg-danger-600 text-white" },
  { value: 2, label: "Hard", className: "bg-warning-500 hover:bg-warning-600 text-white" },
  { value: 3, label: "Good", className: "bg-info-500 hover:bg-info-600 text-white" },
  { value: 4, label: "Easy", className: "bg-success-500 hover:bg-success-600 text-white" },
];

export function StudyPage() {
  const [searchParams] = useSearchParams();
  const presetDeckId = searchParams.get("deckId");

  const [phase, setPhase] = useState<Phase>("picker");
  const [decks, setDecks] = useState<DeckSummary[]>([]);
  const [deckId, setDeckId] = useState<number | null>(presetDeckId ? Number(presetDeckId) : null);
  const [mode, setMode] = useState<"Spaced" | "Free">("Spaced");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const [session, setSession] = useState<Session | null>(null);
  const [queue, setQueue] = useState<SessionCard[]>([]);
  const [index, setIndex] = useState(0);
  const [revealed, setRevealed] = useState(false);
  const [studied, setStudied] = useState(0);
  const [cardStart, setCardStart] = useState<number>(Date.now());
  const [nextDeck, setNextDeck] = useState<{ id: number; title: string; due: number } | null>(null);
  const [currentDeckDue, setCurrentDeckDue] = useState<number | null>(null);

  // Inline card editing during a session: "warn" shows the one-time confirmation,
  // "form" lets the user edit the front/back text, then study resumes unchanged.
  const [editStep, setEditStep] = useState<"none" | "warn" | "form">("none");
  const [editFront, setEditFront] = useState("");
  const [editBack, setEditBack] = useState("");
  const [editBusy, setEditBusy] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  useEffect(() => {
    decksApi
      .list()
      .then((d) => {
        setDecks(d);
        if (deckId === null && d.length > 0) setDeckId(d[0].id);
      })
      .catch((err) => setError(apiErrorMessage(err, "Could not load decks")));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function startSession(targetDeckId: number | null = deckId) {
    if (targetDeckId === null) return;
    setDeckId(targetDeckId);
    setBusy(true);
    setError(null);
    try {
      const s = await studyApi.start({ deckId: targetDeckId, mode });
      setSession(s);
      setQueue(s.cards);
      setIndex(0);
      setRevealed(false);
      setStudied(0);
      setCardStart(Date.now());
      if (s.cards.length === 0) {
        finishTo("done");
      } else {
        setPhase("running");
      }
    } catch (err) {
      setError(apiErrorMessage(err, "Could not start session"));
    } finally {
      setBusy(false);
    }
  }

  async function rate(rating: number) {
    if (!session) return;
    const card = queue[index];
    setBusy(true);
    try {
      await studyApi.review(session.id, card.cardId, rating, Date.now() - cardStart);
      setStudied((n) => n + 1);
      advance();
    } catch (err) {
      setError(apiErrorMessage(err, "Could not submit review"));
    } finally {
      setBusy(false);
    }
  }

  function advance() {
    const next = index + 1;
    if (next >= queue.length) {
      void finish();
    } else {
      setIndex(next);
      setRevealed(false);
      setCardStart(Date.now());
    }
  }

  async function finish() {
    if (session) {
      try {
        await studyApi.end(session.id);
      } catch {
        // best-effort
      }
    }
    finishTo("done");
  }

  // Enter the completion screen and compute the next deck to study using the same
  // logic the Dashboard's "Cards Due Today" uses: the deck with due cards that was
  // studied longest ago (null when nothing is due anywhere).
  function finishTo(target: "done") {
    setPhase(target);
    setNextDeck(null);
    setCurrentDeckDue(null);
    analyticsApi
      .overview()
      .then((o) => {
        const dueFor = (id: number | null) =>
          o.decksDue.find((d) => d.deckId === id)?.cardsDue ?? 0;
        setNextDeck(
          o.nextDeckId
            ? { id: o.nextDeckId, title: o.nextDeckTitle ?? "deck", due: dueFor(o.nextDeckId) }
            : null,
        );
        setCurrentDeckDue(dueFor(deckId));
      })
      .catch(() => {
        setNextDeck(null);
        setCurrentDeckDue(null);
      });
  }

  function restart() {
    setSession(null);
    setQueue([]);
    setPhase("picker");
  }

  function beginEdit() {
    const card = queue[index];
    if (!card) return;
    setEditFront(card.front);
    setEditBack(card.back);
    setEditError(null);
    setEditStep("form");
  }

  async function saveEdit() {
    const card = queue[index];
    if (!card || deckId === null) return;
    const front = editFront.trim();
    const back = editBack.trim();
    if (!front || !back) {
      setEditError("Front and back can't be empty.");
      return;
    }
    setEditBusy(true);
    setEditError(null);
    try {
      // Omit tags so the card's existing tags are preserved (backend only
      // overwrites tags when they're sent).
      await cardsApi.update(deckId, card.cardId, { front, back });
      setQueue((q) =>
        q.map((c) => (c.cardId === card.cardId ? { ...c, front, back } : c)),
      );
      setEditStep("none");
    } catch (err) {
      setEditError(apiErrorMessage(err, "Could not save card"));
    } finally {
      setEditBusy(false);
    }
  }

  if (phase === "picker") {
    return (
      <div className="mx-auto max-w-md space-y-6">
        <div className="rounded-2xl bg-surface-card p-6 shadow-sm">
          <div className="mb-4 flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-brand-100 text-brand-600">
              <GraduationCap className="h-5 w-5" />
            </div>
            <h2 className="text-lg font-semibold text-text-heading">Start studying</h2>
          </div>

          {error && <p className="mb-4 rounded-lg bg-danger-100 px-4 py-3 text-sm text-danger-600">{error}</p>}

          <label className="mb-4 block">
            <span className="mb-1.5 block text-sm font-medium text-gray-700">Deck</span>
            <select
              value={deckId ?? ""}
              onChange={(e) => setDeckId(Number(e.target.value))}
              className="w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
            >
              {decks.length === 0 && <option value="">No decks yet</option>}
              {decks.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.title} ({d.cardCount} cards)
                </option>
              ))}
            </select>
          </label>

          <div className="mb-6">
            <span className="mb-1.5 block text-sm font-medium text-gray-700">Mode</span>
            <div className="flex gap-2">
              {(["Spaced", "Free"] as const).map((m) => (
                <button
                  key={m}
                  onClick={() => setMode(m)}
                  className={`flex-1 rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
                    mode === m
                      ? "border-brand-500 bg-brand-50 text-brand-700"
                      : "border-border-soft text-gray-600 hover:bg-surface"
                  }`}
                >
                  {m === "Spaced" ? "Spaced (FSRS)" : "Free study"}
                </button>
              ))}
            </div>
          </div>

          <Button onClick={() => startSession()} className="w-full" disabled={busy || deckId === null}>
            {busy ? "Starting..." : "Start session"}
          </Button>
        </div>
      </div>
    );
  }

  if (phase === "done") {
    return (
      <div className="mx-auto max-w-md">
        <div className="rounded-2xl bg-surface-card p-8 text-center shadow-sm">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-success-100 text-success-600">
            <Check className="h-7 w-7" />
          </div>
          <h2 className="text-lg font-semibold text-text-heading">Session complete</h2>
          <p className="mb-6 mt-1 text-sm text-text-muted">
            {studied === 0 ? "No cards were due. Nice and caught up!" : `You reviewed ${studied} card${studied === 1 ? "" : "s"}.`}
          </p>
          {error && <p className="mb-4 rounded-lg bg-danger-100 px-4 py-3 text-sm text-danger-600">{error}</p>}
          <div className="flex flex-col items-center gap-2">
            {nextDeck && (
              <Button onClick={() => startSession(nextDeck.id)} className="w-full gap-2" disabled={busy}>
                <GraduationCap className="h-4 w-4" />
                {busy ? "Starting..." : `Next Deck: ${nextDeck.title} : ${cardCount(nextDeck.due)}`}
              </Button>
            )}
            <Button onClick={restart} variant="secondary" className="w-full gap-2" disabled={busy}>
              <RotateCcw className="h-4 w-4" /> Study again
              {currentDeckDue !== null && ` : ${cardCount(currentDeckDue)}`}
            </Button>
          </div>
        </div>
      </div>
    );
  }

  // running
  const card = queue[index];
  return (
    <div className="mx-auto max-w-xl space-y-4">
      <div className="flex items-center justify-between text-sm text-text-muted">
        <span>
          Card {index + 1} of {queue.length}
        </span>
        <button onClick={finish} className="hover:text-brand-600">
          End session
        </button>
      </div>

      <div className="h-1.5 w-full overflow-hidden rounded-full bg-border-soft">
        <div
          className="h-full rounded-full bg-brand-500 transition-all"
          style={{ width: `${(index / queue.length) * 100}%` }}
        />
      </div>

      <div className="min-h-56 rounded-2xl bg-surface-card p-8 shadow-sm">
        <p className="text-center text-xl font-medium text-text-heading">{card.front}</p>
        {revealed && (
          <>
            <div className="my-6 border-t border-border-soft" />
            <p className="text-center text-lg text-gray-700">{card.back}</p>
          </>
        )}
      </div>

      {error && <p className="rounded-lg bg-danger-100 px-4 py-3 text-sm text-danger-600">{error}</p>}

      {!revealed ? (
        <Button onClick={() => setRevealed(true)} className="w-full">
          Show answer
        </Button>
      ) : mode === "Free" ? (
        <Button onClick={() => { setStudied((n) => n + 1); advance(); }} className="w-full">
          Next
        </Button>
      ) : (
        <div className="grid grid-cols-4 gap-2">
          {ratings.map((r) => (
            <button
              key={r.value}
              onClick={() => rate(r.value)}
              disabled={busy}
              className={`rounded-lg px-3 py-3 text-sm font-medium transition-colors disabled:opacity-60 ${r.className}`}
            >
              {r.label}
            </button>
          ))}
        </div>
      )}

      <div className="text-right">
        <button
          type="button"
          onClick={() => setEditStep("warn")}
          className="inline-flex items-center gap-1 text-xs text-text-muted/70 hover:text-text-muted"
        >
          <Pencil className="h-3 w-3" /> Edit card
        </button>
      </div>

      <ConfirmDialog
        open={editStep === "warn"}
        onOpenChange={(o) => setEditStep(o ? "warn" : "none")}
        title="Edit this card?"
        message="This permanently changes the card and can't be undone."
        confirmLabel="Continue"
        onConfirm={beginEdit}
      />

      <Dialog
        open={editStep === "form"}
        onOpenChange={(o) => setEditStep(o ? "form" : "none")}
        title="Edit card"
      >
        <div className="space-y-4">
          <label className="block">
            <span className="mb-1.5 block text-sm font-medium text-gray-700">Front</span>
            <textarea
              value={editFront}
              onChange={(e) => setEditFront(e.target.value)}
              rows={3}
              className="w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
            />
          </label>
          <label className="block">
            <span className="mb-1.5 block text-sm font-medium text-gray-700">Back</span>
            <textarea
              value={editBack}
              onChange={(e) => setEditBack(e.target.value)}
              rows={3}
              className="w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
            />
          </label>

          {editError && <p className="text-sm text-danger-600">{editError}</p>}

          <div className="flex justify-end gap-3 pt-2">
            <Button type="button" variant="secondary" onClick={() => setEditStep("none")} disabled={editBusy}>
              Cancel
            </Button>
            <Button type="button" onClick={saveEdit} disabled={editBusy}>
              {editBusy ? "Saving..." : "Save changes"}
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  );
}
