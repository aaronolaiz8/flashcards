import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog } from "../../components/ui/Dialog";
import { TextField } from "../../components/ui/TextField";
import { Button } from "../../components/ui/Button";
import { apiErrorMessage } from "../../services/api";
import type { DeckSummary } from "../../types";
import type { DeckInput } from "./decksApi";

const schema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().optional(),
  tags: z.string().optional(),
  visibility: z.enum(["Private", "Unlisted", "Public"]),
});

type FormValues = z.infer<typeof schema>;

function parseTags(raw?: string): string[] {
  return (raw ?? "")
    .split(",")
    .map((t) => t.trim())
    .filter(Boolean);
}

export function DeckFormDialog({
  open,
  onOpenChange,
  deck,
  onSubmit,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  deck?: DeckSummary | null;
  onSubmit: (input: DeckInput) => Promise<void>;
}) {
  const isEdit = Boolean(deck);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  useEffect(() => {
    if (open) {
      setServerError(null);
      reset({
        title: deck?.title ?? "",
        description: deck?.description ?? "",
        tags: deck?.tags?.join(", ") ?? "",
        visibility: (deck?.visibility as FormValues["visibility"]) ?? "Private",
      });
    }
  }, [open, deck, reset]);

  async function submit(values: FormValues) {
    setServerError(null);
    try {
      await onSubmit({
        title: values.title.trim(),
        description: values.description?.trim() || null,
        tags: parseTags(values.tags),
        visibility: values.visibility,
      });
      onOpenChange(false);
    } catch (err) {
      setServerError(apiErrorMessage(err, "Could not save deck"));
    }
  }

  return (
    <Dialog
      open={open}
      onOpenChange={onOpenChange}
      title={isEdit ? "Edit deck" : "New deck"}
      description={isEdit ? undefined : "Create a deck to start adding cards."}
    >
      <form onSubmit={handleSubmit(submit)} className="space-y-4">
        <TextField label="Title" {...register("title")} error={errors.title?.message} />
        <TextField label="Description" {...register("description")} />
        <TextField label="Tags (comma-separated)" {...register("tags")} placeholder="spanish, vocab" />
        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-gray-700">Visibility</span>
          <select
            {...register("visibility")}
            className="w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
          >
            <option value="Private">Private</option>
            <option value="Unlisted">Unlisted</option>
            <option value="Public">Public</option>
          </select>
        </label>

        {serverError && <p className="text-sm text-danger-600">{serverError}</p>}

        <div className="flex justify-end gap-3 pt-2">
          <Button type="button" variant="secondary" onClick={() => onOpenChange(false)} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : isEdit ? "Save changes" : "Create deck"}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
