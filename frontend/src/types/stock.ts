export interface DailySummary {
  day: string
  lowAverage: number
  highAverage: number
  volume: number
}

export interface ProblemDetails {
  status: number
  title: string
  type: string
  detail?: string
  traceId?: string
}

export type KnownProblemType =
  | 'urn:stockdata:problem:invalid-symbol'
  | 'urn:stockdata:problem:symbol-not-found'
  | 'urn:stockdata:problem:no-data-available'
  | 'urn:stockdata:problem:rate-limited'
  | 'urn:stockdata:problem:provider-unavailable'
  | 'urn:stockdata:problem:unexpected'
