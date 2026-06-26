import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Link } from "react-router-dom";
import { authApi } from "./authApi";
import { apiErrorMessage } from "../../services/api";
import { AuthCard } from "./AuthCard";
import { TextField } from "../../components/ui/TextField";
import { Button } from "../../components/ui/Button";

const schema = z.object({
  email: z.string().email("Enter a valid email"),
});

type FormValues = z.infer<typeof schema>;

export function ForgotPasswordPage() {
  const [sent, setSent] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  async function onSubmit(values: FormValues) {
    setServerError(null);
    try {
      await authApi.forgotPassword(values.email);
      setSent(true);
    } catch (err) {
      setServerError(apiErrorMessage(err, "Something went wrong. Please try again."));
    }
  }

  if (sent) {
    return (
      <AuthCard title="Check your email" subtitle="Password reset requested">
        <p className="text-sm text-text-muted">
          If an account exists for that email, we've sent a link to reset your password. The link
          expires in 1 hour.
        </p>
        <p className="mt-6 text-center text-sm text-text-muted">
          <Link to="/login" className="font-medium text-brand-600 hover:underline">
            Back to sign in
          </Link>
        </p>
      </AuthCard>
    );
  }

  return (
    <AuthCard title="Forgot your password?" subtitle="We'll email you a reset link">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <TextField label="Email" type="email" {...register("email")} error={errors.email?.message} />
        {serverError && <p className="text-sm text-danger-600">{serverError}</p>}
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? "Sending..." : "Send reset link"}
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-text-muted">
        Remembered it?{" "}
        <Link to="/login" className="font-medium text-brand-600 hover:underline">
          Sign in
        </Link>
      </p>
    </AuthCard>
  );
}
