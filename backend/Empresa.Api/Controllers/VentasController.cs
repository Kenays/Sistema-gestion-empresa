using Empresa.Api.Application.Dtos;
using Empresa.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Empresa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly VentaService _ventaService;

    public VentasController(VentaService ventaService)
    {
        _ventaService = ventaService;
    }

    // 🔹 GET: api/ventas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaDto>>> Get()
    {
        var ventas = await _ventaService.ObtenerVentas();
        return Ok(ventas);
    }

    // 🔹 GET: api/ventas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<VentaDto>> Get(int id)
    {
        var venta = await _ventaService.ObtenerVenta(id);

        if (venta == null)
            return NotFound();

        return Ok(venta);
    }

    // 🔹 POST: api/ventas
    [HttpPost]
    public async Task<ActionResult> Post(CrearVentaDto dto)
    {
        var id = await _ventaService.CrearVenta(dto);

        return CreatedAtAction(
            nameof(Get),
            new { id },
            null
        );
    }

    // 🔹 PUT: api/ventas/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, ActualizarVentaDto dto)
    {
        var actualizado = await _ventaService.ActualizarVenta(id, dto);

        if (!actualizado)
            return NotFound();

        return NoContent();
    }

    // 🔹 DELETE: api/ventas/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var venta = await _ventaService.ObtenerVenta(id);

        if (venta == null)
            return NotFound();

        // soft delete
        var dto = new ActualizarVentaDto
        {
            Activo = false
        };

        await _ventaService.ActualizarVenta(id, dto);

        return NoContent();
    }
}
