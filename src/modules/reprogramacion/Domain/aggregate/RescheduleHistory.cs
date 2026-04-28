// src/modules/reprogramacion/Domain/aggregate/RescheduleHistory.cs
namespace AirTicketSystem.modules.reprogramacion.Domain.aggregate;

public sealed class RescheduleHistory
{
    public int Id { get; private set; }
    public int ReservaId { get; private set; }
    public int VueloAnteriorId { get; private set; }
    public int VueloNuevoId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Motivo { get; private set; } = null!;
    public int? UsuarioId { get; private set; }

    private RescheduleHistory() { }

    public static RescheduleHistory Crear(
        int reservaId,
        int vueloAnteriorId,
        int vueloNuevoId,
        string motivo,
        int? usuarioId = null)
    {
        if (reservaId <= 0)
            throw new ArgumentException("La reserva es obligatoria.");
        if (vueloAnteriorId <= 0)
            throw new ArgumentException("El vuelo anterior es obligatorio.");
        if (vueloNuevoId <= 0)
            throw new ArgumentException("El vuelo nuevo es obligatorio.");
        if (vueloAnteriorId == vueloNuevoId)
            throw new InvalidOperationException("El vuelo nuevo debe ser diferente al anterior.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo es obligatorio.");

        return new RescheduleHistory
        {
            ReservaId       = reservaId,
            VueloAnteriorId = vueloAnteriorId,
            VueloNuevoId    = vueloNuevoId,
            Fecha           = DateTime.UtcNow,
            Motivo          = motivo.Trim(),
            UsuarioId       = usuarioId
        };
    }

    public static RescheduleHistory Reconstituir(
        int id, int reservaId, int vueloAnteriorId,
        int vueloNuevoId, DateTime fecha, string motivo, int? usuarioId)
    {
        var h = new RescheduleHistory
        {
            ReservaId       = reservaId,
            VueloAnteriorId = vueloAnteriorId,
            VueloNuevoId    = vueloNuevoId,
            Fecha           = fecha,
            Motivo          = motivo,
            UsuarioId       = usuarioId
        };
        h.Id = id;
        return h;
    }

    public void EstablecerId(int id) => Id = id;

    public override string ToString() =>
        $"Reprogramación #{Id} — Reserva {ReservaId} | Vuelo {VueloAnteriorId} → {VueloNuevoId} [{Fecha:yyyy-MM-dd HH:mm}]";
}
