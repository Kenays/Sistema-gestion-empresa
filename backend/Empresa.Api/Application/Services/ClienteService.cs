using Empresa.Api.Application.Dtos;
using Empresa.Api.Domain;
using Empresa.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Empresa.Api.Application.Services;

public class ClienteService
{
    private readonly EmpresaDbContext _db;
    private readonly IConfiguration _config;

    public ClienteService(EmpresaDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<IEnumerable<ClienteDto>> GetClientes()
    {
        return await _db
            .Clientes.Select(c => new ClienteDto
            {
                IdCliente = c.IdCliente,
                Nombre = c.Nombre,
                Email = c.Email,
                Telefono = c.Telefono,
                Activo = c.Activo,
                FechaRegistro = c.FechaRegistro,
            })
            .ToListAsync();
    }

    public async Task<ClienteDto?> GetCliente(int id)
    {
        return await _db
            .Clientes.Where(c => c.IdCliente == id)
            .Select(c => new ClienteDto
            {
                IdCliente = c.IdCliente,
                Nombre = c.Nombre,
                Email = c.Email,
                Telefono = c.Telefono,
                Activo = c.Activo,
                FechaRegistro = c.FechaRegistro,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CrearCliente(CrearClienteDto dto)
    {
        var clientes = new Cliente
        {
            Nombre = dto.Nombre,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Activo = true,
        };

        _db.Clientes.Add(clientes);

        await _db.SaveChangesAsync();

        return clientes.IdCliente;
    }

    public async Task<bool> ActualizarCliente(int id, ActualizarClienteDto dto)
    {
        var clientes = await _db.Clientes.FindAsync(id);

        if (clientes == null)
            return false;

        if (dto.Nombre != null)
            clientes.Nombre = dto.Nombre;

        if (dto.Email != null)
            clientes.Email = dto.Email;

        if (dto.Telefono != null)
            clientes.Telefono = dto.Telefono;

        if (dto.Activo.HasValue)
            clientes.Activo = dto.Activo.Value;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarCliente(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);

        if (cliente == null)
            return false;

        cliente.Activo = false;
        await _db.SaveChangesAsync();
        return true;
    }
}
