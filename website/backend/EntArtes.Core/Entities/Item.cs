using System.Collections.Generic;

namespace EntArtes.Core.Entities;

public enum EstadoItem
{
    Pendente,
    Aprovado,
    Rejeitado
}

public class Item
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string EstadoConservacao { get; set; } = string.Empty;
    public string FotoUrl { get; set; } = string.Empty;
    public bool Disponivel { get; set; } = true;
    public decimal TaxaSimbolica { get; set; } = 0;
    public EstadoItem Estado { get; set; }

    public int ContribuidorId { get; set; }
    public Utilizador Contribuidor { get; set; } = null!;

    public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
}
