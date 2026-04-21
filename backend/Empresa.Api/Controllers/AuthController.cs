using Empresa.Api.Application.Dtos;
using Empresa.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Empresa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var result = await _auth.Login(req);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest req)
    {
        try
        {
            await _auth.Register(req);
            return Ok("Usuario creado");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
