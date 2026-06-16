import { Layers, GraduationCap, Flame, Target } from "lucide-react";
import { StatCard } from "../../components/layout/StatCard";
import { DataTable } from "../../components/layout/DataTable";

type RecentDeck = { id: number; name: string; cards: number; due: number };

const recentDecks: RecentDeck[] = [
  { id: 1, name: "Spanish Vocabulary", cards: 248, due: 12 },
  { id: 2, name: "Organic Chemistry", cards: 96, due: 4 },
  { id: 3, name: "US History", cards: 150, due: 0 },
];

export function DashboardPage() {
  return (
    <div className="space-y-6">
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard icon={Layers} label="Total Decks" value={6} color="brand" />
        <StatCard icon={GraduationCap} label="Cards Due Today" value={16} color="info" />
        <StatCard icon={Flame} label="Day Streak" value={9} color="warning" />
        <StatCard icon={Target} label="Active Goals" value={2} color="success" />
      </div>

      <div>
        <h2 className="mb-3 text-base font-medium text-text-heading">Recent Decks</h2>
        <DataTable<RecentDeck>
          rowKey={(row) => row.id}
          rows={recentDecks}
          columns={[
            { header: "Deck", render: (row) => row.name },
            { header: "Cards", render: (row) => row.cards },
            { header: "Due", render: (row) => row.due },
          ]}
        />
      </div>
    </div>
  );
}
