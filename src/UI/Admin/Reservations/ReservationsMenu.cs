// src/UI/Admin/Reservations/ReservationsMenu.cs
using AirTicketSystem.shared.UI;
using AirTicketSystem.shared.helpers;
using AirTicketSystem.UI.Admin.Billing;
using AirTicketSystem.UI.Admin.Reservations;

namespace AirTicketSystem.UI.Admin.Reservations;

public sealed class ReservationsMenu
{
    private readonly IServiceProvider _provider;
    private readonly SessionContext   _session;

    public ReservationsMenu(IServiceProvider provider, SessionContext session)
    {
        _provider = provider;
        _session  = session;
    }

    public async Task MostrarAsync()
    {
        while (true)
        {
            SpectreHelper.MostrarTitulo("Reservas y Pasajeros");

            var opcion = SpectreHelper.SeleccionarOpcionTexto("Seleccione un módulo",
                [
                    "Reservas",
                    "Lista de Espera y Reprogramación",
                    "Pasajeros de reserva",
                    "Check-in",
                    "Tiquetes",
                    "Equipaje",
                    "Cargos adicionales",
                    "Pagos",
                    "Facturas",
                    "Volver"
                ]);

            switch (opcion)
            {
                case "Reservas":           await new BookingMenu(_provider, _session).MostrarAsync(); break;
                case "Lista de Espera y Reprogramación": await new WaitingListMenu(_provider).MostrarAsync(); break;
                case "Pasajeros de reserva": await new PassengerMenu(_provider).MostrarAsync();       break;
                case "Check-in":           await new CheckInMenu(_provider).MostrarAsync();           break;
                case "Tiquetes":           await new TicketAdminMenu(_provider).MostrarAsync();       break;
                case "Equipaje":           await new LuggageAdminMenu(_provider).MostrarAsync();      break;
                case "Cargos adicionales": await new AdditionalChargeMenu(_provider).MostrarAsync();  break;
                case "Pagos":              await new PaymentMenu(_provider).MostrarAsync();           break;
                case "Facturas":           await new InvoiceMenu(_provider).MostrarAsync();           break;
                case "Volver":             return;
            }
        }
    }
}
