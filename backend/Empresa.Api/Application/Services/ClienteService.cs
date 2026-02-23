using Empresa.Api.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Empresa.Api.Domain;

namespace Empresa.Api.Application.Services;

public class ClienteService
{
  private readonly EmpresaDbContext _db;
  private readonly IConfiguration _config;

  public ClienteService(EmpresaDbContext db, IConfiguration config)
  {
    _db = db;
    _config = config;
  }

  public string GenerateToken(Cliente cliente)
  {
    var jwt = _config.GetSection('Jwt');
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
    
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


  }
}
