import { apiClient } from "./client";
import type {
  AvailableMacro,
  ValidateConfigResult,
  SimulationRequest,
  RankingResult,
  MacroWithWeight,
} from "./types";

export const simulationApi = {
  getAvailableMacros: () =>
    apiClient
      .get<AvailableMacro[]>("/simulation/available-macros")
      .then((r) => r.data),

  validateConfig: (configuration: MacroWithWeight[]) =>
    apiClient
      .post<ValidateConfigResult>("/simulation/validate-config", configuration)
      .then((r) => r.data),

  run: (payload: SimulationRequest) =>
    apiClient.post<RankingResult>("/simulation/run", payload).then((r) => r.data),
};