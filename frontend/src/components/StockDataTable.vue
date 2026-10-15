<script setup lang="ts">
import type { DailySummary } from '../types/stock'
import { formatCurrency, formatVolume } from '../utils/formatters'

defineProps<{ summaries: DailySummary[] }>()
</script>

<template>
  <section class="table-card">
    <div class="table-heading">
      <h2>Daily summaries</h2>
      <span>{{ summaries.length }} sessions</span>
    </div>
    <div class="table-scroll">
      <table>
        <caption class="visually-hidden">Daily stock summaries</caption>
        <thead>
          <tr><th scope="col">Day</th><th scope="col">Low average</th><th scope="col">High average</th><th scope="col">Volume</th></tr>
        </thead>
        <tbody>
          <tr v-for="summary in summaries" :key="summary.day">
            <th scope="row">{{ summary.day }}</th>
            <td>{{ formatCurrency(summary.lowAverage) }}</td>
            <td>{{ formatCurrency(summary.highAverage) }}</td>
            <td>{{ formatVolume(summary.volume) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style scoped>
.table-card { min-width: 0; overflow: hidden; border: 1px solid var(--pg-gray-light); border-radius: 6px; background: var(--pg-white); padding: 1.25rem; box-shadow: var(--pg-shadow); }
.table-heading { display: flex; align-items: baseline; justify-content: space-between; gap: 1rem; margin-bottom: 1rem; }
h2 { margin: 0; color: var(--pg-navy); font-size: 1.05rem; }
.table-heading span { color: var(--pg-gray-slate); font-size: 0.8rem; }
.table-scroll { max-height: 440px; overflow-y: auto; }
table { width: 100%; border-collapse: collapse; color: var(--pg-navy); font-size: 0.88rem; text-align: left; }
th, td { padding: 0.75rem 1rem; white-space: nowrap; }
thead th { position: sticky; top: 0; background: var(--pg-navy); color: var(--pg-white); font-size: 0.76rem; letter-spacing: 0.04em; text-transform: uppercase; z-index: 1; }
tbody tr:nth-child(odd) { background: var(--pg-white); }
tbody tr:nth-child(even) { background: #f8fafc; }
tbody tr:hover { background: var(--pg-gray-light); }
tbody th, tbody td { border-bottom: 1px solid var(--pg-gray-light); }
.visually-hidden { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0 0 0 0); clip-path: inset(50%); white-space: nowrap; }
</style>
