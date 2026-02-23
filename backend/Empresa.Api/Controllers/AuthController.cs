using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Empresa.Api.Application.Dtos;
using Empresa.Api.Application.Security;
using Empresa.Api.Domain;
using Empresa.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Empresa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly EmpresaDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(EmpresaDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var user = await _db
            .Usuarios.Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Email == req.Email && u.Activo);

        if (user == null)
            return Unauthorized();

        if (!PasswordHasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized();

        var token = GenerateToken(user);

        return Ok(
            new LoginResponse
            {
                Token = token,
                Nombre = user.Nombre,
                Rol = string.Join(", ", user.UsuarioRoles.Select(ur => ur.Rol.Nombre)),
            }
        );
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest req)
    {
        
        if (await _db.Usuarios.AnyAsync(u => u.Email == req.Email))
            return BadRequest("Email ya registrado.");

        var user = new Usuario
        {
            Nombre = req.Nombre,
            Email = req.Email,
            PasswordHash = PasswordHasher.Hash(req.Password),
            Activo = true,
        };

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();

        // asignar rol operador por defecto
        var rol = await _db.Roles.FirstAsync(r => r.Nombre == req.Rol);

        _db.UsuarioRoles.Add(new UsuarioRol { IdUsuario = user.IdUsuario, IdRol = rol.IdRol });

        await _db.SaveChangesAsync();

        return Ok("Usuario creado.");
    }

    private string GenerateToken(Usuario user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ⚠️ IMPORTANTE: usar List, no array
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Nombre),
        };

        // Agregar TODOS los roles
        claims.AddRange(user.UsuarioRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Rol.Nombre)));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
