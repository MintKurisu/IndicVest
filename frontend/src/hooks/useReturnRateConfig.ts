import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { returnRateApi } from "../api/returnRateApi";
import type { SaveReturnRateConfigPayload } from "../api/types";

const RETURN_RATE_KEY = ["returnRateConfig"];

export function useReturnRateConfig() {
  return useQuery({
    queryKey: RETURN_RATE_KEY,
    queryFn: returnRateApi.get,
  });
}

export function useUpdateReturnRateConfig() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: SaveReturnRateConfigPayload) =>
      returnRateApi.update(payload),
    onSuccess: (data) => {
      queryClient.setQueryData(RETURN_RATE_KEY, data);
    },
  });
}