import { useState, useEffect } from 'react'
import { motion } from 'framer-motion'
import { Button } from '@/components/ui/button'
import StatusBadge from '@/components/StatusBadge'
import DualConfirmation from '@/components/DualConfirmation'
import { sessionsApi } from '@/services/session'
import { toast } from 'sonner'

type Tab = "bookings" | "sessions"

interface BookingRequest {
    id: number
    student: string
    guardian: string
    modality: string
    format: string
    day: string
    time: string
    professor: string
    studio: string
    status: "pending" | "approved" | "rejected"
}

interface Session {
    id: number
    student: string
    modality: string
    date: string
    professor: string
    encConfirmed: boolean
    profConfirmed: boolean
    status: "ready" | "completed" | "pending"
}

const initialBookings: BookingRequest[] = [
    { id: 1, student: "Rita Gomes", guardian: "Carla Gomes", modality: "Ballet Clássico", format: "Individual", day: "Ter", time: "16:00", professor: "Pedro Santos", studio: "Estúdio 2", status: "pending" },
    { id: 2, student: "João Silva", guardian: "Ana Silva", modality: "Dança Contemporânea", format: "Duo", day: "Qua", time: "15:00", professor: "Maria Costa", studio: "Estúdio 1", status: "pending" },
]

const initialSessions: Session[] = [
    { id: 101, student: "Rita Gomes", modality: "Ballet Clássico", date: "2026-03-20 16:00", professor: "Pedro Santos", encConfirmed: true, profConfirmed: true, status: "ready" },
    { id: 102, student: "João Silva", modality: "Dança Contemporânea", date: "2026-03-19 15:00", professor: "Maria Costa", encConfirmed: true, profConfirmed: false, status: "pending" },
]

const ValidationPage = () => {
    const [tab, setTab] = useState<Tab>("bookings")
    const [bookings, setBookings] = useState(initialBookings)
    const [sessions, setSessions] = useState(initialSessions)

    useEffect(() => {
        // In real implementation, fetch from API:
        // sessionsApi.getPendingBookings().then(res => setBookings(res.data))
        // sessionsApi.getSessionsReadyForValidation().then(res => setSessions(res.data))
    }, [])

    const handleBookingAction = async (id: number, action: "approved" | "rejected") => {
        try {
            if (action === "approved") {
                await sessionsApi.approveBooking(id)
                toast.success('Pedido aprovado')
            } else {
                await sessionsApi.rejectBooking(id)
                toast.success('Pedido rejeitado')
            }
            setBookings((prev) => prev.map((b) => (b.id === id ? { ...b, status: action } : b)))
        } catch {
            toast.error('Erro ao processar pedido')
        }
    }

    const handleValidateSession = async (id: number) => {
        try {
            await sessionsApi.validateSession(id)
            setSessions((prev) => prev.map((s) => (s.id === id ? { ...s, status: "completed" } : s)))
            toast.success('Sessão validada e enviada para faturação')
        } catch {
            toast.error('Erro ao validar sessão')
        }
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="font-display text-3xl font-bold text-foreground">Validações</h1>
                <p className="text-muted-foreground mt-1">Aprovar marcações e validar sessões concluídas</p>
            </div>

            <div className="flex gap-1 bg-muted rounded-lg p-1 w-fit">
                <button
                    onClick={() => setTab("bookings")}
                    className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${tab === "bookings" ? "bg-card text-foreground shadow-card" : "text-muted-foreground"}`}
                >
                    Pedidos de Marcação
                </button>
                <button
                    onClick={() => setTab("sessions")}
                    className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${tab === "sessions" ? "bg-card text-foreground shadow-card" : "text-muted-foreground"}`}
                >
                    Confirmação 48h
                </button>
            </div>

            {tab === "bookings" && (
                <div className="bg-card rounded-lg shadow-card border border-border overflow-hidden">
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="bg-muted">
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Aluno</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Encarregado</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Modalidade</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Formato</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Horário</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Professor</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Estado</th>
                                    <th className="text-right p-3 font-semibold text-muted-foreground">Ações</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-border">
                                {bookings.map((b, i) => (
                                    <motion.tr
                                        key={b.id}
                                        initial={{ opacity: 0 }}
                                        animate={{ opacity: 1 }}
                                        transition={{ delay: i * 0.04 }}
                                    >
                                        <td className="p-3 font-medium text-foreground">{b.student}</td>
                                        <td className="p-3 text-muted-foreground">{b.guardian}</td>
                                        <td className="p-3 text-foreground">{b.modality}</td>
                                        <td className="p-3 text-foreground">{b.format}</td>
                                        <td className="p-3 text-foreground">{b.day} {b.time}</td>
                                        <td className="p-3 text-foreground">{b.professor}</td>
                                        <td className="p-3"><StatusBadge status={b.status} /></td>
                                        <td className="p-3 text-right">
                                            {b.status === "pending" && (
                                                <div className="flex gap-2 justify-end">
                                                    <Button size="sm" onClick={() => handleBookingAction(b.id, "approved")}>Aprovar</Button>
                                                    <Button size="sm" variant="outline" onClick={() => handleBookingAction(b.id, "rejected")}>Rejeitar</Button>
                                                </div>
                                            )}
                                        </td>
                                    </motion.tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}

            {tab === "sessions" && (
                <div className="bg-card rounded-lg shadow-card border border-border overflow-hidden">
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="bg-muted">
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Aluno</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Modalidade</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Data</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Professor</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Confirmações</th>
                                    <th className="text-left p-3 font-semibold text-muted-foreground">Estado</th>
                                    <th className="text-right p-3 font-semibold text-muted-foreground">Ações</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-border">
                                {sessions.map((s, i) => {
                                    const bothConfirmed = s.encConfirmed && s.profConfirmed
                                    return (
                                        <motion.tr
                                            key={s.id}
                                            initial={{ opacity: 0 }}
                                            animate={{ opacity: 1 }}
                                            transition={{ delay: i * 0.04 }}
                                        >
                                            <td className="p-3 font-medium text-foreground">{s.student}</td>
                                            <td className="p-3 text-foreground">{s.modality}</td>
                                            <td className="p-3 text-foreground">{s.date}</td>
                                            <td className="p-3 text-foreground">{s.professor}</td>
                                            <td className="p-3"><DualConfirmation encConfirmed={s.encConfirmed} profConfirmed={s.profConfirmed} /></td>
                                            <td className="p-3"><StatusBadge status={s.status} /></td>
                                            <td className="p-3 text-right">
                                                {s.status === "ready" && bothConfirmed && (
                                                    <Button size="sm" onClick={() => handleValidateSession(s.id)}>
                                                        Validar & Faturar
                                                    </Button>
                                                )}
                                                {s.status === "pending" && !bothConfirmed && (
                                                    <span className="text-xs text-muted-foreground">
                                                        A aguardar {!s.encConfirmed && "EE"}{!s.encConfirmed && !s.profConfirmed && " e "}{!s.profConfirmed && "Prof"}
                                                    </span>
                                                )}
                                            </td>
                                        </motion.tr>
                                    )
                                })}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}
        </div>
    )
}

export default ValidationPage