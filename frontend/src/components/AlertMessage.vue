<script setup lang="ts">
import type { ProblemDetails } from '../types/stock'

defineProps<{ error: ProblemDetails; retryAfter: number | null }>()

function messageFor(type: string): string {
  switch (type) {
    case 'urn:stockdata:problem:invalid-symbol': return 'Enter a valid stock symbol.'
    case 'urn:stockdata:problem:symbol-not-found': return 'That symbol was not found.'
    case 'urn:stockdata:problem:no-data-available': return 'No stock data is available.'
    case 'urn:stockdata:problem:rate-limited': return 'The data provider is rate limiting requests.'
    case 'urn:stockdata:problem:provider-unavailable': return 'The data provider is unavailable.'
    default: return 'The request could not be completed.'
  }
}
</script>

<template>
  <div role="alert" aria-live="assertive">
    <p>{{ messageFor(error.type) }}</p>
    <p v-if="retryAfter !== null">Retry after {{ retryAfter }} seconds.</p>
    <p v-if="error.traceId">Reference: {{ error.traceId }}</p>
  </div>
</template>

<style scoped>
</style>
