import type { RankingItem } from "../../api/types";

interface RankingTableProps {
  rankings: RankingItem[];
}

export function RankingTable({ rankings }: RankingTableProps) {
  const maxScore = Math.max(...rankings.map((r) => r.scoring), 1);

  return (
    <div className="overflow-hidden rounded-lg border border-border">
      <table className="w-full text-left text-sm">
        <thead className="bg-surface text-text-secondary">
          <tr className="font-mono text-[11px] uppercase tracking-wider">
            <th className="w-14 px-4 py-3 text-center font-medium">#</th>
            <th className="px-4 py-3 font-medium">Country</th>
            <th className="px-4 py-3 text-right font-medium">Est. Return</th>
            <th className="px-4 py-3 text-right font-medium">Score</th>
          </tr>
        </thead>
        <tbody>
          {rankings.map((item) => (
            <tr
              key={item.isoCode}
              className={`border-t border-border transition-colors hover:bg-surface-hover ${
                item.position === 1 ? "bg-accent/5" : ""
              }`}
            >
              <td className="px-4 py-3 text-center">
                <span
                  className={`font-mono text-sm font-bold ${
                    item.position === 1 ? "text-accent" : "text-text-secondary"
                  }`}
                >
                  {String(item.position).padStart(2, "0")}
                </span>
              </td>
              <td className="px-4 py-3">
                <div className="flex items-center gap-2">
                  <span className="rounded bg-bg px-1.5 py-0.5 font-mono text-xs font-semibold text-text-secondary">
                    {item.isoCode}
                  </span>
                  <span className="font-medium">{item.countryName}</span>
                </div>
              </td>
              <td className="px-4 py-3 text-right">
                <span className="font-mono text-base font-bold text-positive">
                  {(item.estimatedReturnRate * 100).toFixed(2)}%
                </span>
              </td>
              <td className="px-4 py-3 text-right">
                <div className="flex flex-col items-end gap-1">
                  <span className="font-mono text-sm font-semibold text-accent">
                    {(item.scoring * 100).toFixed(1)}
                  </span>
                  <div className="h-1 w-20 overflow-hidden rounded-full bg-surface-hover">
                    <div
                      className="h-full rounded-full bg-accent"
                      style={{ width: `${(item.scoring / maxScore) * 100}%` }}
                    />
                  </div>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
