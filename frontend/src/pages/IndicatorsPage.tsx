import { useMemo, useState } from "react";
import { Database, Search, Globe2, Plus, X } from "lucide-react";
import { useCountries } from "../hooks/useCountries";
import { useMacroIndicators } from "../hooks/useMacroIndicators";
import {
  useIndicators,
  useIndicatorYears,
  useCreateIndicator,
  useUpdateIndicator,
  useDeleteIndicator,
} from "../hooks/useIndicators";
import { EditableCell } from "../components/indicators/EditableCell";

export function IndicatorsPage() {
  const { data: countries } = useCountries();
  const { data: macros } = useMacroIndicators();
  const { data: indicators, isLoading, isError } = useIndicators();
  const { data: years } = useIndicatorYears();

  const createIndicator = useCreateIndicator();
  const updateIndicator = useUpdateIndicator();
  const deleteIndicator = useDeleteIndicator();

  // --- Estado ---
  const currentYear = new Date().getFullYear();
  const MIN_YEAR = 1900;
  const MAX_YEAR = currentYear + 10;

  const [selectedYearState, setSelectedYear] = useState<number | null>(null);
  const [extraYears, setExtraYears] = useState<number[]>([]);
  const [addingYear, setAddingYear] = useState(false);
  const [yearInput, setYearInput] = useState("");
  const [search, setSearch] = useState("");
  const [savingKey, setSavingKey] = useState<string | null>(null);

  const selectedYear =
    selectedYearState ?? (years?.length ? Math.max(...years) : currentYear);

  // --- Memorias ---
  // Años que tienen datos reales (backend). Estos no deben poder quitarse.
  const yearsWithData = useMemo(
    () =>
      new Set<number>([
        ...(years ?? []),
        ...(indicators ?? []).map((i) => i.year),
      ]),
    [years, indicators],
  );

  const yearOptions = useMemo(() => {
    const set = new Set<number>([
      currentYear, // siempre presente
      ...(years ?? []),
      ...extraYears,
      selectedYear,
    ]);
    return Array.from(set).sort((a, b) => a - b);
  }, [currentYear, years, extraYears, selectedYear]);

  // Map: `${countryId}-${macroId}` -> { idIndicator, value }
  const cellMap = useMemo(() => {
    const map = new Map<string, { idIndicator: number; value: number }>();
    indicators
      ?.filter((i) => i.year === selectedYear)
      .forEach((i) => {
        map.set(`${i.idCountry}-${i.idMacroIndicator}`, {
          idIndicator: i.idIndicator,
          value: i.value,
        });
      });
    return map;
  }, [indicators, selectedYear]);

  const filteredCountries = useMemo(() => {
    if (!countries) return [];
    const q = search.toLowerCase().trim();
    if (!q) return countries;
    return countries.filter(
      (c) =>
        c.name.toLowerCase().includes(q) || c.isoCode.toLowerCase().includes(q),
    );
  }, [countries, search]);

  // --- Calculated Metrics ---
  const totalCells = (countries?.length ?? 0) * (macros?.length ?? 0);
  const filledCells = cellMap.size;
  const completionPct =
    totalCells > 0 ? Math.round((filledCells / totalCells) * 100) : 0;
  const yearsTracked = years?.length ?? 0;

  // --- Handlers ---
  const yearInputValid = (() => {
    const y = Number(yearInput);
    return Number.isInteger(y) && y >= MIN_YEAR && y <= MAX_YEAR;
  })();

  const handleAddYear = () => {
    if (!yearInputValid) return;
    const y = Number(yearInput);
    setExtraYears((prev) => (prev.includes(y) ? prev : [...prev, y]));
    setSelectedYear(y);
    setYearInput("");
    setAddingYear(false);
  };

  const cancelAddYear = () => {
    setYearInput("");
    setAddingYear(false);
  };

  const canRemoveYear = (y: number) =>
    y !== currentYear && !yearsWithData.has(y);

  const handleRemoveYear = (y: number) => {
    if (!canRemoveYear(y)) return;
    setExtraYears((prev) => prev.filter((v) => v !== y));
    if (y === selectedYear) setSelectedYear(currentYear);
  };

  const handleCellCommit = (
    countryId: number,
    macroId: number,
    newValue: number | null,
  ) => {
    const key = `${countryId}-${macroId}`;
    const existing = cellMap.get(key);
    setSavingKey(key);

    const done = () => setSavingKey(null);

    if (newValue === null) {
      if (existing)
        deleteIndicator.mutate(existing.idIndicator, { onSettled: done });
      else done();
      return;
    }

    if (existing) {
      updateIndicator.mutate(
        {
          id: existing.idIndicator,
          payload: {
            idCountry: countryId,
            idMacroIndicator: macroId,
            value: newValue,
            year: selectedYear,
          },
        },
        { onSettled: done },
      );
    } else {
      createIndicator.mutate(
        {
          idCountry: countryId,
          idMacroIndicator: macroId,
          value: newValue,
          year: selectedYear,
        },
        { onSettled: done },
      );
    }
  };

  if (isLoading)
    return <p className="text-text-secondary">Loading indicators...</p>;
  if (isError)
    return <p className="text-negative">Failed to load indicators.</p>;

  if (!macros || macros.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center rounded-lg border border-dashed border-border py-16 text-center">
        <Database className="mb-3 text-text-secondary" size={32} />
        <p className="text-sm text-text-secondary">
          No macroindicators configured yet.
        </p>
        <p className="mt-1 text-xs text-text-secondary">
          Set up macroindicators before loading data.
        </p>
      </div>
    );
  }

  if (!countries || countries.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center rounded-lg border border-dashed border-border py-16 text-center">
        <Globe2 className="mb-3 text-text-secondary" size={32} />
        <p className="text-sm text-text-secondary">
          No countries registered yet.
        </p>
        <p className="mt-1 text-xs text-text-secondary">
          Register a country before loading indicator data.
        </p>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-2 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">
            Data Timeseries Matrix
          </h1>
          <div className="mt-1 flex flex-wrap items-center gap-2 font-mono text-xs text-text-secondary">
            <span>
              <strong className="text-text-primary">
                {filledCells}/{totalCells}
              </strong>{" "}
              cells filled
            </span>
            <span className="text-border">/</span>
            <span>
              <strong
                className={
                  completionPct === 100 ? "text-positive" : "text-warning"
                }
              >
                {completionPct}%
              </strong>{" "}
              complete for {selectedYear}
            </span>
            <span className="text-border">/</span>
            <span>
              <strong className="text-text-primary">{yearsTracked}</strong>{" "}
              {yearsTracked === 1 ? "year" : "years"} tracked
            </span>
          </div>
        </div>
      </div>

      <p className="mb-3 mt-2 rounded-md bg-surface px-3 py-2 text-xs text-text-secondary">
        <span className="font-medium text-text-primary">Double-click</span> a
        cell to edit its value. To remove a data point,{" "}
        <span className="font-medium text-text-primary">
          clear the field and press Enter
        </span>
        .
      </p>

      {/* Filters & Year Selector */}
      <div className="mb-3 mt-3 flex flex-wrap items-center justify-between gap-3 rounded-md bg-surface p-2">
        <div className="flex items-center gap-2 rounded bg-bg px-2 py-1">
          <Search size={14} className="text-text-secondary" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Filter country or ISO..."
            className="w-48 bg-transparent font-mono text-xs outline-none placeholder:text-text-secondary"
          />
        </div>

        {/* Year Selector Component */}
        <div className="flex flex-wrap items-center gap-2">
          <div className="flex flex-wrap items-center gap-1.5 rounded bg-bg p-0.5">
            {yearOptions.map((y) => (
              <div key={y} className="group relative">
                <button
                  onClick={() => setSelectedYear(y)}
                  className={`rounded px-2 py-1 font-mono text-xs transition-colors ${
                    y === selectedYear
                      ? "bg-accent text-bg font-semibold"
                      : "text-text-secondary hover:text-text-primary"
                  }`}
                >
                  {y}
                </button>
                {canRemoveYear(y) && (
                  <button
                    onClick={() => handleRemoveYear(y)}
                    aria-label={`Remove year ${y}`}
                    className="absolute -right-1 -top-1 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-surface text-text-secondary opacity-0 shadow transition-opacity hover:text-negative focus:opacity-100 group-hover:opacity-100"
                  >
                    <X size={10} />
                  </button>
                )}
              </div>
            ))}
          </div>
          {addingYear ? (
            <div className="flex items-center gap-1 rounded bg-bg px-2 py-1">
              <input
                autoFocus
                type="number"
                min={MIN_YEAR}
                max={MAX_YEAR}
                value={yearInput}
                onChange={(e) => setYearInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") handleAddYear();
                  if (e.key === "Escape") cancelAddYear();
                }}
                placeholder="e.g. 2024"
                className="w-20 bg-transparent font-mono text-xs outline-none placeholder:text-text-secondary"
              />
              <button
                onClick={handleAddYear}
                disabled={!yearInputValid}
                className="rounded bg-accent px-1.5 py-0.5 font-mono text-[10px] font-semibold text-bg disabled:opacity-40"
              >
                Add
              </button>
              <button
                onClick={cancelAddYear}
                className="text-text-secondary hover:text-text-primary"
                aria-label="Cancel"
              >
                <X size={12} />
              </button>
            </div>
          ) : (
            <button
              onClick={() => setAddingYear(true)}
              className="flex items-center gap-1 rounded bg-bg px-2 py-1 font-mono text-xs text-text-secondary transition-colors hover:text-text-primary"
            >
              <Plus size={12} />
              Add year
            </button>
          )}
        </div>
      </div>

      {/* Matrix */}
      <div className="overflow-x-auto rounded-lg border border-border">
        <table className="w-full border-collapse text-left text-sm">
          <thead className="sticky top-0 z-10 bg-surface">
            <tr className="font-mono text-[10px] uppercase tracking-wider text-text-secondary">
              <th className="sticky left-0 z-20 min-w-[200px] bg-surface px-4 py-3 text-left font-medium">
                Country
              </th>
              {macros.map((m) => (
                <th
                  key={m.idMacroIndicator}
                  className="min-w-[120px] px-3 py-3 text-right font-medium"
                >
                  <div className="flex flex-col items-end gap-0.5">
                    <span className="truncate text-text-primary">{m.name}</span>
                    <span
                      className={
                        m.isHighBetter ? "text-positive" : "text-negative"
                      }
                    >
                      {m.isHighBetter ? "▲" : "▼"}
                    </span>
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filteredCountries.map((country) => {
              const filledForCountry = macros.filter((m) =>
                cellMap.has(`${country.idCountry}-${m.idMacroIndicator}`),
              ).length;
              const rowPct =
                macros.length > 0
                  ? (filledForCountry / macros.length) * 100
                  : 0;

              return (
                <tr
                  key={country.idCountry}
                  className="border-t border-border hover:bg-surface-hover"
                >
                  <td className="sticky left-0 z-10 bg-bg px-4 py-2 hover:bg-surface-hover">
                    <div className="flex flex-col gap-1">
                      <div className="flex items-center gap-2">
                        <span className="font-medium">{country.name}</span>
                        <span className="rounded bg-surface px-1.5 py-0.5 font-mono text-[10px] text-text-secondary">
                          {country.isoCode}
                        </span>
                      </div>
                      <div className="h-0.5 w-full max-w-[140px] overflow-hidden rounded-full bg-surface-hover">
                        <div
                          className={`h-full rounded-full ${rowPct === 100 ? "bg-positive" : "bg-accent"}`}
                          style={{ width: `${rowPct}%` }}
                        />
                      </div>
                    </div>
                  </td>
                  {macros.map((macro) => {
                    const key = `${country.idCountry}-${macro.idMacroIndicator}`;
                    const cell = cellMap.get(key);
                    return (
                      <td key={macro.idMacroIndicator} className="px-2 py-1">
                        <EditableCell
                          value={cell?.value ?? null}
                          isSaving={savingKey === key}
                          onCommit={(val) =>
                            handleCellCommit(
                              country.idCountry,
                              macro.idMacroIndicator,
                              val,
                            )
                          }
                        />
                      </td>
                    );
                  })}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
