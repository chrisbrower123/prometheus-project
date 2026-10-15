<script setup lang="ts">
import AlertMessage from '../components/AlertMessage.vue'
import StockChart from '../components/StockChart.vue'
import StockDataTable from '../components/StockDataTable.vue'
import StockSearch from '../components/StockSearch.vue'
import { useStockData } from '../composables/useStockData'
import logoUrl from '../resources/logo.png'

const { data, isLoading, error, retryAfter, currentSymbol, searchSymbol } = useStockData()
</script>

<template>
  <div class="dashboard-page">
    <header class="dashboard-header">
      <div class="dashboard-header__inner">
        <div class="brand-block">
          <img class="brand-logo" :src="logoUrl" alt="Prometheus Group" />
          <div>
            <h1 class="brand-name">Prometheus Group</h1>
          </div>
        </div>
        <StockSearch :is-loading="isLoading" @search="searchSymbol" />
      </div>
    </header>

    <main class="dashboard-content">
      <div class="page-heading">
        <div>
          <p class="eyebrow">Market overview</p>
          <h1>Stock Intraday Data</h1>
        </div>
        <p v-if="currentSymbol" class="selected-symbol">{{ currentSymbol }}</p>
      </div>
      <p v-if="isLoading" class="loading-state" aria-live="polite">Loading stock data…</p>
      <AlertMessage v-if="error" :error="error" :retry-after="retryAfter" />
      <div v-if="data" class="dashboard-grid">
        <StockChart :summaries="data" />
        <StockDataTable :summaries="data" />
      </div>
      <div v-else-if="!isLoading && !error" class="empty-state">
        Search for a stock symbol to view its daily summaries.
      </div>
    </main>
  </div>
</template>

<style scoped>
.dashboard-page { min-height: 100vh; }
.dashboard-header { background: var(--pg-white); border-bottom: 1px solid var(--pg-gray-light); box-shadow: var(--pg-shadow); }
.dashboard-header__inner { display: flex; align-items: center; justify-content: space-between; gap: 2rem; max-width: 1400px; min-height: 76px; margin: 0 auto; padding: 1rem 2rem; }
.brand-block { display: flex; align-items: center; gap: 0.75rem; min-width: max-content; }
.brand-logo { display: block; width: 2.85rem; height: 2.85rem; object-fit: contain; }
.brand-name, .brand-product, .eyebrow { margin: 0; }
.brand-name { color: var(--pg-navy); font-size: 1.1rem; font-weight: 800; }
.brand-product, .eyebrow { color: var(--pg-gray-slate); font-size: 0.72rem; letter-spacing: 0.08em; text-transform: uppercase; }
.dashboard-content { max-width: 1400px; margin: 0 auto; padding: 2.25rem 2rem 3rem; }
.page-heading { display: flex; align-items: end; justify-content: space-between; gap: 1rem; margin-bottom: 1.5rem; }
h1 { margin: 0.3rem 0 0; color: var(--pg-navy); font-size: clamp(1.65rem, 3vw, 2.25rem); letter-spacing: -0.025em; }
.selected-symbol { margin: 0; color: var(--pg-navy); font-size: 1rem; font-weight: 700; }
.dashboard-grid { display: grid; grid-template-columns: minmax(0, 1.1fr) minmax(360px, 0.9fr); gap: 1.5rem; align-items: start; }
.loading-state, .empty-state { margin: 0; border: 1px solid var(--pg-gray-light); border-radius: 6px; background: var(--pg-white); color: var(--pg-gray-slate); padding: 1.25rem 1.5rem; box-shadow: var(--pg-shadow); }
@media (max-width: 900px) { .dashboard-header__inner { align-items: stretch; flex-direction: column; gap: 1rem; } .dashboard-grid { grid-template-columns: 1fr; } }
@media (max-width: 560px) { .dashboard-header__inner, .dashboard-content { padding-right: 1rem; padding-left: 1rem; } .page-heading { align-items: start; flex-direction: column; } }
</style>
