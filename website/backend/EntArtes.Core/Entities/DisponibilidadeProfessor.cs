using System;

namespace EntArtes.Core.Entities;

public class DisponibilidadeProfessor
{
    public int Id { get; set; }
    public int ProfessorId { get; set; }
    public int DiaSemana { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
    public bool Recorrente { get; set; } = true;

    public Utilizador Professor { get; set; } = null!;
}
