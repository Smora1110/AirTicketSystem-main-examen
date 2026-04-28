// src/modules/reprogramacion/Application/UseCases/GetRescheduleHistoryUseCase.cs
using AirTicketSystem.modules.reprogramacion.Domain.aggregate;
using AirTicketSystem.modules.reprogramacion.Domain.Repositories;

namespace AirTicketSystem.modules.reprogramacion.Application.UseCases;

public sealed class GetRescheduleHistoryUseCase
{
    private readonly IRescheduleHistoryRepository _repository;

    public GetRescheduleHistoryUseCase(IRescheduleHistoryRepository repository) =>
        _repository = repository;

    public async Task<IReadOnlyCollection<RescheduleHistory>> ExecuteAsync(
        int reservaId,
        CancellationToken cancellationToken = default)
    {
        if (reservaId <= 0)
            throw new ArgumentException("El ID de la reserva no es válido.");

        return await _repository.FindByReservaAsync(reservaId);
    }
}
