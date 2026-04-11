using Microsoft.EntityFrameworkCore;
using EntArtes.Core.Entities;
using EntArtes.Core.Interfaces;
using EntArtes.Infrastructure.Data;

namespace EntArtes.Infrastructure.Services;

public class ConfirmationService : IConfirmationService
{
    private readonly AppDbContext _context;

    public ConfirmationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ConfirmByEncAsync(int sessaoId)
    {
        var sessao = await _context.Sessoes.FindAsync(sessaoId);
        if (sessao == null) throw new Exception("Session not found");
        if (sessao.Estado != EstadoSessao.Agendada) throw new Exception("Session not in agendada state");
        sessao.EncConfirmado = true;
        await CheckAndSetProntoValidar(sessao);
        await _context.SaveChangesAsync();
    }

    public async Task ConfirmByProfAsync(int sessaoId)
    {
        var sessao = await _context.Sessoes.FindAsync(sessaoId);
        if (sessao == null) throw new Exception("Session not found");
        if (sessao.Estado != EstadoSessao.Agendada) throw new Exception("Session not in agendada state");
        sessao.ProfConfirmado = true;
        await CheckAndSetProntoValidar(sessao);
        await _context.SaveChangesAsync();
    }

    private async Task CheckAndSetProntoValidar(Sessao sessao)
    {
        if (sessao.EncConfirmado && sessao.ProfConfirmado)
        {
            sessao.Estado = EstadoSessao.ProntoValidar;
            // Notify Direção (in real implementation, send email or push notification)
        }
    }

    public async Task ValidateSessionAsync(int sessaoId)
    {
        var sessao = await _context.Sessoes.FindAsync(sessaoId);
        if (sessao == null) throw new Exception("Session not found");
        if (sessao.Estado != EstadoSessao.ProntoValidar) throw new Exception("Session not ready for validation");
        sessao.Estado = EstadoSessao.Concluida;
        // Mark for billing: we'll pick up sessions where estado = Concluida and faturaId is null
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Sessao>> GetSessionsReadyForValidationAsync()
    {
        return await _context.Sessoes
            .Where(s => s.Estado == EstadoSessao.ProntoValidar)
            .Include(s => s.Estudio)
            .Include(s => s.Professor)
            .Include(s => s.Modalidade)
            .ToListAsync();
    }
}