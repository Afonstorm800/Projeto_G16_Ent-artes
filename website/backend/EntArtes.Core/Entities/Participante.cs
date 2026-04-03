namespace EntArtes.Core.Entities;

public class Participante
{
    public int SessaoId { get; set; }
    public int AlunoId { get; set; }

    public Sessao Sessao { get; set; } = null!;
    public Aluno Aluno { get; set; } = null!;
}
