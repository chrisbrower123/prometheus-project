<script setup lang="ts">
import StockDashboard from './views/StockDashboard.vue'
</script>

<template>
  <div class="app-shell">
    <StockDashboard />
  </div>
</template>

<style scoped>
:global(*) {
  box-sizing: border-box;
}

:global(html),
:global(body),
:global(#app) {
  min-height: 100%;
  margin: 0;
}

:global(:root) {
  --pg-navy: #101828;
  --pg-red: #b92221;
  --pg-white: #ffffff;
  --pg-gray-light: #eef2f6;
  --pg-gray-slate: #475467;
  --pg-shadow: 0 1px 12px 0 rgba(16, 24, 40, 0.1);
  --pg-overlay-navy: rgba(16, 24, 40, 0.9);
  --pg-overlay-red: rgba(185, 34, 33, 0.98);
}

:global(body) {
  background: var(--pg-gray-light);
  color: var(--pg-navy);
  font-family: Inter, Roboto, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

.app-shell {
  min-height: 100vh;
}
</style>
