// src/modules/waitinglist/Domain/aggregate/WaitingList.cs
namespace AirTicketSystem.modules.waitinglist.Domain.aggregate;

public sealed class WaitingList
{
    public int Id { get; private set; }
    public int ReservaId { get; private set; }
    public int VueloId { get; private set; }
    public DateTime FechaRegistro { get; private set; }
    public int Prioridad { get; private set; }
    public string Estado { get; private set; } = "PENDIENTE";

    private WaitingList() { }

    public static WaitingList Crear(int reservaId, int vueloId, int prioridad)
    {
        if (reservaId <= 0)
            throw new ArgumentException("La reserva es obligatoria.");
        if (vueloId <= 0)
            throw new ArgumentException("El vuelo es obligatorio.");
        if (prioridad <= 0)
            throw new ArgumentException("La prioridad debe ser mayor a 0.");

        return new WaitingList
        {
            ReservaId    = reservaId,
            VueloId      = vueloId,
            FechaRegistro = DateTime.UtcNow,
            Prioridad    = prioridad,
            Estado       = "PENDIENTE"
        };
    }

    public static WaitingList Reconstituir(
        int id, int reservaId, int vueloId,
        DateTime fechaRegistro, int prioridad, string estado)
    {
        var w = new WaitingList
        {
            ReservaId    = reservaId,
            VueloId      = vueloId,
            FechaRegistro = fechaRegistro,
            Prioridad    = prioridad,
            Estado       = estado
        };
        w.Id = id;
        return w;
    }

    public void Promover()
    {
        if (Estado != "PENDIENTE")
            throw new InvalidOperationException(
                $"Solo se pueden promover entradas PENDIENTES. Estado actual: '{Estado}'.");
        Estado = "PROMOVIDO";
    }

    public void Cancelar()
    {
        if (Estado != "PENDIENTE")
            throw new InvalidOperationException(
                $"Solo se pueden cancelar entradas PENDIENTES. Estado actual: '{Estado}'.");
        Estado = "CANCELADO";
    }

    public bool EstaPendiente => Estado == "PENDIENTE";

    public void EstablecerId(int id) => Id = id;

    public override string ToString() =>
        $"Lista espera #{Id} — Reserva {ReservaId} | Vuelo {VueloId} | Prioridad {Prioridad} | {Estado}";
}
