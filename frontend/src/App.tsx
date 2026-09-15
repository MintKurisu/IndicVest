import { Routes, Route, Navigate } from "react-router-dom";
import { DashboardShell } from "./components/layout/DashboardShell";
import { CountriesPage } from "./pages/CountriesPage";
import { MacroIndicatorsPage } from "./pages/MacroIndicatorsPage";
// import { RankingPage } from './pages/RankingPage'
// import { SimulationPage } from './pages/SimulationPage'
import { IndicatorsPage } from "./pages/IndicatorsPage";
// import { ReturnRateSettingsPage } from './pages/ReturnRateSettingsPage'

function App() {
  return (
    <Routes>
      <Route element={<DashboardShell />}>
        <Route index element={<Navigate to="/countries" replace />} />
        <Route path="countries" element={<CountriesPage />} />
        <Route path="macro-indicators" element={<MacroIndicatorsPage />} />
        {/* <Route path="ranking" element={<RankingPage />} /> */}
        {/* <Route path="simulation" element={<SimulationPage />} /> */}
        {<Route path="indicators" element={<IndicatorsPage />} />}
        {/* <Route path="settings/return-rate" element={<ReturnRateSettingsPage />} /> */}
      </Route>
    </Routes>
  );
}

export default App;
