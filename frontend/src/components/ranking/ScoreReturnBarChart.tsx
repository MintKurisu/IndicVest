import type { RankingItem } from "../../api/types";

interface ScoreReturnBarChartProps {
  rankings: RankingItem[];
  limit?: number;
}

export function ScoreReturnBarChart({
  rankings,
  limit = 10,
}: ScoreReturnBarChartProps) {
  const top = rankings.slice(0, limit);
  const maxScore = Math.max(...top.map((r) => r.scoring), 1);

  return (
    <div className="flex flex-col gap-3 rounded-lg bg-surface p-4">
      <div className="flex items-center justify-between">
        <span className="text-sm font-semibold text-text-primary">
          Score vs. Estimated Return
        </span>
        <span className="font-mono text-[10px] text-text-secondary">
          TOP {top.length}
        </span>
      </div>
      <div className="flex flex-col gap-2">
        {top.map((item) => (
          <div key={item.isoCode} className="flex items-center gap-2">
            <span className="w-9 shrink-0 font-mono text-xs font-semibold text-text-secondary">
              {item.isoCode}
            </span>
            <div className="relative h-5 flex-1 overflow-hidden rounded bg-bg">
              <div
                className="h-full rounded bg-accent/70"
                style={{ width: `${(item.scoring / maxScore) * 100}%` }}
              />
              <span className="absolute inset-y-0 left-1.5 flex items-center font-mono text-[10px] font-semibold text-bg">
                {(item.scoring * 100).toFixed(1)}
              </span>
            </div>
            <span className="w-14 shrink-0 text-right font-mono text-xs font-bold text-positive">
              {(item.estimatedReturnRate * 100).toFixed(2)}%
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
