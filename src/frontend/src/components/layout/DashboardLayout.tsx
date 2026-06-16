import type { ReactNode } from "react";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";

export function DashboardLayout({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className="min-h-screen bg-surface">
      <Sidebar />
      <Topbar title={title} />
      <main className="p-6 lg:pl-72">{children}</main>
    </div>
  );
}
