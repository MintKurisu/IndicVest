import { useEffect, useState } from "react";
import { SlidersHorizontal, CircleAlert } from "lucide-react";
import {
  useReturnRateConfig,
  useUpdateReturnRateConfig,
} from "../hooks/useReturnRateConfig";
import { ReturnRateBoundCard } from "../components/returnRate/ReturnRateBoundCard";
import { RangeBoundsMeter } from "../components/returnRate/RangeBoundsMeter";
import { getApiErrorMessage } from "../lib/apiError";

export function ReturnRateSettingsPage() {
  const { data, isLoading, isError } = useReturnRateConfig();
  const updateConfig = useUpdateReturnRateConfig();

  const [minPct, setMinPct] = useState(0);
  const [maxPct, setMaxPct] = useState(0);
  const [serverError, setServerError] = useState<string | null>(null);

  // Sync local draft when server data arrives (only until the user edits)
  const [dirty, setDirty] = useState(false);
  useEffect(() => {
    if (data && !dirty) {
      setMinPct(data.minReturnRate * 100);
      setMaxPct(data.maxReturnRate * 100);
    }
  }, [data, dirty]);

  if (isLoading)
    return <p className="text-text-secondary">Loading return rate config...</p>;
  if (isError)
    return <p className="text-negative">Failed to load return rate config.</p>;

  const isInvalid = maxPct <= minPct;
  const hasChanges =
    data != null &&
    (Math.round(minPct * 100) !== Math.round(data.minReturnRate * 10000) ||
      Math.round(maxPct * 100) !== Math.round(data.maxReturnRate * 10000));

  const handleChange = (setter: (v: number) => void) => (v: number) => {
    setDirty(true);
    setter(v);
  };

  const handleSave = () => {
    if (isInvalid) return;
    setServerError(null);
    updateConfig
      .mutateAsync({
        minReturnRate: minPct / 100,
        maxReturnRate: maxPct / 100,
      })
      .then(() => setDirty(false))
      .catch((err) => setServerError(getApiErrorMessage(err)));
  };

  return (
    <div>
      <div className="mb-2 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">
            Return Rate Bounds
          </h1>
          <p className="mt-1 max-w-2xl text-xs text-text-secondary">
            Configure the global floor and ceiling that clamp the estimated
            return rate produced by the ranking engine.
          </p>
        </div>
        <div className="flex items-center gap-2">
          {hasChanges && !isInvalid && (
            <span className="flex items-center gap-1 font-mono text-[11px] text-warning">
              <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-warning" />
              Unsaved changes
            </span>
          )}
          <button
            onClick={handleSave}
            disabled={!hasChanges || isInvalid || updateConfig.isPending}
            className="flex items-center gap-1.5 rounded-md bg-accent px-4 py-2 text-sm font-medium text-bg hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-50"
          >
            <SlidersHorizontal size={16} />
            {updateConfig.isPending ? "Saving..." : "Save bounds"}
          </button>
        </div>
      </div>

      {serverError && (
        <div className="mb-4 flex items-center gap-2 rounded-md bg-negative/10 px-3 py-2 text-sm text-negative">
          <CircleAlert size={16} />
          {serverError}
        </div>
      )}

      <div className="mt-4 flex flex-col gap-4 rounded-lg border border-border p-4">
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          <ReturnRateBoundCard
            label="Minimum Floor (r_min)"
            description="Prevents the engine from projecting negative or unrealistically low returns for low-risk countries."
            valuePct={minPct}
            onChange={handleChange(setMinPct)}
            accentClassName="text-positive"
          />
          <ReturnRateBoundCard
            label="Maximum Ceiling (r_max)"
            description="Caps unrealistic return projections driven by high-volatility or high-risk countries."
            valuePct={maxPct}
            onChange={handleChange(setMaxPct)}
            accentClassName="text-negative"
          />
        </div>

        <RangeBoundsMeter minPct={minPct} maxPct={maxPct} />

        {isInvalid && (
          <p className="text-xs text-negative">
            The floor must be strictly less than the ceiling.
          </p>
        )}
      </div>
    </div>
  );
}
