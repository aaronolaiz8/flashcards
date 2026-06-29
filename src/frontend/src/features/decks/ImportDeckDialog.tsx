import { useEffect, useRef, useState } from "react";
import { Upload } from "lucide-react";
import { Dialog } from "../../components/ui/Dialog";
import { TextField } from "../../components/ui/TextField";
import { Button } from "../../components/ui/Button";
import { apiErrorMessage } from "../../services/api";
import { decksApi } from "./decksApi";

type Format = "csv" | "json";

export function ImportDeckDialog({
  open,
  onOpenChange,
  onImported,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onImported: () => void;
}) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [tags, setTags] = useState("");
  const [format, setFormat] = useState<Format>("csv");
  const [content, setContent] = useState("");
  const [fileName, setFileName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [dragOver, setDragOver] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (open) {
      setTitle("");
      setDescription("");
      setTags("");
      setFormat("csv");
      setContent("");
      setFileName("");
      setError(null);
      setBusy(false);
      setDragOver(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  }, [open]);

  async function readFile(file: File) {
    const name = file.name.toLowerCase();
    if (!/\.(csv|json|txt)$/.test(name)) {
      setError("Please choose a .csv or .json file.");
      return;
    }
    setError(null);
    setFileName(file.name);
    setFormat(name.endsWith(".json") ? "json" : "csv");
    try {
      setContent(await file.text());
    } catch {
      setError("Could not read that file.");
      setContent("");
    }
  }

  async function handleFile(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) await readFile(file);
  }

  async function handleDrop(e: React.DragEvent) {
    e.preventDefault();
    setDragOver(false);
    const file = e.dataTransfer.files?.[0];
    if (file) await readFile(file);
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    if (!content.trim()) {
      setError("Choose a file to import.");
      return;
    }
    setBusy(true);
    setError(null);
    try {
      const tagList = tags
        .split(",")
        .map((t) => t.trim())
        .filter(Boolean);
      await decksApi.importDeck({
        format,
        content,
        title: title.trim() || undefined,
        description: description.trim() || undefined,
        tags: tagList.length ? tagList : undefined,
      });
      onImported();
      onOpenChange(false);
    } catch (err) {
      setError(apiErrorMessage(err, "Could not import deck"));
    } finally {
      setBusy(false);
    }
  }

  return (
    <Dialog
      open={open}
      onOpenChange={onOpenChange}
      title="Import deck"
      description="Upload a CSV or JSON file. CSV columns: Front, Back, Tags (tags separated by semicolons)."
    >
      <form onSubmit={submit} className="space-y-4">
        <TextField
          label="Deck name (optional)"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Imported Deck"
        />

        <TextField
          label="Description (optional)"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="What this deck covers"
        />

        <TextField
          label="Tags (optional, comma-separated)"
          value={tags}
          onChange={(e) => setTags(e.target.value)}
          placeholder="spanish, vocab"
        />

        <div>
          <span className="mb-1.5 block text-sm font-medium text-gray-700">File</span>
          <input
            ref={fileInputRef}
            type="file"
            accept=".csv,.json,.txt,text/csv,application/json"
            onChange={handleFile}
            className="hidden"
          />
          <button
            type="button"
            onClick={() => fileInputRef.current?.click()}
            onDragOver={(e) => {
              e.preventDefault();
              setDragOver(true);
            }}
            onDragLeave={() => setDragOver(false)}
            onDrop={handleDrop}
            className={`flex w-full flex-col items-center justify-center gap-1.5 rounded-lg border border-dashed px-3 py-6 text-sm transition-colors ${
              dragOver
                ? "border-brand-500 bg-brand-50 text-brand-700"
                : "border-border-soft bg-white text-gray-600 hover:bg-surface"
            }`}
          >
            <Upload className={`h-5 w-5 ${dragOver ? "text-brand-600" : "text-text-muted"}`} />
            <span className="font-medium">{fileName || "Choose a file or drag it here"}</span>
            {!fileName && <span className="text-xs text-text-muted">CSV or JSON</span>}
          </button>
          {fileName && (
            <p className="mt-1 text-xs text-text-muted">
              Detected format: <span className="font-medium uppercase">{format}</span>
            </p>
          )}
        </div>

        {error && <p className="text-sm text-danger-600">{error}</p>}

        <div className="flex justify-end gap-3 pt-2">
          <Button type="button" variant="secondary" onClick={() => onOpenChange(false)} disabled={busy}>
            Cancel
          </Button>
          <Button type="submit" disabled={busy || !content.trim()}>
            {busy ? "Importing…" : "Import deck"}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
