import { useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { Button } from '@/components/ui/button'
import { X } from 'lucide-react'
import { toast } from 'sonner'

const days = ["Seg", "Ter", "Qua", "Qui", "Sex"]
const hours = ["09:00", "10:00", "11:00", "12:00", "14:00", "15:00", "16:00", "17:00"]

const availableSlots: Record<string, string[]> = {
    Seg: ["10:00", "14:00", "15:00"],
    Ter: ["09:00", "11:00", "16:00"],
    Qua: ["10:00", "15:00", "17:00"],
    Qui: ["09:00", "14:00", "16:00"],
    Sex: ["10:00", "11:00", "15:00"],
}

const bookedSlots: Record<string, string[]> = {
    Seg: ["09:00"],
    Ter: ["14:00"],
    Qua: ["09:00", "14:00"],
    Qui: ["10:00"],
    Sex: ["09:00"],
}

const modalities = ["Ballet Clássico", "Dança Contemporânea", "Jazz", "Hip Hop", "Sapateado"]
const formats = ["Individual", "Duo", "Trio", "Ensemble"]

interface SelectedSlot {
    day: string
    hour: string
}

const BookingPage = () => {
    const [selected, setSelected] = useState<SelectedSlot | null>(null)
    const [modality, setModality] = useState(modalities[0])
    const [format, setFormat] = useState(formats[0])
    const [submitted, setSubmitted] = useState(false)

    const handleSlotClick = (day: string, hour: string) => {
        if (availableSlots[day]?.includes(hour)) {
            setSelected({ day, hour })
            setSubmitted(false)
        }
    }

    const handleSubmitBooking = () => {
        if (!selected) return
        toast.success('Pedido enviado para validação')
        setSubmitted(true)
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="font-display text-3xl font-bold text-foreground">Marcação de Aulas</h1>
                <p className="text-muted-foreground mt-1">Selecione um horário disponível para pedir marcação</p>
            </div>

            <div className="flex gap-4 text-xs text-muted-foreground">
                <span className="flex items-center gap-1.5"><span className="w-3 h-3 rounded border-2 border-primary bg-primary/10" /> Disponível</span>
                <span className="flex items-center gap-1.5"><span className="w-3 h-3 rounded bg-primary" /> Selecionado</span>
                <span className="flex items-center gap-1.5"><span className="w-3 h-3 rounded bg-muted" /> Ocupado</span>
            </div>

            <div className="bg-card rounded-lg shadow-card border border-border overflow-hidden">
                <div className="grid grid-cols-6">
                    <div className="bg-muted p-3 text-xs font-semibold text-muted-foreground">Hora</div>
                    {days.map((d) => (
                        <div key={d} className="bg-muted p-3 text-xs font-semibold text-center text-muted-foreground">{d}</div>
                    ))}
                </div>
                {hours.map((hour) => (
                    <div key={hour} className="grid grid-cols-6 border-t border-border">
                        <div className="p-3 text-sm font-medium text-muted-foreground">{hour}</div>
                        {days.map((day) => {
                            const isAvailable = availableSlots[day]?.includes(hour)
                            const isBooked = bookedSlots[day]?.includes(hour)
                            const isSelected = selected?.day === day && selected?.hour === hour
                            return (
                                <button
                                    key={`${day}-${hour}`}
                                    onClick={() => handleSlotClick(day, hour)}
                                    disabled={!isAvailable}
                                    className={`p-3 text-center text-sm transition-all duration-150 border-l border-border ${isSelected
                                            ? "bg-primary text-primary-foreground font-semibold"
                                            : isAvailable
                                                ? "bg-primary/5 hover:bg-primary/15 text-primary cursor-pointer border-primary/20"
                                                : isBooked
                                                    ? "bg-muted/50 text-muted-foreground/50"
                                                    : "bg-background text-muted-foreground/30"
                                        }`}
                                >
                                    {isBooked ? "Ocupado" : isAvailable ? "Livre" : "–"}
                                </button>
                            )
                        })}
                    </div>
                ))}
            </div>

            <AnimatePresence>
                {selected && (
                    <motion.div
                        initial={{ opacity: 0, x: 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        exit={{ opacity: 0, x: 20 }}
                        transition={{ duration: 0.2 }}
                        className="fixed right-0 top-0 h-full w-96 bg-card shadow-elevated border-l border-border z-50 overflow-y-auto"
                    >
                        <div className="p-6 space-y-6">
                            <div className="flex items-center justify-between">
                                <h2 className="font-display text-xl font-semibold text-foreground">Pedir Marcação</h2>
                                <button onClick={() => setSelected(null)} className="text-muted-foreground hover:text-foreground">
                                    <X className="h-5 w-5" />
                                </button>
                            </div>

                            <div className="bg-primary/5 rounded-lg p-4 border border-primary/20">
                                <p className="text-sm font-medium text-primary">{selected.day} · {selected.hour}</p>
                                <p className="text-xs text-muted-foreground mt-0.5">Semana de 24–28 Março 2026</p>
                            </div>

                            <div className="space-y-4">
                                <div>
                                    <label className="text-sm font-medium text-foreground block mb-2">Modalidade</label>
                                    <select
                                        value={modality}
                                        onChange={(e) => setModality(e.target.value)}
                                        className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                                    >
                                        {modalities.map((m) => (
                                            <option key={m}>{m}</option>
                                        ))}
                                    </select>
                                </div>

                                <div>
                                    <label className="text-sm font-medium text-foreground block mb-2">Formato</label>
                                    <div className="grid grid-cols-2 gap-2">
                                        {formats.map((f) => (
                                            <button
                                                key={f}
                                                onClick={() => setFormat(f)}
                                                className={`px-3 py-2 rounded-md text-sm font-medium border transition-colors ${format === f
                                                        ? "bg-primary text-primary-foreground border-primary"
                                                        : "bg-background text-foreground border-input hover:bg-muted"
                                                    }`}
                                            >
                                                {f}
                                            </button>
                                        ))}
                                    </div>
                                </div>

                                <div>
                                    <label className="text-sm font-medium text-foreground block mb-2">Professor atribuído</label>
                                    <div className="bg-muted rounded-md px-3 py-2 text-sm text-foreground">Pedro Santos</div>
                                </div>

                                <div>
                                    <label className="text-sm font-medium text-foreground block mb-2">Estúdio</label>
                                    <div className="bg-muted rounded-md px-3 py-2 text-sm text-foreground">Estúdio 2</div>
                                </div>
                            </div>

                            {submitted ? (
                                <div className="bg-secondary/10 border border-secondary/30 rounded-lg p-4 text-center">
                                    <p className="text-secondary font-semibold text-sm">✓ Pedido enviado com sucesso!</p>
                                    <p className="text-xs text-muted-foreground mt-1">A Direção irá validar o seu pedido.</p>
                                </div>
                            ) : (
                                <Button className="w-full" onClick={handleSubmitBooking}>
                                    Submeter Pedido
                                </Button>
                            )}
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    )
}

export default BookingPage