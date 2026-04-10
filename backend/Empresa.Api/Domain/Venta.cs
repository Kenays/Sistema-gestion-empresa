public class Venta
{
    public int IdVenta { get; set; }
    public int IdCliente { get; set; }
    public DateTime Fecha { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }

    public bool Activo { get; set; }

    // ✅ AGREGAR ESTO
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}      
