namespace Empresa.Api.Application.Dtos;

public class UsuarioDto
{
  public int IdUsuario {get; set; }
  public string Nombre {get; set; } = "";
  public string Email{get; set; } = "";
  public string Rol {get; set; } = "";
}
