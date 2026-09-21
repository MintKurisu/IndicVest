interface ReturnRateBoundCardProps {
  label: string;
  description: string;
  valuePct: number;
  onChange: (valuePct: number) => void;
  accentClassName?: string; // e.g. "text-positive" for floor, "text-negative" for ceiling
}

export function ReturnRateBoundCard({
  label,
  description,
  valuePct,
  onChange,
  accentClassName = "text-accent",
}: ReturnRateBoundCardProps) {
  return (
    <div className="flex flex-col gap-2 rounded-lg bg-surface p-4">
      <span className="font-mono text-[11px] uppercase tracking-wider text-text-secondary">
        {label}
      </span>
      <div className="flex items-baseline gap-2">
        <input
          type="number"
          step="0.01"
          min={0}
          max={100}
          value={valuePct}
          onChange={(e) => onChange(parseFloat(e.target.value) || 0)}
          className={`w-24 border-b border-border bg-transparent font-mono text-2xl font-medium ${accentClassName} focus:border-accent focus:outline-none`}
        />
        <span className="font-mono text-sm text-text-secondary">%</span>
      </div>
      <p className="text-xs leading-relaxed text-text-secondary">
        {description}
      </p>
    </div>
  );
}
