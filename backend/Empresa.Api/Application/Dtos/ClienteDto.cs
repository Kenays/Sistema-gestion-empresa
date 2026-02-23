namespace Empresa.Api.Application.Dtos;

public class ClienteDto
{
  public int IdCliente{get; set;}
  public string Nombre{get; set;} = "";
  public string? Email{get; set;} = "";
}
