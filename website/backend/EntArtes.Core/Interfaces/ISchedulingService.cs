using EntArtes.Core.DTOs;
using EntArtes.Core.Entities;

namespace EntArtes.Core.Interfaces;

public interface ISchedulingService
{
    Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(DateTime date, int modalidadeId, FormatoAula formato);
    Task<Sessao> CreateBookingRequestAsync(int encarregadoId, BookingRequestDto dto);
    Task<Sessao?> GetSessionByIdAsync(int id);
    Task ApproveBookingAsync(int sessaoId);
    Task RejectBookingAsync(int sessaoId, string motivo);
}
