<script setup lang="ts">
import type { DailySummary } from '../types/stock'
import { formatCurrency, formatVolume } from '../utils/formatters'

defineProps<{ summaries: DailySummary[] }>()
</script>

<template>
  <table>
    <caption>Daily stock summaries</caption>
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
</template>

<style scoped>
</style>
