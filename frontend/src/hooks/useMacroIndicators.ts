import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { macroIndicatorApi } from '../api/macroIndicatorApi'
import type { SaveMacroIndicatorPayload } from '../api/types'

const MACROS_KEY = ['macroIndicators']
const REMAINING_KEY = ['macroIndicators', 'remaining-weight']

export function useMacroIndicators() {
  return useQuery({
    queryKey: MACROS_KEY,
    queryFn: macroIndicatorApi.getAll,
  })
}

export function useRemainingWeight() {
  return useQuery({
    queryKey: REMAINING_KEY,
    queryFn: macroIndicatorApi.getRemainingWeight,
  })
}

function useInvalidateMacros() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: MACROS_KEY })
    queryClient.invalidateQueries({ queryKey: REMAINING_KEY })
  }
}

export function useCreateMacroIndicator() {
  const invalidate = useInvalidateMacros()
  return useMutation({
    mutationFn: (payload: SaveMacroIndicatorPayload) => macroIndicatorApi.create(payload),
    onSuccess: invalidate,
  })
}

export function useUpdateMacroIndicator() {
  const invalidate = useInvalidateMacros()
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: SaveMacroIndicatorPayload }) =>
      macroIndicatorApi.update(id, payload),
    onSuccess: invalidate,
  })
}

export function useDeleteMacroIndicator() {
  const invalidate = useInvalidateMacros()
  return useMutation({
    mutationFn: (id: number) => macroIndicatorApi.remove(id),
    onSuccess: invalidate,
  })
}