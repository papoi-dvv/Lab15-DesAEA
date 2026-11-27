using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public interface ICitaRepositorio
{
    Task<List<Cita>> ListarAsync(string? estado, DateTime? desde, DateTime? hasta);
    Task<List<Cita>> ListarPorMascotaAsync(int mascotaId);
    Task<Cita?> ObtenerAsync(int id);
    Task<bool> ExisteEnHorarioAsync(int mascotaId, DateTime fechaHora, int? excluirId = null);
    Task AgregarAsync(Cita cita);
    Task EliminarAsync(Cita cita);
    Task GuardarAsync();
}
