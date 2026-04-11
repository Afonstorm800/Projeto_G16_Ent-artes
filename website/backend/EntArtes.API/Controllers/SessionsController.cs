using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EntArtes.Core.DTOs;
using EntArtes.Core.Entities;
using EntArtes.Core.Interfaces;

namespace EntArtes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISchedulingService _scheduling;
    private readonly IConfirmationService _confirmation;

    public SessionsController(ISchedulingService scheduling, IConfirmationService confirmation)
    {
        _scheduling = scheduling;
        _confirmation = confirmation;
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime date, [FromQuery] int modalidadeId, [FromQuery] FormatoAula formato)
    {
        var slots = await _scheduling.GetAvailableSlotsAsync(date, modalidadeId, formato);
        return Ok(slots);
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest(BookingRequestDto dto)
    {
        var encarregadoId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var sessao = await _scheduling.CreateBookingRequestAsync(encarregadoId, dto);
        return CreatedAtAction(nameof(GetSession), new { id = sessao.Id }, sessao);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(int id)
    {
        var sessao = await _scheduling.GetSessionByIdAsync(id);
        if (sessao == null) return NotFound();
        return Ok(sessao);
    }

    [HttpPost("{id}/confirm-enc")]
    public async Task<IActionResult> ConfirmByEnc(int id)
    {
        await _confirmation.ConfirmByEncAsync(id);
        return Ok();
    }

    [HttpPost("{id}/confirm-prof")]
    public async Task<IActionResult> ConfirmByProf(int id)
    {
        // In real app, check that current user is the professor of this session
        await _confirmation.ConfirmByProfAsync(id);
        return Ok();
    }

    [HttpPost("{id}/validate")]
    [Authorize(Roles = "Direcao")]
    public async Task<IActionResult> ValidateSession(int id)
    {
        await _confirmation.ValidateSessionAsync(id);
        return Ok();
    }

    [HttpGet("ready-for-validation")]
    [Authorize(Roles = "Direcao")]
    public async Task<IActionResult> GetReadyForValidation()
    {
        var sessions = await _confirmation.GetSessionsReadyForValidationAsync();
        return Ok(sessions);
    }
}