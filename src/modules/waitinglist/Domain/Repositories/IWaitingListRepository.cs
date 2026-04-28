// src/modules/waitinglist/Domain/Repositories/IWaitingListRepository.cs
using AirTicketSystem.modules.waitinglist.Domain.aggregate;

namespace AirTicketSystem.modules.waitinglist.Domain.Repositories;

public interface IWaitingListRepository
{
    Task<WaitingList?> FindByIdAsync(int id);
    Task<IReadOnlyCollection<WaitingList>> FindByVueloAsync(int vueloId);
    Task<IReadOnlyCollection<WaitingList>> FindPendientesByVueloAsync(int vueloId);
    Task<WaitingList?> FindPrimeroPendienteAsync(int vueloId);
    Task<bool> ExisteReservaEnEsperaAsync(int reservaId, int vueloId);
    Task<int> SiguientePrioridadAsync(int vueloId);
    Task SaveAsync(WaitingList entry);
    Task UpdateAsync(WaitingList entry);
}
