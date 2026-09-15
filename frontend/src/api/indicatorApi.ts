import { apiClient } from "./client";
import type { Indicator, SaveIndicatorPayload } from "./types";

export const indicatorApi = {
  getAll: () => apiClient.get<Indicator[]>("/indicator").then((r) => r.data),

  getById: (id: number) =>
    apiClient.get<Indicator>(`/indicator/${id}`).then((r) => r.data),

  getYears: () =>
    apiClient.get<number[]>("/indicator/years").then((r) => r.data),

  getByCountryAndYear: (countryId: number, year: number) =>
    apiClient
      .get<Indicator[]>(`/indicator/by-country/${countryId}/year/${year}`)
      .then((r) => r.data),

  create: (payload: SaveIndicatorPayload) =>
    apiClient.post<Indicator>("/indicator", payload).then((r) => r.data),

  update: (id: number, payload: SaveIndicatorPayload) =>
    apiClient.put<Indicator>(`/indicator/${id}`, payload).then((r) => r.data),

  remove: (id: number) => apiClient.delete(`/indicator/${id}`),
};