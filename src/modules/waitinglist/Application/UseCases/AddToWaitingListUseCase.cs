// src/modules/waitinglist/Application/UseCases/AddToWaitingListUseCase.cs
using AirTicketSystem.modules.waitinglist.Domain.aggregate;
using AirTicketSystem.modules.waitinglist.Domain.Repositories;

namespace AirTicketSystem.modules.waitinglist.Application.UseCases;

public sealed class AddToWaitingListUseCase
{
    private readonly IWaitingListRepository _repository;

    public AddToWaitingListUseCase(IWaitingListRepository repository) =>
        _repository = repository;

    public async Task<WaitingList> ExecuteAsync(
        int reservaId,
        int vueloId,
        CancellationToken cancellationToken = default)
    {
        if (reservaId <= 0)
            throw new ArgumentException("El ID de la reserva no es válido.");
        if (vueloId <= 0)
            throw new ArgumentException("El ID del vuelo no es válido.");

        var yaExiste = await _repository.ExisteReservaEnEsperaAsync(reservaId, vueloId);
        if (yaExiste)
            throw new InvalidOperationException(
                $"La reserva {reservaId} ya está en lista de espera para el vuelo {vueloId}.");

        var prioridad = await _repository.SiguientePrioridadAsync(vueloId);
        var entry = WaitingList.Crear(reservaId, vueloId, prioridad);

        await _repository.SaveAsync(entry);
        return entry;
    }
}
