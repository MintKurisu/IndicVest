import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { countryApi } from '../api/countryApi'
import type { SaveCountryPayload } from '../api/types'

const COUNTRIES_KEY = ['countries']

export function useCountries() {
  return useQuery({
    queryKey: COUNTRIES_KEY,
    queryFn: countryApi.getAll,
  })
}

export function useCreateCountry() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: SaveCountryPayload) => countryApi.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COUNTRIES_KEY })
    },
  })
}

export function useUpdateCountry() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: SaveCountryPayload }) =>
      countryApi.update(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COUNTRIES_KEY })
    },
  })
}

export function useDeleteCountry() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => countryApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COUNTRIES_KEY })
    },
  })
}