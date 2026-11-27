using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Negocio.Servicios;

public interface IAuthServicio
{
    /// <summary>Registra un usuario con rol Cliente. Lanza ReglaNegocioException si el correo ya existe.</summary>
    Task<PerfilDto> RegistrarAsync(RegistroDto dto);

    /// <summary>Devuelve el token, o null si las credenciales no son válidas.</summary>
    Task<TokenDto?> LoginAsync(LoginDto dto);
}
