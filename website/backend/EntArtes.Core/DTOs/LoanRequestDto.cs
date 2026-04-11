namespace EntArtes.Core.DTOs;

public class LoanRequestDto
{
    public int ItemId { get; set; }
    public DateTime DataFimPrevisto { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}