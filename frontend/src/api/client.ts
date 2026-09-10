import axios from "axios";

const client = axios.create({
  baseURL: "https://localhost:7129/api",
});

export default client;
