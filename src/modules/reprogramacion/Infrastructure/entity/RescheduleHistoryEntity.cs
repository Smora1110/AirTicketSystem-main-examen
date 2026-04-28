// src/modules/reprogramacion/Infrastructure/entity/RescheduleHistoryEntity.cs
using AirTicketSystem.modules.booking.Infrastructure.entity;
using AirTicketSystem.modules.flight.Infrastructure.entity;
using AirTicketSystem.modules.user.Infrastructure.entity;

namespace AirTicketSystem.modules.reprogramacion.Infrastructure.entity;

public class RescheduleHistoryEntity
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public int VueloAnteriorId { get; set; }
    public int VueloNuevoId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Motivo { get; set; } = null!;
    public int? UsuarioId { get; set; }

    public BookingEntity Reserva { get; set; } = null!;
    public FlightEntity VueloAnterior { get; set; } = null!;
    public FlightEntity VueloNuevo { get; set; } = null!;
    public UserEntity? Usuario { get; set; }
}
