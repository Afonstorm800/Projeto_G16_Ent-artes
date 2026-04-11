using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using EntArtes.Core.Entities;
using EntArtes.Core.Interfaces;
using EntArtes.Infrastructure.Data;

namespace EntArtes.Infrastructure.Services;

public class BillingService : IBillingService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService; // to be implemented later

    public BillingService(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
        #pragma warning disable CS0618
            #pragma warning disable CS0618
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
#pragma warning restore CS0618 
    }

    public async Task ProcessMonthlyBillingAsync(int ano, int mes)
    {
        // Get all concluded sessions from given month that are not yet billed
        var startDate = new DateTime(ano, mes, 1);
        var endDate = startDate.AddMonths(1);
        var sessions = await _context.Sessoes
            .Where(s => s.Estado == EstadoSessao.Concluida && !s.FaturaId.HasValue &&
                        s.DataHoraInicio >= startDate && s.DataHoraInicio < endDate)
            .Include(s => s.Participantes)
                .ThenInclude(p => p.Aluno)
            .Include(s => s.Professor)
            .ToListAsync();

        // Group by encarregado (through participantes)
        var groups = sessions
            .SelectMany(s => s.Participantes.Select(p => new { Session = s, Aluno = p.Aluno }))
            .Where(x => x.Aluno.EncarregadoId != null)
            .GroupBy(x => x.Aluno.EncarregadoId)
            .ToList();

        foreach (var group in groups)
        {
            var encarregadoId = group.Key;
            var encarregado = await _context.Utilizadores.FindAsync(encarregadoId);
            if (encarregado == null) continue;

            var total = group.Sum(x => x.Session.Preco);
            var fatura = new Fatura
            {
                Mes = mes,
                Ano = ano,
                DataEmissao = DateTime.Today,
                ValorTotal = total,
                UtilizadorId = encarregadoId,
                Paga = false
            };
            _context.Faturas.Add(fatura);
            await _context.SaveChangesAsync();

            // Assign sessions to this fatura
            var sessionIds = group.Select(x => x.Session.Id).Distinct();
            var sessionsToUpdate = await _context.Sessoes.Where(s => sessionIds.Contains(s.Id)).ToListAsync();
            foreach (var s in sessionsToUpdate)
            {
                s.FaturaId = fatura.Id;
            }
            await _context.SaveChangesAsync();

            // Generate Excel file
            var excelBytes = await GenerateExcelForFaturaAsync(fatura.Id);
            // Save to disk or cloud and send link
            var filePath = Path.Combine("Invoices", $"Fatura_{fatura.Id}_{encarregado.Email}.xlsx");
            Directory.CreateDirectory("Invoices");
            await System.IO.File.WriteAllBytesAsync(filePath, excelBytes);

            // Send email with link (simplified)
            // await _emailService.SendInvoiceEmail(encarregado.Email, filePath);
        }
    }

    public async Task<byte[]> GenerateExcelForFaturaAsync(int faturaId)
    {
        var fatura = await _context.Faturas
            .Include(f => f.Utilizador)
            .Include(f => f.Sessoes)
                .ThenInclude(s => s.Modalidade)
            .FirstOrDefaultAsync(f => f.Id == faturaId);
        if (fatura == null) throw new Exception("Fatura not found");

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add($"Fatura {fatura.Mes}/{fatura.Ano}");

        // Headers
        worksheet.Cells[1, 1].Value = "Data";
        worksheet.Cells[1, 2].Value = "Modalidade";
        worksheet.Cells[1, 3].Value = "Formato";
        worksheet.Cells[1, 4].Value = "Professor";
        worksheet.Cells[1, 5].Value = "Preço";
        worksheet.Cells[1, 6].Value = "Alunos";

        int row = 2;
        foreach (var sessao in fatura.Sessoes)
        {
            worksheet.Cells[row, 1].Value = sessao.DataHoraInicio.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cells[row, 2].Value = sessao.Modalidade.Nome;
            worksheet.Cells[row, 3].Value = sessao.Formato.ToString();
            worksheet.Cells[row, 4].Value = sessao.Professor.Nome;
            worksheet.Cells[row, 5].Value = sessao.Preco;
            var alunos = string.Join(", ", sessao.Participantes.Select(p => p.Aluno.Nome));
            worksheet.Cells[row, 6].Value = alunos;
            row++;
        }

        worksheet.Cells[row, 5].Value = "Total:";
        worksheet.Cells[row, 6].Value = fatura.ValorTotal;
        worksheet.Cells[row, 6].Style.Font.Bold = true;

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }
}