// src/modules/waitinglist/Infrastructure/entity/WaitingListEntity.cs
using AirTicketSystem.modules.booking.Infrastructure.entity;
using AirTicketSystem.modules.flight.Infrastructure.entity;

namespace AirTicketSystem.modules.waitinglist.Infrastructure.entity;

public class WaitingListEntity
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public int VueloId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public int Prioridad { get; set; }
    public string Estado { get; set; } = "PENDIENTE";

    public BookingEntity Reserva { get; set; } = null!;
    public FlightEntity Vuelo { get; set; } = null!;
}
