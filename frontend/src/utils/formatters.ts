export function parseLocalDate(dateStr: string): Date {
  const [yearText, monthText, dayText] = dateStr.split('-')
  const year = Number(yearText)
  const month = Number(monthText)
  const day = Number(dayText)

  if (!Number.isInteger(year) || !Number.isInteger(month) || !Number.isInteger(day)) {
    throw new Error('Invalid date format')
  }

  return new Date(year, month - 1, day)
}

const currencyFormatter = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 4,
  maximumFractionDigits: 4,
})

const volumeFormatter = new Intl.NumberFormat(undefined, {
  maximumFractionDigits: 0,
})

export function formatCurrency(value: number): string {
  return currencyFormatter.format(value)
}

export function formatVolume(value: number): string {
  return volumeFormatter.format(value)
}

export function validateSymbol(symbol: string): string | null {
  const normalized = symbol.trim().toUpperCase()
  if (normalized.length < 1 || normalized.length > 12 || !/^[A-Z0-9.\-^=?]+$/.test(normalized)) {
    return null
  }

  return normalized
}
