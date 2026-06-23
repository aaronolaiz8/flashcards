import { api } from "../../services/api";
import type { AiSettings, GeneratedCard } from "../../types";

export interface SaveAiSettingsInput {
  provider: string;
  apiKey: string;
  model?: string | null;
}

export interface GenerateCardsInput {
  topic?: string | null;
  text?: string | null;
  count: number;
  deckId?: number | null;
}

/** Provider options surfaced in the Settings UI. First model in each list is the default. */
export const AI_PROVIDERS: { value: string; label: string; keysUrl: string; keysLabel: string }[] = [
  {
    value: "Anthropic",
    label: "Anthropic (Claude)",
    keysUrl: "https://console.anthropic.com/settings/keys",
    keysLabel: "console.anthropic.com",
  },
  {
    value: "OpenAI",
    label: "OpenAI (GPT)",
    keysUrl: "https://platform.openai.com/api-keys",
    keysLabel: "platform.openai.com",
  },
];

export const AI_MODELS: Record<string, string[]> = {
  Anthropic: ["claude-haiku-4-5", "claude-sonnet-4-6", "claude-opus-4-8"],
  OpenAI: ["gpt-4o-mini", "gpt-4o"],
};

export const aiApi = {
  async getSettings(): Promise<AiSettings> {
    const { data } = await api.get<AiSettings>("/settings/ai");
    return data;
  },
  async saveSettings(input: SaveAiSettingsInput): Promise<AiSettings> {
    const { data } = await api.put<AiSettings>("/settings/ai", input);
    return data;
  },
  async deleteSettings(): Promise<void> {
    await api.delete("/settings/ai");
  },
  async generate(input: GenerateCardsInput): Promise<GeneratedCard[]> {
    const { data } = await api.post<GeneratedCard[]>("/ai/generate", input);
    return data;
  },
};
