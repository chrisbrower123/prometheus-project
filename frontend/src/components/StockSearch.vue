<script setup lang="ts">
import { ref } from 'vue'

defineProps<{ isLoading: boolean }>()
const emit = defineEmits<{ search: [symbol: string] }>()
const symbol = ref('')

function submit(): void {
  emit('search', symbol.value)
}
</script>

<template>
  <form class="search-form" @submit.prevent="submit">
    <label for="stock-symbol">Stock symbol</label>
    <div class="search-controls">
      <input id="stock-symbol" v-model="symbol" autocomplete="off" placeholder="e.g. TSLA" :disabled="isLoading" />
      <button type="submit" :disabled="isLoading">Search</button>
    </div>
  </form>
</template>

<style scoped>
.search-form { display: flex; flex: 1; flex-direction: column; gap: 0.35rem; max-width: 520px; }
label { color: var(--pg-gray-slate); font-size: 0.75rem; font-weight: 700; letter-spacing: 0.05em; text-transform: uppercase; }
.search-controls { display: flex; gap: 0.6rem; }
input { min-width: 0; flex: 1; border: 1px solid var(--pg-gray-light); border-radius: 4px; background: var(--pg-white); color: var(--pg-navy); font: inherit; padding: 0.7rem 0.8rem; }
input:focus { border-color: var(--pg-navy); outline: 2px solid rgba(16, 24, 40, 0.2); outline-offset: 1px; }
button { border: 0; border-radius: 4px; background: var(--pg-navy); color: var(--pg-white); cursor: pointer; font: inherit; font-weight: 700; padding: 0.7rem 1.15rem; transition: background-color 150ms ease; }
button:hover:not(:disabled) { background: var(--pg-red); }
button:focus-visible { outline: 3px solid rgba(185, 34, 33, 0.35); outline-offset: 2px; }
button:disabled, input:disabled { cursor: not-allowed; opacity: 0.65; }
</style>
