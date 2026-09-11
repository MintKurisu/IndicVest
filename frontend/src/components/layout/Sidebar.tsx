import { NavLink } from "react-router-dom";

const NAV_ITEMS = [
  { to: "/ranking", label: "Ranking" },
  { to: "/simulation", label: "Simulador" },
  { to: "/countries", label: "Países" },
  { to: "/macro-indicators", label: "Macroindicadores" },
  { to: "/indicators", label: "Indicadores" },
  { to: "/settings/return-rate", label: "Configuración" },
];

export function Sidebar() {
  return (
    <aside className="w-60 shrink-0 border-r border-border bg-surface">
      <div className="px-6 py-5 text-lg font-semibold tracking-tight">
        IndicVest
      </div>
      <nav className="flex flex-col gap-1 px-3">
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `rounded-md px-3 py-2 text-sm transition-colors ${
                isActive
                  ? "bg-surface-hover text-accent"
                  : "text-text-secondary hover:bg-surface-hover hover:text-text-primary"
              }`
            }
          >
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
