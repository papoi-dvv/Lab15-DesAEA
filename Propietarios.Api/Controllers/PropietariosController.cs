using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propietarios.Api.Models;

namespace Propietarios.Api.Controllers;

[ApiController]
[Authorize] // todos los endpoints exigen un token válido
[Route("api/[controller]")]
public class PropietariosController : ControllerBase
{
    // Datos en memoria (sin base de datos): los dueños de las mascotas de ejemplo de la clínica
    private static readonly IReadOnlyList<Propietario> Datos =
    [
        new(1, "Carlos Ramos", "987654321", "carlos.ramos@example.com", "Ate"),
        new(2, "Ana Torres", "912345678", "ana.torres@example.com", "La Molina"),
        new(3, "Luis Paredes", "955111222", "luis.paredes@example.com", "Santa Anita"),
        new(4, "Marta Díaz", "944333222", "marta.diaz@example.com", "San Borja"),
        new(5, "Jorge Vega", "966777888", "jorge.vega@example.com", "Surco")
    ];

    // GET api/propietarios  -> 200, 401
    [HttpGet]
    public ActionResult<IReadOnlyList<Propietario>> Listar() => Ok(Datos);

    // GET api/propietarios/1  -> 200, 401, 404
    [HttpGet("{id:int}")]
    public ActionResult<Propietario> Obtener(int id)
    {
        var propietario = Datos.FirstOrDefault(p => p.Id == id);
        return propietario is null ? NotFound() : Ok(propietario);
    }
}
