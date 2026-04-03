namespace EntArtes.Core.Entities;

public class ProfessorModalidade
{
    public int ProfessorId { get; set; }
    public int ModalidadeId { get; set; }

    public Utilizador Professor { get; set; } = null!;
    public Modalidade Modalidade { get; set; } = null!;
}
