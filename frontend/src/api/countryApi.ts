import { apiClient } from "./client";
import type { Country, SaveCountryPayload, DeleteDependents } from "./types";

export const countryApi = {
  getAll: () => apiClient.get<Country[]>("/country").then((r) => r.data),
  getById: (id: number) =>
    apiClient.get<Country>(`/country/${id}`).then((r) => r.data),
  create: (payload: SaveCountryPayload) =>
    apiClient.post<Country>("/country", payload).then((r) => r.data),
  update: (id: number, payload: SaveCountryPayload) =>
    apiClient.put<Country>(`/country/${id}`, payload).then((r) => r.data),
  getDependents: (id: number) =>
    apiClient
      .get<DeleteDependents>(`/country/${id}/dependents`)
      .then((r) => r.data),
  remove: (id: number, cascade = false) =>
    apiClient.delete(`/country/${id}`, { params: { cascade } }),
};
