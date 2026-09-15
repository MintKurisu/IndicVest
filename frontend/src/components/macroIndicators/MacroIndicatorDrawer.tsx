import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useEffect } from "react";
import { X, Scale } from "lucide-react";
import type { MacroIndicator } from "../../api/types";

const macroSchema = z.object({
  name: z.string().min(1, "Name is required").max(100),
  weight: z.coerce
    .number()
    .min(0.01, "Weight must be greater than 0")
    .max(1, "Weight cannot exceed 1.0"),
  isHighBetter: z.boolean(),
});

type MacroFormInput = z.input<typeof macroSchema>;
type MacroFormValues = z.output<typeof macroSchema>;

interface MacroIndicatorDrawerProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (values: MacroFormValues) => void;
  editing: MacroIndicator | null;
  isSubmitting: boolean;
  serverError: string | null;
  availableWeight: number;
}

export function MacroIndicatorDrawer({
  open,
  onClose,
  onSubmit,
  editing,
  isSubmitting,
  serverError,
  availableWeight,
}: MacroIndicatorDrawerProps) {
  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<MacroFormInput, any, MacroFormValues>({
    resolver: zodResolver(macroSchema),
    defaultValues: { name: "", weight: 0, isHighBetter: true },
  });

  useEffect(() => {
    if (open) {
      reset(
        editing
          ? {
              name: editing.name,
              weight: editing.weight,
              isHighBetter: editing.isHighBetter,
            }
          : { name: "", weight: 0, isHighBetter: true },
      );
    }
  }, [open, editing, reset]);

  const liveWeight = Number(watch("weight")) || 0;
  const overBudget = liveWeight > availableWeight;

  return (
    <>
      <div
        onClick={onClose}
        className={`fixed inset-0 z-40 bg-black/60 backdrop-blur-sm transition-opacity ${
          open ? "opacity-100" : "pointer-events-none opacity-0"
        }`}
      />
      <div
        className={`fixed inset-y-0 right-0 z-50 flex w-full max-w-md flex-col border-l border-border bg-surface transition-transform duration-200 ${
          open ? "translate-x-0" : "translate-x-full"
        }`}
      >
        <div className="flex items-center justify-between border-b border-border px-6 py-4">
          <div className="flex items-center gap-2">
            <Scale className="text-accent" size={18} />
            <div>
              <h2 className="text-sm font-semibold">
                {editing ? "Edit macroindicator" : "New macroindicator"}
              </h2>
              <p className="font-mono text-[10px] uppercase tracking-wider text-text-secondary">
                Weight allocation schema
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="text-text-secondary hover:text-text-primary"
          >
            <X size={18} />
          </button>
        </div>

        <form
          onSubmit={handleSubmit(onSubmit)}
          className="flex flex-1 flex-col gap-5 overflow-y-auto px-6 py-5"
        >
          <div>
            <label className="mb-1 block text-sm text-text-secondary">
              Name
            </label>
            <input
              {...register("name")}
              className="w-full rounded-md border border-border bg-bg px-3 py-2 text-sm outline-none focus:border-accent"
              placeholder="e.g. GDP Growth Rate"
            />
            {errors.name && (
              <p className="mt-1 text-xs text-negative">
                {errors.name.message}
              </p>
            )}
          </div>

          <div>
            <label className="mb-1 block text-sm text-text-secondary">
              Weight
            </label>
            <div className="flex items-center gap-2">
              <input
                {...register("weight")}
                type="number"
                step="0.01"
                min="0"
                max="1"
                className="w-full rounded-md border border-border bg-bg px-3 py-2 font-mono text-sm outline-none focus:border-accent"
                placeholder="0.25"
              />
              <span className="font-mono text-sm text-text-secondary">
                = {(liveWeight * 100).toFixed(0)}%
              </span>
            </div>
            {errors.weight && (
              <p className="mt-1 text-xs text-negative">
                {errors.weight.message}
              </p>
            )}

            <div className="mt-2 flex items-center justify-between font-mono text-[11px]">
              <span className="text-text-secondary">
                Available: {(availableWeight * 100).toFixed(1)}%
              </span>
              <span className={overBudget ? "text-negative" : "text-positive"}>
                {overBudget ? "EXCEEDS BUDGET" : "WITHIN BUDGET"}
              </span>
            </div>
            <div className="mt-1 h-1.5 w-full overflow-hidden rounded-full bg-surface-hover">
              <div
                className={`h-full rounded-full transition-all ${overBudget ? "bg-negative" : "bg-accent"}`}
                style={{
                  width: `${Math.min(100, (liveWeight / Math.max(availableWeight, 0.0001)) * 100)}%`,
                }}
              />
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm text-text-secondary">
              Optimization direction
            </label>
            <div className="grid grid-cols-2 gap-2">
              <label className="flex cursor-pointer items-center gap-2 rounded-md border border-border bg-bg p-3 hover:bg-surface-hover has-[:checked]:border-positive">
                <input
                  type="radio"
                  value="true"
                  {...register("isHighBetter", {
                    setValueAs: (v) => v === "true" || v === true,
                  })}
                  className="accent-positive"
                  defaultChecked
                />
                <div>
                  <p className="text-xs font-medium text-positive">
                    ▲ Higher is better
                  </p>
                  <p className="text-[10px] text-text-secondary">
                    Direct correlation
                  </p>
                </div>
              </label>
              <label className="flex cursor-pointer items-center gap-2 rounded-md border border-border bg-bg p-3 hover:bg-surface-hover has-[:checked]:border-negative">
                <input
                  type="radio"
                  value="false"
                  {...register("isHighBetter", {
                    setValueAs: (v) => v === "true" || v === true,
                  })}
                  className="accent-negative"
                />
                <div>
                  <p className="text-xs font-medium text-negative">
                    ▼ Lower is better
                  </p>
                  <p className="text-[10px] text-text-secondary">
                    Inverse correlation
                  </p>
                </div>
              </label>
            </div>
          </div>

          {serverError && (
            <p className="rounded-md bg-negative/10 px-3 py-2 text-xs text-negative">
              {serverError}
            </p>
          )}

          <div className="mt-auto flex justify-end gap-2 border-t border-border pt-4">
            <button
              type="button"
              onClick={onClose}
              className="rounded-md px-4 py-2 text-sm text-text-secondary hover:bg-surface-hover"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-bg hover:bg-accent-hover disabled:opacity-50"
            >
              {isSubmitting ? "Saving..." : "Save"}
            </button>
          </div>
        </form>
      </div>
    </>
  );
}
