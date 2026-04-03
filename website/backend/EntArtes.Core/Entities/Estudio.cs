using System.Collections.Generic;

namespace EntArtes.Core.Entities;

public class Estudio
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    public ICollection<EstudioModalidade> ModalidadesCompativeis { get; set; } = new List<EstudioModalidade>();
}
