import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { indicatorApi } from '../api/indicatorApi'
import type { SaveIndicatorPayload } from '../api/types'

const INDICATORS_KEY = ['indicators']
const YEARS_KEY = ['indicators', 'years']

export function useIndicators() {
  return useQuery({
    queryKey: INDICATORS_KEY,
    queryFn: indicatorApi.getAll,
  })
}

export function useIndicatorYears() {
  return useQuery({
    queryKey: YEARS_KEY,
    queryFn: indicatorApi.getYears,
  })
}

function useInvalidateIndicators() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: INDICATORS_KEY })
    queryClient.invalidateQueries({ queryKey: YEARS_KEY })
  }
}

export function useCreateIndicator() {
  const invalidate = useInvalidateIndicators()
  return useMutation({
    mutationFn: (payload: SaveIndicatorPayload) => indicatorApi.create(payload),
    onSuccess: invalidate,
  })
}

export function useUpdateIndicator() {
  const invalidate = useInvalidateIndicators()
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: SaveIndicatorPayload }) =>
      indicatorApi.update(id, payload),
    onSuccess: invalidate,
  })
}

export function useDeleteIndicator() {
  const invalidate = useInvalidateIndicators()
  return useMutation({
    mutationFn: (id: number) => indicatorApi.remove(id),
    onSuccess: invalidate,
  })
}