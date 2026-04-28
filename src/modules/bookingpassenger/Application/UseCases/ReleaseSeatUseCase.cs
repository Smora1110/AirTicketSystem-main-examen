// src/modules/bookingpassenger/Application/UseCases/ReleaseSeatUseCase.cs
using AirTicketSystem.modules.bookingpassenger.Domain.aggregate;
using AirTicketSystem.modules.bookingpassenger.Domain.Repositories;
using AirTicketSystem.modules.seatavailability.Domain.Repositories;

namespace AirTicketSystem.modules.bookingpassenger.Application.UseCases;

public sealed class ReleaseSeatUseCase
{
    private readonly IBookingPassengerRepository _passengerRepo;
    private readonly ISeatAvailabilityRepository _seatRepo;

    public ReleaseSeatUseCase(
        IBookingPassengerRepository passengerRepo,
        ISeatAvailabilityRepository seatRepo)
    {
        _passengerRepo = passengerRepo;
        _seatRepo      = seatRepo;
    }

    public async Task<BookingPassenger> ExecuteAsync(
        int pasajeroReservaId, CancellationToken cancellationToken = default)
    {
        var passenger = await _passengerRepo.FindByIdAsync(pasajeroReservaId)
            ?? throw new KeyNotFoundException(
                $"No se encontró un pasajero de reserva con ID {pasajeroReservaId}.");

        // Liberar el asiento en disponibilidad_asientos
        if (passenger.TieneAsientoAsignado)
        {
            var seat = await _seatRepo.FindByIdAsync(passenger.AsientoId!.Value);
            if (seat is not null && seat.Estado.Valor == "RESERVADO")
            {
                seat.Liberar();
                await _seatRepo.UpdateAsync(seat);
            }
        }

        passenger.LiberarAsiento();
        await _passengerRepo.UpdateAsync(passenger);

        return passenger;
    }
}
