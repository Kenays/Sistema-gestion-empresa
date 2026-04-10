public class ActualizarProductoDto
{
    public string? Nombre { get; set; } = "";

    public string? Descripcion { get; set; }

    public decimal? Precio { get; set; }

    public decimal? Costo { get; set; }

    public int? Stock { get; set; }

    public decimal? Impuesto { get; set; }

    public bool? Activo { get; set; }
}
