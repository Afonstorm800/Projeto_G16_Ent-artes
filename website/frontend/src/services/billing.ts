import api from './api'

export const billingApi = {
    runMonthlyBilling: (ano: number, mes: number) =>
        api.post('/billing/monthly', null, { params: { ano, mes } }),
    downloadInvoiceExcel: (faturaId: number) =>
        api.get(`/billing/fatura/${faturaId}/excel`, { responseType: 'blob' }),
}