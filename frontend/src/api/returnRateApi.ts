import { apiClient } from "./client";
import type { ReturnRateConfig, SaveReturnRateConfigPayload } from "./types";

export const returnRateApi = {
  get: () => apiClient.get<ReturnRateConfig>("/returnrate").then((r) => r.data),
  update: (payload: SaveReturnRateConfigPayload) =>
    apiClient.put<ReturnRateConfig>("/returnrate", payload).then((r) => r.data),
};
