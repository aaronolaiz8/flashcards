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
  email: z.string().email("Enter a valid email"),
  password: z.string().min(1, "Password is required"),
});

type FormValues = z.infer<typeof schema>;

export function LoginPage() {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  async function onSubmit(values: FormValues) {
    setServerError(null);
    try {
      await login(values.email, values.password);
      navigate("/", { replace: true });
    } catch (err) {
      setServerError(apiErrorMessage(err, "Login failed"));
    }
  }

  return (
    <AuthCard title="Welcome back" subtitle="Sign in to Retainica">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <TextField label="Email" type="email" {...register("email")} error={errors.email?.message} />
        <TextField
          label="Password"
          type="password"
          {...register("password")}
          error={errors.password?.message}
        />
        {serverError && <p className="text-sm text-danger-600">{serverError}</p>}
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? "Signing in..." : "Sign in"}
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-text-muted">
        No account?{" "}
        <Link to="/register" className="font-medium text-brand-600 hover:underline">
          Create one
        </Link>
      </p>
    </AuthCard>
  );
}
