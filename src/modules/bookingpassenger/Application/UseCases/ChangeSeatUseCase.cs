// src/modules/bookingpassenger/Application/UseCases/ChangeSeatUseCase.cs
using AirTicketSystem.modules.bookingpassenger.Domain.aggregate;
using AirTicketSystem.modules.bookingpassenger.Domain.Repositories;
using AirTicketSystem.modules.seatavailability.Domain.Repositories;

namespace AirTicketSystem.modules.bookingpassenger.Application.UseCases;

public sealed class ChangeSeatUseCase
{
    private readonly IBookingPassengerRepository _passengerRepo;
    private readonly ISeatAvailabilityRepository _seatRepo;

    public ChangeSeatUseCase(
        IBookingPassengerRepository passengerRepo,
        ISeatAvailabilityRepository seatRepo)
    {
        _passengerRepo = passengerRepo;
        _seatRepo      = seatRepo;
    }

    public async Task<BookingPassenger> ExecuteAsync(
        int pasajeroReservaId,
        int nuevoSeatAvailabilityId,
        CancellationToken cancellationToken = default)
    {
        var passenger = await _passengerRepo.FindByIdAsync(pasajeroReservaId)
            ?? throw new KeyNotFoundException(
                $"No se encontró un pasajero de reserva con ID {pasajeroReservaId}.");

        // Liberar el asiento anterior en disponibilidad_asientos
        if (passenger.TieneAsientoAsignado)
        {
            var seatAnterior = await _seatRepo.FindByIdAsync(passenger.AsientoId!.Value);
            if (seatAnterior is not null && seatAnterior.Estado.Valor == "RESERVADO")
            {
                seatAnterior.Liberar();
                await _seatRepo.UpdateAsync(seatAnterior);
            }
        }

        // Verificar y reservar el nuevo asiento en disponibilidad_asientos
        var nuevoSeat = await _seatRepo.FindByIdAsync(nuevoSeatAvailabilityId)
            ?? throw new KeyNotFoundException(
                $"No se encontró disponibilidad de asiento con ID {nuevoSeatAvailabilityId}.");

        if (!nuevoSeat.EstaDisponible)
            throw new InvalidOperationException(
                $"El asiento con ID {nuevoSeatAvailabilityId} no está disponible (estado: {nuevoSeat.Estado}).");

        nuevoSeat.Reservar();
        await _seatRepo.UpdateAsync(nuevoSeat);

        passenger.CambiarAsiento(nuevoSeatAvailabilityId);
        await _passengerRepo.UpdateAsync(passenger);

        return passenger;
    }
}
