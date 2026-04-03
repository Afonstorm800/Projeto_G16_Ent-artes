using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EntArtes.API.DTOs;
using EntArtes.API.Services;
using EntArtes.Core.Entities;
using EntArtes.Infrastructure.Data;

namespace EntArtes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _auth;

    public AuthController(AppDbContext context, IAuthService auth)
    {
        _context = context;
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Utilizadores.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email already registered" });

        var user = new Utilizador
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = _auth.HashPassword(dto.Password),
            Tipo = dto.Tipo
        };

        _context.Utilizadores.Add(user);
        await _context.SaveChangesAsync();

        var token = _auth.GenerateJwtToken(user);
        return Ok(new { token, user.Tipo, user.Nome });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !_auth.VerifyPassword(dto.Password, user.SenhaHash))
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _auth.GenerateJwtToken(user);
        return Ok(new { token, user.Tipo, user.Nome });
    }
}
