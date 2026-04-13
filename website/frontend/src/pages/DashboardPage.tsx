import { Calendar, CheckSquare, Package, Receipt } from "lucide-react";
import { motion } from "framer-motion";

const stats = [
  { label: "Aulas Agendadas", value: "12", icon: Calendar, color: "text-primary" },
  { label: "Pendentes Validação", value: "5", icon: CheckSquare, color: "text-accent" },
  { label: "Itens no Inventário", value: "38", icon: Package, color: "text-secondary" },
  { label: "Faturas este Mês", value: "8", icon: Receipt, color: "text-primary" },
];

const recentActivity = [
  { text: "Rita Gomes – Aula de Ballet confirmada por EE", time: "Há 2h" },
  { text: "Pedro Santos – Confirmou sessão de Ballet Clássico", time: "Há 3h" },
  { text: "Novo item submetido: Tutu de ensaio", time: "Há 5h" },
  { text: "Empréstimo aprovado: Sapatilhas de ponta #12", time: "Há 1d" },
  { text: "Fatura gerada para Carla Gomes – Março 2026", time: "Há 2d" },
];

const DashboardPage = () => {
  return (
    <div className="space-y-8">
      <div>
        <h1 className="font-display text-3xl font-bold text-foreground">Dashboard</h1>
        <p className="text-muted-foreground mt-1">Visão geral do sistema Ent'Artes</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat, i) => {
          const Icon = stat.icon;
          return (
            <motion.div
              key={stat.label}
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.05, duration: 0.2 }}
              className="bg-card rounded-lg p-5 shadow-card border border-border"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-muted-foreground">{stat.label}</p>
                  <p className="text-2xl font-bold text-foreground mt-1">{stat.value}</p>
                </div>
                <Icon className={`h-8 w-8 ${stat.color} opacity-70`} />
              </div>
            </motion.div>
          );
        })}
      </div>

      <div className="bg-card rounded-lg shadow-card border border-border">
        <div className="p-5 border-b border-border">
          <h2 className="font-display text-lg font-semibold text-foreground">Atividade Recente</h2>
        </div>
        <div className="divide-y divide-border">
          {recentActivity.map((item, i) => (
            <motion.div
              key={i}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ delay: 0.2 + i * 0.05 }}
              className="px-5 py-3.5 flex items-center justify-between"
            >
              <p className="text-sm text-foreground">{item.text}</p>
              <span className="text-xs text-muted-foreground whitespace-nowrap ml-4">{item.time}</span>
            </motion.div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
