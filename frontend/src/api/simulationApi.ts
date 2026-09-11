import { apiClient } from "./client";
import type {
  AvailableMacro,
  MacroWithWeight,
  RankingResult,
  SimulationRequest,
  ValidateConfigResult,
} from "./types";

export const simulationApi = {
  getAvailableMacros: () =>
    apiClient
      .get<AvailableMacro[]>("/simulation/available-macros")
      .then((r) => r.data),

  validateConfig: (config: MacroWithWeight[]) =>
    apiClient
      .post<ValidateConfigResult>("/simulation/validate-config", config)
      .then((r) => r.data),

  run: (payload: SimulationRequest) =>
    apiClient
      .post<RankingResult>("/simulation/run", payload)
      .then((r) => r.data),
};
