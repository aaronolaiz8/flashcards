import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Plus, Pencil, Trash2, Layers } from "lucide-react";
import { DataTable } from "../../components/layout/DataTable";
import { Button } from "../../components/ui/Button";
import { ConfirmDialog } from "../../components/ui/ConfirmDialog";
import { apiErrorMessage } from "../../services/api";
import type { DeckSummary } from "../../types";
import { decksApi, type DeckInput } from "./decksApi";
import { DeckFormDialog } from "./DeckFormDialog";

const visibilityStyles: Record<string, string> = {
  Private: "bg-gray-100 text-gray-600",
  Unlisted: "bg-warning-100 text-warning-600",
  Public: "bg-success-100 text-success-600",
};

export function DecksPage() {
  const [decks, setDecks] = useState<DeckSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<DeckSummary | null>(null);
  const [deleting, setDeleting] = useState<DeckSummary | null>(null);
  const [deleteBusy, setDeleteBusy] = useState(false);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      setDecks(await decksApi.list());
    } catch (err) {
      setError(apiErrorMessage(err, "Could not load decks"));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  function openCreate() {
    setEditing(null);
    setFormOpen(true);
  }

  function openEdit(deck: DeckSummary) {
    setEditing(deck);
    setFormOpen(true);
  }

  async function handleSubmit(input: DeckInput) {
    if (editing) {
      await decksApi.update(editing.id, input);
    } else {
      await decksApi.create(input);
    }
    await load();
  }

  async function confirmDelete() {
    if (!deleting) return;
    setDeleteBusy(true);
    try {
      await decksApi.remove(deleting.id);
      setDeleting(null);
      await load();
    } catch (err) {
      setError(apiErrorMessage(err, "Could not delete deck"));
    } finally {
      setDeleteBusy(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <p className="text-sm text-text-muted">
          {decks.length} {decks.length === 1 ? "deck" : "decks"}
        </p>
        <Button onClick={openCreate} className="gap-2">
          <Plus className="h-4 w-4" /> New deck
        </Button>
      </div>

      {error && <p className="rounded-lg bg-danger-100 px-4 py-3 text-sm text-danger-600">{error}</p>}

      {loading ? (
        <p className="text-sm text-text-muted">Loading decks...</p>
      ) : decks.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-xl bg-surface-card py-16 text-center shadow-sm">
          <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-brand-100 text-brand-600">
            <Layers className="h-6 w-6" />
          </div>
          <p className="font-medium text-text-heading">No decks yet</p>
          <p className="mb-4 mt-1 text-sm text-text-muted">Create your first deck to start studying.</p>
          <Button onClick={openCreate} className="gap-2">
            <Plus className="h-4 w-4" /> New deck
          </Button>
        </div>
      ) : (
        <DataTable<DeckSummary>
          rowKey={(d) => d.id}
          rows={decks}
          columns={[
            {
              header: "Title",
              render: (d) => (
                <div>
                  <Link
                    to={`/decks/${d.id}`}
                    className="font-medium text-text-heading hover:text-brand-600 hover:underline"
                  >
                    {d.title}
                  </Link>
                  {d.description && <p className="text-xs text-text-muted">{d.description}</p>}
                </div>
              ),
            },
            {
              header: "Tags",
              render: (d) =>
                d.tags.length > 0 ? (
                  <div className="flex flex-wrap gap-1">
                    {d.tags.map((t) => (
                      <span key={t} className="rounded-full bg-surface px-2 py-0.5 text-xs text-gray-600">
                        {t}
                      </span>
                    ))}
                  </div>
                ) : (
                  <span className="text-xs text-text-muted">—</span>
                ),
            },
            { header: "Cards", render: (d) => d.cardCount },
            {
              header: "Visibility",
              render: (d) => (
                <span
                  className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                    visibilityStyles[d.visibility] ?? "bg-gray-100 text-gray-600"
                  }`}
                >
                  {d.visibility}
                </span>
              ),
            },
            {
              header: "",
              render: (d) => (
                <div className="flex justify-end gap-1">
                  <button
                    onClick={() => openEdit(d)}
                    className="rounded-md p-2 text-text-muted hover:bg-surface hover:text-brand-600"
                    aria-label="Edit deck"
                  >
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button
                    onClick={() => setDeleting(d)}
                    className="rounded-md p-2 text-text-muted hover:bg-danger-100 hover:text-danger-600"
                    aria-label="Delete deck"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              ),
            },
          ]}
        />
      )}

      <DeckFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        deck={editing}
        onSubmit={handleSubmit}
      />

      <ConfirmDialog
        open={Boolean(deleting)}
        onOpenChange={(o) => !o && setDeleting(null)}
        title="Delete deck"
        message={`Delete "${deleting?.title}" and all its cards? This cannot be undone.`}
        confirmLabel="Delete"
        busy={deleteBusy}
        onConfirm={confirmDelete}
      />
    </div>
  );
}
