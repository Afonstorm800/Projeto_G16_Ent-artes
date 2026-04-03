using System.Collections.Generic;

namespace EntArtes.Core.Entities;

public class Modalidade
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    public ICollection<EstudioModalidade> EstudiosCompativeis { get; set; } = new List<EstudioModalidade>();
    public ICollection<ProfessorModalidade> ProfessoresHabilitados { get; set; } = new List<ProfessorModalidade>();
}
