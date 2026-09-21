interface MacroWeightSliderProps {
  name: string;
  isHighBetter: boolean;
  weight: number; // 0–1
  productionWeight: number; // 0–1, current real value
  onChange: (weight: number) => void;
}

export function MacroWeightSlider({
  name,
  isHighBetter,
  weight,
  productionWeight,
  onChange,
}: MacroWeightSliderProps) {
  const pct = weight * 100;
  const deltaPct = (weight - productionWeight) * 100;
  const hasDelta = Math.abs(deltaPct) > 0.05;

  return (
    <div className="flex flex-col gap-2 rounded-lg bg-surface p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium text-text-primary">{name}</span>
          <span
            className={`rounded px-1.5 py-0.5 font-mono text-[10px] font-semibold ${
              isHighBetter
                ? "bg-positive/10 text-positive"
                : "bg-negative/10 text-negative"
            }`}
          >
            {isHighBetter ? "HIGHER IS BETTER" : "LOWER IS BETTER"}
          </span>
        </div>
        <div className="flex items-baseline gap-2">
          {hasDelta && (
            <span
              className={`font-mono text-[10px] ${deltaPct > 0 ? "text-positive" : "text-negative"}`}
            >
              {deltaPct > 0 ? "+" : ""}
              {deltaPct.toFixed(1)}% vs current
            </span>
          )}
          <span className="font-mono text-sm font-semibold text-accent">
            {pct.toFixed(1)}%
          </span>
        </div>
      </div>
      <input
        type="range"
        min={0}
        max={100}
        step={0.5}
        value={pct}
        onChange={(e) => onChange(parseFloat(e.target.value) / 100)}
        className="w-full accent-accent"
      />
    </div>
  );
}
