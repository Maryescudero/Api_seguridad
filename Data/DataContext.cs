using Api_seguridad.Reportes;
using Microsoft.EntityFrameworkCore;

namespace Api_seguridad.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Guardia> Guardias { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<HistorialUsuario> HistorialUsuarios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<AsignacionServicio> AsignacionServicios { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<TurnoServicio> TurnoServicios { get; set; }
        public DbSet<FrancoGuardia> FrancoGuardias { get; set; }

        public DbSet<Notificacion> Notificaciones { get; set; }
 



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Clave compuesta de la entidad de unión
            modelBuilder.Entity<TurnoServicio>()
                .HasKey(ts => new { ts.idServicio, ts.idTurno });

            // Relación TurnoServicio -> Servicio (muchos a uno)
            modelBuilder.Entity<TurnoServicio>()
                .HasOne(ts => ts.Servicio)
                .WithMany(s => s.TurnoServicios)
                .HasForeignKey(ts => ts.idServicio);

            // Relación TurnoServicio -> Turno (muchos a uno)
            modelBuilder.Entity<TurnoServicio>()
                .HasOne(ts => ts.Turno)
                .WithMany(t => t.TurnoServicios)
                .HasForeignKey(ts => ts.idTurno);

            // 👇 Configuración para AsignacionServicio
            modelBuilder.Entity<AsignacionServicio>()
                .Property(a => a.fechaAsignacion)
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),   // DateOnly → DateTime (SQL)
                    v => DateOnly.FromDateTime(v)          // DateTime (SQL) → DateOnly
                );

            // 👇 Configuración para FrancoGuardia (si también usás DateOnly)
            modelBuilder.Entity<FrancoGuardia>()
                .Property(f => f.fechaFranco)
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v)
                );


        }
    }
}
