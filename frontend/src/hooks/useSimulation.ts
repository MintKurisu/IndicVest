import { useMutation, useQuery } from "@tanstack/react-query";
import { simulationApi } from "../api/simulationApi";
import type { MacroWithWeight, SimulationRequest } from "../api/types";

const AVAILABLE_MACROS_KEY = ["simulationAvailableMacros"];

export function useAvailableMacros() {
  return useQuery({
    queryKey: AVAILABLE_MACROS_KEY,
    queryFn: simulationApi.getAvailableMacros,
  });
}

export function useValidateConfig() {
  return useMutation({
    mutationFn: (configuration: MacroWithWeight[]) =>
      simulationApi.validateConfig(configuration),
  });
}

export function useRunSimulation() {
  return useMutation({
    mutationFn: (payload: SimulationRequest) => simulationApi.run(payload),
  });
}