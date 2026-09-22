import type { RankingItem } from "../../api/types";

interface SimulationResultsPanelProps {
  base: RankingItem[];
  projected: RankingItem[];
}

interface DiffRow {
  isoCode: string;
  countryName: string;
  projectedPosition: number;
  positionShift: number; // base - projected; positive = moved up
  baseScore: number;
  projectedScore: number;
  projectedReturn: number;
}

function buildDiff(base: RankingItem[], projected: RankingItem[]): DiffRow[] {
  const baseByIso = new Map(base.map((b) => [b.isoCode, b]));
  return projected
    .map((p) => {
      const b = baseByIso.get(p.isoCode);
      if (!b) return null;
      return {
        isoCode: p.isoCode,
        countryName: p.countryName,
        projectedPosition: p.position,
        positionShift: b.position - p.position,
        baseScore: b.scoring,
        projectedScore: p.scoring,
        projectedReturn: p.estimatedReturnRate,
      };
    })
    .filter((r): r is DiffRow => r !== null)
    .sort((a, b) => a.projectedPosition - b.projectedPosition);
}

export function SimulationResultsPanel({
  base,
  projected,
}: SimulationResultsPanelProps) {
  const diff = buildDiff(base, projected);
  const movedUp = diff.filter((d) => d.positionShift > 0).length;
  const movedDown = diff.filter((d) => d.positionShift < 0).length;

  const topRiser = [...diff].sort(
    (a, b) =>
      b.positionShift - a.positionShift ||
      b.projectedScore - b.baseScore - (a.projectedScore - a.baseScore),
  )[0];

  const topFaller = [...diff].sort(
    (a, b) =>
      a.positionShift - b.positionShift ||
      a.projectedScore - a.baseScore - (b.projectedScore - b.baseScore),
  )[0];

  return (
    <div className="flex flex-col gap-4">
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <SummaryCard
          label="Rank Mobility"
          value={`${movedUp + movedDown} of ${diff.length} shifted`}
          sub={`${movedUp} up / ${movedDown} down`}
        />
        {topRiser && topRiser.positionShift > 0 && (
          <SummaryCard
            label="Top Riser"
            value={`${topRiser.countryName} (${topRiser.isoCode})`}
            sub={`▲ +${topRiser.positionShift} ranks`}
            accent="positive"
          />
        )}
        {topFaller && topFaller.positionShift < 0 && (
          <SummaryCard
            label="Top Faller"
            value={`${topFaller.countryName} (${topFaller.isoCode})`}
            sub={`▼ ${topFaller.positionShift} ranks`}
            accent="negative"
          />
        )}
      </div>

      <div className="overflow-hidden rounded-lg border border-border">
        <table className="w-full text-left text-sm">
          <thead className="bg-surface text-text-secondary">
            <tr className="font-mono text-[11px] uppercase tracking-wider">
              <th className="px-4 py-3 text-center font-medium">Proj #</th>
              <th className="px-4 py-3 text-center font-medium">Shift</th>
              <th className="px-4 py-3 font-medium">Country</th>
              <th className="px-4 py-3 text-right font-medium">Proj Score</th>
              <th className="px-4 py-3 text-right font-medium">Base Score</th>
              <th className="px-4 py-3 text-right font-medium">Proj Return</th>
            </tr>
          </thead>
          <tbody>
            {diff.map((row) => (
              <tr
                key={row.isoCode}
                className="border-t border-border hover:bg-surface-hover"
              >
                <td className="px-4 py-3 text-center font-mono text-sm font-bold text-accent">
                  {String(row.projectedPosition).padStart(2, "0")}
                </td>
                <td className="px-4 py-3 text-center font-mono text-xs">
                  {row.positionShift > 0 && (
                    <span className="text-positive">
                      ▲ +{row.positionShift}
                    </span>
                  )}
                  {row.positionShift < 0 && (
                    <span className="text-negative">▼ {row.positionShift}</span>
                  )}
                  {row.positionShift === 0 && (
                    <span className="text-text-secondary">— 0</span>
                  )}
                </td>
                <td className="px-4 py-3">
                  <div className="flex items-center gap-2">
                    <span className="rounded bg-bg px-1.5 py-0.5 font-mono text-xs font-semibold text-text-secondary">
                      {row.isoCode}
                    </span>
                    <span className="font-medium">{row.countryName}</span>
                  </div>
                </td>
                <td className="px-4 py-3 text-right font-mono text-sm font-semibold text-accent">
                  {(row.projectedScore * 100).toFixed(1)}
                </td>
                <td className="px-4 py-3 text-right font-mono text-sm text-text-secondary">
                  {(row.baseScore * 100).toFixed(1)}
                </td>
                <td className="px-4 py-3 text-right font-mono text-sm font-bold text-positive">
                  {(row.projectedReturn * 100).toFixed(2)}%
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function SummaryCard({
  label,
  value,
  sub,
  accent,
}: {
  label: string;
  value: string;
  sub: string;
  accent?: "positive" | "negative";
}) {
  const accentClass =
    accent === "positive"
      ? "text-positive"
      : accent === "negative"
        ? "text-negative"
        : "text-text-primary";
  return (
    <div className="flex flex-col gap-1 rounded-lg bg-surface p-3">
      <span className="font-mono text-[10px] uppercase tracking-wider text-text-secondary">
        {label}
      </span>
      <span className={`truncate text-base font-semibold ${accentClass}`}>
        {value}
      </span>
      <span className="font-mono text-[10px] text-text-secondary">{sub}</span>
    </div>
  );
}