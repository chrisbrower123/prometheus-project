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
    { type: 'line', label: 'Low average', data: props.summaries.map((summary) => summary.lowAverage), borderColor: '#2563eb', yAxisID: 'prices' },
    { type: 'line', label: 'High average', data: props.summaries.map((summary) => summary.highAverage), borderColor: '#16a34a', yAxisID: 'prices' },
    { type: 'bar', label: 'Volume', data: props.summaries.map((summary) => summary.volume), backgroundColor: '#94a3b8', yAxisID: 'volume' },
  ],
}))

const chartOptions: ChartOptions<ChartType> = {
  responsive: true,
  scales: {
    prices: { type: 'linear', position: 'left' },
    volume: { type: 'linear', position: 'right', grid: { drawOnChartArea: false } },
  },
}
</script>

<template>
  <Chart type="bar" :data="chartData" :options="chartOptions" aria-label="Stock price and volume chart" role="img" />
</template>

<style scoped>
</style>
