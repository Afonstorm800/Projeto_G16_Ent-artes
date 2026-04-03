using System.Collections.Generic;

namespace EntArtes.Core.Entities;

public class Aluno
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int EncarregadoId { get; set; }
    public Utilizador Encarregado { get; set; } = null!;

    public ICollection<Participante> Participacoes { get; set; } = new List<Participante>();
}
