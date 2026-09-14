import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useEffect } from "react";
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

interface CountryFormModalProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (values: CountryFormValues) => void;
  editingCountry: Country | null;
  isSubmitting: boolean;
  serverError: string | null;
}

export function CountryFormModal({
  open,
  onClose,
  onSubmit,
  editingCountry,
  isSubmitting,
  serverError,
}: CountryFormModalProps) {
  const {
    register,
    handleSubmit,
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

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="w-full max-w-sm rounded-lg border border-border bg-surface p-6">
        <h2 className="mb-4 text-lg font-semibold">
          {editingCountry ? "Edit country" : "New country"}
        </h2>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <div>
            <label className="mb-1 block text-sm text-text-secondary">
              Name
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
              className="w-full rounded-md border border-border bg-bg px-3 py-2 text-sm uppercase outline-none focus:border-accent"
              placeholder="e.g. DO"
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

          <div className="mt-2 flex justify-end gap-2">
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
    </div>
  );
}
