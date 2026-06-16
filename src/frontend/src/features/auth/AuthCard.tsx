import type { ReactNode } from "react";
import { LogoMark } from "../../components/ui/Logo";

export function AuthCard({
  title,
  subtitle,
  children,
}: {
  title: string;
  subtitle: string;
  children: ReactNode;
}) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-surface px-4">
      <div className="w-full max-w-sm">
        <div className="mb-6 flex items-center justify-center gap-2">
          <LogoMark className="h-9 w-9" />
          <span className="text-xl font-semibold tracking-tight text-text-heading">Memora</span>
        </div>
        <div className="rounded-2xl bg-surface-card p-8 shadow-sm">
          <h1 className="text-xl font-semibold text-text-heading">{title}</h1>
          <p className="mb-6 mt-1 text-sm text-text-muted">{subtitle}</p>
          {children}
        </div>
      </div>
    </div>
  );
}
