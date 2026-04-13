import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { Button } from '@/components/ui/button'
import StatusBadge from '@/components/StatusBadge'
import { Plus, X, Package, ArrowLeftRight } from 'lucide-react'
import { inventoryApi } from '@/services/inventory'
import { loansApi } from '@/services/loans'
import { toast } from 'sonner'

type InventoryTab = "items" | "contributions" | "loans"

interface Item {
    id: number
    name: string
    description: string
    category: string
    condition: string
    available: boolean
    onLoan: boolean
    fee: number
    contributor: string
    imageColor: string
}

interface Contribution {
    id: number
    name: string
    category: string
    contributor: string
    status: "pending" | "approved" | "rejected"
}

interface Loan {
    id: number
    item: string
    requester: string
    startDate: string
    endDate: string
    fee: number
    status: "pending" | "approved" | "returned" | "rejected"
}

// Initial data defined before the component
const initialItems: Item[] = [
    { id: 1, name: "Tutu de Ballet Rosa", description: "Tutu clássico para ensaios e apresentações", category: "Figurino", condition: "Bom", available: true, onLoan: false, fee: 5, contributor: "Maria Costa", imageColor: "bg-pink-100" },
    { id: 2, name: "Sapatilhas de Ponta #12", description: "Sapatilhas de ponta tamanho 38", category: "Calçado", condition: "Excelente", available: false, onLoan: true, fee: 8, contributor: "Ana Silva", imageColor: "bg-amber-100" },
    { id: 3, name: "Collant Preto M", description: "Collant de malha para aulas diárias", category: "Figurino", condition: "Bom", available: true, onLoan: false, fee: 3, contributor: "Pedro Santos", imageColor: "bg-slate-100" },
]

const initialContributions: Contribution[] = [
    { id: 10, name: "Tutu de Ensaio Branco", category: "Figurino", contributor: "Sofia Almeida", status: "pending" },
]

const initialLoans: Loan[] = [
    { id: 20, item: "Sapatilhas de Ponta #12", requester: "Rita Gomes", startDate: "2026-03-10", endDate: "2026-03-25", fee: 8, status: "approved" },
    { id: 21, item: "Véu de Dança", requester: "João Silva", startDate: "", endDate: "", fee: 4, status: "pending" },
]

