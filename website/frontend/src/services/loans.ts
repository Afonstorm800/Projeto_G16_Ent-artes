import api from './api'

export interface LoanRequest {
    itemId: number
    dataFimPrevisto: string
    observacoes?: string
}

export const loansApi = {
    requestLoan: (data: LoanRequest) => api.post('/loans/requests', data),
    getPendingLoans: () => api.get('/loans/requests/pending'),
    approveLoan: (id: number, taxa: number) => api.post(`/loans/requests/${id}/approve`, { taxaAplicada: taxa }),
    rejectLoan: (id: number) => api.post(`/loans/requests/${id}/reject`),
    returnLoan: (id: number) => api.post(`/loans/${id}/return`),
    getMyLoans: () => api.get('/loans/my'),
}