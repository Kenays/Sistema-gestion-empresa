namespace Empresa.Api.Domain;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool Activo { get; set; }
}

