// src/modules/booking/Application/UseCases/CancelBookingUseCase.cs
using AirTicketSystem.modules.booking.Domain.aggregate;
using AirTicketSystem.modules.booking.Domain.Repositories;
using AirTicketSystem.modules.bookinghistory.Domain.aggregate;
using AirTicketSystem.modules.bookinghistory.Domain.Repositories;
using AirTicketSystem.modules.seatavailability.Domain.Repositories;
using AirTicketSystem.modules.bookingpassenger.Domain.Repositories;
using AirTicketSystem.modules.waitinglist.Application.UseCases;

namespace AirTicketSystem.modules.booking.Application.UseCases;

public sealed class CancelBookingUseCase
{
    private readonly IBookingRepository          _bookingRepository;
    private readonly IBookingHistoryRepository   _historyRepository;
    private readonly ISeatAvailabilityRepository _seatRepository;
    private readonly IBookingPassengerRepository _passengerRepository;
    private readonly PromoteFromWaitingListUseCase _promoteUseCase;

    public CancelBookingUseCase(
        IBookingRepository           bookingRepository,
        IBookingHistoryRepository    historyRepository,
        ISeatAvailabilityRepository  seatRepository,
        IBookingPassengerRepository  passengerRepository,
        PromoteFromWaitingListUseCase promoteUseCase)
    {
        _bookingRepository   = bookingRepository;
        _historyRepository   = historyRepository;
        _seatRepository      = seatRepository;
        _passengerRepository = passengerRepository;
        _promoteUseCase      = promoteUseCase;
    }

    public async Task<Booking> ExecuteAsync(
        int id,
        string motivo,
        int? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("El ID de la reserva no es válido.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo de cancelación es obligatorio.");

        var booking = await _bookingRepository.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"No se encontró una reserva con ID {id}.");

        var estadoAnterior = booking.Estado.Valor;
        var vueloId        = booking.VueloId;

        booking.Cancelar();

        // Liberar asientos de todos los pasajeros
        var pasajeros = await _passengerRepository.FindByReservaAsync(id);
        foreach (var pasajero in pasajeros)
        {
            if (!pasajero.TieneAsientoAsignado) continue;

            var seat = await _seatRepository.FindByIdAsync(pasajero.AsientoId!.Value);
            if (seat is not null && seat.Estado.Valor == "RESERVADO")
            {
                seat.Liberar();
                await _seatRepository.UpdateAsync(seat);
            }

            pasajero.LiberarAsiento();
            await _passengerRepository.UpdateAsync(pasajero);
        }

        await _bookingRepository.UpdateAsync(booking);
        await _historyRepository.SaveAsync(
            BookingHistory.CrearCancelacion(booking.Id, motivo, usuarioId));

        // Intentar promoción automática desde lista de espera
        await _promoteUseCase.ExecuteAsync(vueloId, cancellationToken);

        return booking;
    }
}
