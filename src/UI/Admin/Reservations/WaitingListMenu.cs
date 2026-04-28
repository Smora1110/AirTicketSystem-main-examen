// src/UI/Admin/Reservations/WaitingListMenu.cs
using Microsoft.Extensions.DependencyInjection;
using AirTicketSystem.shared.UI;
using AirTicketSystem.shared.helpers;
using AirTicketSystem.modules.waitinglist.Application.UseCases;
using AirTicketSystem.modules.reprogramacion.Application.UseCases;

namespace AirTicketSystem.UI.Admin.Reservations;

public sealed class WaitingListMenu
{
    private readonly IServiceProvider _provider;

    public WaitingListMenu(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task MostrarAsync()
    {
        while (true)
        {
            SpectreHelper.MostrarTitulo("Lista de Espera y Reprogramación");

            var opcion = SpectreHelper.SeleccionarOpcionTexto("Seleccione una acción",
                [
                    "Ver lista de espera por vuelo",
                    "Ver historial de reprogramación por reserva",
                    "Volver"
                ]);

            switch (opcion)
            {
                case "Ver lista de espera por vuelo":
                    await VerListaEsperaAsync();
                    break;
                case "Ver historial de reprogramación por reserva":
                    await VerHistorialReprogramacionAsync();
                    break;
                case "Volver":
                    return;
            }
        }
    }

    private async Task VerListaEsperaAsync()
    {
        var vueloId = SpectreHelper.PedirEntero("ID del vuelo");
        await ConsoleErrorHandler.ExecuteAsync(async () =>
        {
            await using var scope = _provider.CreateAsyncScope();
            var lista = await scope.ServiceProvider
                .GetRequiredService<GetWaitingListByFlightUseCase>()
                .ExecuteAsync(vueloId);

            if (lista.Count == 0)
            {
                SpectreHelper.MostrarInfo("No hay entradas en lista de espera para este vuelo.");
                SpectreHelper.EsperarTecla();
                return;
            }

            var tabla = SpectreHelper.CrearTabla(
                "ID", "ReservaID", "Prioridad", "Fecha Registro", "Estado");
            foreach (var e in lista)
                SpectreHelper.AgregarFila(tabla,
                    e.Id.ToString(),
                    e.ReservaId.ToString(),
                    e.Prioridad.ToString(),
                    e.FechaRegistro.ToString("yyyy-MM-dd HH:mm"),
                    e.Estado);
            SpectreHelper.MostrarTabla(tabla);
            SpectreHelper.EsperarTecla();
        });
    }

    private async Task VerHistorialReprogramacionAsync()
    {
        var reservaId = SpectreHelper.PedirEntero("ID de la reserva");
        await ConsoleErrorHandler.ExecuteAsync(async () =>
        {
            await using var scope = _provider.CreateAsyncScope();
            var historial = await scope.ServiceProvider
                .GetRequiredService<GetRescheduleHistoryUseCase>()
                .ExecuteAsync(reservaId);

            if (historial.Count == 0)
            {
                SpectreHelper.MostrarInfo("No hay reprogramaciones para esta reserva.");
                SpectreHelper.EsperarTecla();
                return;
            }

            var tabla = SpectreHelper.CrearTabla(
                "ID", "VueloAnterior", "VueloNuevo", "Fecha", "Motivo");
            foreach (var h in historial)
                SpectreHelper.AgregarFila(tabla,
                    h.Id.ToString(),
                    h.VueloAnteriorId.ToString(),
                    h.VueloNuevoId.ToString(),
                    h.Fecha.ToString("yyyy-MM-dd HH:mm"),
                    h.Motivo);
            SpectreHelper.MostrarTabla(tabla);
            SpectreHelper.EsperarTecla();
        });
    }
}
