import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog } from "../../components/ui/Dialog";
import { TextField } from "../../components/ui/TextField";
import { Button } from "../../components/ui/Button";
import { apiErrorMessage } from "../../services/api";
import type { Card } from "../../types";
import type { CardInput } from "./cardsApi";

const schema = z.object({
  front: z.string().min(1, "Front is required"),
  back: z.string().min(1, "Back is required"),
  tags: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

function parseTags(raw?: string): string[] {
  return (raw ?? "")
    .split(",")
    .map((t) => t.trim())
    .filter(Boolean);
}

export function CardFormDialog({
  open,
  onOpenChange,
  card,
  onSubmit,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  card?: Card | null;
  onSubmit: (input: CardInput) => Promise<void>;
}) {
  const isEdit = Boolean(card);
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
        front: card?.front ?? "",
        back: card?.back ?? "",
        tags: card?.tags?.join(", ") ?? "",
      });
    }
  }, [open, card, reset]);

  async function submit(values: FormValues) {
    setServerError(null);
    try {
      await onSubmit({
        front: values.front.trim(),
        back: values.back.trim(),
        tags: parseTags(values.tags),
      });
      onOpenChange(false);
    } catch (err) {
      setServerError(apiErrorMessage(err, "Could not save card"));
    }
  }

  return (
    <Dialog
      open={open}
      onOpenChange={onOpenChange}
      title={isEdit ? "Edit card" : "New card"}
    >
      <form onSubmit={handleSubmit(submit)} className="space-y-4">
        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-gray-700">Front</span>
          <textarea
            {...register("front")}
            rows={3}
            className="w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
          />
          {errors.front && <span className="mt-1 block text-xs text-danger-600">{errors.front.message}</span>}
        </label>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-gray-700">Back</span>
          <textarea
            {...register("back")}
            rows={3}
            className="w-full rounded-lg border border-border-soft bg-white px-3 py-2 text-sm text-gray-800 outline-none focus:border-brand-500 focus:ring-2 focus:ring-brand-100"
          />
          {errors.back && <span className="mt-1 block text-xs text-danger-600">{errors.back.message}</span>}
        </label>

        <TextField label="Tags (comma-separated)" {...register("tags")} placeholder="verb, irregular" />

        {serverError && <p className="text-sm text-danger-600">{serverError}</p>}

        <div className="flex justify-end gap-3 pt-2">
          <Button type="button" variant="secondary" onClick={() => onOpenChange(false)} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : isEdit ? "Save changes" : "Add card"}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
