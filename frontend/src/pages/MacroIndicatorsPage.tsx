import { useState } from "react";
import { PieChart, Pie, Cell } from "recharts";
import { Plus, Pencil, Trash2, Scale } from "lucide-react";
import {
  useMacroIndicators,
  useRemainingWeight,
  useCreateMacroIndicator,
  useUpdateMacroIndicator,
  useDeleteMacroIndicator,
} from "../hooks/useMacroIndicators";
import { MacroIndicatorDrawer } from "../components/macroIndicators/MacroIndicatorDrawer";
import { getApiErrorMessage } from "../lib/apiError";
import type { MacroIndicator } from "../api/types";

const PALETTE = [
  "#38bdf8",
  "#10b981",
  "#a78bfa",
  "#fbbf24",
  "#f472b6",
  "#2dd4bf",
  "#fb923c",
  "#f87171",
];

export function MacroIndicatorsPage() {
  const { data: macros, isLoading, isError } = useMacroIndicators();
  const { data: remaining } = useRemainingWeight();
  const createMacro = useCreateMacroIndicator();
  const updateMacro = useUpdateMacroIndicator();
  const deleteMacro = useDeleteMacroIndicator();

  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editing, setEditing] = useState<MacroIndicator | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);

  const openCreate = () => {
    setEditing(null);
    setServerError(null);
    setDrawerOpen(true);
  };

  const openEdit = (macro: MacroIndicator) => {
    setEditing(macro);
    setServerError(null);
    setDrawerOpen(true);
  };

  const handleSubmit = (values: {
    name: string;
    weight: number;
    isHighBetter: boolean;
  }) => {
    setServerError(null);
    const mutation = editing
      ? updateMacro.mutateAsync({
          id: editing.idMacroIndicator,
          payload: values,
        })
      : createMacro.mutateAsync(values);

    mutation
      .then(() => setDrawerOpen(false))
      .catch((err) => setServerError(getApiErrorMessage(err)));
  };

  const handleDelete = (id: number) => {
    if (!confirm("Delete this macroindicator?")) return;
    deleteMacro.mutate(id);
  };

  if (isLoading)
    return <p className="text-text-secondary">Loading macroindicators...</p>;
  if (isError)
    return <p className="text-negative">Failed to load macroindicators.</p>;

  const totalWeight =
    remaining?.totalWeight ??
    macros?.reduce((sum, m) => sum + m.weight, 0) ??
    0;
  const remainingWeight =
    remaining?.remainingWeight ?? Math.max(0, 1 - totalWeight);
  const availableForNew = editing
    ? remainingWeight + editing.weight
    : remainingWeight;

  const chartData =
    macros?.map((m, i) => ({
      name: m.name,
      value: m.weight,
      color: PALETTE[i % PALETTE.length],
    })) ?? [];
  if (remainingWeight > 0.001) {
    chartData.push({
      name: "Unallocated",
      value: remainingWeight,
      color: "#1f2937",
    });
  }

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">
            Weight Allocation Schema
          </h1>
          <p className="mt-1 font-mono text-xs text-text-secondary">
            Σ w = 1.00 · scoring formula depends on this distribution
          </p>
        </div>
        <button
          onClick={openCreate}
          disabled={remainingWeight <= 0}
          className="flex items-center gap-1.5 rounded-md bg-accent px-4 py-2 text-sm font-medium text-bg hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-50"
        >
          <Plus size={16} />
          Add macroindicator
        </button>
      </div>

      {/* Summary panel: donut + ribbon */}
      <div className="mb-6 grid grid-cols-1 gap-4 lg:grid-cols-12">
        <div className="flex flex-col items-center justify-center gap-3 rounded-lg border border-border bg-surface p-4 lg:col-span-4">
          <div className="relative">
            <PieChart width={160} height={160}>
              <Pie
                data={chartData}
                dataKey="value"
                innerRadius={55}
                outerRadius={75}
                strokeWidth={0}
              >
                {chartData.map((entry, i) => (
                  <Cell key={i} fill={entry.color} />
                ))}
              </Pie>
            </PieChart>
            <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
              <span className="font-mono text-lg font-semibold">
                {(totalWeight * 100).toFixed(0)}%
              </span>
              <span className="font-mono text-[9px] uppercase tracking-wider text-text-secondary">
                Allocated
              </span>
            </div>
          </div>
          <div
            className={`flex items-center gap-1.5 rounded-md px-3 py-1 font-mono text-[11px] font-medium ${
              remainingWeight <= 0.001
                ? "bg-positive/10 text-positive"
                : "bg-warning/10 text-warning"
            }`}
          >
            <Scale size={12} />
            {remainingWeight <= 0.001
              ? "FULLY ALLOCATED"
              : `${(remainingWeight * 100).toFixed(1)}% REMAINING`}
          </div>
        </div>

        <div className="flex flex-col justify-center gap-3 rounded-lg border border-border bg-surface p-4 lg:col-span-8">
          <span className="font-mono text-[11px] uppercase tracking-wider text-text-secondary">
            Cumulative weight ribbon
          </span>
          <div className="flex h-7 w-full overflow-hidden rounded-md bg-bg">
            {chartData.map((entry, i) => (
              <div
                key={i}
                title={`${entry.name}: ${(entry.value * 100).toFixed(1)}%`}
                style={{
                  width: `${entry.value * 100}%`,
                  backgroundColor: entry.color,
                }}
                className="flex items-center justify-center font-mono text-[10px] font-medium text-bg"
              >
                {entry.value > 0.08 ? `${(entry.value * 100).toFixed(0)}%` : ""}
              </div>
            ))}
          </div>
          <div className="flex flex-wrap gap-x-4 gap-y-1 font-mono text-[11px] text-text-secondary">
            {macros?.map((m, i) => (
              <span
                key={m.idMacroIndicator}
                className="flex items-center gap-1.5"
              >
                <span
                  className="h-2 w-2 rounded-sm"
                  style={{ backgroundColor: PALETTE[i % PALETTE.length] }}
                />
                {m.name} ({(m.weight * 100).toFixed(0)}%)
              </span>
            ))}
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-hidden rounded-lg border border-border">
        {macros?.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-center">
            <Scale className="mb-3 text-text-secondary" size={32} />
            <p className="text-sm text-text-secondary">
              No macroindicators configured yet.
            </p>
            <button
              onClick={openCreate}
              className="mt-4 text-sm font-medium text-accent hover:underline"
            >
              Add your first macroindicator
            </button>
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead className="bg-surface text-text-secondary">
              <tr className="font-mono text-[11px] uppercase tracking-wider">
                <th className="px-4 py-3 font-medium">Name</th>
                <th className="px-4 py-3 font-medium">Direction</th>
                <th className="px-4 py-3 font-medium">Weight</th>
                <th className="px-4 py-3 font-medium">Indicators</th>
                <th className="w-20 px-4 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {macros?.map((macro, i) => (
                <tr
                  key={macro.idMacroIndicator}
                  className="border-t border-border hover:bg-surface-hover"
                >
                  <td className="px-4 py-3 font-medium">{macro.name}</td>
                  <td className="px-4 py-3">
                    <span
                      className={`rounded px-2 py-0.5 font-mono text-xs font-medium ${
                        macro.isHighBetter
                          ? "bg-positive/10 text-positive"
                          : "bg-negative/10 text-negative"
                      }`}
                    >
                      {macro.isHighBetter ? "▲ HIGHER" : "▼ LOWER"}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex w-32 flex-col gap-1">
                      <span className="font-mono text-xs">
                        {(macro.weight * 100).toFixed(1)}%
                      </span>
                      <div className="h-1 w-full overflow-hidden rounded-full bg-surface-hover">
                        <div
                          className="h-full rounded-full"
                          style={{
                            width: `${macro.weight * 100}%`,
                            backgroundColor: PALETTE[i % PALETTE.length],
                          }}
                        />
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-text-secondary">
                    {macro.indicatorsQuantity ?? 0}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-3">
                      <button
                        onClick={() => openEdit(macro)}
                        title="Edit"
                        className="text-text-secondary hover:text-accent"
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        onClick={() => handleDelete(macro.idMacroIndicator)}
                        title="Delete"
                        className="text-text-secondary hover:text-negative"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <MacroIndicatorDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        onSubmit={handleSubmit}
        editing={editing}
        isSubmitting={createMacro.isPending || updateMacro.isPending}
        serverError={serverError}
        availableWeight={availableForNew}
      />
    </div>
  );
}
