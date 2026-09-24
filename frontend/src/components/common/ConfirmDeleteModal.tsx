import { AlertTriangle, X } from "lucide-react";

interface ConfirmDeleteModalProps {
  open: boolean;
  entityName: string;
  indicatorCount: number;
  years: number[];
  extraWarning?: string;
  isLoading?: boolean;
  isDeleting?: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

export function ConfirmDeleteModal({
  open,
  entityName,
  indicatorCount,
  years,
  extraWarning,
  isLoading,
  isDeleting,
  onCancel,
  onConfirm,
}: ConfirmDeleteModalProps) {
  if (!open) return null;

  const yearsLabel = years.length > 0 ? years.join(", ") : "—";
  const hasDependents = indicatorCount > 0;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-lg border border-border bg-surface p-5 shadow-xl">
        <div className="flex items-start justify-between gap-3">
          <div className="flex items-center gap-2 text-negative">
            <AlertTriangle size={18} />
            <h2 className="text-sm font-semibold">Confirm deletion</h2>
          </div>
          <button
            onClick={onCancel}
            className="text-text-secondary hover:text-text-primary"
          >
            <X size={16} />
          </button>
        </div>

        <div className="mt-4 text-sm text-text-secondary">
          {isLoading ? (
            <p>Checking associated records...</p>
          ) : hasDependents ? (
            <>
              <p>
                Deleting{" "}
                <strong className="text-text-primary">{entityName}</strong> will
                also permanently delete{" "}
                <strong className="text-text-primary">{indicatorCount}</strong>{" "}
                indicator record{indicatorCount === 1 ? "" : "s"} across{" "}
                <strong className="text-text-primary">{yearsLabel}</strong>.
              </p>
              {extraWarning && (
                <p className="mt-2 text-warning">{extraWarning}</p>
              )}
              <p className="mt-2">This action cannot be undone.</p>
            </>
          ) : (
            <p>
              Delete <strong className="text-text-primary">{entityName}</strong>
              ? This action cannot be undone.
            </p>
          )}
        </div>

        <div className="mt-5 flex justify-end gap-3">
          <button
            autoFocus
            onClick={onCancel}
            disabled={isDeleting}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-text-primary hover:bg-surface-hover disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            disabled={isLoading || isDeleting}
            className="rounded-md bg-negative px-4 py-2 text-sm font-medium text-bg hover:bg-negative/90 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isDeleting ? "Deleting..." : "Delete"}
          </button>
        </div>
      </div>
    </div>
  );
}
