import { apiClient } from "./client";
import type { Country, SaveCountryPayload } from "./types";

export const countryApi = {
  getAll: () => apiClient.get<Country[]>("/country").then((r) => r.data),
  getById: (id: number) =>
    apiClient.get<Country>(`/country/${id}`).then((r) => r.data),
  create: (payload: SaveCountryPayload) =>
    apiClient.post<Country>("/country", payload).then((r) => r.data),
  update: (id: number, payload: SaveCountryPayload) =>
    apiClient.put<Country>(`/country/${id}`, payload).then((r) => r.data),
  remove: (id: number) => apiClient.delete(`/country/${id}`),
};
