import { useState, useRef, useEffect } from "react";

interface EditableCellProps {
  value: number | null;
  onCommit: (value: number | null) => void;
  isSaving: boolean;
}

export function EditableCell({ value, onCommit, isSaving }: EditableCellProps) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (editing) {
      setDraft(value !== null ? String(value) : "");
      inputRef.current?.focus();
      inputRef.current?.select();
    }
  }, [editing, value]);

  const commit = () => {
    setEditing(false);
    const trimmed = draft.trim();
    if (trimmed === "") {
      if (value !== null) onCommit(null);
      return;
    }
    const parsed = Number(trimmed);
    if (!Number.isNaN(parsed) && parsed !== value) {
      onCommit(parsed);
    }
  };

  if (editing) {
    return (
      <input
        ref={inputRef}
        value={draft}
        onChange={(e) => setDraft(e.target.value)}
        onBlur={commit}
        onKeyDown={(e) => {
          if (e.key === "Enter") commit();
          if (e.key === "Escape") setEditing(false);
        }}
        className="w-full rounded bg-surface-hover px-2 py-1 text-right font-mono text-xs text-accent outline-none"
      />
    );
  }

  return (
    <button
      onDoubleClick={() => setEditing(true)}
      disabled={isSaving}
      title="Double-click to edit"
      className={`w-full cursor-cell rounded px-2 py-1 text-right font-mono text-xs transition-colors hover:bg-surface-hover ${
        value === null ? "text-text-secondary" : "text-text-primary"
      } ${isSaving ? "opacity-50" : ""}`}
    >
      {isSaving ? "..." : value !== null ? value.toFixed(2) : "—"}
    </button>
  );
}
