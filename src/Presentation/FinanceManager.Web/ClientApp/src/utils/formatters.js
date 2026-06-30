// Utility helpers
export const formatCurrency = (amount, currency = 'PHP') => {
  const symbols = { PHP: '₱', USD: '$', EUR: '€', GBP: '£', JPY: '¥', SGD: 'S$', AED: 'AED ' }
  const symbol = symbols[currency] ?? currency + ' '
  if (amount === null || amount === undefined) return `${symbol}0.00`
  const abs = Math.abs(amount)
  const formatted = abs.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
  return amount < 0 ? `-${symbol}${formatted}` : `${symbol}${formatted}`
}

export const formatCompact = (amount, currency = 'PHP') => {
  const symbols = { PHP: '₱', USD: '$', EUR: '€', GBP: '£' }
  const symbol = symbols[currency] ?? currency + ' '
  const abs = Math.abs(amount)
  if (abs >= 1_000_000) return `${symbol}${(abs / 1_000_000).toFixed(1)}M`
  if (abs >= 1_000)     return `${symbol}${(abs / 1_000).toFixed(1)}K`
  return `${symbol}${abs.toFixed(2)}`
}

export const formatDate = (date, fmt = 'short') => {
  if (!date) return '—'
  const d = new Date(date)
  if (fmt === 'short') return d.toLocaleDateString('en-PH', { month: 'short', day: 'numeric', year: 'numeric' })
  if (fmt === 'long')  return d.toLocaleDateString('en-PH', { month: 'long', day: 'numeric', year: 'numeric' })
  if (fmt === 'time')  return d.toLocaleDateString('en-PH', { month: 'short', day: 'numeric' }) + ' ' + d.toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit' })
  return d.toLocaleDateString('en-PH')
}

export const formatPercent = (value, decimals = 1) =>
  `${Math.min(Math.max(value ?? 0, 0), 100).toFixed(decimals)}%`

export const getAmountClass = (amount) =>
  amount > 0 ? 'amount-positive' : amount < 0 ? 'amount-negative' : 'amount-neutral'

export const accountTypeLabel = {
  1: 'Checking', 2: 'Savings', 3: 'Credit Card', 4: 'Investment', 5: 'Loan', 6: 'Cash', 7: 'Other'
}

export const transactionTypeLabel = {
  1: 'Income', 2: 'Expense', 3: 'Transfer', 4: 'Loan Payment',
  5: 'Investment Buy', 6: 'Investment Sell', 7: 'Credit Card Payment', 8: 'Savings Deposit'
}

export const loanTypeLabel = {
  1: 'Personal', 2: 'Auto', 3: 'Mortgage', 4: 'Student', 5: 'Business',
  6: 'Medical', 7: 'Home Equity', 8: 'HELOC', 9: 'Pay Day', 10: 'Other'
}

export const downloadBlob = (blob, filename) => {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
