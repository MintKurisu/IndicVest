import axios from "axios";

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7000/api/v1",
  headers: { "Content-Type": "application/json" },
});
