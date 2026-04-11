using Microsoft.EntityFrameworkCore;
using EntArtes.Core.DTOs;
using EntArtes.Core.Entities;
using EntArtes.Core.Interfaces;
using EntArtes.Infrastructure.Data;

namespace EntArtes.Infrastructure.Services;

public class SchedulingService : ISchedulingService
{
    private readonly AppDbContext _context;

    public SchedulingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(DateTime date, int modalidadeId, FormatoAula formato)
    {
        // Get all studios that support the modality
        var compatibleStudioIds = await _context.EstudioModalidades
            .Where(em => em.ModalidadeId == modalidadeId)
            .Select(em => em.EstudioId)
            .ToListAsync();

        // Get professors that can teach this modality
        var professorIds = await _context.ProfessorModalidades
            .Where(pm => pm.ModalidadeId == modalidadeId)
            .Select(pm => pm.ProfessorId)
            .ToListAsync();

        // Get available times based on professor availability and existing sessions
        var dayOfWeek = (int)date.DayOfWeek;
        var availabilities = await _context.DisponibilidadesProfessores
            .Where(dp => professorIds.Contains(dp.ProfessorId) && dp.DiaSemana == dayOfWeek)
            .Include(dp => dp.Professor)
            .ToListAsync();

        var slots = new List<AvailableSlotDto>();
        var duration = GetDurationForFormato(formato); // e.g., 1 hour for individual, 1.5h for ensemble

        foreach (var avail in availabilities)
        {
            var start = date.Date + avail.HoraInicio;
            var end = date.Date + avail.HoraFim;

            // Check existing sessions for this professor/studio
            var occupied = await _context.Sessoes
                .Where(s => s.ProfessorId == avail.ProfessorId && s.Estado != EstadoSessao.Rejeitada &&
                            s.DataHoraInicio < end && s.DataHoraFim > start)
                .Select(s => new { s.DataHoraInicio, s.DataHoraFim })
                .ToListAsync();

            // Simplified slot generation: for each studio, create slots if not overlapping
            foreach (var studioId in compatibleStudioIds)
            {
                var studio = await _context.Estudios.FindAsync(studioId);
                var current = start;
                while (current.Add(duration) <= end)
                {
                    var slotEnd = current.Add(duration);
                    var isFree = !occupied.Any(o => o.DataHoraInicio < slotEnd && o.DataHoraFim > current);
                    if (isFree)
                    {
                        slots.Add(new AvailableSlotDto
                        {
                            StartTime = current,
                            EndTime = slotEnd,
                            EstudioId = studioId,
                            EstudioNome = studio?.Nome ?? string.Empty,
                            ProfessorId = avail.ProfessorId,
                            ProfessorNome = avail.Professor.Nome
                        });
                    }
                    current = current.Add(duration);
                }
            }
        }

        return slots.OrderBy(s => s.StartTime).ToList();
    }

    public async Task<Sessao> CreateBookingRequestAsync(int encarregadoId, BookingRequestDto dto)
    {
        // Validate that professor, studio, modalidade exist
        var professor = await _context.Utilizadores.FindAsync(dto.ProfessorId);
        if (professor == null || professor.Tipo != TipoUtilizador.Professor)
            throw new Exception("Invalid professor");

        var studio = await _context.Estudios.FindAsync(dto.EstudioId);
        if (studio == null) throw new Exception("Invalid studio");

        var modalidade = await _context.Modalidades.FindAsync(dto.ModalidadeId);
        if (modalidade == null) throw new Exception("Invalid modalidade");

        // Check compatibility: studio must support modality
        var compatible = await _context.EstudioModalidades
            .AnyAsync(em => em.EstudioId == dto.EstudioId && em.ModalidadeId == dto.ModalidadeId);
        if (!compatible) throw new Exception("Studio does not support this modality");

        // Check professor teaches modality
        var teaches = await _context.ProfessorModalidades
            .AnyAsync(pm => pm.ProfessorId == dto.ProfessorId && pm.ModalidadeId == dto.ModalidadeId);
        if (!teaches) throw new Exception("Professor does not teach this modality");

        // Check availability
        var overlapping = await _context.Sessoes
            .AnyAsync(s => s.ProfessorId == dto.ProfessorId && s.EstudioId == dto.EstudioId &&
                           s.DataHoraInicio < dto.DataHoraFim && s.DataHoraFim > dto.DataHoraInicio &&
                           s.Estado != EstadoSessao.Rejeitada);
        if (overlapping) throw new Exception("Time slot is not available");

        var sessao = new Sessao
        {
            DataHoraInicio = dto.DataHoraInicio,
            DataHoraFim = dto.DataHoraFim,
            Estado = EstadoSessao.Pendente,
            Formato = dto.Formato,
            EncConfirmado = false,
            ProfConfirmado = false,
            Preco = CalculatePreco(dto.Formato), // define later
            EstudioId = dto.EstudioId,
            ProfessorId = dto.ProfessorId,
            ModalidadeId = dto.ModalidadeId,
            FaturaId = null
        };

        _context.Sessoes.Add(sessao);
        await _context.SaveChangesAsync();

        // Add participants (alunos)
        foreach (var alunoId in dto.AlunosIds)
        {
            // Verify that aluno belongs to this encarregado
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == alunoId && a.EncarregadoId == encarregadoId);
            if (aluno != null)
            {
                _context.Participantes.Add(new Participante { SessaoId = sessao.Id, AlunoId = alunoId });
            }
        }
        await _context.SaveChangesAsync();

        // Notify Direção (simplified: just log, but you'd call IEmailService)
        // TODO: send notification

        return sessao;
    }

    public async Task<Sessao?> GetSessionByIdAsync(int id) => await _context.Sessoes.FindAsync(id);

    public async Task ApproveBookingAsync(int sessaoId)
    {
        var sessao = await _context.Sessoes.FindAsync(sessaoId);
        if (sessao == null) throw new Exception("Session not found");
        if (sessao.Estado != EstadoSessao.Pendente) throw new Exception("Session already processed");
        sessao.Estado = EstadoSessao.Agendada;
        await _context.SaveChangesAsync();
        // TODO: notify encarregado
    }

    public async Task RejectBookingAsync(int sessaoId, string motivo)
    {
        var sessao = await _context.Sessoes.FindAsync(sessaoId);
        if (sessao == null) throw new Exception("Session not found");
        sessao.Estado = EstadoSessao.Rejeitada;
        await _context.SaveChangesAsync();
        // TODO: notify encarregado with motivo
    }

    private TimeSpan GetDurationForFormato(FormatoAula formato) => formato switch
    {
        FormatoAula.Individual => TimeSpan.FromHours(1),
        FormatoAula.Dueto => TimeSpan.FromHours(1),
        FormatoAula.Trio => TimeSpan.FromHours(1.5),
        FormatoAula.Ensemble => TimeSpan.FromHours(2),
        _ => TimeSpan.FromHours(1)
    };

    private decimal CalculatePreco(FormatoAula formato) => formato switch
    {
        FormatoAula.Individual => 30m,
        FormatoAula.Dueto => 45m,
        FormatoAula.Trio => 60m,
        FormatoAula.Ensemble => 80m,
        _ => 30m
    };
}