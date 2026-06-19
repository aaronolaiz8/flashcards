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
  const [format, setFormat] = useState<Format>("csv");
  const [content, setContent] = useState("");
  const [fileName, setFileName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (open) {
      setTitle("");
      setFormat("csv");
      setContent("");
      setFileName("");
      setError(null);
      setBusy(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  }, [open]);

  async function handleFile(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setError(null);
    setFileName(file.name);
    setFormat(file.name.toLowerCase().endsWith(".json") ? "json" : "csv");
    try {
      setContent(await file.text());
    } catch {
      setError("Could not read that file.");
      setContent("");
    }
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
      await decksApi.importDeck({ format, content, title: title.trim() || undefined });
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
            className="flex w-full items-center gap-2 rounded-lg border border-dashed border-border-soft bg-white px-3 py-3 text-sm text-gray-600 hover:bg-surface"
          >
            <Upload className="h-4 w-4 text-text-muted" />
            {fileName || "Choose a .csv or .json file"}
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
