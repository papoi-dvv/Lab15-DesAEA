namespace Veterinaria.Entidades;

/// <summary>Se lanza cuando se incumple una regla de negocio (el controlador responde 409 Conflict).</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}
