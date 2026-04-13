import api from './api'

export interface AvailableSlot {
    startTime: string
    endTime: string
    estudioId: number
    estudioNome: string
    professorId: number
    professorNome: string
}

export interface BookingRequest {
    dataHoraInicio: string
    dataHoraFim: string
    formato: number
    modalidadeId: number
    professorId: number
    estudioId: number
    alunosIds: number[]
}

export const sessionsApi = {
    getAvailableSlots: (date: string, modalidadeId: number, formato: number) =>
        api.get<AvailableSlot[]>('/sessions/available', { params: { date, modalidadeId, formato } }),
    createBooking: (data: BookingRequest) => api.post('/sessions/requests', data),
    confirmByEnc: (sessionId: number) => api.post(`/sessions/${sessionId}/confirm-enc`),
    confirmByProf: (sessionId: number) => api.post(`/sessions/${sessionId}/confirm-prof`),
    getPendingBookings: () => api.get('/sessions?estado=pendente'),
    approveBooking: (id: number) => api.post(`/sessions/${id}/approve`),
    rejectBooking: (id: number) => api.post(`/sessions/${id}/reject`),
    getSessionsReadyForValidation: () => api.get('/sessions/ready-for-validation'),
    validateSession: (id: number) => api.post(`/sessions/${id}/validate`),
}