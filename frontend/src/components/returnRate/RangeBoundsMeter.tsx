interface RangeBoundsMeterProps {
  minPct: number;
  maxPct: number;
  scaleMaxPct?: number; // visual scale ceiling, purely presentational
}

export function RangeBoundsMeter({
  minPct,
  maxPct,
  scaleMaxPct = 30,
}: RangeBoundsMeterProps) {
  const scale = Math.max(scaleMaxPct, maxPct + 5);
  const clampedMin = Math.min(Math.max(minPct, 0), scale);
  const clampedMax = Math.min(Math.max(maxPct, 0), scale);
  const leftPct = (clampedMin / scale) * 100;
  const widthPct = Math.max(((clampedMax - clampedMin) / scale) * 100, 0);
  const isInvalid = maxPct <= minPct;

  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-center justify-between font-mono text-[11px] text-text-secondary">
        <span>PERMISSIBLE RANGE</span>
        <span className={isInvalid ? "text-negative" : "text-accent"}>
          {isInvalid
            ? "INVALID — min must be below max"
            : `Δ ${(maxPct - minPct).toFixed(2)} pts (${minPct.toFixed(2)}% – ${maxPct.toFixed(2)}%)`}
        </span>
      </div>
      <div className="relative h-6 w-full overflow-hidden rounded-md bg-surface-hover">
        {!isInvalid && (
          <div
            className="absolute top-1 bottom-1 flex items-center justify-between rounded bg-accent/20 px-2"
            style={{ left: `${leftPct}%`, width: `${widthPct}%` }}
          >
            <span className="font-mono text-[10px] font-semibold text-accent">
              {minPct.toFixed(2)}%
            </span>
            <span className="font-mono text-[10px] font-semibold text-accent">
              {maxPct.toFixed(2)}%
            </span>
          </div>
        )}
      </div>
      <div className="flex justify-between font-mono text-[10px] text-text-secondary">
        <span>0%</span>
        <span>{scale.toFixed(0)}%</span>
      </div>
    </div>
  );
}
