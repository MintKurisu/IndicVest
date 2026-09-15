import { apiClient } from "./client";
import type { MacroIndicator, SaveMacroIndicatorPayload, RemainingWeightInfo } from "./types";

export const macroIndicatorApi = {
  getAll: () =>
    apiClient.get<MacroIndicator[]>("/macroindicator").then((r) => r.data),
  getById: (id: number) =>
    apiClient.get<MacroIndicator>(`/macroindicator/${id}`).then((r) => r.data),
  getRemainingWeight: () =>
    apiClient
      .get<RemainingWeightInfo>("/macroindicator/remaining-weight")
      .then((r) => r.data),
  create: (payload: SaveMacroIndicatorPayload) =>
    apiClient
      .post<MacroIndicator>("/macroindicator", payload)
      .then((r) => r.data),
  update: (id: number, payload: SaveMacroIndicatorPayload) =>
    apiClient
      .put<MacroIndicator>(`/macroindicator/${id}`, payload)
      .then((r) => r.data),
  remove: (id: number) => apiClient.delete(`/macroindicator/${id}`),
};