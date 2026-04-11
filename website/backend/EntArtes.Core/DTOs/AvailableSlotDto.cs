namespace EntArtes.Core.DTOs;

public class AvailableSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int EstudioId { get; set; }
    public string EstudioNome { get; set; } = string.Empty;
    public int ProfessorId { get; set; }
    public string ProfessorNome { get; set; } = string.Empty;
}