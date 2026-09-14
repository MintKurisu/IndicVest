import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useEffect } from "react";
import { X, Globe2 } from "lucide-react";
import type { Country } from "../../api/types";

const countrySchema = z.object({
  name: z.string().min(1, "Name is required").max(100),
  isoCode: z
    .string()
    .min(2, "ISO code must be 2-3 letters")
    .max(3, "ISO code must be 2-3 letters")
    .toUpperCase(),
});

type CountryFormValues = z.infer<typeof countrySchema>;

interface CountryDrawerProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (values: CountryFormValues) => void;
  editingCountry: Country | null;
  isSubmitting: boolean;
  serverError: string | null;
}

export function CountryDrawer({
  open,
  onClose,
  onSubmit,
  editingCountry,
  isSubmitting,
  serverError,
}: CountryDrawerProps) {
  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<CountryFormValues>({
    resolver: zodResolver(countrySchema),
    defaultValues: { name: "", isoCode: "" },
  });

  useEffect(() => {
    if (open) {
      reset(
        editingCountry
          ? { name: editingCountry.name, isoCode: editingCountry.isoCode }
          : { name: "", isoCode: "" },
      );
    }
  }, [open, editingCountry, reset]);

  const liveName = watch("name");
  const liveIso = watch("isoCode");

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
            <Globe2 className="text-accent" size={18} />
            <div>
              <h2 className="text-sm font-semibold">
                {editingCountry ? "Edit sovereign entity" : "Register country"}
              </h2>
              <p className="font-mono text-[10px] uppercase tracking-wider text-text-secondary">
                ISO-3166 reference
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
          <div className="flex items-center gap-3 rounded-md bg-bg px-3 py-3">
            <div className="flex h-9 w-9 items-center justify-center rounded bg-surface-hover font-mono text-xs text-text-secondary">
              {liveIso ? liveIso.slice(0, 3).toUpperCase() : "---"}
            </div>
            <div>
              <p className="text-sm font-medium">
                {liveName || "Unnamed entity"}
              </p>
              <p className="font-mono text-[10px] text-text-secondary">
                {liveIso ? liveIso.toUpperCase() : "PENDING CODE"}
              </p>
            </div>
          </div>

          <div>
            <label className="mb-1 block text-sm text-text-secondary">
              Country name
            </label>
            <input
              {...register("name")}
              className="w-full rounded-md border border-border bg-bg px-3 py-2 text-sm outline-none focus:border-accent"
              placeholder="e.g. Dominican Republic"
            />
            {errors.name && (
              <p className="mt-1 text-xs text-negative">
                {errors.name.message}
              </p>
            )}
          </div>

          <div>
            <label className="mb-1 block text-sm text-text-secondary">
              ISO code
            </label>
            <input
              {...register("isoCode")}
              className="w-full rounded-md border border-border bg-bg px-3 py-2 font-mono text-sm uppercase outline-none focus:border-accent"
              placeholder="e.g. DOM"
              maxLength={3}
            />
            {errors.isoCode && (
              <p className="mt-1 text-xs text-negative">
                {errors.isoCode.message}
              </p>
            )}
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
              {isSubmitting ? "Saving..." : "Save entity"}
            </button>
          </div>
        </form>
      </div>
    </>
  );
}
