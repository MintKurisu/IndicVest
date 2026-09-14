import { useQuery } from '@tanstack/react-query'
import { macroIndicatorApi } from '../api/macroIndicatorApi'

export function useMacroIndicators() {
  return useQuery({
    queryKey: ['macroIndicators'],
    queryFn: macroIndicatorApi.getAll,
  })
}