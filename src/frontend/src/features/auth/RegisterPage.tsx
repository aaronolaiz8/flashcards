import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Link, useNavigate } from "react-router-dom";
import { useAuthStore } from "../../store/authStore";
import { apiErrorMessage } from "../../services/api";
import { AuthCard } from "./AuthCard";
import { TextField } from "../../components/ui/TextField";
import { Button } from "../../components/ui/Button";

const schema = z.object({
  displayName: z.string().min(1, "Name is required"),
  email: z.string().email("Enter a valid email"),
  password: z.string().min(8, "Password must be at least 8 characters"),
});

type FormValues = z.infer<typeof schema>;

export function RegisterPage() {
  const navigate = useNavigate();
  const registerUser = useAuthStore((s) => s.register);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  async function onSubmit(values: FormValues) {
    setServerError(null);
    try {
      await registerUser(values.email, values.password, values.displayName);
      navigate("/", { replace: true });
    } catch (err) {
      setServerError(apiErrorMessage(err, "Registration failed"));
    }
  }

  return (
    <AuthCard title="Create your account" subtitle="Start building decks in minutes">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <TextField label="Display name" {...register("displayName")} error={errors.displayName?.message} />
        <TextField label="Email" type="email" {...register("email")} error={errors.email?.message} />
        <TextField
          label="Password"
          type="password"
          {...register("password")}
          error={errors.password?.message}
        />
        {serverError && <p className="text-sm text-danger-600">{serverError}</p>}
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? "Creating account..." : "Create account"}
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-text-muted">
        Already have an account?{" "}
        <Link to="/login" className="font-medium text-brand-600 hover:underline">
          Sign in
        </Link>
      </p>
    </AuthCard>
  );
}
