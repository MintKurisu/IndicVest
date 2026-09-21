interface YearSelectorProps {
  years: number[];
  selectedYear: number | null;
  onSelect: (year: number) => void;
  disabled?: boolean;
}

export function YearSelector({
  years,
  selectedYear,
  onSelect,
  disabled,
}: YearSelectorProps) {
  return (
    <div className="flex items-center gap-0.5 rounded-md bg-surface p-0.5">
      {years.map((year) => {
        const isActive = year === selectedYear;
        return (
          <button
            key={year}
            type="button"
            disabled={disabled}
            onClick={() => onSelect(year)}
            className={`rounded px-3 py-1 font-mono text-xs font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${
              isActive
                ? "bg-accent text-bg font-semibold"
                : "text-text-secondary hover:text-text-primary"
            }`}
          >
            {year}
          </button>
        );
      })}
    </div>
  );
}
