using AirTicketSystem.modules.waitinglist.Domain.aggregate;
using AirTicketSystem.modules.waitinglist.Domain.Repositories;

namespace AirTicketSystem.modules.waitinglist.Application.UseCases;

public sealed class GetWaitingListByFlightUseCase
{
    private readonly IWaitingListRepository _repository;

    public GetWaitingListByFlightUseCase(IWaitingListRepository repository) =>
        _repository = repository;

    public async Task<IReadOnlyCollection<WaitingList>> ExecuteAsync(
        int vueloId,
        bool soloActivos = false,
        CancellationToken cancellationToken = default)
    {
        if (vueloId <= 0)
            throw new ArgumentException("El ID del vuelo no es válido.");

        return soloActivos
            ? await _repository.FindPendientesByVueloAsync(vueloId)
            : await _repository.FindByVueloAsync(vueloId);
    }
}
