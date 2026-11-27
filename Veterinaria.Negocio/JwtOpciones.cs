namespace Veterinaria.Negocio;

/// <summary>Sección "Jwt" de la configuración. La clave nunca se guarda en el repositorio.</summary>
public class JwtOpciones
{
    public const string Seccion = "Jwt";

    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int Minutos { get; set; } = 60;
}
