// src/modules/waitinglist/Application/UseCases/PromoteFromWaitingListUseCase.cs
using AirTicketSystem.modules.waitinglist.Domain.Repositories;
using AirTicketSystem.modules.booking.Domain.Repositories;
using AirTicketSystem.modules.seatavailability.Domain.Repositories;
using AirTicketSystem.modules.bookingpassenger.Domain.Repositories;
using AirTicketSystem.modules.reprogramacion.Domain.aggregate;
using AirTicketSystem.modules.reprogramacion.Domain.Repositories;

namespace AirTicketSystem.modules.waitinglist.Application.UseCases;

/// <summary>
/// Intenta promover al primer candidato PENDIENTE en la lista de espera
/// de un vuelo. Se invoca automáticamente cuando se libera un asiento.
/// </summary>
public sealed class PromoteFromWaitingListUseCase
{
    private readonly IWaitingListRepository       _waitingListRepo;
    private readonly IBookingRepository           _bookingRepo;
    private readonly ISeatAvailabilityRepository  _seatRepo;
    private readonly IBookingPassengerRepository  _passengerRepo;
    private readonly IRescheduleHistoryRepository _rescheduleHistoryRepo;

    public PromoteFromWaitingListUseCase(
        IWaitingListRepository       waitingListRepo,
        IBookingRepository           bookingRepo,
        ISeatAvailabilityRepository  seatRepo,
        IBookingPassengerRepository  passengerRepo,
        IRescheduleHistoryRepository rescheduleHistoryRepo)
    {
        _waitingListRepo      = waitingListRepo;
        _bookingRepo          = bookingRepo;
        _seatRepo             = seatRepo;
        _passengerRepo        = passengerRepo;
        _rescheduleHistoryRepo = rescheduleHistoryRepo;
    }

    /// <summary>
    /// Devuelve true si se realizó una promoción, false si no había candidatos
    /// o no había asientos disponibles.
    /// </summary>
    public async Task<bool> ExecuteAsync(
        int vueloId,
        CancellationToken cancellationToken = default)
    {
        var candidato = await _waitingListRepo.FindPrimeroPendienteAsync(vueloId);
        if (candidato is null) return false;

        var asientosDisponibles = await _seatRepo.ContarDisponiblesByVueloAsync(vueloId);
        if (asientosDisponibles <= 0) return false;

        var booking = await _bookingRepo.FindByIdAsync(candidato.ReservaId);
        if (booking is null) return false;

        var vueloAnterior = booking.VueloId;

        // Liberar asientos del vuelo anterior asignados a los pasajeros de esta reserva
        var pasajeros = await _passengerRepo.FindByReservaAsync(candidato.ReservaId);
        foreach (var pasajero in pasajeros)
        {
            if (!pasajero.TieneAsientoAsignado) continue;

            var seatAnterior = await _seatRepo.FindByIdAsync(pasajero.AsientoId!.Value);
            if (seatAnterior is not null && seatAnterior.Estado.Valor == "RESERVADO")
            {
                seatAnterior.Liberar();
                await _seatRepo.UpdateAsync(seatAnterior);
            }

            pasajero.LiberarAsiento();
            await _passengerRepo.UpdateAsync(pasajero);
        }

        // Actualizar el vuelo en la reserva
        booking.CambiarVuelo(vueloId);
        await _bookingRepo.UpdateAsync(booking);

        // Marcar la entrada de lista de espera como promovida
        candidato.Promover();
        await _waitingListRepo.UpdateAsync(candidato);

        // Registrar en historial de reprogramación
        await _rescheduleHistoryRepo.SaveAsync(
            RescheduleHistory.Crear(
                booking.Id,
                vueloAnterior,
                vueloId,
                "Promoción automática desde lista de espera"));

        return true;
    }
}
