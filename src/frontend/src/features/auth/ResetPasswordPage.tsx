import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { authApi } from "./authApi";
import { apiErrorMessage } from "../../services/api";
import { AuthCard } from "./AuthCard";
import { TextField } from "../../components/ui/TextField";
import { Button } from "../../components/ui/Button";

const schema = z
  .object({
    password: z.string().min(8, "Password must be at least 8 characters"),
    confirm: z.string(),
  })
  .refine((v) => v.password === v.confirm, {
    message: "Passwords don't match",
    path: ["confirm"],
  });

type FormValues = z.infer<typeof schema>;

export function ResetPasswordPage() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get("token") ?? "";
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  async function onSubmit(values: FormValues) {
    setServerError(null);
    try {
      await authApi.resetPassword(token, values.password);
      navigate("/login", {
        replace: true,
        state: { notice: "Your password has been reset. Please sign in." },
      });
    } catch (err) {
      setServerError(
        apiErrorMessage(err, "This reset link is invalid or has expired. Request a new one."),
      );
    }
  }

  if (!token) {
    return (
      <AuthCard title="Invalid reset link" subtitle="This link is missing or malformed">
        <p className="text-sm text-text-muted">
          Please request a new password reset link.
        </p>
        <p className="mt-6 text-center text-sm text-text-muted">
          <Link to="/forgot-password" className="font-medium text-brand-600 hover:underline">
            Request a new link
          </Link>
        </p>
      </AuthCard>
    );
  }

  return (
    <AuthCard title="Set a new password" subtitle="Choose a strong password you'll remember">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <TextField
          label="New password"
          type="password"
          autoComplete="new-password"
          {...register("password")}
          error={errors.password?.message}
        />
        <TextField
          label="Confirm new password"
          type="password"
          autoComplete="new-password"
          {...register("confirm")}
          error={errors.confirm?.message}
        />
        {serverError && <p className="text-sm text-danger-600">{serverError}</p>}
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? "Resetting..." : "Reset password"}
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-text-muted">
        <Link to="/login" className="font-medium text-brand-600 hover:underline">
          Back to sign in
        </Link>
      </p>
    </AuthCard>
  );
}
