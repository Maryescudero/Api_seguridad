using Microsoft.AspNetCore.SignalR;

namespace Api_seguridad.Hubs
{
    public class NotificacionesHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var query = Context.GetHttpContext()?.Request.Query;

            string? userId = query?["userId"].ToString();
            string? rol = query?["rol"].ToString()?.ToLower();

            string grupo = string.Empty;

            if (rol == "admin" || rol == "administrador")
            {
                grupo = "admin";
                await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
            }
            else if (rol == "guardia" && !string.IsNullOrEmpty(userId))
            {
                // Grupo individual
                grupo = $"guardia_{userId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, grupo);

                // Grupo global para todos los guardias
                await Groups.AddToGroupAsync(Context.ConnectionId, "guardias");
            }

            Console.WriteLine($"✅ Cliente conectado: ConnId={Context.ConnectionId}, Rol={rol}, UserId={userId}, Grupo={grupo}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var query = Context.GetHttpContext()?.Request.Query;

            string? userId = query?["userId"].ToString();
            string? rol = query?["rol"].ToString()?.ToLower();

            string grupo = string.Empty;

            if (rol == "admin" || rol == "administrador")
            {
                grupo = "admin";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
            }
            else if (rol == "guardia" && !string.IsNullOrEmpty(userId))
            {
                // Sacar del grupo individual
                grupo = $"guardia_{userId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);

                // Sacar también del grupo global
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "guardias");
            }

            Console.WriteLine($"⚠ Cliente desconectado: ConnId={Context.ConnectionId}, Rol={rol}, UserId={userId}, Grupo={grupo}");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
