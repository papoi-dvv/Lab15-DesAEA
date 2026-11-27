namespace Propietarios.Api.Models;

/// <summary>Dueño de una mascota. Modelo propio de este servicio: no comparte clases con la API de la clínica.</summary>
public record Propietario(int Id, string Nombre, string Telefono, string Correo, string Distrito);
