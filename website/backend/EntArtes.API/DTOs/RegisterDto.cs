using System.ComponentModel.DataAnnotations;
using EntArtes.Core.Entities;

namespace EntArtes.API.DTOs;

public class RegisterDto
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public TipoUtilizador Tipo { get; set; }
}
