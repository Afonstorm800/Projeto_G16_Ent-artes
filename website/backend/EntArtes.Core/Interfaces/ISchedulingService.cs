using EntArtes.Core.Entities;

namespace EntArtes.Core.Interfaces;

public interface ISchedulingService
{
    Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(DateTime date, int modalidadeId, FormatoAula formato);
}

public class AvailableSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int EstudioId { get; set; }
    public string EstudioNome { get; set; } = string.Empty;
    public int ProfessorId { get; set; }
    public string ProfessorNome { get; set; } = string.Empty;
}