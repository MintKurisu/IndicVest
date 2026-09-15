import { NavLink } from "react-router-dom";
import {
  TrendingUp,
  Sliders,
  Globe2,
  Scale,
  Database,
  Settings,
} from "lucide-react";

const NAV_GROUPS = [
  {
    label: "Analysis",
    items: [
      { to: "/ranking", label: "Ranking", icon: TrendingUp },
      { to: "/simulation", label: "Simulation", icon: Sliders },
    ],
  },
  {
    label: "Configuration",
    items: [
      { to: "/countries", label: "Countries", icon: Globe2 },
      { to: "/macro-indicators", label: "Macroindicators", icon: Scale },
      { to: "/indicators", label: "Indicators", icon: Database },
      { to: "/settings/return-rate", label: "Settings", icon: Settings },
    ],
  },
];

export function Sidebar() {
  return (
    <aside className="flex w-60 shrink-0 flex-col border-r border-border bg-surface">
      <div className="border-b border-border px-6 py-5">
        <span className="text-lg font-semibold tracking-tight">IndicVest</span>
        <p className="mt-0.5 font-mono text-[10px] uppercase tracking-wider text-text-secondary">
          Terminal Core
        </p>
      </div>

      <nav className="flex flex-1 flex-col gap-5 px-3 py-4">
        {NAV_GROUPS.map((group) => (
          <div key={group.label}>
            <p className="mb-1.5 px-3 font-mono text-[10px] uppercase tracking-wider text-text-secondary">
              {group.label}
            </p>
            <div className="flex flex-col gap-0.5">
              {group.items.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  className={({ isActive }) =>
                    `flex items-center gap-2.5 rounded-md px-3 py-2 text-sm transition-colors ${
                      isActive
                        ? "bg-surface-hover text-accent"
                        : "text-text-secondary hover:bg-surface-hover hover:text-text-primary"
                    }`
                  }
                >
                  <item.icon size={16} />
                  {item.label}
                </NavLink>
              ))}
            </div>
          </div>
        ))}
      </nav>

      <div className="border-t border-border px-6 py-4">
        <div className="flex items-center gap-2 font-mono text-[10px] uppercase tracking-wider text-text-secondary">
          <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-positive" />
          API Connected
        </div>
      </div>
    </aside>
  );
}
