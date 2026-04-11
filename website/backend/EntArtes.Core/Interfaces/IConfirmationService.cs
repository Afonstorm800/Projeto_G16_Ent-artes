using EntArtes.Core.Entities;

namespace EntArtes.Core.Interfaces;

public interface IConfirmationService
{
    Task ConfirmByEncAsync(int sessaoId);
    Task ConfirmByProfAsync(int sessaoId);
    Task ValidateSessionAsync(int sessaoId);
    Task<IEnumerable<Sessao>> GetSessionsReadyForValidationAsync();
}