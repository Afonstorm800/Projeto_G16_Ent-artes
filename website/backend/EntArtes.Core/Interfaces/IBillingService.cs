using EntArtes.Core.Entities;

namespace EntArtes.Core.Interfaces;

public interface IBillingService
{
    Task ProcessMonthlyBillingAsync(int ano, int mes);
    Task<byte[]> GenerateExcelForFaturaAsync(int faturaId);
}