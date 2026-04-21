using Empresa.Api.Application.Dtos;
using Empresa.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Empresa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly EmpresaDbContext _db;

    public UsuariosController(EmpresaDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> Get()
    {
        var usuarios = await _db
            .Usuarios.SelectMany(u =>
                u.UsuarioRoles.Select(ur => new UsuarioDto
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = ur.Rol.Nombre,
                })
            )
            .ToListAsync();

        return Ok(usuarios);
    }
}
