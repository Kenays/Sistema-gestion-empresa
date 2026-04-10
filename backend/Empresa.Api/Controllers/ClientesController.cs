using Empresa.Api.Application.Dtos;
using Empresa.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Empresa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ClientesController : ControllerBase
{
    private readonly ClienteService _clienteService;

    public ClientesController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> Get()
    {
        var clientes = await _clienteService.GetClientes();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> Get(int id)
    {
        var clientes = await _clienteService.GetCliente(id);

        if (clientes == null)
            return NotFound();

        return Ok(clientes);
    }

    [HttpPost]
    public async Task<ActionResult> Post(CrearClienteDto dto)
    {
        var id = await _clienteService.CrearCliente(dto);

        return CreatedAtAction(nameof(Get), new { id }, null);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, ActualizarClienteDto dto)
    {
      var actualizado = await _clienteService.ActualizarCliente(id, dto);

      if (!actualizado)
        return NotFound();

      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
      var eliminado = await _clienteService.EliminarCliente(id);
      
      if(!eliminado)
        return NotFound();

      return NoContent();

    }
}
