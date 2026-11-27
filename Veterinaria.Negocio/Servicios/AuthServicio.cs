using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Negocio.Servicios;

public class AuthServicio : IAuthServicio
{
    // Hash de relleno: el login hace el mismo trabajo de BCrypt aunque el correo no exista
    // (evita distinguir "correo inexistente" de "clave incorrecta" por el tiempo de respuesta).
    private static readonly string HashFalso = BCrypt.Net.BCrypt.HashPassword("clave-de-relleno");

    private readonly IUsuarioRepositorio _repo;
    private readonly JwtOpciones _jwt;

    public AuthServicio(IUsuarioRepositorio repo, IOptions<JwtOpciones> jwt)
    {
        _repo = repo;
        _jwt = jwt.Value;
    }

    public async Task<PerfilDto> RegistrarAsync(RegistroDto dto)
    {
        var correo = Normalizar(dto.Correo);

        // Regla de negocio: correo único
        if (await _repo.BuscarPorCorreoAsync(correo) is not null)
            throw new ReglaNegocioException($"El correo '{correo}' ya está registrado.");

        var usuario = new Usuario
        {
            Correo = correo,
            ClaveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave),
            Rol = Roles.Cliente // el rol lo decide el servidor, nunca el cliente
        };
        await _repo.AgregarAsync(usuario);
        await _repo.GuardarAsync();
        return new PerfilDto { Correo = usuario.Correo, Rol = usuario.Rol };
    }

    public async Task<TokenDto?> LoginAsync(LoginDto dto)
    {
        var usuario = await _repo.BuscarPorCorreoAsync(Normalizar(dto.Correo));
        var claveValida = BCrypt.Net.BCrypt.Verify(dto.Clave, usuario?.ClaveHash ?? HashFalso);
        if (usuario is null || !claveValida) return null;

        return GenerarToken(usuario);
    }

    private TokenDto GenerarToken(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_jwt.Minutos);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim("role", usuario.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return new TokenDto { Token = new JwtSecurityTokenHandler().WriteToken(token), Expira = expira };
    }

    private static string Normalizar(string correo) => correo.Trim().ToLowerInvariant();
}
