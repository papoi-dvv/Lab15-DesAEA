using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

public class LoginDto
{
    [Required, EmailAddress, StringLength(120)]
    public string Correo { get; set; } = null!;

    [Required, StringLength(100)]
    public string Clave { get; set; } = null!;
}

/// <summary>Registro: no incluye rol; el servidor asigna el rol por defecto.</summary>
public class RegistroDto
{
    [Required, EmailAddress, StringLength(120)]
    public string Correo { get; set; } = null!;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Clave { get; set; } = null!;
}

public class TokenDto
{
    public string Token { get; set; } = null!;
    public DateTime Expira { get; set; }
}

public class PerfilDto
{
    public string Correo { get; set; } = null!;
    public string Rol { get; set; } = null!;
}
