import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import * as Avatar from "@radix-ui/react-avatar";
import { ChevronDown } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../../store/authStore";
import { MobileNav } from "./MobileNav";
import { Logo } from "../ui/Logo";

export function Topbar({ title }: { title: string }) {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const initial = user?.displayName?.charAt(0).toUpperCase() ?? "?";

  async function handleLogout() {
    await logout();
    navigate("/login", { replace: true });
  }

  return (
    <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-border-soft bg-white px-6 lg:pl-72">
      {/* Brand lockup fills the sidebar-width gutter on desktop (the sidebar's own
          logo row sits behind this bar, so we render it here instead). */}
      <div className="absolute inset-y-0 left-0 hidden w-64 items-center px-6 lg:flex">
        <Logo markClassName="h-11 w-11" />
      </div>

      <div className="flex items-center gap-2">
        <MobileNav />
        <h1 className="text-lg font-medium text-text-heading">{title}</h1>
      </div>

      <DropdownMenu.Root>
        <DropdownMenu.Trigger asChild>
          <button className="flex items-center gap-2 rounded-full px-2 py-1 hover:bg-surface">
            <Avatar.Root className="flex h-8 w-8 items-center justify-center overflow-hidden rounded-full bg-brand-100 text-sm font-medium text-brand-700">
              <Avatar.Fallback>{initial}</Avatar.Fallback>
            </Avatar.Root>
            <ChevronDown className="h-4 w-4 text-text-muted" strokeWidth={1.75} />
          </button>
        </DropdownMenu.Trigger>

        <DropdownMenu.Portal>
          <DropdownMenu.Content
            align="end"
            sideOffset={8}
            className="min-w-52 rounded-lg border border-border-soft bg-white p-1 shadow-lg"
          >
            <DropdownMenu.Label className="px-3 py-2">
              <p className="truncate text-sm font-medium text-text-heading">{user?.displayName}</p>
              {user?.email && <p className="truncate text-xs text-text-muted">{user.email}</p>}
            </DropdownMenu.Label>
            <DropdownMenu.Separator className="my-1 h-px bg-border-soft" />
            <DropdownMenu.Item
              onSelect={handleLogout}
              className="cursor-pointer rounded-md px-3 py-2 text-sm text-danger-600 outline-none hover:bg-danger-100"
            >
              Log out
            </DropdownMenu.Item>
          </DropdownMenu.Content>
        </DropdownMenu.Portal>
      </DropdownMenu.Root>
    </header>
  );
}
