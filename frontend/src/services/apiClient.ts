import type { DailySummary, ProblemDetails } from '../types/stock'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

if (typeof apiBaseUrl !== 'string' || apiBaseUrl.trim().length === 0) {
  throw new Error('VITE_API_BASE_URL is required')
}

const baseUrl = apiBaseUrl.replace(/\/$/, '')

export class ApiError extends Error {
  readonly problem: ProblemDetails
  readonly retryAfter: number | null

  constructor(problem: ProblemDetails, retryAfter: number | null = null) {
    super(problem.detail ?? problem.title)
    this.name = 'ApiError'
    this.problem = problem
    this.retryAfter = retryAfter
  }
}

function fallbackProblem(status: number, text: string): ProblemDetails {
  return {
    status,
    title: 'Request failed',
    type: 'urn:stockdata:problem:unexpected',
    detail: text.length > 0 && !/<html[\s>]/i.test(text) ? text : undefined,
  }
}

async function readProblem(response: Response): Promise<ProblemDetails> {
  const text = await response.text()
  try {
    const payload: unknown = JSON.parse(text)
    if (typeof payload === 'object' && payload !== null) {
      const value = payload as Record<string, unknown>
      return {
        status: typeof value.status === 'number' ? value.status : response.status,
        title: typeof value.title === 'string' ? value.title : 'Request failed',
        type: typeof value.type === 'string' ? value.type : 'urn:stockdata:problem:unexpected',
        detail: typeof value.detail === 'string' ? value.detail : undefined,
        traceId: typeof value.traceId === 'string' ? value.traceId : undefined,
      }
    }
  } catch {
    return fallbackProblem(response.status, text)
  }

  return fallbackProblem(response.status, text)
}

function readRetryAfter(response: Response): number | null {
  const value = response.headers.get('Retry-After')
  if (value === null) {
    return null
  }

  const seconds = Number(value)
  return Number.isFinite(seconds) && seconds > 0 ? Math.ceil(seconds) : null
}

export async function fetchDailySummaries(symbol: string, signal?: AbortSignal): Promise<DailySummary[]> {
  const response = await fetch(`${baseUrl}/api/stocks/${encodeURIComponent(symbol)}/daily-summaries`, {
    signal,
    headers: { Accept: 'application/json' },
  })

  if (!response.ok) {
    throw new ApiError(await readProblem(response), response.status === 429 ? readRetryAfter(response) : null)
  }

  const payload: unknown = await response.json()
  if (!Array.isArray(payload)) {
    throw new ApiError({
      status: response.status,
      title: 'Invalid response',
      type: 'urn:stockdata:problem:unexpected',
      detail: 'The server returned an invalid response.',
    })
  }

  return payload as DailySummary[]
}
