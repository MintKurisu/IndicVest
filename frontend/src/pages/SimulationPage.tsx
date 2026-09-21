import { useEffect, useState } from "react";
import { PlayCircle, Loader2, RotateCcw } from "lucide-react";
import { useAvailableMacros, useRunSimulation } from "../hooks/useSimulation";
import { useRankingYears, useCalculateRanking } from "../hooks/useRanking";
import { YearSelector } from "../components/ranking/YearSelector";
import { MacroWeightSlider } from "../components/simulation/MacroWeightSlider";
import { WeightBudgetBar } from "../components/simulation/WeightBudgetBar";
import { SimulationResultsPanel } from "../components/simulation/SimulationResultsPanel";
import { getApiErrorMessage } from "../lib/apiError";
import type { RankingItem } from "../api/types";

export function SimulationPage() {
  const { data: availableMacros, isLoading, isError } = useAvailableMacros();
  const { data: years } = useRankingYears();
  const calculateBaseRanking = useCalculateRanking();
  const runSimulation = useRunSimulation();

  const [selectedYear, setSelectedYear] = useState<number | null>(null);
  const [weights, setWeights] = useState<Record<number, number>>({});
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<{
    base: RankingItem[];
    projected: RankingItem[];
  } | null>(null);

  // Seed sliders with real production weights once loaded
  useEffect(() => {
    if (availableMacros && Object.keys(weights).length === 0) {
      const initial: Record<number, number> = {};
      availableMacros.forEach((m) => {
        initial[m.idMacroIndicator] = m.weight;
      });
      setWeights(initial);
    }
  }, [availableMacros, weights]);

  const effectiveYear =
    selectedYear ??
    (years && years.length > 0 ? years[years.length - 1] : null);

  const totalPct = Object.values(weights).reduce((sum, w) => sum + w, 0) * 100;
  const isBudgetValid = Math.abs(totalPct - 100) < 0.05;

  const handleResetWeights = () => {
    if (!availableMacros) return;
    const reset: Record<number, number> = {};
    availableMacros.forEach((m) => {
      reset[m.idMacroIndicator] = m.weight;
    });
    setWeights(reset);
  };

  const handleRun = () => {
    if (!availableMacros || effectiveYear == null) return;
    setError(null);

    const configuration = availableMacros.map((m) => ({
      idMacroIndicator: m.idMacroIndicator,
      name: m.name,
      weight: weights[m.idMacroIndicator] ?? m.weight,
      isHighBetter: m.isHighBetter,
    }));

    Promise.all([
      calculateBaseRanking.mutateAsync(effectiveYear),
      runSimulation.mutateAsync({ year: effectiveYear, configuration }),
    ])
      .then(([baseResult, projectedResult]) => {
        setResult({
          base: baseResult.rankings,
          projected: projectedResult.rankings,
        });
      })
      .catch((err) => setError(getApiErrorMessage(err)));
  };

  const isRunning = calculateBaseRanking.isPending || runSimulation.isPending;

  if (isLoading)
    return (
      <p className="text-text-secondary">
        Loading available macroindicators...
      </p>
    );
  if (isError)
    return <p className="text-negative">Failed to load macroindicators.</p>;

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">
            Simulation Sandbox
          </h1>
          <p className="mt-1 max-w-2xl text-xs text-text-secondary">
            Adjust macroindicator weights and preview how the ranking would
            change for a given year, compared against the current production
            configuration.
          </p>
        </div>
        <div className="flex items-center gap-2">
          {years && years.length > 0 && (
            <YearSelector
              years={years}
              selectedYear={effectiveYear}
              onSelect={setSelectedYear}
              disabled={isRunning}
            />
          )}
          <button
            onClick={handleResetWeights}
            disabled={isRunning}
            className="flex items-center gap-1.5 rounded-md bg-surface px-3 py-2 text-sm text-text-secondary hover:text-text-primary disabled:cursor-not-allowed disabled:opacity-50"
          >
            <RotateCcw size={14} />
            Reset to current
          </button>
          <button
            onClick={handleRun}
            disabled={!isBudgetValid || isRunning || effectiveYear == null}
            className="flex items-center gap-1.5 rounded-md bg-accent px-4 py-2 text-sm font-medium text-bg hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isRunning ? (
              <Loader2 size={16} className="animate-spin" />
            ) : (
              <PlayCircle size={16} />
            )}
            {isRunning ? "Running..." : "Run simulation"}
          </button>
        </div>
      </div>

      {error && (
        <div className="mb-4 rounded-md bg-negative/10 px-3 py-2 text-sm text-negative">
          {error}
        </div>
      )}

      <div className="mb-4">
        <WeightBudgetBar totalPct={totalPct} />
      </div>

      <div className="flex flex-col gap-4 lg:flex-row">
        <div className="flex flex-col gap-2 lg:w-[45%]">
          {availableMacros?.map((macro) => (
            <MacroWeightSlider
              key={macro.idMacroIndicator}
              name={macro.name}
              isHighBetter={macro.isHighBetter}
              weight={weights[macro.idMacroIndicator] ?? macro.weight}
              productionWeight={macro.weight}
              onChange={(w) =>
                setWeights((prev) => ({ ...prev, [macro.idMacroIndicator]: w }))
              }
            />
          ))}
        </div>

        <div className="lg:w-[55%]">
          {!result && !isRunning && (
            <div className="flex h-full flex-col items-center justify-center rounded-lg border border-border py-16 text-center">
              <p className="text-sm text-text-secondary">
                Adjust weights and run the simulation to compare against the
                current production ranking.
              </p>
            </div>
          )}
          {result && (
            <SimulationResultsPanel
              base={result.base}
              projected={result.projected}
            />
          )}
        </div>
      </div>
    </div>
  );
}
