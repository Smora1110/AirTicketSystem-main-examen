// src/modules/reprogramacion/Domain/Repositories/IRescheduleHistoryRepository.cs
using AirTicketSystem.modules.reprogramacion.Domain.aggregate;

namespace AirTicketSystem.modules.reprogramacion.Domain.Repositories;

public interface IRescheduleHistoryRepository
{
    Task<IReadOnlyCollection<RescheduleHistory>> FindByReservaAsync(int reservaId);
    Task<RescheduleHistory?> FindUltimoByReservaAsync(int reservaId);
    Task SaveAsync(RescheduleHistory history);
}
