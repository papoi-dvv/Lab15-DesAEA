using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Authorize] // por defecto: solo usuarios autenticados
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaServicio _mascotas;
    private readonly ICitaServicio _citas;

    public MascotasController(IMascotaServicio mascotas, ICitaServicio citas)
    {
        _mascotas = mascotas;
        _citas = citas;
    }

    // GET api/mascotas?especie=Perro&nombre=fi&soloActivas=true
    [AllowAnonymous] // público
    [HttpGet]
    public async Task<ActionResult<List<MascotaDto>>> Listar(
        [FromQuery] string? especie, [FromQuery] string? nombre, [FromQuery] bool soloActivas = false)
        => Ok(await _mascotas.ListarAsync(especie, nombre, soloActivas));

    // GET api/mascotas/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MascotaDto>> Obtener(int id)
    {
        var mascota = await _mascotas.ObtenerAsync(id);
        return mascota is null ? NotFound() : Ok(mascota);
    }

    // GET api/mascotas/1/citas  (ruta anidada)
    [HttpGet("{id:int}/citas")]
    public async Task<ActionResult<List<CitaDto>>> ListarCitas(int id)
    {
        var citas = await _citas.ListarPorMascotaAsync(id);
        return citas is null ? NotFound() : Ok(citas);
    }

    [Authorize(Roles = Roles.Recepcionista)]
    [HttpPost]
    public async Task<ActionResult<MascotaDto>> Crear(MascotaCrearDto dto)
    {
        try
        {
            var creada = await _mascotas.CrearAsync(dto);
            return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [Authorize(Roles = Roles.Recepcionista)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, MascotaActualizarDto dto)
    {
        try
        {
            return await _mascotas.ActualizarAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [Authorize(Roles = Roles.Veterinario)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return await _mascotas.EliminarAsync(id) ? NoContent() : NotFound();
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
