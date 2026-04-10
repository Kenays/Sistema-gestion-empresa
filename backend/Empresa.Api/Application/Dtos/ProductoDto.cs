namespace Empresa.Api.Application.Dtos;

public class ProductoDto()
{
    public int IdProducto { get; set; }
    public string Nombre {get; set;} = "";
    public string? Descripcion {get; set;}
    public decimal Precio {get; set;}
    public int Stock {get; set;}
    public string Codigo {get; set;} = "";
    public decimal? Impuesto {get; set;}
    public bool Activo {get; set;}
}
//quitamos Costo y Fecha porque esto es lo que vera el cliente y no necesita saber
