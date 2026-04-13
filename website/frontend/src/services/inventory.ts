import api from './api'

export interface Item {
    id: number
    nome: string
    descricao: string
    categoria: string
    estadoConservacao: string
    fotoUrl: string
    disponivel: boolean
    taxaSimbolica: number
    estado: number
}

export const inventoryApi = {
    getAvailableItems: () => api.get<Item[]>('/inventory/items/available'),
    getPendingItems: () => api.get<Item[]>('/inventory/items/pending'),
    submitItem: (data: Record<string, unknown>) => api.post('/inventory/items', data),
    approveItem: (id: number) => api.post(`/inventory/items/${id}/approve`),
    rejectItem: (id: number) => api.post(`/inventory/items/${id}/reject`),
}