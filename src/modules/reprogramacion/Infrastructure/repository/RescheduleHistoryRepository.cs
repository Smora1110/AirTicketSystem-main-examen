// src/modules/reprogramacion/Infrastructure/repository/RescheduleHistoryRepository.cs
using Microsoft.EntityFrameworkCore;
using AirTicketSystem.shared.context;
using AirTicketSystem.modules.reprogramacion.Domain.aggregate;
using AirTicketSystem.modules.reprogramacion.Domain.Repositories;
using AirTicketSystem.modules.reprogramacion.Infrastructure.entity;

namespace AirTicketSystem.modules.reprogramacion.Infrastructure.repository;

public sealed class RescheduleHistoryRepository : IRescheduleHistoryRepository
{
    private readonly AppDbContext _context;

    public RescheduleHistoryRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyCollection<RescheduleHistory>> FindByReservaAsync(int reservaId)
    {
        var entities = await _context.HistorialReprogramacion
            .Where(h => h.ReservaId == reservaId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();
        return entities.Select(MapToDomain).ToList();
    }

    public async Task<RescheduleHistory?> FindUltimoByReservaAsync(int reservaId)
    {
        var entity = await _context.HistorialReprogramacion
            .Where(h => h.ReservaId == reservaId)
            .OrderByDescending(h => h.Fecha)
            .FirstOrDefaultAsync();
        return entity is null ? null : MapToDomain(entity);
    }

    public async Task SaveAsync(RescheduleHistory history)
    {
        var entity = MapToEntity(history);
        _context.HistorialReprogramacion.Add(entity);
        await _context.SaveChangesAsync();
        history.EstablecerId(entity.Id);
    }

    private static RescheduleHistory MapToDomain(RescheduleHistoryEntity e) =>
        RescheduleHistory.Reconstituir(
            e.Id, e.ReservaId, e.VueloAnteriorId,
            e.VueloNuevoId, e.Fecha, e.Motivo, e.UsuarioId);

    private static RescheduleHistoryEntity MapToEntity(RescheduleHistory h) => new()
    {
        ReservaId       = h.ReservaId,
        VueloAnteriorId = h.VueloAnteriorId,
        VueloNuevoId    = h.VueloNuevoId,
        Fecha           = h.Fecha,
        Motivo          = h.Motivo,
        UsuarioId       = h.UsuarioId
    };
}
