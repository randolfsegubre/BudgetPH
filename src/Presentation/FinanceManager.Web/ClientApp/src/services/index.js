import api from './api'

export const authService = {
  login:          (data)   => api.post('/auth/login', data).then(r => r.data),
  register:       (data)   => api.post('/auth/register', data).then(r => r.data),
  logout:         ()       => api.post('/auth/logout'),
  refresh:        (token)  => api.post('/auth/refresh', { refreshToken: token }).then(r => r.data),
  getProfile:     ()       => api.get('/auth/profile').then(r => r.data),
  updateProfile:  (data)   => api.put('/auth/profile', data).then(r => r.data),
  changePassword: (data)   => api.post('/auth/change-password', data),
}

export const accountsService = {
  getAll:    ()         => api.get('/accounts').then(r => r.data),
  getById:   (id)       => api.get(`/accounts/${id}`).then(r => r.data),
  getSummary:()         => api.get('/accounts/summary').then(r => r.data),
  create:    (data)     => api.post('/accounts', data).then(r => r.data),
  update:    (id, data) => api.put(`/accounts/${id}`, data).then(r => r.data),
  delete:    (id)       => api.delete(`/accounts/${id}`),
}

export const transactionsService = {
  getAll:          (params) => api.get('/transactions', { params }).then(r => r.data),
  getById:         (id)     => api.get(`/transactions/${id}`).then(r => r.data),
  create:          (data)   => api.post('/transactions', data).then(r => r.data),
  update:          (id, d)  => api.put(`/transactions/${id}`, d).then(r => r.data),
  delete:          (id)     => api.delete(`/transactions/${id}`),
  getMonthlySummary: (y, m) => api.get('/transactions/monthly-summary', { params: { year: y, month: m } }).then(r => r.data),
  getCategories:   ()       => api.get('/transactions/categories').then(r => r.data),
}

export const budgetsService = {
  getAll:    ()     => api.get('/budgets').then(r => r.data),
  getCurrent:()     => api.get('/budgets/current').then(r => r.data),
  create:    (data) => api.post('/budgets', data).then(r => r.data),
  delete:    (id)   => api.delete(`/budgets/${id}`),
}

export const dashboardService = {
  getSummary: () => api.get('/dashboard').then(r => r.data),
}

export const reportsService = {
  generate: (data) => api.post('/reports/generate', data, { responseType: 'blob' }).then(r => r.data),
  getNetWorthTrend:  (months) => api.get('/reports/net-worth-trend', { params: { months } }).then(r => r.data),
  getSpendingTrend:  (months) => api.get('/reports/spending-trend', { params: { months } }).then(r => r.data),
}
