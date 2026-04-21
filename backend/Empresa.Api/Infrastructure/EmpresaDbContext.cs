using Empresa.Api.Application.Dtos;
using Empresa.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Empresa.Api.Infrastructure;

public class EmpresaDbContext : DbContext
{
    public EmpresaDbContext(DbContextOptions<EmpresaDbContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(u => u.IdUsuario);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Rol");
            entity.HasKey(r => r.IdRol);
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.ToTable("UsuarioRol");

            entity.HasKey(ur => new { ur.IdUsuario, ur.IdRol });

            entity
                .HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.IdUsuario);

            entity.HasOne(ur => ur.Rol).WithMany(r => r.UsuarioRoles).HasForeignKey(ur => ur.IdRol);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Producto");
            entity.HasKey(p => p.IdProducto);

            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Precio).HasColumnType("numeric(10,2)");
            entity.Property(p => p.Costo).HasColumnType("numeric(10,2)");
            entity.Property(p => p.Codigo).HasMaxLength(50);

            entity.HasIndex(p => p.Codigo).IsUnique();

            entity.Property(p => p.Activo).HasDefaultValue(true);

            // 🔥 CAMBIO CLAVE
            entity.Property(p => p.FechaCreacion).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable("Venta");
            entity.HasKey(v => v.IdVenta);

            // 🔥 CAMBIO CLAVE
            entity.Property(v => v.Fecha).HasDefaultValueSql("NOW()");

            entity.Property(v => v.Subtotal).HasColumnType("numeric(10,2)");
            entity.Property(v => v.Impuesto).HasColumnType("numeric(10,2)");
            entity.Property(v => v.Total).HasColumnType("numeric(10,2)");

            entity.Property(v => v.Activo).HasDefaultValue(true);

            entity
                .HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(v => v.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.ToTable("DetalleVenta");

            entity.HasKey(d => d.IdDetalle);

            entity.Property(d => d.PrecioUnitario).HasColumnType("numeric(10,2)");
            entity.Property(d => d.Impuesto).HasColumnType("numeric(10,2)");
            entity.Property(d => d.Subtotal).HasColumnType("numeric(10,2)");

            entity
                .HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
            entity.HasKey(c => c.IdCliente);

            // 🔥 CAMBIO CLAVE
            entity.Property(c => c.FechaRegistro).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<UsuarioDto>(entity =>
        {
            if (Database.IsSqlServer())
            {
                entity.ToView("vw_Usuarios");
            }
            else
            {
                entity.HasNoKey();
            }
        });

        base.OnModelCreating(modelBuilder);
    }
}
