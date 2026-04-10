using Empresa.Api.Application.Dtos;
using Empresa.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Empresa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly ProductoService _productoService;

    public ProductosController(ProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> Get()
    {
        var productos = await _productoService.GetProductos();
        return Ok(productos);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("admin")]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAdmin()
    {
        var productos = await _productoService.GetProductos(true);
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDto>> Get(int id)
    {
        var producto = await _productoService.GetProducto(id);

        if (producto == null)
            return NotFound();

        return Ok(producto);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult> Post(CrearProductoDto dto)
    {
        var id = await _productoService.PostProducto(dto);
        return CreatedAtAction(nameof(Get), new { id }, null);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, ActualizarProductoDto dto)
    {
        if (await _productoService.ActualizarProducto(id, dto))
            return NoContent();

        return NotFound();
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var resultado = await _productoService.EliminarProducto(id);

        if (resultado)
            return NoContent();

        return NotFound();
    }
}
