export interface Indicator {
  idIndicator: number;
  idCountry: number;
  idMacroIndicator: number;
  value: number;
  year: number;
  countryName?: string;
  macroIndicatorName?: string;
}

// MacroIndicator
export interface MacroIndicator {
  idMacroIndicator: number;
  name: string;
  weight: number; // 0–1
  isHighBetter: boolean;
  indicatorsQuantity?: number;
}

export interface SaveMacroIndicatorPayload {
  name: string;
  weight: number;
  isHighBetter: boolean;
}

// ReturnRate
export interface ReturnRateConfig {
  idReturnRate: number;
  minReturnRate: number;
  maxReturnRate: number;
}

export interface SaveReturnRateConfigPayload {
  minReturnRate: number;
  maxReturnRate: number;
}

// Ranking
export interface RankingItem {
  position: number;
  countryName: string;
  isoCode: string;
  scoring: number;
  estimatedReturnRate: number;
}

export interface RankingResult {
  year: number;
  rankings: RankingItem[];
}

// Simulation
export interface MacroWithWeight {
  idMacroIndicator: number;
  name: string;
  weight: number;
  isHighBetter: boolean;
}

export interface AvailableMacro {
  idMacroIndicator: number;
  name: string;
  weight: number;
  isHighBetter: boolean;
}

export interface ValidateConfigResult {
  valid: boolean;
  totalWeight: number;
}

export interface SimulationRequest {
  year: number;
  configuration: MacroWithWeight[];
}
