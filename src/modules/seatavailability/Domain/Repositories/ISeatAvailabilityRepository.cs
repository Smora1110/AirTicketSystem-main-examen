using AirTicketSystem.modules.seatavailability.Domain.aggregate;

namespace AirTicketSystem.modules.seatavailability.Domain.Repositories;

public interface ISeatAvailabilityRepository
{
    Task<SeatAvailability?> FindByIdAsync(int id);
    Task<SeatAvailability?> FindByVueloAndAsientoAsync(int vueloId, int asientoId);
    Task<IReadOnlyCollection<SeatAvailability>> FindByVueloAsync(int vueloId);
    Task<IReadOnlyCollection<SeatAvailability>> FindDisponiblesByVueloAsync(int vueloId);
    Task<IReadOnlyCollection<SeatAvailability>> FindDisponiblesByVueloAndClaseAsync(
        int vueloId, int claseServicioId);
    Task<int> ContarDisponiblesByVueloAsync(int vueloId);
    Task<bool> AsientoDisponibleAsync(int vueloId, int asientoId);
    Task<SeatAvailability?> FindPrimerDisponibleByVueloAsync(int vueloId);
    Task SaveAllAsync(IEnumerable<SeatAvailability> asientos);
    Task UpdateAsync(SeatAvailability seatAvailability);
}