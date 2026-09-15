import { useLocation } from "react-router-dom";
import {
  TrendingUp,
  Sliders,
  Globe2,
  Scale,
  Database,
  Settings,
  LayoutDashboard,
} from "lucide-react";

const PAGE_META: Record<
  string,
  { label: string; icon: typeof LayoutDashboard }
> = {
  "/ranking": { label: "Ranking", icon: TrendingUp },
  "/simulation": { label: "Simulation", icon: Sliders },
  "/countries": { label: "Countries", icon: Globe2 },
  "/macro-indicators": { label: "Macroindicators", icon: Scale },
  "/indicators": { label: "Indicators", icon: Database },
  "/settings/return-rate": { label: "Settings", icon: Settings },
};

export function Topbar() {
  const location = useLocation();
  const meta = PAGE_META[location.pathname] ?? {
    label: "Dashboard",
    icon: LayoutDashboard,
  };
  const Icon = meta.icon;

  return (
    <header className="flex h-14 items-center justify-between border-b border-border bg-surface px-6">
      <div className="flex items-center gap-2 text-sm">
        <Icon size={16} className="text-accent" />
        <span className="font-medium">{meta.label}</span>
      </div>

      <div className="flex items-center gap-2 rounded-md bg-bg px-3 py-1 font-mono text-[11px] uppercase tracking-wider text-text-secondary">
        <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-accent" />
        Live
      </div>
    </header>
  );
}