const InventoryPage = () => {
    const [tab, setTab] = useState<InventoryTab>("items")
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const [items, setItems] = useState<Item[]>(initialItems)
    const [contributions, setContributions] = useState(initialContributions)
    const [loans, setLoans] = useState(initialLoans)
    const [showContributeModal, setShowContributeModal] = useState(false)
    const [showLoanModal, setShowLoanModal] = useState<Item | null>(null)
    const [loanObservations, setLoanObservations] = useState("")

    useEffect(() => {
        // Future API calls
    }, [])

    const handleApproveContribution = async (id: number) => {
        try {
            await inventoryApi.approveItem(id)
            setContributions((prev) => prev.map((c) => (c.id === id ? { ...c, status: "approved" } : c)))
            toast.success('Item aprovado')
        } catch {
            toast.error('Erro ao aprovar item')
        }
    }

    const handleRequestLoan = async (itemId: number) => {
        try {
            await loansApi.requestLoan({
                itemId,
                dataFimPrevisto: new Date(Date.now() + 14 * 86400000).toISOString(),
                observacoes: loanObservations,
            })
            toast.success('Pedido de empréstimo enviado')
            setShowLoanModal(null)
            setLoanObservations("")
        } catch {
            toast.error('Erro ao solicitar empréstimo')
        }
    }

    const handleLoanAction = async (id: number, action: "approved" | "rejected") => {
        try {
            if (action === "approved") {
                await loansApi.approveLoan(id, 5)
                toast.success('Empréstimo aprovado')
            } else {
                await loansApi.rejectLoan(id)
                toast.success('Empréstimo rejeitado')
            }
            setLoans((prev) => prev.map((l) => (l.id === id ? { ...l, status: action } : l)))
        } catch {
            toast.error('Erro ao processar empréstimo')
        }
    }

    const handleReturn = async (id: number) => {
        try {
            await loansApi.returnLoan(id)
            setLoans((prev) => prev.map((l) => (l.id === id ? { ...l, status: "returned" } : l)))
            toast.success('Item devolvido')
        } catch {
            toast.error('Erro ao devolver item')
        }
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="font-display text-3xl font-bold text-foreground">Inventário Comunitário</h1>
                    <p className="text-muted-foreground mt-1">Figurinos, acessórios e cenários da comunidade</p>
                </div>
                <Button onClick={() => setShowContributeModal(true)}>
                    <Plus className="h-4 w-4 mr-2" /> Contribuir Item
                </Button>
            </div>

            <div className="flex gap-1 bg-muted rounded-lg p-1 w-fit">
                {(["items", "contributions", "loans"] as const).map((t) => (
                    <button
                        key={t}
                        onClick={() => setTab(t)}
                        className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${tab === t ? "bg-card text-foreground shadow-card" : "text-muted-foreground"}`}
                    >
                        {t === "items" ? "Inventário" : t === "contributions" ? "Contribuições" : "Empréstimos"}
                    </button>
                ))}
            </div>

            {tab === "items" && (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {items.map((item, i) => (
                        <motion.div
                            key={item.id}
                            initial={{ opacity: 0, y: 12 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: i * 0.05 }}
                            className="bg-card rounded-lg shadow-card border border-border overflow-hidden group"
                        >
                            <div className={`h-40 ${item.imageColor} flex items-center justify-center relative`}>
                                <Package className="h-12 w-12 text-muted-foreground/30" />
                                <div className="absolute top-3 right-3">
                                    <StatusBadge status={item.onLoan ? "on_loan" : "available"} />
                                </div>
                            </div>
                            <div className="p-4">
                                <h3 className="font-semibold text-foreground">{item.name}</h3>
                                <p className="text-xs text-muted-foreground mt-1">{item.description}</p>
                                <div className="flex items-center justify-between mt-3">
                                    <span className="text-xs text-muted-foreground">{item.category} · {item.condition}</span>
                                    <span className="text-sm font-semibold text-foreground">{item.fee.toFixed(2)} €</span>
                                </div>
                                {item.available && (
                                    <Button size="sm" variant="outline" className="w-full mt-3" onClick={() => setShowLoanModal(item)}>
                                        <ArrowLeftRight className="h-3.5 w-3.5 mr-1" /> Solicitar Empréstimo
                                    </Button>
                                )}
                            </div>
                        </motion.div>
                    ))}
                </div>
            )}

            {tab === "contributions" && (
                <div className="bg-card rounded-lg shadow-card border border-border overflow-hidden">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="bg-muted">
                                <th className="text-left p-3 font-semibold text-muted-foreground">Item</th>
                                <th className="text-left p-3 font-semibold text-muted-foreground">Categoria</th>
                                <th className="text-left p-3 font-semibold text-muted-foreground">Contribuidor</th>
                                <th className="text-left p-3 font-semibold text-muted-foreground">Estado</th>
                                <th className="text-right p-3 font-semibold text-muted-foreground">Ações</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-border">
                            {contributions.map((c) => (
                                <tr key={c.id}>
                                    <td className="p-3 font-medium text-foreground">{c.name}</td>
                                    <td className="p-3 text-foreground">{c.category}</td>
                                    <td className="p-3 text-foreground">{c.contributor}</td>
                                    <td className="p-3"><StatusBadge status={c.status} /></td>
                                    <td className="p-3 text-right">
                                        {c.status === "pending" && (
                                            <Button size="sm" onClick={() => handleApproveContribution(c.id)}>Aprovar</Button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {tab === "loans" && (
                <div className="bg-card rounded-lg shadow-card border border-border overflow-hidden">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="bg-muted">
                                <th className="text-left p-3 font-semibold text-muted-foreground">Item</th>
                                <th className="text-left p-3 font-semibold text-muted-foreground">Requisitante</th>
                                <th className="text-left p-3 font-semibold text-muted-foreground">Início</th>
                                <th className="text-left p-3 font-semibold text-muted-foreground">Fim Previsto</th>
                                <th className="text-left p-3 font-semibold text-muted-foreground">Taxa</th>
                                <th className="text-left p-3 font-semibold text-muted-foreground">Estado</th>
                                <th className="text-right p-3 font-semibold text-muted-foreground">Ações</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-border">
                            {loans.map((l) => (
                                <tr key={l.id}>
                                    <td className="p-3 font-medium text-foreground">{l.item}</td>
                                    <td className="p-3 text-foreground">{l.requester}</td>
                                    <td className="p-3 text-foreground">{l.startDate || "–"}</td>
                                    <td className="p-3 text-foreground">{l.endDate || "–"}</td>
                                    <td className="p-3 text-foreground">{l.fee.toFixed(2)} €</td>
                                    <td className="p-3"><StatusBadge status={l.status} /></td>
                                    <td className="p-3 text-right">
                                        {l.status === "pending" && (
                                            <div className="flex gap-2 justify-end">
                                                <Button size="sm" onClick={() => handleLoanAction(l.id, "approved")}>Aprovar</Button>
                                                <Button size="sm" variant="outline" onClick={() => handleLoanAction(l.id, "rejected")}>Rejeitar</Button>
                                            </div>
                                        )}
                                        {l.status === "approved" && (
                                            <Button size="sm" variant="outline" onClick={() => handleReturn(l.id)}>Devolver</Button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {/* Contribute Modal */}
            <AnimatePresence>
                {showContributeModal && (
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        className="fixed inset-0 bg-foreground/20 z-50 flex items-center justify-center"
                        onClick={() => setShowContributeModal(false)}
                    >
                        <motion.div
                            initial={{ scale: 0.95, opacity: 0 }}
                            animate={{ scale: 1, opacity: 1 }}
                            exit={{ scale: 0.95, opacity: 0 }}
                            transition={{ duration: 0.15 }}
                            className="bg-card rounded-lg shadow-elevated border border-border w-full max-w-md p-6 space-y-4"
                            onClick={(e) => e.stopPropagation()}
                        >
                            <div className="flex items-center justify-between">
                                <h2 className="font-display text-xl font-semibold text-foreground">Contribuir Item</h2>
                                <button onClick={() => setShowContributeModal(false)} className="text-muted-foreground hover:text-foreground">
                                    <X className="h-5 w-5" />
                                </button>
                            </div>
                            <div className="space-y-3">
                                <div>
                                    <label className="text-sm font-medium text-foreground block mb-1">Nome do Item</label>
                                    <input className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm" placeholder="Ex: Tutu de Ballet Branco" />
                                </div>
                                <div>
                                    <label className="text-sm font-medium text-foreground block mb-1">Descrição</label>
                                    <textarea className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm" rows={3} placeholder="Descreva o item..." />
                                </div>
                                <div className="grid grid-cols-2 gap-3">
                                    <div>
                                        <label className="text-sm font-medium text-foreground block mb-1">Categoria</label>
                                        <select className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm">
                                            <option>Figurino</option>
                                            <option>Calçado</option>
                                            <option>Acessório</option>
                                            <option>Cenário</option>
                                        </select>
                                    </div>
                                    <div>
                                        <label className="text-sm font-medium text-foreground block mb-1">Estado</label>
                                        <select className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm">
                                            <option>Novo</option>
                                            <option>Excelente</option>
                                            <option>Bom</option>
                                            <option>Razoável</option>
                                        </select>
                                    </div>
                                </div>
                                <div>
                                    <label className="text-sm font-medium text-foreground block mb-1">Foto</label>
                                    <div className="border-2 border-dashed border-input rounded-lg p-6 text-center">
                                        <p className="text-sm text-muted-foreground">Clique ou arraste uma foto</p>
                                    </div>
                                </div>
                            </div>
                            <Button className="w-full" onClick={() => setShowContributeModal(false)}>Submeter para Validação</Button>
                        </motion.div>
                    </motion.div>
                )}
            </AnimatePresence>

            {/* Loan Request Modal */}
            <AnimatePresence>
                {showLoanModal && (
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        className="fixed inset-0 bg-foreground/20 z-50 flex items-center justify-center"
                        onClick={() => setShowLoanModal(null)}
                    >
                        <motion.div
                            initial={{ scale: 0.95, opacity: 0 }}
                            animate={{ scale: 1, opacity: 1 }}
                            exit={{ scale: 0.95, opacity: 0 }}
                            transition={{ duration: 0.15 }}
                            className="bg-card rounded-lg shadow-elevated border border-border w-full max-w-md p-6 space-y-4"
                            onClick={(e) => e.stopPropagation()}
                        >
                            <div className="flex items-center justify-between">
                                <h2 className="font-display text-xl font-semibold text-foreground">Solicitar Empréstimo</h2>
                                <button onClick={() => setShowLoanModal(null)} className="text-muted-foreground hover:text-foreground">
                                    <X className="h-5 w-5" />
                                </button>
                            </div>
                            <div className="bg-muted rounded-lg p-3">
                                <p className="font-medium text-foreground">{showLoanModal.name}</p>
                                <p className="text-xs text-muted-foreground">{showLoanModal.category} · Taxa: {showLoanModal.fee.toFixed(2)} €</p>
                            </div>
                            <div>
                                <label className="text-sm font-medium text-foreground block mb-1">Observações</label>
                                <textarea
                                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                                    rows={3}
                                    placeholder="Motivo do empréstimo, datas pretendidas..."
                                    value={loanObservations}
                                    onChange={(e) => setLoanObservations(e.target.value)}
                                />
                            </div>
                            <Button className="w-full" onClick={() => handleRequestLoan(showLoanModal.id)}>Submeter Pedido</Button>
                        </motion.div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    )
}

export default InventoryPage