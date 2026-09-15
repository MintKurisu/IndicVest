import { useMemo, useState } from "react";
import { Database, Search, Globe2 } from "lucide-react";
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

  const currentYear = new Date().getFullYear();
  const [selectedYear, setSelectedYear] = useState<number>(currentYear);
  const [search, setSearch] = useState("");
  const [savingKey, setSavingKey] = useState<string | null>(null);

  const yearOptions = useMemo(() => {
    const set = new Set(years ?? []);
    set.add(selectedYear);
    return Array.from(set).sort((a, b) => a - b);
  }, [years, selectedYear]);

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

  const totalCells = (countries?.length ?? 0) * (macros?.length ?? 0);
  const filledCells = cellMap.size;
  const completionPct =
    totalCells > 0 ? Math.round((filledCells / totalCells) * 100) : 0;
  const yearsTracked = years?.length ?? 0;

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

      {/* Filters */}
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
        <div className="flex items-center gap-1 rounded bg-bg p-0.5">
          {yearOptions.map((y) => (
            <button
              key={y}
              onClick={() => setSelectedYear(y)}
              className={`rounded px-2 py-1 font-mono text-xs transition-colors ${
                y === selectedYear
                  ? "bg-accent text-bg font-semibold"
                  : "text-text-secondary hover:text-text-primary"
              }`}
            >
              {y}
            </button>
          ))}
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
