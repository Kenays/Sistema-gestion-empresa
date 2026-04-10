public class VentaDto
{
    public int IdVenta { get; set; }

    public int IdCliente { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }

    public List<DetalleVentaDto> Detalles { get; set; } = new();
}
