<script setup lang="ts">
import AlertMessage from '../components/AlertMessage.vue'
import StockChart from '../components/StockChart.vue'
import StockDataTable from '../components/StockDataTable.vue'
import StockSearch from '../components/StockSearch.vue'
import { useStockData } from '../composables/useStockData'

const { data, isLoading, error, retryAfter, searchSymbol } = useStockData()
</script>

<template>
  <main>
    <h1>Stock intraday data</h1>
    <StockSearch :is-loading="isLoading" @search="searchSymbol" />
    <p v-if="isLoading" aria-live="polite">Loading stock data…</p>
    <AlertMessage v-if="error" :error="error" :retry-after="retryAfter" />
    <template v-if="data">
      <StockChart :summaries="data" />
      <StockDataTable :summaries="data" />
    </template>
  </main>
</template>

<style scoped>
</style>
