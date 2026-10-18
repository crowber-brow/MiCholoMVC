using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Models;

namespace MiCholoMVC.Data
{
    public class MiCholoDbContext : DbContext
    {
        public MiCholoDbContext(
            DbContextOptions<MiCholoDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<Perdida> Perdidas { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Venta>().HasIndex(v => v.TokenOperacion).IsUnique();
            modelBuilder.Entity<Caja>().HasIndex(c => c.Estado).IsUnique().HasFilter("[Estado] = N'ABIERTA'");
            modelBuilder.Entity<Venta>().Property(v => v.DineroRecibido).HasPrecision(10, 2);
            modelBuilder.Entity<Venta>().Property(v => v.Vuelto).HasPrecision(10, 2);
            modelBuilder.Entity<Lote>().Property(l => l.CostoUnitario).HasPrecision(10, 2);
            modelBuilder.Entity<Perdida>().Property(p => p.ValorUnitario).HasPrecision(10, 2);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.CostoUnitario).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.EfectivoContado).HasPrecision(10, 2);
            modelBuilder.Entity<Caja>().Property(c => c.Diferencia).HasPrecision(10, 2);
            modelBuilder.Entity<MovimientoInventario>().Property(m => m.ValorUnitario).HasPrecision(10, 2);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(p => p.Productos)
                .HasForeignKey(p => p.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lote>()
                .HasOne(l => l.Producto)
                .WithMany(p => p.Lotes)
                .HasForeignKey(l => l.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Caja)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Lote)
                .WithMany()
                .HasForeignKey(d => d.IdLote)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoCaja>()
                .HasOne(m => m.Caja)
                .WithMany(c => c.MovimientosCaja)
                .HasForeignKey(m => m.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Perdida>()
                .HasOne(p => p.Lote)
                .WithMany()
                .HasForeignKey(p => p.IdLote)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Producto)
                .WithMany()
                .HasForeignKey(m => m.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Lote)
                .WithMany()
                .HasForeignKey(m => m.IdLote)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
