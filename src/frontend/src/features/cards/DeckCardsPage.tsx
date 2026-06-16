import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Plus, Pencil, Trash2, ArrowLeft, Layers, GraduationCap } from "lucide-react";
import { Button } from "../../components/ui/Button";
import { ConfirmDialog } from "../../components/ui/ConfirmDialog";
import { apiErrorMessage } from "../../services/api";
import { decksApi } from "../decks/decksApi";
import type { Card, DeckDetail } from "../../types";
import { cardsApi, type CardInput } from "./cardsApi";
import { CardFormDialog } from "./CardFormDialog";

export function DeckCardsPage() {
  const { deckId } = useParams<{ deckId: string }>();
  const id = Number(deckId);

  const [deck, setDeck] = useState<DeckDetail | null>(null);
  const [cards, setCards] = useState<Card[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<Card | null>(null);
  const [deleting, setDeleting] = useState<Card | null>(null);
  const [deleteBusy, setDeleteBusy] = useState(false);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const [deckData, cardData] = await Promise.all([decksApi.get(id), cardsApi.list(id)]);
      setDeck(deckData);
      setCards(cardData);
    } catch (err) {
      setError(apiErrorMessage(err, "Could not load deck"));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (Number.isFinite(id)) void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  function openCreate() {
    setEditing(null);
    setFormOpen(true);
  }

  function openEdit(card: Card) {
    setEditing(card);
    setFormOpen(true);
  }

  async function handleSubmit(input: CardInput) {
    if (editing) {
      await cardsApi.update(id, editing.id, input);
    } else {
      await cardsApi.create(id, input);
    }
    await load();
  }

  async function confirmDelete() {
    if (!deleting) return;
    setDeleteBusy(true);
    try {
      await cardsApi.remove(id, deleting.id);
      setDeleting(null);
      await load();
    } catch (err) {
      setError(apiErrorMessage(err, "Could not delete card"));
    } finally {
      setDeleteBusy(false);
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <Link to="/decks" className="inline-flex items-center gap-1 text-sm text-text-muted hover:text-brand-600">
          <ArrowLeft className="h-4 w-4" /> Back to decks
        </Link>
        <div className="mt-2 flex items-center justify-between">
          <div>
            <h2 className="text-xl font-semibold text-text-heading">{deck?.title ?? "Deck"}</h2>
            {deck?.description && <p className="text-sm text-text-muted">{deck.description}</p>}
          </div>
          <div className="flex gap-2">
            {cards.length > 0 && (
              <Link to={`/study?deckId=${id}`}>
                <Button variant="secondary" className="gap-2">
                  <GraduationCap className="h-4 w-4" /> Study
                </Button>
              </Link>
            )}
            <Button onClick={openCreate} className="gap-2">
              <Plus className="h-4 w-4" /> Add card
            </Button>
          </div>
        </div>
      </div>

      {error && <p className="rounded-lg bg-danger-100 px-4 py-3 text-sm text-danger-600">{error}</p>}

      {loading ? (
        <p className="text-sm text-text-muted">Loading cards...</p>
      ) : cards.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-xl bg-surface-card py-16 text-center shadow-sm">
          <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-brand-100 text-brand-600">
            <Layers className="h-6 w-6" />
          </div>
          <p className="font-medium text-text-heading">No cards yet</p>
          <p className="mb-4 mt-1 text-sm text-text-muted">Add your first card to this deck.</p>
          <Button onClick={openCreate} className="gap-2">
            <Plus className="h-4 w-4" /> Add card
          </Button>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {cards.map((card) => (
            <div key={card.id} className="group rounded-xl bg-surface-card p-5 shadow-sm">
              <div className="flex items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <p className="font-medium text-text-heading">{card.front}</p>
                  <div className="my-3 border-t border-border-soft" />
                  <p className="text-sm text-gray-600">{card.back}</p>
                  {card.tags.length > 0 && (
                    <div className="mt-3 flex flex-wrap gap-1">
                      {card.tags.map((t) => (
                        <span key={t} className="rounded-full bg-surface px-2 py-0.5 text-xs text-gray-600">
                          {t}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
                <div className="flex shrink-0 gap-1 opacity-0 transition-opacity group-hover:opacity-100">
                  <button
                    onClick={() => openEdit(card)}
                    className="rounded-md p-2 text-text-muted hover:bg-surface hover:text-brand-600"
                    aria-label="Edit card"
                  >
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button
                    onClick={() => setDeleting(card)}
                    className="rounded-md p-2 text-text-muted hover:bg-danger-100 hover:text-danger-600"
                    aria-label="Delete card"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <CardFormDialog open={formOpen} onOpenChange={setFormOpen} card={editing} onSubmit={handleSubmit} />

      <ConfirmDialog
        open={Boolean(deleting)}
        onOpenChange={(o) => !o && setDeleting(null)}
        title="Delete card"
        message="Delete this card? This cannot be undone."
        confirmLabel="Delete"
        busy={deleteBusy}
        onConfirm={confirmDelete}
      />
    </div>
  );
}
