namespace Empresa.Api.Domain;

public class UsuarioRol
{
    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;
}

