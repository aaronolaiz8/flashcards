import { useEffect, useState } from "react";
import { Sparkles, CheckCircle2, ExternalLink } from "lucide-react";
import { Button } from "../../components/ui/Button";
import { TextField } from "../../components/ui/TextField";
import { ConfirmDialog } from "../../components/ui/ConfirmDialog";
import { apiErrorMessage } from "../../services/api";
import { aiApi, AI_PROVIDERS, AI_MODELS } from "../ai/aiApi";
import type { AiSettings } from "../../types";

const CUSTOM = "__custom__";

export function SettingsPage() {
  const [settings, setSettings] = useState<AiSettings | null>(null);
  const [loading, setLoading] = useState(true);

  const [provider, setProvider] = useState("Anthropic");
  const [modelChoice, setModelChoice] = useState(AI_MODELS.Anthropic[0]);
  const [customModel, setCustomModel] = useState("");
  const [apiKey, setApiKey] = useState("");

  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);
  const [removing, setRemoving] = useState(false);
  const [removeBusy, setRemoveBusy] = useState(false);

  async function load() {
    setLoading(true);
    try {
      const data = await aiApi.getSettings();
      setSettings(data);
      if (data.provider) setProvider(data.provider);
    } catch (err) {
      setError(apiErrorMessage(err, "Could not load settings"));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  function onProviderChange(next: string) {
    setProvider(next);
    setModelChoice(AI_MODELS[next]?.[0] ?? CUSTOM);
    setCustomModel("");
  }

  async function save(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setSaved(false);
    if (!apiKey.trim()) {
      setError("Enter your API key.");
      return;
    }
    const model = modelChoice === CUSTOM ? customModel.trim() : modelChoice;
    if (!model) {
      setError("Enter a model name.");
      return;
    }
    setBusy(true);
    try {
      const updated = await aiApi.saveSettings({ provider, apiKey: apiKey.trim(), model });
      setSettings(updated);
      setApiKey("");
      setSaved(true);
    } catch (err) {
      setError(apiErrorMessage(err, "Could not save — the key or model may be invalid."));
    } finally {
      setBusy(false);
    }
  }

  async function confirmRemove() {
    setRemoveBusy(true);
    try {
      await aiApi.deleteSettings();
      setRemoving(false);
      setApiKey("");
      setSaved(false);
      await load();
    } catch (err) {
      setError(apiErrorMessage(err, "Could not remove key"));
    } finally {
      setRemoveBusy(false);
    }
  }

  const activeProvider = AI_PROVIDERS.find((p) => p.value === provider) ?? AI_PROVIDERS[0];
  const models = AI_MODELS[provider] ?? [];

  return (
    <div className="max-w-2xl space-y-6">
      <section className="rounded-xl bg-surface-card p-6 shadow-sm">
        <div className="mb-1 flex items-center gap-2">
          <Sparkles className="h-5 w-5 text-brand-600" />
          <h2 className="text-lg font-semibold text-text-heading">AI Card Generation</h2>
        </div>
        <p className="mb-5 text-sm text-text-muted">
          Bring your own OpenAI or Anthropic API key to generate cards from a topic or your notes.
          Your key is encrypted and used only for your generations — Retainica never pays for or sees
          your usage.
        </p>

        {loading ? (
          <p className="text-sm text-text-muted">Loading…</p>
        ) : (
          <>
            {settings?.isConfigured && (
              <div className="mb-5 flex items-center justify-between rounded-lg bg-success-100 px-4 py-3">
                <span className="flex items-center gap-2 text-sm text-success-700">
                  <CheckCircle2 className="h-4 w-4" />
                  Connected — {settings.provider}
                  {settings.model && <span className="text-success-600">· {settings.model}</span>}
                </span>
                <button
                  type="button"
                  onClick={() => setRemoving(true)}
                  className="text-sm font-medium text-danger-600 hover:underline"
                >
                  Remove key
                </button>
              </div>
            )}

            <form onSubmit={save} className="space-y-4">
              <label className="block">
                <span className="mb-1.5 block text-sm font-medium text-gray-700">Provider</span>
                <select
                  value={provider}
                  onChange={(e) => onProviderChange(e.target.value)}
                  className="w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
                >
                  {AI_PROVIDERS.map((p) => (
                    <option key={p.value} value={p.value}>
                      {p.label}
                    </option>
                  ))}
                </select>
              </label>

              <label className="block">
                <span className="mb-1.5 block text-sm font-medium text-gray-700">Model</span>
                <select
                  value={modelChoice}
                  onChange={(e) => setModelChoice(e.target.value)}
                  className="w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
                >
                  {models.map((m, i) => (
                    <option key={m} value={m}>
                      {m}
                      {i === 0 ? " (recommended)" : ""}
                    </option>
                  ))}
                  <option value={CUSTOM}>Custom…</option>
                </select>
              </label>

              {modelChoice === CUSTOM && (
                <TextField
                  label="Custom model id"
                  value={customModel}
                  onChange={(e) => setCustomModel(e.target.value)}
                  placeholder="e.g. claude-opus-4-8"
                />
              )}

              <TextField
                label={settings?.isConfigured ? "New API key (to replace the saved one)" : "API key"}
                type="password"
                autoComplete="off"
                value={apiKey}
                onChange={(e) => setApiKey(e.target.value)}
                placeholder={provider === "OpenAI" ? "sk-…" : "sk-ant-…"}
              />

              <p className="text-xs text-text-muted">
                Create a key at{" "}
                <a
                  href={activeProvider.keysUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-0.5 text-brand-600 hover:underline"
                >
                  {activeProvider.keysLabel}
                  <ExternalLink className="h-3 w-3" />
                </a>
                . We validate it before saving.
              </p>

              {error && <p className="text-sm text-danger-600">{error}</p>}
              {saved && <p className="text-sm text-success-700">Saved and validated.</p>}

              <div className="flex justify-end pt-1">
                <Button type="submit" disabled={busy}>
                  {busy ? "Validating…" : settings?.isConfigured ? "Update key" : "Save key"}
                </Button>
              </div>
            </form>
          </>
        )}
      </section>

      <ConfirmDialog
        open={removing}
        onOpenChange={(o) => !o && setRemoving(false)}
        title="Remove API key"
        message="Remove your saved API key? AI generation will be disabled until you add one again."
        confirmLabel="Remove"
        busy={removeBusy}
        onConfirm={confirmRemove}
      />
    </div>
  );
}
