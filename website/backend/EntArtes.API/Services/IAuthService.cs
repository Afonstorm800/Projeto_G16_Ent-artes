using EntArtes.Core.Entities;

namespace EntArtes.API.Services;

public interface IAuthService
{
    string GenerateJwtToken(Utilizador user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
