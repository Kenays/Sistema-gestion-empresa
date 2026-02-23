using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Empresa.Api.Infrastructure;
using Empresa.Api.Application.Dtos;
using Microsoft.AspNetCore.Authorization;

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
      .Set<UsuarioDto>()
      .FromSqlRaw("SELECT IdUsuario, Nombre, EMail, Rol FROM vw_Usuarios")
      .ToListAsync();

    return Ok(usuarios);
  }
}
