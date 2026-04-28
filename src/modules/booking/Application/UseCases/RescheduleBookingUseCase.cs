// src/modules/booking/Application/UseCases/RescheduleBookingUseCase.cs
using AirTicketSystem.modules.booking.Domain.aggregate;
using AirTicketSystem.modules.booking.Domain.Repositories;
using AirTicketSystem.modules.flight.Domain.Repositories;
using AirTicketSystem.modules.seatavailability.Domain.Repositories;
using AirTicketSystem.modules.bookingpassenger.Domain.Repositories;
using AirTicketSystem.modules.reprogramacion.Domain.aggregate;
using AirTicketSystem.modules.reprogramacion.Domain.Repositories;
using AirTicketSystem.modules.waitinglist.Domain.Repositories;
using AirTicketSystem.modules.waitinglist.Domain.aggregate;

namespace AirTicketSystem.modules.booking.Application.UseCases;

public sealed class RescheduleBookingUseCase
{
    private readonly IBookingRepository           _bookingRepo;
    private readonly IFlightRepository            _flightRepo;
    private readonly ISeatAvailabilityRepository  _seatRepo;
    private readonly IBookingPassengerRepository  _passengerRepo;
    private readonly IRescheduleHistoryRepository _rescheduleHistoryRepo;
    private readonly IWaitingListRepository       _waitingListRepo;

    public RescheduleBookingUseCase(
        IBookingRepository           bookingRepo,
        IFlightRepository            flightRepo,
        ISeatAvailabilityRepository  seatRepo,
        IBookingPassengerRepository  passengerRepo,
        IRescheduleHistoryRepository rescheduleHistoryRepo,
        IWaitingListRepository       waitingListRepo)
    {
        _bookingRepo           = bookingRepo;
        _flightRepo            = flightRepo;
        _seatRepo              = seatRepo;
        _passengerRepo         = passengerRepo;
        _rescheduleHistoryRepo = rescheduleHistoryRepo;
        _waitingListRepo       = waitingListRepo;
    }

    /// <summary>
    /// Reprograma la reserva al nuevo vuelo.
    /// Si hay cupos disponibles: mueve la reserva y retorna (booking, false).
    /// Si no hay cupos: agrega a lista de espera y retorna (booking, true).
    /// </summary>
    public async Task<(Booking Booking, bool EnListaEspera)> ExecuteAsync(
        int reservaId,
        int nuevoVueloId,
        string motivo,
        int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        // ── Validaciones ──────────────────────────────────────────────────────

        if (reservaId <= 0)
            throw new ArgumentException("El ID de la reserva no es válido.");

        if (nuevoVueloId <= 0)
            throw new ArgumentException("El ID del nuevo vuelo no es válido.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo de reprogramación es obligatorio.");

        var booking = await _bookingRepo.FindByIdAsync(reservaId)
            ?? throw new KeyNotFoundException($"No se encontró la reserva con ID {reservaId}.");

        if (!booking.EstaConfirmada)
            throw new InvalidOperationException(
                $"Solo se pueden reprogramar reservas CONFIRMADAS. " +
                $"Estado actual: '{booking.Estado.Valor}'.");

        if (booking.VueloId == nuevoVueloId)
            throw new InvalidOperationException(
                "El nuevo vuelo debe ser diferente al vuelo actual de la reserva.");

        var nuevoVuelo = await _flightRepo.FindByIdAsync(nuevoVueloId)
            ?? throw new KeyNotFoundException($"No se encontró el vuelo con ID {nuevoVueloId}.");

        if (nuevoVuelo.Estado.Valor == "CANCELADO")
            throw new InvalidOperationException(
                $"No se puede reprogramar a un vuelo CANCELADO.");

        if (nuevoVuelo.FechaSalida.Valor <= DateTime.UtcNow)
            throw new InvalidOperationException(
                "No se puede reprogramar a un vuelo cuya fecha de salida ya pasó.");

        // ── Verificar disponibilidad ───────────────────────────────────────────

        var cuposDisponibles = await _seatRepo.ContarDisponiblesByVueloAsync(nuevoVueloId);
        var vueloAnteriorId  = booking.VueloId;

        if (cuposDisponibles <= 0)
        {
            // Sin cupo → agregar a lista de espera
            var yaEnEspera = await _waitingListRepo.ExisteReservaEnEsperaAsync(reservaId, nuevoVueloId);
            if (!yaEnEspera)
            {
                var prioridad = await _waitingListRepo.SiguientePrioridadAsync(nuevoVueloId);
                var entrada = WaitingList.Crear(reservaId, nuevoVueloId, prioridad);
                await _waitingListRepo.SaveAsync(entrada);
            }
            return (booking, true);
        }

        // ── Hay cupo: realizar el cambio ─────────────────────────────────────

        // Liberar asientos del vuelo anterior
        var pasajeros = await _passengerRepo.FindByReservaAsync(reservaId);
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

        // Actualizar el vuelo de la reserva
        booking.CambiarVuelo(nuevoVueloId);
        await _bookingRepo.UpdateAsync(booking);

        // Registrar en historial de reprogramación
        await _rescheduleHistoryRepo.SaveAsync(
            RescheduleHistory.Crear(
                reservaId,
                vueloAnteriorId,
                nuevoVueloId,
                motivo,
                usuarioId));

        return (booking, false);
    }
}
