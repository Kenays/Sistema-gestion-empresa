namespace Empresa.Api.Application.Dtos;

public class CrearProductoDto
{
    public string Nombre { get; set; } = "";

    public string? Descripcion { get; set; }  // opcional ✔

    public decimal Precio { get; set; }

    public decimal Costo { get; set; }

    public int Stock { get; set; }

    public string Codigo { get; set; } = "";

    public decimal Impuesto { get; set; }

  // ❌ no incluir Activo
}
