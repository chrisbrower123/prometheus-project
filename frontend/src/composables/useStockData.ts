import { ref } from 'vue'
import { ApiError, fetchDailySummaries } from '../services/apiClient'
import { validateSymbol } from '../utils/formatters'
import type { DailySummary, ProblemDetails } from '../types/stock'

export function useStockData() {
  const data = ref<DailySummary[] | null>(null)
  const isLoading = ref(false)
  const error = ref<ProblemDetails | null>(null)
  const retryAfter = ref<number | null>(null)
  const currentSymbol = ref('')
  let activeController: AbortController | null = null
  let requestVersion = 0

  async function searchSymbol(rawSymbol: string): Promise<void> {
    activeController?.abort()
    const version = ++requestVersion
    const normalized = validateSymbol(rawSymbol)
    currentSymbol.value = normalized ?? rawSymbol.trim().toUpperCase()
    data.value = null
    error.value = null
    retryAfter.value = null

    if (normalized === null) {
      error.value = {
        status: 400,
        title: 'Invalid symbol',
        type: 'urn:stockdata:problem:invalid-symbol',
        detail: 'Enter a valid stock symbol.',
      }
      return
    }

    const controller = new AbortController()
    activeController = controller
    isLoading.value = true

    try {
      const summaries = await fetchDailySummaries(normalized, controller.signal)
      if (version === requestVersion) {
        data.value = summaries
      }
    } catch (caught: unknown) {
      if (controller.signal.aborted || version !== requestVersion) {
        return
      }

      if (caught instanceof ApiError) {
        error.value = caught.problem
        retryAfter.value = caught.retryAfter
      } else {
        error.value = {
          status: 0,
          title: 'Request failed',
          type: 'urn:stockdata:problem:unexpected',
          detail: 'The request could not be completed.',
        }
      }
    } finally {
      if (version === requestVersion) {
        isLoading.value = false
        activeController = null
      }
    }
  }

  return { data, isLoading, error, retryAfter, currentSymbol, searchSymbol }
}
