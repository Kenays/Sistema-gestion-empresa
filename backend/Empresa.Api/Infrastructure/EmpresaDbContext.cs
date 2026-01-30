using Empresa.Api.Application.Dtos;
using Empresa.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Empresa.Api.Infrastructure;

public class EmpresaDbContext : DbContext
{
    public EmpresaDbContext(DbContextOptions<EmpresaDbContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entidades persistentes (tablas)
        modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);

        // Proyecciones de lectura (vistas / reportes)
        modelBuilder.Entity<UsuarioDto>().HasNoKey().ToView("vw_Usuarios");

        base.OnModelCreating(modelBuilder);
    }
}
