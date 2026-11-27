using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _auth;

    public AuthController(IAuthServicio auth) => _auth = auth;

    // POST api/auth/registro  -> 201, 400, 409
    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<ActionResult<PerfilDto>> Registro(RegistroDto dto)
    {
        try
        {
            var perfil = await _auth.RegistrarAsync(dto);
            return StatusCode(StatusCodes.Status201Created, perfil);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    // POST api/auth/login  -> 200, 400, 401 (mismo mensaje si el correo no existe o la clave es incorrecta)
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
    {
        var token = await _auth.LoginAsync(dto);
        return token is null ? Unauthorized(new { mensaje = "Credenciales inválidas." }) : Ok(token);
    }

    // GET api/auth/perfil  -> 200, 401. Datos tomados de los claims del token, no de la base de datos.
    [Authorize]
    [HttpGet("perfil")]
    public ActionResult<PerfilDto> Perfil() => Ok(new PerfilDto
    {
        Correo = User.FindFirst("email")?.Value ?? "",
        Rol = User.FindFirst("role")?.Value ?? ""
    });
}
