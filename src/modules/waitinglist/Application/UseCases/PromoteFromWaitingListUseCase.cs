// src/modules/waitinglist/Application/UseCases/PromoteFromWaitingListUseCase.cs
using AirTicketSystem.modules.waitinglist.Domain.Repositories;
using AirTicketSystem.modules.booking.Domain.Repositories;
using AirTicketSystem.modules.seatavailability.Domain.Repositories;
using AirTicketSystem.modules.bookingpassenger.Domain.Repositories;
using AirTicketSystem.modules.reprogramacion.Domain.aggregate;
using AirTicketSystem.modules.reprogramacion.Domain.Repositories;

namespace AirTicketSystem.modules.waitinglist.Application.UseCases;

/// <summary>
/// Promueve al primer candidato PENDIENTE en la lista de espera de un vuelo.
/// Se invoca automáticamente cuando se libera un asiento.
/// Casos:
///   - Reserva ya en el vuelo (PENDIENTE por cupos llenos al crear) → confirma + asigna asiento.
///   - Reserva en otro vuelo (reprogramación pendiente) → cambia vuelo + confirma.
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
        _waitingListRepo       = waitingListRepo;
        _bookingRepo           = bookingRepo;
        _seatRepo              = seatRepo;
        _passengerRepo         = passengerRepo;
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

        var asientoLibre = await _seatRepo.FindPrimerDisponibleByVueloAsync(vueloId);
        if (asientoLibre is null) return false;

        var booking = await _bookingRepo.FindByIdAsync(candidato.ReservaId);
        if (booking is null) return false;

        if (booking.VueloId == vueloId)
        {
            var pasajeros = await _passengerRepo.FindByReservaAsync(candidato.ReservaId);
            var sinAsiento = pasajeros.FirstOrDefault(p => !p.TieneAsientoAsignado);

            if (sinAsiento is not null)
            {
                asientoLibre.Reservar();
                await _seatRepo.UpdateAsync(asientoLibre);
                sinAsiento.AsignarAsiento(asientoLibre.Id);
                await _passengerRepo.UpdateAsync(sinAsiento);
            }

            booking.Confirmar();
            await _bookingRepo.UpdateAsync(booking);
        }
        else
        {
            var vueloAnterior = booking.VueloId;

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

            booking.CambiarVuelo(vueloId);
            await _bookingRepo.UpdateAsync(booking);

            await _rescheduleHistoryRepo.SaveAsync(
                RescheduleHistory.Crear(
                    booking.Id,
                    vueloAnterior,
                    vueloId,
                    "Promoción automática desde lista de espera"));
        }

        candidato.Promover();
        await _waitingListRepo.UpdateAsync(candidato);

        return true;
    }
}
