using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Authorize] // por defecto: solo usuarios autenticados
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly ICitaServicio _citas;

    public CitasController(ICitaServicio citas) => _citas = citas;

    // GET api/citas?estado=Pendiente&desde=2030-01-01&hasta=2030-12-31
    [HttpGet]
    public async Task<ActionResult<List<CitaDto>>> Listar(
        [FromQuery] string? estado, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        => Ok(await _citas.ListarAsync(estado, desde, hasta));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CitaDto>> Obtener(int id)
    {
        var cita = await _citas.ObtenerAsync(id);
        return cita is null ? NotFound() : Ok(cita);
    }

    [Authorize(Roles = Roles.Recepcionista)]
    [HttpPost]
    public async Task<ActionResult<CitaDto>> Crear(CitaCrearDto dto)
    {
        try
        {
            var creada = await _citas.CrearAsync(dto);
            if (creada is null) return NotFound(new { mensaje = $"La mascota {dto.MascotaId} no existe." });
            return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [Authorize(Roles = Roles.RecepcionistaOVeterinario)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, CitaActualizarDto dto)
    {
        try
        {
            return await _citas.ActualizarAsync(id, dto) ? NoContent() : NotFound();
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
            return await _citas.EliminarAsync(id) ? NoContent() : NotFound();
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
