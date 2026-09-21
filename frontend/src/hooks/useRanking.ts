import { useMutation, useQuery } from "@tanstack/react-query";
import { rankingApi } from "../api/rankingApi";

const RANKING_YEARS_KEY = ["rankingYears"];

export function useRankingYears() {
  return useQuery({
    queryKey: RANKING_YEARS_KEY,
    queryFn: rankingApi.getAvailableYears,
  });
}

export function useCalculateRanking() {
  return useMutation({
    mutationFn: (year: number) => rankingApi.calculate(year),
  });
}