import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import * as Avatar from "@radix-ui/react-avatar";
import { Search, ChevronDown } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../../store/authStore";

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
      <h1 className="text-lg font-medium text-text-heading">{title}</h1>

      <div className="flex items-center gap-4">
        <div className="hidden items-center gap-2 rounded-full bg-surface px-4 py-2 sm:flex">
          <Search className="h-4 w-4 text-text-muted" strokeWidth={1.75} />
          <input
            type="text"
            placeholder="Search..."
            className="w-40 bg-transparent text-sm text-gray-700 placeholder:text-text-muted focus:outline-none"
          />
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
              className="min-w-40 rounded-lg border border-border-soft bg-white p-1 shadow-lg"
            >
              <DropdownMenu.Item className="cursor-pointer rounded-md px-3 py-2 text-sm text-gray-700 outline-none hover:bg-surface">
                Profile
              </DropdownMenu.Item>
              <DropdownMenu.Item className="cursor-pointer rounded-md px-3 py-2 text-sm text-gray-700 outline-none hover:bg-surface">
                Account Settings
              </DropdownMenu.Item>
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
      </div>
    </header>
  );
}
