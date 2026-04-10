namespace Empresa.Api.Domain;

public class Producto
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = "";

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public decimal Costo { get; set; }

    public int Stock { get; set; }

    public string Codigo { get; set; } = "";

    public decimal? Impuesto { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}
