<script setup lang="ts">
import { computed } from 'vue'
import { Chart } from 'vue-chartjs'
import { BarElement, CategoryScale, Chart as ChartJS, Legend, LinearScale, LineElement, PointElement, Tooltip, type ChartData, type ChartOptions, type ChartType } from 'chart.js'
import type { DailySummary } from '../types/stock'

ChartJS.register(CategoryScale, LinearScale, BarElement, LineElement, PointElement, Tooltip, Legend)
const props = defineProps<{ summaries: DailySummary[] }>()

const chartData = computed<ChartData<ChartType>>(() => ({
  labels: props.summaries.map((summary) => summary.day),
  datasets: [
    { type: 'line', label: 'Low average', data: props.summaries.map((summary) => summary.lowAverage), borderColor: '#101828', backgroundColor: '#101828', borderWidth: 2, pointRadius: 2, yAxisID: 'prices' },
    { type: 'line', label: 'High average', data: props.summaries.map((summary) => summary.highAverage), borderColor: '#b92221', backgroundColor: '#b92221', borderWidth: 2, pointRadius: 2, yAxisID: 'prices' },
    { type: 'bar', label: 'Volume', data: props.summaries.map((summary) => summary.volume), backgroundColor: 'rgba(185, 34, 33, 0.5)', borderColor: '#b92221', yAxisID: 'volume' },
  ],
}))

const chartOptions: ChartOptions<ChartType> = {
  responsive: true,
  plugins: {
    legend: {
      position: 'top',
      labels: { usePointStyle: true, color: '#101828' },
    },
  },
  scales: {
    prices: { type: 'linear', position: 'left', ticks: { color: '#101828' }, grid: { color: '#eef2f6' } },
    volume: { type: 'linear', position: 'right', ticks: { color: '#b92221' }, grid: { drawOnChartArea: false } },
  },
}
</script>

<template>
  <section class="chart-card" aria-label="Stock price and volume chart">
    <div class="chart-heading">
      <h2>Price and volume trend</h2>
      <span>Low and high averages are shown together. Use the legend to hide either line.</span>
    </div>
    <div class="chart-container">
      <Chart type="bar" :data="chartData" :options="chartOptions" aria-label="Stock price and volume chart" role="img" />
    </div>
  </section>
</template>

<style scoped>
.chart-card { min-width: 0; border: 1px solid var(--pg-gray-light); border-radius: 6px; background: var(--pg-white); padding: 1.25rem; box-shadow: var(--pg-shadow); }
.chart-heading { display: flex; align-items: baseline; justify-content: space-between; gap: 1rem; margin-bottom: 1rem; }
h2 { margin: 0; color: var(--pg-navy); font-size: 1.05rem; }
.chart-heading span { color: var(--pg-gray-slate); font-size: 0.8rem; }
.chart-container { min-height: 360px; }
</style>
