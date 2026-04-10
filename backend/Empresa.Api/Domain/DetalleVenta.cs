using Empresa.Api.Domain;
public class DetalleVenta
{
    public int IdDetalle { get; set; } // ✔ cambiar esto

    public int IdVenta { get; set; }
    public Venta Venta { get; set; } = null!;

    public int IdProducto { get; set; }
    public Producto Producto { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
}
