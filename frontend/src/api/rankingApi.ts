import { apiClient } from "./client";
import type { RankingResult } from "./types";

export const rankingApi = {
  getAvailableYears: () =>
    apiClient.get<number[]>("/ranking/years").then((r) => r.data),

  calculate: (selectedYear: number) =>
    apiClient
      .post<RankingResult>("/ranking/calculate", selectedYear)
      .then((r) => r.data),
};
