using Api_seguridad.Hubs;
using Api_seguridad.Models;
using Api_seguridad.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Api_seguridad.Repositorios
{
    public class RepositorioNotificacion
    {
        private readonly DataContext _db;
        private readonly IHubContext<NotificacionesHub> _hub;

        public RepositorioNotificacion(DataContext db, IHubContext<NotificacionesHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // =====================  Crear y enviar una notificación puntual =====================
        public async Task<NotificacionResponse> CrearNotificacion(string mensaje, int? enviadaPor, int? enviadaA, string rolDestino)
        {
            var notif = new Notificacion
            {
                mensaje = mensaje,
                enviada_por = enviadaPor,
                enviada_a = enviadaA,
                fecha_envio = DateTime.Now,
                leido = false
            };

            _db.Notificaciones.Add(notif);
            await _db.SaveChangesAsync();

            var dto = MapearDto(notif);

            if (rolDestino == "admin")
            {
                Console.WriteLine($"📢 Enviando a grupo=admin → {dto.Mensaje}");
                await _hub.Clients.Group("admin").SendAsync("RecibirNotificacion", dto);
            }
            else if (rolDestino == "guardia" && enviadaA.HasValue)
            {
                Console.WriteLine($"📢 Enviando a grupo=guardia_{enviadaA.Value} → {dto.Mensaje}");
                await _hub.Clients.Group($"guardia_{enviadaA.Value}").SendAsync("RecibirNotificacion", dto);
            }

            return dto;
        }

        // =====================  Crear notificaciones para varios admins =====================
        public async Task CrearNotificacionesParaAdmins(string mensaje, int? enviadaPor, List<int> destinatarios)
        {
            foreach (var adminId in destinatarios)
            {
                var notif = new Notificacion
                {
                    mensaje = mensaje,
                    enviada_por = enviadaPor,
                    enviada_a = adminId,
                    fecha_envio = DateTime.Now,
                    leido = false
                };

                _db.Notificaciones.Add(notif);
                await _db.SaveChangesAsync();

                var dto = MapearDto(notif);

                Console.WriteLine($"📢 Enviando a grupo=admin → {dto.Mensaje}");
                await _hub.Clients.Group("admin").SendAsync("RecibirNotificacion", dto);
            }
        }

        // =====================  Enviar aviso de quincena =====================
        
        public async Task EnviarAvisoQuincena(int? enviadaPor)
        {
            var usuariosGuardias = await (
                from g in _db.Guardias
                join u in _db.Usuarios on g.idGuardia equals u.idGuardia
                select u
            ).ToListAsync();

            var mensaje = "Ya están disponibles las nuevas asignaciones y francos.";

            foreach (var usuario in usuariosGuardias)
            {
                var notif = new Notificacion
                {
                    mensaje = mensaje,
                    enviada_por = enviadaPor,
                    enviada_a = usuario.idUsuario,
                    fecha_envio = DateTime.Now,
                    leido = false
                };

                _db.Notificaciones.Add(notif);
                await _db.SaveChangesAsync();

                var dto = MapearDto(notif);

                Console.WriteLine($"📢 Enviando aviso quincena a guardia_{usuario.idUsuario} → {dto.Mensaje}");
                await _hub.Clients.Group($"guardia_{usuario.idUsuario}")
                                  .SendAsync("RecibirNotificacion", dto);
            }
        }



        // =====================  Obtener por usuario =====================
        public List<Notificacion> ObtenerPorUsuario(int idUsuario)
        {
            return _db.Notificaciones
                      .Where(n => n.enviada_a == idUsuario)
                      .OrderByDescending(n => n.fecha_envio)
                      .ToList();
        }

        // =====================  Marcar como leída =====================
        public async Task<NotificacionResponse?> MarcarComoLeida(int idNotificacion, int idUsuario)
        {
            var notif = await _db.Notificaciones
                .FirstOrDefaultAsync(n => n.id_notificacion == idNotificacion && n.enviada_a == idUsuario);

            if (notif == null) return null;

            notif.leido = true;
            await _db.SaveChangesAsync();

            var dto = MapearDto(notif);

            Console.WriteLine($"📢 Notificación marcada como leída. Avisando a admin → {dto.IdNotificacion}");
            await _hub.Clients.Group("admin").SendAsync("NotificacionLeida", dto);

            return dto;
        }

        // =====================  Helper =====================
        private NotificacionResponse MapearDto(Notificacion notif)
        {
            return new NotificacionResponse
            {
                IdNotificacion = notif.id_notificacion,
                Mensaje = notif.mensaje,
                Fecha = notif.fecha_envio.ToString("yyyy-MM-dd HH:mm"),
                EnviadaPor = notif.enviada_por,
                EnviadaA = notif.enviada_a,
                Leido = notif.leido
            };
        }
    }
}
