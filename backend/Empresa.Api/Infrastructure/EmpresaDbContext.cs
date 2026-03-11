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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MAPEO EXPLÍCITO (MUY PRO)
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

        // Vista
        modelBuilder.Entity<UsuarioDto>().HasNoKey().ToView("vw_Usuarios");

        modelBuilder.Entity<Cliente>().ToTable("Cliente").HasKey(c => c.IdCliente);

        modelBuilder
            .Entity<Cliente>()
            .Property(c => c.FechaRegistro)
            .HasDefaultValueSql("SYSDATETIME()");

        base.OnModelCreating(modelBuilder);
    }
}
