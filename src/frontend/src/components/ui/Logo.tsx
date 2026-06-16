import { cn } from "../../lib/cn";

/** Memora brand mark — an "M" drawn in white on the brand-purple tile. */
export function LogoMark({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 64 64" className={cn("h-8 w-8", className)} aria-hidden="true">
      <rect width="64" height="64" rx="16" fill="var(--color-brand-600)" />
      <path
        d="M16 46V20.5c0-1.2 1.5-1.8 2.3-.9L32 35l13.7-15.4c.8-.9 2.3-.3 2.3.9V46"
        fill="none"
        stroke="#ffffff"
        strokeWidth="6"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

/** Full Memora lockup: mark + wordmark. */
export function Logo({ className, markClassName }: { className?: string; markClassName?: string }) {
  return (
    <div className={cn("flex items-center gap-2", className)}>
      <LogoMark className={markClassName} />
      <span className="text-lg font-semibold tracking-tight text-text-heading">Memora</span>
    </div>
  );
}
