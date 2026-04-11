using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EntArtes.Core.Interfaces;

namespace EntArtes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Direcao")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billing;

    public BillingController(IBillingService billing)
    {
        _billing = billing;
    }

    [HttpPost("monthly")]
    public async Task<IActionResult> ProcessMonthlyBilling(int ano, int mes)
    {
        await _billing.ProcessMonthlyBillingAsync(ano, mes);
        return Ok(new { message = "Billing processed" });
    }

    [HttpGet("fatura/{id}/excel")]
    public async Task<IActionResult> DownloadFaturaExcel(int id)
    {
        var excelBytes = await _billing.GenerateExcelForFaturaAsync(id);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"fatura_{id}.xlsx");
    }
}