using Empresa.Api.Domain;
using Empresa.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Empresa.Api.Application.Services;

public class VentaService
{
    private readonly EmpresaDbContext _db;

    public VentaService(EmpresaDbContext db)
    {
        _db = db;
    }

    // ============================
    // GET TODAS
    // ============================
    public async Task<IEnumerable<VentaDto>> ObtenerVentas()
    {
        return await _db.Ventas
            .Where(v => v.Activo)
            .Select(v => new VentaDto
            {
                IdVenta = v.IdVenta,
                IdCliente = v.IdCliente,
                Fecha = v.Fecha,
                Subtotal = v.Subtotal,
                Impuesto = v.Impuesto,
                Total = v.Total,
                Detalles = v.Detalles.Select(d => new DetalleVentaDto
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Impuesto = d.Impuesto,
                    NombreProducto = d.Producto.Nombre
                }).ToList()
            })
            .ToListAsync();
    }

    // ============================
    // GET POR ID (ACTIVO)
    // ============================
    public async Task<VentaDto?> ObtenerVenta(int id)
    {
        return await _db.Ventas
            .Where(v => v.IdVenta == id && v.Activo)
            .Select(v => new VentaDto
            {
                IdVenta = v.IdVenta,
                IdCliente = v.IdCliente,
                Fecha = v.Fecha,
                Subtotal = v.Subtotal,
                Impuesto = v.Impuesto,
                Total = v.Total,
                Detalles = v.Detalles.Select(d => new DetalleVentaDto
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Impuesto = d.Impuesto,
                    NombreProducto = d.Producto.Nombre
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    // ============================
    // CREAR VENTA
    // ============================
    public async Task<int> CrearVenta(CrearVentaDto dto)
    {
        if (dto.Detalles == null || !dto.Detalles.Any())
            throw new Exception("La venta debe tener al menos un producto.");

        if (dto.Detalles.Any(d => d.Cantidad <= 0))
            throw new Exception("Cantidad inválida.");

        using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            var cliente = await _db.Clientes
                .FirstOrDefaultAsync(c => c.IdCliente == dto.IdCliente && c.Activo);

            if (cliente == null)
                throw new Exception("Cliente inválido.");

            var ids = dto.Detalles.Select(d => d.IdProducto).Distinct().ToList();

            var productos = await _db.Productos
                .Where(p => ids.Contains(p.IdProducto) && p.Activo)
                .ToListAsync();

            if (productos.Count != ids.Count)
                throw new Exception("Productos inválidos.");

            var detalles = new List<DetalleVenta>();

            foreach (var item in dto.Detalles)
            {
                var producto = productos.First(p => p.IdProducto == item.IdProducto);

                if (producto.Stock < item.Cantidad)
                    throw new Exception($"Stock insuficiente: {producto.Nombre}");

                var precio = producto.Precio;
                var subtotal = precio * item.Cantidad;
                var impuesto = subtotal * (producto.Impuesto ?? 0) / 100;

                detalles.Add(new DetalleVenta
                {
                    IdProducto = producto.IdProducto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precio,
                    Subtotal = subtotal,
                    Impuesto = impuesto
                });

                producto.Stock -= item.Cantidad;
            }

            var subtotalVenta = detalles.Sum(d => d.Subtotal);
            var impuestoVenta = detalles.Sum(d => d.Impuesto);

            var venta = new Venta
            {
                IdCliente = dto.IdCliente,
                Fecha = DateTime.UtcNow,
                Subtotal = subtotalVenta,
                Impuesto = impuestoVenta,
                Total = subtotalVenta + impuestoVenta,
                Activo = true
            };

            _db.Ventas.Add(venta);
            await _db.SaveChangesAsync();

            foreach (var d in detalles)
                d.IdVenta = venta.IdVenta;

            _db.DetalleVentas.AddRange(detalles);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            return venta.IdVenta;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ============================
    // ACTUALIZAR VENTA
    // ============================
    public async Task<bool> ActualizarVenta(int id, ActualizarVentaDto dto)
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            var venta = await _db.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
                return false;

            // 🔹 Activar / desactivar
            if (dto.Activo.HasValue)
                venta.Activo = dto.Activo.Value;

            // 🔹 Si no hay detalles → solo estado
            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }

            // 🔹 Devolver stock anterior
            var productosActuales = await _db.Productos
                .Where(p => venta.Detalles.Select(d => d.IdProducto).Contains(p.IdProducto))
                .ToListAsync();

            foreach (var d in venta.Detalles)
            {
                var producto = productosActuales.First(p => p.IdProducto == d.IdProducto);
                producto.Stock += d.Cantidad;
            }

            _db.DetalleVentas.RemoveRange(venta.Detalles);

            var ids = dto.Detalles.Select(d => d.IdProducto).Distinct().ToList();

            var productos = await _db.Productos
                .Where(p => ids.Contains(p.IdProducto) && p.Activo)
                .ToListAsync();

            var nuevosDetalles = new List<DetalleVenta>();

            foreach (var item in dto.Detalles)
            {
                if (item.Cantidad <= 0)
                    throw new Exception("Cantidad inválida.");

                var producto = productos.First(p => p.IdProducto == item.IdProducto);

                if (producto.Stock < item.Cantidad)
                    throw new Exception($"Stock insuficiente: {producto.Nombre}");

                var precio = producto.Precio;
                var subtotal = precio * item.Cantidad;
                var impuesto = subtotal * (producto.Impuesto ?? 0) / 100;

                nuevosDetalles.Add(new DetalleVenta
                {
                    IdVenta = id,
                    IdProducto = producto.IdProducto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precio,
                    Subtotal = subtotal,
                    Impuesto = impuesto
                });

                producto.Stock -= item.Cantidad;
            }

            _db.DetalleVentas.AddRange(nuevosDetalles);

            var subtotalVenta = nuevosDetalles.Sum(d => d.Subtotal);
            var impuestoVenta = nuevosDetalles.Sum(d => d.Impuesto);

            venta.Subtotal = subtotalVenta;
            venta.Impuesto = impuestoVenta;
            venta.Total = subtotalVenta + impuestoVenta;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ============================
    // ACTIVAR / DESACTIVAR
    // ============================
    public async Task<bool> CambiarEstado(int id, bool activo)
    {
        var venta = await _db.Ventas
            .FirstOrDefaultAsync(v => v.IdVenta == id);

        if (venta == null)
            return false;

        venta.Activo = activo;

        await _db.SaveChangesAsync();

        return true;
    }

    // ============================
    // ELIMINAR (SOFT DELETE)
    // ============================
    public async Task<bool> EliminarVenta(int id)
    {
        return await CambiarEstado(id, false);
    }
}
