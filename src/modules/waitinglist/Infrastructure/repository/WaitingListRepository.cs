// src/modules/waitinglist/Infrastructure/repository/WaitingListRepository.cs
using Microsoft.EntityFrameworkCore;
using AirTicketSystem.shared.context;
using AirTicketSystem.modules.waitinglist.Domain.aggregate;
using AirTicketSystem.modules.waitinglist.Domain.Repositories;
using AirTicketSystem.modules.waitinglist.Infrastructure.entity;

namespace AirTicketSystem.modules.waitinglist.Infrastructure.repository;

public sealed class WaitingListRepository : IWaitingListRepository
{
    private readonly AppDbContext _context;

    public WaitingListRepository(AppDbContext context) => _context = context;

    public async Task<WaitingList?> FindByIdAsync(int id)
    {
        var entity = await _context.ListaEspera.FindAsync(id);
        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyCollection<WaitingList>> FindByVueloAsync(int vueloId)
    {
        var entities = await _context.ListaEspera
            .Where(w => w.VueloId == vueloId)
            .OrderBy(w => w.Prioridad)
            .ToListAsync();
        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyCollection<WaitingList>> FindPendientesByVueloAsync(int vueloId)
    {
        var entities = await _context.ListaEspera
            .Where(w => w.VueloId == vueloId && w.Estado == "PENDIENTE")
            .OrderBy(w => w.Prioridad)
            .ToListAsync();
        return entities.Select(MapToDomain).ToList();
    }

    public async Task<WaitingList?> FindPrimeroPendienteAsync(int vueloId)
    {
        var entity = await _context.ListaEspera
            .Where(w => w.VueloId == vueloId && w.Estado == "PENDIENTE")
            .OrderBy(w => w.Prioridad)
            .FirstOrDefaultAsync();
        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<bool> ExisteReservaEnEsperaAsync(int reservaId, int vueloId)
        => await _context.ListaEspera
            .AnyAsync(w => w.ReservaId == reservaId && w.VueloId == vueloId && w.Estado == "PENDIENTE");

    public async Task<int> SiguientePrioridadAsync(int vueloId)
    {
        var max = await _context.ListaEspera
            .Where(w => w.VueloId == vueloId)
            .MaxAsync(w => (int?)w.Prioridad);
        return (max ?? 0) + 1;
    }

    public async Task SaveAsync(WaitingList entry)
    {
        var entity = MapToEntity(entry);
        _context.ListaEspera.Add(entity);
        await _context.SaveChangesAsync();
        entry.EstablecerId(entity.Id);
    }

    public async Task UpdateAsync(WaitingList entry)
    {
        var entity = await _context.ListaEspera.FindAsync(entry.Id)
            ?? throw new KeyNotFoundException(
                $"No se encontró la entrada de lista de espera con ID {entry.Id}.");
        entity.Estado = entry.Estado;
        await _context.SaveChangesAsync();
    }

    private static WaitingList MapToDomain(WaitingListEntity e) =>
        WaitingList.Reconstituir(e.Id, e.ReservaId, e.VueloId,
            e.FechaRegistro, e.Prioridad, e.Estado);

    private static WaitingListEntity MapToEntity(WaitingList w) => new()
    {
        ReservaId     = w.ReservaId,
        VueloId       = w.VueloId,
        FechaRegistro = w.FechaRegistro,
        Prioridad     = w.Prioridad,
        Estado        = w.Estado
    };
}
