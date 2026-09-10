import client from "./client";

export const getCountries = () => client.get("/country");
export const createCountry = (data: SaveCountryViewModel) =>
  client.post("/country", data);
export const updateCountry = (id: number, data: SaveCountryViewModel) =>
  client.put(`/country/${id}`, data);
export const deleteCountry = (id: number) => client.delete(`/country/${id}`);
