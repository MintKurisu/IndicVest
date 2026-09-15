import { useState } from "react";
import { Plus, Pencil, Trash2, Globe2 } from "lucide-react";
import {
  useCountries,
  useCreateCountry,
  useUpdateCountry,
  useDeleteCountry,
} from "../hooks/useCountries";
import { useMacroIndicators } from "../hooks/useMacroIndicators";
import { CountryDrawer } from "../components/countries/CountryDrawer";
import { getApiErrorMessage } from "../lib/apiError";
import type { Country } from "../api/types";

export function CountriesPage() {
  const { data: countries, isLoading, isError } = useCountries();
  const { data: macroIndicators } = useMacroIndicators();
  const createCountry = useCreateCountry();
  const updateCountry = useUpdateCountry();
  const deleteCountry = useDeleteCountry();

  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editingCountry, setEditingCountry] = useState<Country | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);

  const openCreate = () => {
    setEditingCountry(null);
    setServerError(null);
    setDrawerOpen(true);
  };

  const openEdit = (country: Country) => {
    setEditingCountry(country);
    setServerError(null);
    setDrawerOpen(true);
  };

  const handleSubmit = (values: { name: string; isoCode: string }) => {
    setServerError(null);
    const mutation = editingCountry
      ? updateCountry.mutateAsync({
          id: editingCountry.idCountry,
          payload: values,
        })
      : createCountry.mutateAsync(values);

    mutation
      .then(() => setDrawerOpen(false))
      .catch((err) => setServerError(getApiErrorMessage(err)));
  };

  const handleDelete = (id: number) => {
    if (!confirm("Delete this country?")) return;
    deleteCountry.mutate(id);
  };

  if (isLoading)
    return <p className="text-text-secondary">Loading countries...</p>;
  if (isError)
    return <p className="text-negative">Failed to load countries.</p>;

  const total = countries?.length ?? 0;
  const totalMacros = macroIndicators?.length ?? 0;
  const fullyCovered =
    countries?.filter(
      (c) => (c.indicatorsQuantity ?? 0) >= totalMacros && totalMacros > 0,
    ).length ?? 0;
  const noData =
    countries?.filter((c) => (c.indicatorsQuantity ?? 0) === 0).length ?? 0;

  return (
    <div>
      <div className="mb-2 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">
            Country Registry
          </h1>
          <div className="mt-1 flex flex-wrap items-center gap-2 font-mono text-xs text-text-secondary">
            <span>
              <strong className="text-text-primary">{total}</strong> registered
            </span>
            <span className="text-border">/</span>
            <span>
              <strong className="text-positive">{fullyCovered}</strong> fully
              covered
            </span>
            <span className="text-border">/</span>
            <span>
              <strong
                className={noData > 0 ? "text-warning" : "text-text-primary"}
              >
                {noData}
              </strong>{" "}
              without data
            </span>
          </div>
        </div>
        <button
          onClick={openCreate}
          className="flex items-center gap-1.5 rounded-md bg-accent px-4 py-2 text-sm font-medium text-bg hover:bg-accent-hover"
        >
          <Plus size={16} />
          Register country
        </button>
      </div>

      <div className="mt-4 overflow-hidden rounded-lg border border-border">
        {total === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-center">
            <Globe2 className="mb-3 text-text-secondary" size={32} />
            <p className="text-sm text-text-secondary">
              No countries registered yet.
            </p>
            <button
              onClick={openCreate}
              className="mt-4 text-sm font-medium text-accent hover:underline"
            >
              Register your first country
            </button>
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead className="bg-surface text-text-secondary">
              <tr className="font-mono text-[11px] uppercase tracking-wider">
                <th className="px-4 py-3 font-medium">Country</th>
                <th className="px-4 py-3 font-medium">ISO</th>
                <th className="px-4 py-3 font-medium">Data coverage</th>
                <th className="w-20 px-4 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {countries?.map((country) => {
                const count = country.indicatorsQuantity ?? 0;
                const pct =
                  totalMacros > 0
                    ? Math.min(100, Math.round((count / totalMacros) * 100))
                    : 0;
                const isEmpty = count === 0;

                return (
                  <tr
                    key={country.idCountry}
                    className={`border-t transition-colors ${
                      isEmpty
                        ? "border-warning/20 bg-warning/5 hover:bg-warning/10"
                        : "border-border hover:bg-surface-hover"
                    }`}
                  >
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <Globe2
                          size={14}
                          className={
                            isEmpty ? "text-warning" : "text-text-secondary"
                          }
                        />
                        <span className="font-medium">{country.name}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <span className="rounded bg-bg px-2 py-0.5 font-mono text-xs tracking-wide text-text-secondary">
                        {country.isoCode}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex w-40 flex-col gap-1">
                        <div className="flex items-center justify-between font-mono text-[11px]">
                          <span
                            className={
                              isEmpty ? "text-warning" : "text-text-secondary"
                            }
                          >
                            {count}/{totalMacros || "—"}
                          </span>
                          {isEmpty && (
                            <span className="text-warning">NO DATA</span>
                          )}
                        </div>
                        <div className="h-1 w-full overflow-hidden rounded-full bg-surface-hover">
                          <div
                            className={`h-full rounded-full ${isEmpty ? "bg-warning/50" : "bg-positive"}`}
                            style={{ width: `${pct}%` }}
                          />
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-3">
                        <button
                          onClick={() => openEdit(country)}
                          title="Edit"
                          className="text-text-secondary hover:text-accent"
                        >
                          <Pencil size={16} />
                        </button>
                        <button
                          onClick={() => handleDelete(country.idCountry)}
                          title="Delete"
                          className="text-text-secondary hover:text-negative"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      <CountryDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        onSubmit={handleSubmit}
        editingCountry={editingCountry}
        isSubmitting={createCountry.isPending || updateCountry.isPending}
        serverError={serverError}
      />
    </div>
  );
}
