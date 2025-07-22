using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TallerAutomotriz.Core.Entities;

namespace TallerAutomotriz.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SolicitudRepuesto> SolicitudesRepuesto { get; internal set; }
        public DbSet<Repuesto> Repuestos { get; internal set; }
        public DbSet<Usuario> Usuarios { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Repuesto>()
                .Property(r => r.PrecioUnitario)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<SolicitudRepuesto>()
                .HasOne(sr => sr.Solicitante)
                .WithMany() 
                .HasForeignKey(sr => sr.IdSolicitante)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<SolicitudRepuesto>()
                .HasOne(sr => sr.UsuarioEntrega)
                .WithMany()
                .HasForeignKey(sr => sr.IdUsuarioEntrega)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SolicitudRepuesto>()
                .HasOne(sr => sr.Repuesto)
                .WithMany()
                .HasForeignKey(sr => sr.IdRepuesto)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
