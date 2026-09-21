import { useState } from "react";
import { PlayCircle, Loader2, TrendingUp } from "lucide-react";
import { useRankingYears, useCalculateRanking } from "../hooks/useRanking";
import { YearSelector } from "../components/ranking/YearSelector";
import { RankingStatsStrip } from "../components/ranking/RankingStatsStrip";
import { RankingTable } from "../components/ranking/RankingTable";
import { ScoreReturnBarChart } from "../components/ranking/ScoreReturnBarChart";
import { getApiErrorMessage } from "../lib/apiError";
import type { RankingResult } from "../api/types";

export function RankingPage() {
  const {
    data: years,
    isLoading: yearsLoading,
    isError: yearsError,
  } = useRankingYears();
  const calculateRanking = useCalculateRanking();

  const [selectedYear, setSelectedYear] = useState<number | null>(null);
  const [result, setResult] = useState<RankingResult | null>(null);
  const [computeMs, setComputeMs] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const effectiveYear =
    selectedYear ??
    (years && years.length > 0 ? years[years.length - 1] : null);

  const handleSelectYear = (year: number) => {
    setSelectedYear(year);
  };

  const handleCalculate = () => {
    if (effectiveYear == null) return;
    setError(null);
    const start = performance.now();
    calculateRanking
      .mutateAsync(effectiveYear)
      .then((data) => {
        setComputeMs(performance.now() - start);
        setResult(data);
      })
      .catch((err) => setError(getApiErrorMessage(err)));
  };

  if (yearsLoading)
    return <p className="text-text-secondary">Loading available years...</p>;
  if (yearsError)
    return <p className="text-negative">Failed to load available years.</p>;

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">
            Global Investment Ranking
          </h1>
          <p className="mt-1 max-w-2xl text-xs text-text-secondary">
            Runs the ranking engine for a selected year: composite score per
            country and its estimated return rate, clamped by your configured
            bounds.
          </p>
        </div>
        <div className="flex items-center gap-3">
          {years && years.length > 0 && (
            <YearSelector
              years={years}
              selectedYear={effectiveYear}
              onSelect={handleSelectYear}
              disabled={calculateRanking.isPending}
            />
          )}
          <button
            onClick={handleCalculate}
            disabled={effectiveYear == null || calculateRanking.isPending}
            className="flex items-center gap-1.5 rounded-md bg-accent px-4 py-2 text-sm font-medium text-bg hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-50"
          >
            {calculateRanking.isPending ? (
              <Loader2 size={16} className="animate-spin" />
            ) : (
              <PlayCircle size={16} />
            )}
            {calculateRanking.isPending
              ? "Calculating..."
              : "Calculate ranking"}
          </button>
        </div>
      </div>

      {error && (
        <div className="mb-4 rounded-md bg-negative/10 px-3 py-2 text-sm text-negative">
          {error}
        </div>
      )}

      {!result && !calculateRanking.isPending && (
        <div className="flex flex-col items-center justify-center rounded-lg border border-border py-16 text-center">
          <TrendingUp className="mb-3 text-text-secondary" size={32} />
          <p className="text-sm text-text-secondary">
            Select a year and run the engine to see the ranking.
          </p>
        </div>
      )}

      {result && (
        <div className="flex flex-col gap-4">
          <RankingStatsStrip rankings={result.rankings} computeMs={computeMs} />
          <div className="flex flex-col gap-4 lg:flex-row">
            <div className="lg:w-[65%]">
              <RankingTable rankings={result.rankings} />
            </div>
            <div className="lg:w-[35%]">
              <ScoreReturnBarChart rankings={result.rankings} />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
