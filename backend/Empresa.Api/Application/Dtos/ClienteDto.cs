namespace Empresa.Api.Application.Dtos;

public class ClienteDto
{
  public int IdCliente{get; set;}
  public string Nombre{get; set;} = "";
  public string? Email{get; set;} = "";
  public string? Telefono{get; set;}
  public bool Activo{get; set;} 
  public DateTime FechaRegistro{get; set;}
}
