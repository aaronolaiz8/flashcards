import type { LucideIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { cn } from "../../lib/cn";

type StatCardColor = "brand" | "info" | "success" | "warning" | "danger";

const colorClasses: Record<StatCardColor, string> = {
  brand: "bg-brand-100 text-brand-700",
  info: "bg-info-100 text-info-600",
  success: "bg-success-100 text-success-600",
  warning: "bg-warning-100 text-warning-600",
  danger: "bg-danger-100 text-danger-600",
};

export function StatCard({
  icon: Icon,
  label,
  value,
  footer,
  color = "brand",
  to,
}: {
  icon: LucideIcon;
  label: string;
  value: string | number;
  footer?: string;
  color?: StatCardColor;
  to?: string;
}) {
  const card = (
    <div
      className={cn(
        "h-full rounded-xl bg-surface-card p-5 shadow-sm transition-shadow",
        to && "hover:shadow-md",
      )}
    >
      <div className="flex items-center gap-4">
        <div className={cn("flex h-12 w-12 items-center justify-center rounded-full", colorClasses[color])}>
          <Icon className="h-5 w-5" strokeWidth={1.75} />
        </div>
        <div>
          <p className="text-2xl font-semibold text-text-heading">{value}</p>
          <p className="text-sm text-text-muted">{label}</p>
        </div>
      </div>
      {footer && (
        <div className="mt-4 border-t border-border-soft pt-3 text-xs text-text-muted">
          {footer}
        </div>
      )}
    </div>
  );

  if (to) {
    return (
      <Link to={to} className="block rounded-xl focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-300">
        {card}
      </Link>
    );
  }
  return card;
}
