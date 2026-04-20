using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Empresa.Api.Application.Dtos;
using Empresa.Api.Application.Security;
using Empresa.Api.Domain;
using Empresa.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Empresa.Api.Application.Services;

public class AuthService
{
    private readonly EmpresaDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(EmpresaDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private string GenerateToken(Usuario user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Nombre),
        };

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

    public async Task Register(RegisterRequest req)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == req.Email))
            throw new Exception("Email ya registrado.");

        var rol = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == req.Rol);

        if (rol == null)
            throw new Exception("El rol no existe.");

        var user = new Usuario
        {
            Nombre = req.Nombre,
            Email = req.Email,
            PasswordHash = PasswordHasher.Hash(req.Password),
            Activo = true,
            UsuarioRoles = new List<UsuarioRol>(), // 👈 importante
        };

        user.UsuarioRoles.Add(
            new UsuarioRol
            {
                Rol = rol, // 👈 no uses IdRol directamente
            }
        );

        _db.Usuarios.Add(user);

        await _db.SaveChangesAsync();
    }

    public async Task<LoginResponse> Login(LoginRequest req)
    {
        var user = await _db
            .Usuarios.Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Email == req.Email && u.Activo);

        if (user == null)
            throw new Exception("Credenciales invalidas.");

        if (!PasswordHasher.Verify(req.Password, user.PasswordHash))
            throw new Exception("Credenciales invalidas.");

        var token = GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            Nombre = user.Nombre,
            Rol = string.Join(", ", user.UsuarioRoles.Select(r => r.Rol.Nombre)),
        };
    }
}
