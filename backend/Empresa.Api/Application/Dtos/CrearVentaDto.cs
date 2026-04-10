public class CrearVentaDto
{
    public int IdCliente { get; set; }
    public List<CrearDetalleVentaDto> Detalles { get; set; } = new();
}
