import { TrendingUp, Trophy, Percent, BarChart3 } from "lucide-react";
import type { RankingItem } from "../../api/types";

interface RankingStatsStripProps {
  rankings: RankingItem[];
  computeMs: number | null;
}

export function RankingStatsStrip({
  rankings,
  computeMs,
}: RankingStatsStripProps) {
  if (rankings.length === 0) return null;

  const top = rankings[0];
  const highestReturn = [...rankings].sort(
    (a, b) => b.estimatedReturnRate - a.estimatedReturnRate,
  )[0];
  const highestReturnPosition =
    rankings.findIndex((r) => r.isoCode === highestReturn.isoCode) + 1;
  const avgScore =
    rankings.reduce((sum, r) => sum + r.scoring, 0) / rankings.length;
  const avgReturn =
    rankings.reduce((sum, r) => sum + r.estimatedReturnRate, 0) /
    rankings.length;

  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <StatCard
        icon={<TrendingUp size={16} className="text-positive" />}
        label="Highest Est. Return"
        value={`${(highestReturn.estimatedReturnRate * 100).toFixed(2)}%`}
        sub={`${highestReturn.countryName} · #${highestReturnPosition} of ${rankings.length}`}
      />
      <StatCard
        icon={<Trophy size={16} className="text-accent" />}
        label="Top Ranked"
        value={top.countryName}
        sub={`${top.isoCode} · Score ${(top.scoring * 100).toFixed(1)} · #1 of ${rankings.length}`}
      />
      <StatCard
        icon={<Percent size={16} className="text-text-secondary" />}
        label="Average Est. Return"
        value={`${(avgReturn * 100).toFixed(2)}%`}
        sub={`across ${rankings.length} countries`}
      />
      <StatCard
        icon={<BarChart3 size={16} className="text-text-secondary" />}
        label="Average Score"
        value={(avgScore * 100).toFixed(1)}
        sub={
          computeMs != null
            ? `computed in ${computeMs.toFixed(0)}ms`
            : undefined
        }
      />
    </div>
  );
}

function StatCard({
  icon,
  label,
  value,
  sub,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
  sub?: string;
}) {
  return (
    <div className="flex flex-col gap-1.5 rounded-lg bg-surface p-4">
      <div className="flex items-center gap-1.5 font-mono text-[10px] uppercase tracking-wider text-text-secondary">
        {icon}
        {label}
      </div>
      <div className="truncate text-lg font-semibold text-text-primary">
        {value}
      </div>
      {sub && (
        <div className="font-mono text-[10px] text-text-secondary">{sub}</div>
      )}
    </div>
  );
}
