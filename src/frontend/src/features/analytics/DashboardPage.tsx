import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import {
  Layers,
  GraduationCap,
  Flame,
  Target,
  Plus,
  Sparkles,
} from "lucide-react";
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  Cell,
  XAxis,
  YAxis,
  Tooltip,
  PieChart,
  Pie,
  LineChart,
  Line,
  RadialBarChart,
  RadialBar,
  PolarAngleAxis,
  Legend,
} from "recharts";
import { StatCard } from "../../components/layout/StatCard";
import { DataTable } from "../../components/layout/DataTable";
import { Button } from "../../components/ui/Button";
import { apiErrorMessage } from "../../services/api";
import { analyticsApi } from "./analyticsApi";
import { decksApi } from "../decks/decksApi";
import type { DashboardOverview, DeckSummary } from "../../types";

const PIE_COLORS = ["#7c4dee", "#51bcda", "#6bd098", "#fbc658", "#ef8157", "#9368e9"];
const BRAND = "#7c4dee";
const BRAND_SOFT = "#cdbaf5";

function iso(date: Date): string {
  return date.toISOString().slice(0, 10);
}
function fmtDay(s: string): string {
  return new Date(`${s}T00:00:00`).toLocaleDateString(undefined, { weekday: "short" });
}
function fmtMonthDay(s: string): string {
  return new Date(`${s}T00:00:00`).toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

// Lifelike sample data shown only at first login (no decks and no goals yet).
function buildSample(): DashboardOverview {
  const today = new Date();
  const dueForecast = Array.from({ length: 7 }, (_, i) => {
    const d = new Date(today);
    d.setDate(today.getDate() + i);
    return { date: iso(d), count: [16, 9, 14, 7, 11, 5, 8][i] };
  });
  const reviewsByDay = Array.from({ length: 14 }, (_, i) => {
    const d = new Date(today);
    d.setDate(today.getDate() - (13 - i));
    return { date: iso(d), count: [12, 18, 9, 22, 15, 0, 14, 20, 17, 11, 25, 19, 16, 23][i] };
  });
  return {
    totalDecks: 6,
    totalCards: 494,
    cardsDueToday: 16,
    reviewsToday: 23,
    currentStreak: 9,
    longestStreak: 14,
    activeGoals: 1,
    mostDueDeckId: null,
    mostDueDeckTitle: "Spanish Vocabulary",
    dueForecast,
    deckUsage: [
      { deckId: 1, title: "Spanish Vocabulary", reviews: 120 },
      { deckId: 2, title: "Organic Chemistry", reviews: 80 },
      { deckId: 3, title: "US History", reviews: 45 },
      { deckId: 4, title: "Anatomy", reviews: 30 },
    ],
    reviewsByDay,
    goals: [
      { id: -1, label: "Finish Spanish basics", dailyReviewTarget: 20, reviewsToday: 12, currentStreak: 9, progressPct: 60 },
    ],
    isEmpty: true,
  };
}

function Panel({ title, action, children }: { title: string; action?: React.ReactNode; children: React.ReactNode }) {
  return (
    <div className="rounded-xl bg-surface-card p-5 shadow-sm">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-base font-medium text-text-heading">{title}</h2>
        {action}
      </div>
      {children}
    </div>
  );
}

function GoalCard({ data }: { data: DashboardOverview }) {
  const goal = data.goals[0];

  if (!goal) {
    return (
      <Panel title="Daily goal">
        <div className="flex flex-col items-center justify-center py-8 text-center">
          <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-success-100 text-success-600">
            <Target className="h-6 w-6" />
          </div>
          <p className="text-sm text-text-muted">Set a daily review goal to build a streak.</p>
          <Link to="/goals" className="mt-4">
            <Button className="gap-2">
              <Plus className="h-4 w-4" /> Create a goal
            </Button>
          </Link>
        </div>
      </Panel>
    );
  }

  const pct = Math.round(goal.progressPct);
  const ringData = [{ name: "progress", value: pct }];

  return (
    <Panel
      title="Daily goal"
      action={data.goals.length > 1 ? <Link to="/goals" className="text-xs text-brand-600 hover:underline">View all</Link> : undefined}
    >
      <div className="flex items-center gap-5">
        <div className="relative h-32 w-32 shrink-0">
          <ResponsiveContainer width="100%" height="100%">
            <RadialBarChart
              cx="50%"
              cy="50%"
              innerRadius="72%"
              outerRadius="100%"
              barSize={10}
              data={ringData}
              startAngle={90}
              endAngle={-270}
            >
              <PolarAngleAxis type="number" domain={[0, 100]} angleAxisId={0} tick={false} />
              <RadialBar background dataKey="value" cornerRadius={6} fill={BRAND} angleAxisId={0} />
            </RadialBarChart>
          </ResponsiveContainer>
          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <span className="text-2xl font-semibold text-text-heading">{pct}%</span>
            <span className="text-xs text-text-muted">today</span>
          </div>
        </div>
        <div className="min-w-0">
          {goal.label && <p className="truncate font-medium text-text-heading">{goal.label}</p>}
          <p className="mt-1 text-sm text-text-muted">
            <span className="font-semibold text-text-heading">{goal.reviewsToday}</span> / {goal.dailyReviewTarget} reviews
          </p>
          <p className="mt-2 inline-flex items-center gap-1.5 rounded-full bg-warning-100 px-2.5 py-1 text-xs font-medium text-warning-600">
            <Flame className="h-3.5 w-3.5" /> {goal.currentStreak}-day streak
          </p>
        </div>
      </div>
    </Panel>
  );
}

function DueForecastChart({ data }: { data: DashboardOverview }) {
  const points = data.dueForecast.map((d, i) => ({
    label: i === 0 ? "Today" : fmtDay(d.date),
    count: d.count,
    isToday: i === 0,
  }));
  return (
    <Panel title="Due in the next 7 days">
      <div className="h-44">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={points} margin={{ top: 4, right: 4, bottom: 0, left: -20 }}>
            <XAxis dataKey="label" tickLine={false} axisLine={false} fontSize={12} stroke="#9a9a9a" />
            <YAxis allowDecimals={false} tickLine={false} axisLine={false} fontSize={12} stroke="#9a9a9a" width={32} />
            <Tooltip cursor={{ fill: "#f4f3ef" }} contentStyle={{ borderRadius: 8, border: "1px solid #e8e6ea", fontSize: 13 }} />
            <Bar dataKey="count" radius={[4, 4, 0, 0]}>
              {points.map((p, i) => (
                <Cell key={i} fill={p.isToday ? BRAND : BRAND_SOFT} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
    </Panel>
  );
}

function DeckUsageChart({ data }: { data: DashboardOverview }) {
  const points = data.deckUsage.map((d) => ({ name: d.title, value: d.reviews }));
  return (
    <Panel title="Most-studied decks">
      {points.length === 0 ? (
        <p className="py-12 text-center text-sm text-text-muted">No reviews logged yet.</p>
      ) : (
        <div className="h-56">
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie data={points} dataKey="value" nameKey="name" innerRadius={45} outerRadius={80} paddingAngle={2}>
                {points.map((_, i) => (
                  <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip contentStyle={{ borderRadius: 8, border: "1px solid #e8e6ea", fontSize: 13 }} />
              <Legend iconType="circle" wrapperStyle={{ fontSize: 12 }} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      )}
    </Panel>
  );
}

function ActivityChart({ data }: { data: DashboardOverview }) {
  const points = data.reviewsByDay.map((d) => ({ label: fmtMonthDay(d.date), count: d.count }));
  return (
    <Panel title="Review activity (14 days)">
      <div className="h-56">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={points} margin={{ top: 4, right: 8, bottom: 0, left: -20 }}>
            <XAxis dataKey="label" tickLine={false} axisLine={false} fontSize={11} stroke="#9a9a9a" interval={1} />
            <YAxis allowDecimals={false} tickLine={false} axisLine={false} fontSize={12} stroke="#9a9a9a" width={32} />
            <Tooltip contentStyle={{ borderRadius: 8, border: "1px solid #e8e6ea", fontSize: 13 }} />
            <Line type="monotone" dataKey="count" stroke={BRAND} strokeWidth={2} dot={false} activeDot={{ r: 4 }} />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </Panel>
  );
}

export function DashboardPage() {
  const [overview, setOverview] = useState<DashboardOverview | null>(null);
  const [decks, setDecks] = useState<DeckSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const [ov, dk] = await Promise.all([analyticsApi.overview(), decksApi.list()]);
        if (!active) return;
        setOverview(ov);
        setDecks(dk);
      } catch (err) {
        if (active) setError(apiErrorMessage(err, "Could not load your dashboard"));
      } finally {
        if (active) setLoading(false);
      }
    })();
    return () => {
      active = false;
    };
  }, []);

  const sample = useMemo(buildSample, []);
  const isSample = !overview || overview.isEmpty;
  const data = isSample ? sample : overview;

  const studyHref = data.mostDueDeckId ? `/study?deckId=${data.mostDueDeckId}` : "/study";
  const recentDecks = decks.slice(0, 5);

  if (loading) {
    return <p className="text-sm text-text-muted">Loading dashboard…</p>;
  }

  return (
    <div className="space-y-6">
      {error && <p className="rounded-lg bg-danger-100 px-4 py-3 text-sm text-danger-600">{error}</p>}

      <div className="flex flex-wrap items-center justify-between gap-3">
        <p className="text-sm text-text-muted">
          {isSample
            ? "Here's a preview of what your dashboard will look like."
            : `${data.cardsDueToday} card${data.cardsDueToday === 1 ? "" : "s"} due today — keep your streak going.`}
        </p>
        <Link to={studyHref}>
          <Button className="gap-2">
            <GraduationCap className="h-4 w-4" /> Study now
          </Button>
        </Link>
      </div>

      {isSample && (
        <div className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-brand-100 bg-brand-50 px-5 py-4">
          <div className="flex items-start gap-3">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-brand-100 text-brand-600">
              <Sparkles className="h-5 w-5" />
            </div>
            <div>
              <p className="font-medium text-text-heading">This is sample data</p>
              <p className="text-sm text-text-muted">
                Create your first deck or goal and your real stats will appear here.
              </p>
            </div>
          </div>
          <div className="flex gap-2">
            <Link to="/decks">
              <Button className="gap-2">
                <Plus className="h-4 w-4" /> Create a deck
              </Button>
            </Link>
            <Link to="/goals">
              <Button variant="secondary" className="gap-2">
                <Target className="h-4 w-4" /> Create a goal
              </Button>
            </Link>
          </div>
        </div>
      )}

      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard icon={Layers} label="Total Decks" value={data.totalDecks} color="brand" to="/decks" />
        <StatCard icon={GraduationCap} label="Cards Due Today" value={data.cardsDueToday} color="info" to={studyHref} />
        <StatCard icon={Flame} label="Day Streak" value={data.currentStreak} color="warning" to="/analytics" />
        <StatCard icon={Target} label="Active Goals" value={data.activeGoals} color="success" to="/goals" />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <GoalCard data={data} />
        <DueForecastChart data={data} />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <DeckUsageChart data={data} />
        <ActivityChart data={data} />
      </div>

      <div>
        <h2 className="mb-3 text-base font-medium text-text-heading">Recent Decks</h2>
        {isSample ? (
          <DataTable<{ id: number; name: string; cards: number }>
            rowKey={(row) => row.id}
            rows={[
              { id: 1, name: "Spanish Vocabulary", cards: 248 },
              { id: 2, name: "Organic Chemistry", cards: 96 },
              { id: 3, name: "US History", cards: 150 },
            ]}
            columns={[
              { header: "Deck", render: (row) => row.name },
              { header: "Cards", render: (row) => row.cards },
            ]}
          />
        ) : recentDecks.length === 0 ? (
          <div className="rounded-xl bg-surface-card p-6 text-center text-sm text-text-muted shadow-sm">
            No decks yet.{" "}
            <Link to="/decks" className="text-brand-600 hover:underline">
              Create one
            </Link>{" "}
            to get started.
          </div>
        ) : (
          <DataTable<DeckSummary>
            rowKey={(d) => d.id}
            rows={recentDecks}
            columns={[
              {
                header: "Deck",
                render: (d) => (
                  <Link to={`/decks/${d.id}`} className="font-medium text-text-heading hover:text-brand-600 hover:underline">
                    {d.title}
                  </Link>
                ),
              },
              { header: "Cards", render: (d) => d.cardCount },
              {
                header: "",
                render: (d) => (
                  <div className="flex justify-end">
                    <Link to={`/study?deckId=${d.id}`} className="text-sm text-brand-600 hover:underline">
                      Study
                    </Link>
                  </div>
                ),
              },
            ]}
          />
        )}
      </div>
    </div>
  );
}
