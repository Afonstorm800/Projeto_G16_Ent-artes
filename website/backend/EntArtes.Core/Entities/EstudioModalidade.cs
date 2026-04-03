namespace EntArtes.Core.Entities;

public class EstudioModalidade
{
    public int EstudioId { get; set; }
    public int ModalidadeId { get; set; }

    public Estudio Estudio { get; set; } = null!;
    public Modalidade Modalidade { get; set; } = null!;
}
