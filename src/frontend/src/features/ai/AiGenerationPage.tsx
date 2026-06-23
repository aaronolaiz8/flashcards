import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { Sparkles, ArrowRight, Trash2, Plus, ExternalLink, Wand2, Layers } from "lucide-react";
import { Button } from "../../components/ui/Button";
import { apiErrorMessage } from "../../services/api";
import { decksApi } from "../decks/decksApi";
import { cardsApi } from "../cards/cardsApi";
import { aiApi, AI_PROVIDERS } from "./aiApi";
import type { AiSettings, DeckSummary, GeneratedCard } from "../../types";

const inputClass =
  "w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none transition-colors focus:border-brand-500 focus:ring-2 focus:ring-brand-100";

export function AiGenerationPage() {
  const [params] = useSearchParams();
  const [settings, setSettings] = useState<AiSettings | null>(null);
  const [decks, setDecks] = useState<DeckSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [deckId, setDeckId] = useState<number | null>(null);
  const [source, setSource] = useState("");
  const [count, setCount] = useState(10);

  const [generating, setGenerating] = useState(false);
  const [cards, setCards] = useState<GeneratedCard[]>([]);
  const [adding, setAdding] = useState(false);
  const [added, setAdded] = useState<{ count: number; deckId: number; deckTitle: string } | null>(null);

  useEffect(() => {
    (async () => {
      try {
        const [s, d] = await Promise.all([aiApi.getSettings(), decksApi.list()]);
        setSettings(s);
        setDecks(d);
        const preset = Number(params.get("deckId"));
        setDeckId(Number.isFinite(preset) && preset > 0 ? preset : (d[0]?.id ?? null));
      } catch (err) {
        setError(apiErrorMessage(err, "Could not load AI generation"));
      } finally {
        setLoading(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function generate(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setAdded(null);
    if (!source.trim()) {
      setError("Enter a topic or paste some text to generate from.");
      return;
    }
    setGenerating(true);
    setCards([]);
    try {
      const result = await aiApi.generate({ text: source.trim(), count, deckId });
      setCards(result);
      if (result.length === 0) setError("No cards came back. Try a more specific topic.");
    } catch (err) {
      setError(apiErrorMessage(err, "Generation failed"));
    } finally {
      setGenerating(false);
    }
  }

  function editCard(i: number, field: "front" | "back", value: string) {
    setCards((prev) => prev.map((c, idx) => (idx === i ? { ...c, [field]: value } : c)));
  }

  function removeCard(i: number) {
    setCards((prev) => prev.filter((_, idx) => idx !== i));
  }

  async function addToDeck() {
    if (!deckId) {
      setError("Pick a deck to add the cards to.");
      return;
    }
    const valid = cards.filter((c) => c.front.trim() && c.back.trim());
    if (valid.length === 0) {
      setError("No cards to add.");
      return;
    }
    setAdding(true);
    setError(null);
    try {
      await cardsApi.bulkCreate(
        deckId,
        valid.map((c) => ({ front: c.front.trim(), back: c.back.trim() })),
      );
      const deck = decks.find((d) => d.id === deckId);
      setAdded({ count: valid.length, deckId, deckTitle: deck?.title ?? "deck" });
      setCards([]);
      setSource("");
    } catch (err) {
      setError(apiErrorMessage(err, "Could not add cards"));
    } finally {
      setAdding(false);
    }
  }

  if (loading) return <p className="text-sm text-text-muted">Loading…</p>;

  if (!settings?.isConfigured) return <SetupNeeded />;

  if (decks.length === 0) {
    return (
      <EmptyCard
        title="Create a deck first"
        body="AI generation adds cards to one of your decks. Make a deck, then come back to generate cards for it."
        action={
          <Link to="/decks">
            <Button className="gap-2">
              <Layers className="h-4 w-4" /> Go to Decks
            </Button>
          </Link>
        }
      />
    );
  }

  return (
    <div className="max-w-3xl space-y-6">
      <form onSubmit={generate} className="space-y-4 rounded-xl bg-surface-card p-6 shadow-sm">
        <div className="flex items-center gap-2">
          <Sparkles className="h-5 w-5 text-brand-600" />
          <h2 className="text-lg font-semibold text-text-heading">Generate cards</h2>
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <label className="block sm:col-span-2">
            <span className="mb-1.5 block text-sm font-medium text-gray-700">Add to deck</span>
            <select
              value={deckId ?? ""}
              onChange={(e) => setDeckId(Number(e.target.value))}
              className={inputClass}
            >
              {decks.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.title}
                </option>
              ))}
            </select>
          </label>
          <label className="block">
            <span className="mb-1.5 block text-sm font-medium text-gray-700">Number of cards</span>
            <input
              type="number"
              min={5}
              max={50}
              value={count}
              onChange={(e) => setCount(Math.max(5, Math.min(50, Number(e.target.value) || 5)))}
              className={inputClass}
            />
          </label>
        </div>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-gray-700">Topic or source text</span>
          <textarea
            value={source}
            onChange={(e) => setSource(e.target.value)}
            rows={5}
            placeholder="e.g. The HTTP status code families, or paste your notes here…"
            className={inputClass}
          />
        </label>

        <div className="flex items-center justify-between">
          <span className="text-xs text-text-muted">
            Using {settings.provider}
            {settings.model ? ` · ${settings.model}` : ""}
          </span>
          <Button type="submit" disabled={generating} className="gap-2">
            <Wand2 className="h-4 w-4" />
            {generating ? "Generating…" : "Generate"}
          </Button>
        </div>
      </form>

      {error && <p className="rounded-lg bg-danger-100 px-4 py-3 text-sm text-danger-600">{error}</p>}

      {added && (
        <div className="flex items-center justify-between rounded-lg bg-success-100 px-4 py-3 text-sm text-success-700">
          <span>Added {added.count} cards to {added.deckTitle}.</span>
          <Link to={`/decks/${added.deckId}`} className="inline-flex items-center gap-1 font-medium hover:underline">
            View deck <ArrowRight className="h-4 w-4" />
          </Link>
        </div>
      )}

      {cards.length > 0 && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="font-semibold text-text-heading">
              Review {cards.length} card{cards.length === 1 ? "" : "s"}
            </h3>
            <div className="flex gap-2">
              <Button variant="secondary" onClick={() => setCards([])} disabled={adding}>
                Discard all
              </Button>
              <Button onClick={addToDeck} disabled={adding} className="gap-2">
                <Plus className="h-4 w-4" />
                {adding ? "Adding…" : `Add ${cards.length} to deck`}
              </Button>
            </div>
          </div>
          <p className="text-sm text-text-muted">Edit anything before saving, or remove cards you don't want.</p>

          <div className="space-y-3">
            {cards.map((card, i) => (
              <div key={i} className="rounded-xl bg-surface-card p-4 shadow-sm">
                <div className="flex items-start gap-3">
                  <div className="grid flex-1 gap-2 sm:grid-cols-2">
                    <textarea
                      value={card.front}
                      onChange={(e) => editCard(i, "front", e.target.value)}
                      rows={2}
                      className={inputClass}
                      aria-label={`Card ${i + 1} front`}
                    />
                    <textarea
                      value={card.back}
                      onChange={(e) => editCard(i, "back", e.target.value)}
                      rows={2}
                      className={inputClass}
                      aria-label={`Card ${i + 1} back`}
                    />
                  </div>
                  <button
                    type="button"
                    onClick={() => removeCard(i)}
                    className="mt-1 rounded-md p-2 text-text-muted hover:bg-danger-100 hover:text-danger-600"
                    aria-label="Remove card"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

function SetupNeeded() {
  return (
    <div className="max-w-2xl space-y-6">
      <div className="rounded-xl bg-surface-card p-6 shadow-sm">
        <div className="mb-2 flex items-center gap-2">
          <Sparkles className="h-5 w-5 text-brand-600" />
          <h2 className="text-lg font-semibold text-text-heading">Generate cards with AI</h2>
        </div>
        <p className="mb-5 text-sm text-text-muted">
          Turn a topic or your notes into a set of flashcards. Retainica uses{" "}
          <span className="font-medium">your own</span> OpenAI or Anthropic API key, so you only ever
          pay your provider directly — there's no subscription here. Set it up once:
        </p>

        <ol className="mb-6 space-y-4">
          <li className="flex gap-3">
            <Step n={1} />
            <div className="text-sm text-gray-700">
              <p className="font-medium text-text-heading">Get an API key</p>
              <p className="text-text-muted">Create one from your provider:</p>
              <div className="mt-1 flex flex-wrap gap-x-4 gap-y-1">
                {AI_PROVIDERS.map((p) => (
                  <a
                    key={p.value}
                    href={p.keysUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-1 text-brand-600 hover:underline"
                  >
                    {p.label} <ExternalLink className="h-3 w-3" />
                  </a>
                ))}
              </div>
            </div>
          </li>
          <li className="flex gap-3">
            <Step n={2} />
            <div className="text-sm text-gray-700">
              <p className="font-medium text-text-heading">Paste it into Settings</p>
              <p className="text-text-muted">
                Open Settings → AI, choose your provider and model, and save the key. It's validated
                and encrypted at rest.
              </p>
            </div>
          </li>
          <li className="flex gap-3">
            <Step n={3} />
            <div className="text-sm text-gray-700">
              <p className="font-medium text-text-heading">Come back and generate</p>
              <p className="text-text-muted">Pick a deck, enter a topic, and review the cards before saving.</p>
            </div>
          </li>
        </ol>

        <Link to="/settings">
          <Button className="gap-2">
            Go to Settings <ArrowRight className="h-4 w-4" />
          </Button>
        </Link>
      </div>
    </div>
  );
}

function Step({ n }: { n: number }) {
  return (
    <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-brand-100 text-sm font-semibold text-brand-700">
      {n}
    </span>
  );
}

function EmptyCard({ title, body, action }: { title: string; body: string; action: React.ReactNode }) {
  return (
    <div className="flex max-w-2xl flex-col items-center justify-center rounded-xl bg-surface-card py-16 text-center shadow-sm">
      <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-brand-100 text-brand-600">
        <Sparkles className="h-6 w-6" />
      </div>
      <p className="font-medium text-text-heading">{title}</p>
      <p className="mb-4 mt-1 max-w-sm px-4 text-sm text-text-muted">{body}</p>
      {action}
    </div>
  );
}
