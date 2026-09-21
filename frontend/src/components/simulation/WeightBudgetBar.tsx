interface WeightBudgetBarProps {
  totalPct: number;
}

export function WeightBudgetBar({ totalPct }: WeightBudgetBarProps) {
  const isValid = Math.abs(totalPct - 100) < 0.05;
  const barWidth = Math.min(totalPct, 100);
  const diff = (totalPct - 100).toFixed(1);

  return (
    <div className="flex flex-col gap-1.5 rounded-lg bg-surface p-3">
      <div className="flex items-center justify-between font-mono text-[11px]">
        <span className="uppercase tracking-wider text-text-secondary">
          Weight Budget
        </span>
        <span
          className={`font-semibold ${isValid ? "text-positive" : "text-warning"}`}
        >
          {isValid
            ? "VALID — 100.0% ALLOCATED"
            : totalPct > 100
              ? `OVER BUDGET (+${diff}%)`
              : `UNDER BUDGET (${diff}%)`}
        </span>
      </div>
      <div className="h-2 w-full overflow-hidden rounded-full bg-surface-hover">
        <div
          className={`h-full rounded-full transition-all ${isValid ? "bg-positive" : "bg-warning"}`}
          style={{ width: `${barWidth}%` }}
        />
      </div>
      <div className="text-right font-mono text-lg font-semibold text-text-primary">
        {totalPct.toFixed(1)}%{" "}
        <span className="text-xs font-normal text-text-secondary">
          / 100.0% required
        </span>
      </div>
    </div>
  );
}
