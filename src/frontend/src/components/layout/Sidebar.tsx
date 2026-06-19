import { NavLink } from "react-router-dom";
import {
  LayoutDashboard,
  Layers,
  GraduationCap,
  Target,
  BellRing,
  BarChart3,
  Sparkles,
  Settings,
} from "lucide-react";
import { cn } from "../../lib/cn";
import { Logo } from "../ui/Logo";

export const navItems = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard },
  { to: "/decks", label: "Decks", icon: Layers },
  { to: "/study", label: "Study", icon: GraduationCap },
  { to: "/goals", label: "Goals", icon: Target },
  { to: "/reminders", label: "Reminders", icon: BellRing },
  { to: "/analytics", label: "Analytics", icon: BarChart3 },
  { to: "/ai", label: "AI Generation", icon: Sparkles },
  { to: "/settings", label: "Settings", icon: Settings },
];

export function SidebarNav({ onNavigate }: { onNavigate?: () => void }) {
  return (
    <nav className="flex-1 space-y-1 px-3 py-4">
      {navItems.map(({ to, label, icon: Icon }) => (
        <NavLink
          key={to}
          to={to}
          end={to === "/"}
          onClick={onNavigate}
          className={({ isActive }) =>
            cn(
              "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
              isActive
                ? "border-l-4 border-brand-600 bg-brand-50 text-brand-700"
                : "border-l-4 border-transparent text-gray-500 hover:bg-gray-50 hover:text-text-heading",
            )
          }
        >
          <Icon className="h-4.5 w-4.5" strokeWidth={1.75} />
          {label}
        </NavLink>
      ))}
    </nav>
  );
}

export function Sidebar() {
  return (
    <aside className="fixed inset-y-0 left-0 hidden w-64 flex-col border-r border-border-soft bg-white lg:flex">
      <div className="flex h-16 items-center px-6">
        <Logo />
      </div>

      <SidebarNav />

      <div className="border-t border-border-soft p-4 text-xs text-text-muted">
        Phase 1 build
      </div>
    </aside>
  );
}
