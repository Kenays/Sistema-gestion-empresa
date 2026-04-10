public class ActualizarVentaDto
{
    public bool? Activo { get; set; }

    public List<ActualizarDetalleVentaDto>? Detalles { get; set; }
}
