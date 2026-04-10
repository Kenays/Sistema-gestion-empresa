using Empresa.Api.Application.Dtos;
using Empresa.Api.Domain;
using Empresa.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Empresa.Api.Application.Services;

public class ProductoService
{
    private readonly EmpresaDbContext _db;

    public ProductoService(EmpresaDbContext db)
    {
        _db = db;
    }

    //Recordemos que el tipo es un objeto IEnumerable porque eso le queremos dar al Controller.
    public async Task<IEnumerable<ProductoDto>> GetProductos(bool incluirInactivos = false)
    {
        var query = _db.Productos.AsQueryable();

        if (!incluirInactivos)
            query = query.Where(p => p.Activo);

        return await query
            .Select(p => new ProductoDto
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                Codigo = p.Codigo,
                Impuesto = p.Impuesto,
                Activo = p.Activo,
            })
            .ToListAsync();
    }

    public async Task<ProductoDto?> GetProducto(int id)
    {
        return await _db
            .Productos.Where(p => p.IdProducto == id && p.Activo)
            .Select(p => new ProductoDto
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                Codigo = p.Codigo,
                Impuesto = p.Impuesto,
                Activo = p.Activo,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> PostProducto(CrearProductoDto dto)
    {
        // Validar código único
        if (await _db.Productos.AnyAsync(p => p.Codigo == dto.Codigo))
            throw new Exception("El código ya existe.");

        // Validaciones básicas
        if (dto.Precio < 0)
            throw new Exception("Precio inválido.");

        if (dto.Stock < 0)
            throw new Exception("Stock inválido.");

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Costo = dto.Costo,
            Stock = dto.Stock,
            Codigo = dto.Codigo,
            Impuesto = dto.Impuesto,
            Activo = true,
        };

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        return producto.IdProducto;
    }

    public async Task<bool> EliminarProducto(int id)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p =>
            p.IdProducto == id && p.Activo == true
        );

        if (producto == null)
            return false;

        producto.Activo = false;
        await _db.SaveChangesAsync();
        return true;
    }

    // PATCH (ACTUALIZACIÓN PARCIAL)
    public async Task<bool> ActualizarProducto(int id, ActualizarProductoDto dto)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);

        if (producto == null)
            return false;

        if (dto.Nombre != null)
            producto.Nombre = dto.Nombre;

        if (dto.Descripcion != null)
            producto.Descripcion = dto.Descripcion;

        if (dto.Precio.HasValue)
        {
            if (dto.Precio.Value < 0)
                throw new Exception("Precio inválido.");

            producto.Precio = dto.Precio.Value;
        }

        if (dto.Costo.HasValue)
            producto.Costo = dto.Costo.Value;

        if (dto.Stock.HasValue)
        {
            if (dto.Stock.Value < 0)
                throw new Exception("Stock inválido.");

            producto.Stock = dto.Stock.Value;
        }

        if (dto.Impuesto.HasValue)
            producto.Impuesto = dto.Impuesto.Value;

        if (dto.Activo.HasValue)
            producto.Activo = dto.Activo.Value;

        await _db.SaveChangesAsync();
        return true;
    }
}
