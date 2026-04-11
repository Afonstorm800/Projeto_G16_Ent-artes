using EntArtes.Core.Entities;

namespace EntArtes.Core.DTOs;

public class BookingRequestDto
{
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public FormatoAula Formato { get; set; }
    public int ModalidadeId { get; set; }
    public int ProfessorId { get; set; }
    public int EstudioId { get; set; }
    public List<int> AlunosIds { get; set; } = new();
}
