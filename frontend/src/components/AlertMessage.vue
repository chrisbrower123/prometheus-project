<script setup lang="ts">
import type { ProblemDetails } from '../types/stock'

const props = defineProps<{ error: ProblemDetails; retryAfter: number | null }>()

function messageFor(type: string): string {
  switch (type) {
    case 'urn:stockdata:problem:invalid-symbol': return 'That ticker format is not recognized. Check the symbol and try again.'
    case 'urn:stockdata:problem:symbol-not-found': return 'We could not find a stock with that ticker. Check the symbol and try again.'
    case 'urn:stockdata:problem:no-data-available': return 'That ticker is valid, but no recent intraday data is available.'
    case 'urn:stockdata:problem:rate-limited': return 'Stock data requests are temporarily limited. Please try again shortly.'
    case 'urn:stockdata:problem:provider-unavailable': return 'The stock data service is temporarily unavailable. Please try again shortly.'
    default: return 'We could not retrieve data for that stock. Check the ticker and try again.'
  }
}
</script>

<template>
  <div class="alert-message" :class="{ 'alert-message--rate-limit': props.error.type === 'urn:stockdata:problem:rate-limited' }" role="alert" aria-live="assertive">
    <p class="alert-title">{{ messageFor(props.error.type) }}</p>
    <p v-if="retryAfter !== null">Retry after {{ retryAfter }} seconds.</p>
    <details v-if="error.traceId" class="support-reference">
      <summary>Need help? Show support reference</summary>
      <p>This reference helps diagnose the failed request: <code>{{ error.traceId }}</code></p>
    </details>
  </div>
</template>

<style scoped>
.alert-message { margin-bottom: 1.5rem; border: 1px solid #f3c4c3; border-left: 4px solid var(--pg-red); border-radius: 4px; background: #fff1f1; color: #7f1d1d; padding: 1rem 1.1rem; }
.alert-message--rate-limit { border-color: #f2c8b0; border-left-color: var(--pg-red); background: #fff7ed; color: #8a2c1e; }
.alert-message p { margin: 0.25rem 0; }
.alert-title { font-weight: 700; }
.support-reference { margin-top: 0.75rem; font-size: 0.85rem; }
.support-reference summary { cursor: pointer; font-weight: 700; }
.support-reference code { overflow-wrap: anywhere; }
</style>
