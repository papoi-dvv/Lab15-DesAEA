using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Negocio.Servicios;

public interface ICitaServicio
{
    Task<List<CitaDto>> ListarAsync(string? estado, DateTime? desde, DateTime? hasta);
    Task<List<CitaDto>?> ListarPorMascotaAsync(int mascotaId);
    Task<CitaDto?> ObtenerAsync(int id);
    /// <summary>Devuelve null si la mascota no existe.</summary>
    Task<CitaDto?> CrearAsync(CitaCrearDto dto);
    Task<bool> ActualizarAsync(int id, CitaActualizarDto dto);
    Task<bool> EliminarAsync(int id);
}
