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
  <form @submit.prevent="submit">
    <label for="stock-symbol">Stock symbol</label>
    <input id="stock-symbol" v-model="symbol" autocomplete="off" :disabled="isLoading" />
    <button type="submit" :disabled="isLoading">Search</button>
  </form>
</template>

<style scoped>
</style>
