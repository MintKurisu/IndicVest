import { isAxiosError } from 'axios'

export function getApiErrorMessage(error: unknown, fallback = 'Something went wrong'): string {
  if (isAxiosError(error)) {
    const detail = error.response?.data?.detail
    if (typeof detail === 'string') return detail

    const errors = error.response?.data?.errors
    if (errors) {
      const firstKey = Object.keys(errors)[0]
      const firstMessage = errors[firstKey]?.[0]
      if (firstMessage) return firstMessage
    }
  }
  return fallback
}