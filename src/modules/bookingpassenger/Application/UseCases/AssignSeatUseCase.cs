// src/modules/bookingpassenger/Application/UseCases/AssignSeatUseCase.cs
using AirTicketSystem.modules.bookingpassenger.Domain.aggregate;
using AirTicketSystem.modules.bookingpassenger.Domain.Repositories;
using AirTicketSystem.modules.seatavailability.Domain.Repositories;

namespace AirTicketSystem.modules.bookingpassenger.Application.UseCases;

public sealed class AssignSeatUseCase
{
    private readonly IBookingPassengerRepository _passengerRepo;
    private readonly ISeatAvailabilityRepository _seatRepo;

    public AssignSeatUseCase(
        IBookingPassengerRepository passengerRepo,
        ISeatAvailabilityRepository seatRepo)
    {
        _passengerRepo = passengerRepo;
        _seatRepo      = seatRepo;
    }

    public async Task<BookingPassenger> ExecuteAsync(
        int pasajeroReservaId,
        int seatAvailabilityId,
        CancellationToken cancellationToken = default)
    {
        var passenger = await _passengerRepo.FindByIdAsync(pasajeroReservaId)
            ?? throw new KeyNotFoundException(
                $"No se encontró un pasajero de reserva con ID {pasajeroReservaId}.");

        // Verificar y reservar el asiento en disponibilidad_asientos
        var seat = await _seatRepo.FindByIdAsync(seatAvailabilityId)
            ?? throw new KeyNotFoundException(
                $"No se encontró disponibilidad de asiento con ID {seatAvailabilityId}.");

        if (!seat.EstaDisponible)
            throw new InvalidOperationException(
                $"El asiento con ID {seatAvailabilityId} no está disponible (estado: {seat.Estado}).");

        seat.Reservar();
        await _seatRepo.UpdateAsync(seat);

        passenger.AsignarAsiento(seatAvailabilityId);
        await _passengerRepo.UpdateAsync(passenger);

        return passenger;
    }
}
