using Empresa.Api.Application.Dtos;
using Empresa.Api.Domain;
using Empresa.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Empresa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ClientesController : ControllerBase
{
  private readonly EmpresaDbContext _db;

  public ClientesController(EmpresaDbContext db)
  {
      _db = db;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<ClienteDto>>> Get()
  {
      var clientes = await _db
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
      return Ok(clientes);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<ClienteDto>> Get(int id)
  {
      var clientes = await _db
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

      if (clientes == null)
      {
          return NotFound();
      }

      return Ok(clientes);
  }

  [HttpPost]
  public async Task<ActionResult> Post(CrearClienteDto dto)
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

      return CreatedAtAction(nameof(Get), new { id = clientes.IdCliente }, clientes);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult> Put(int id, ActualizarClienteDto dto)
  {
      var clientes = await _db.Clientes.FindAsync(id);

      if (clientes == null)
          return NotFound();

      if (dto.Nombre != null)
          clientes.Nombre = dto.Nombre;

      if (dto.Email != null)
          clientes.Email = dto.Email;

      if (dto.Telefono != null)
          clientes.Telefono = dto.Telefono;

      if (dto.Activo.HasValue)
          clientes.Activo = dto.Activo.Value;

      await _db.SaveChangesAsync();
      return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> Delete(int id)
  {
    var cliente = await _db.Clientes.FindAsync(id);

    if(cliente==null)
      return NotFound();

    cliente.Activo = false;

    await _db.SaveChangesAsync();

    return NoContent();
  }
}
