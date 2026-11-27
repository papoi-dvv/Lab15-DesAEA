using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public interface IMascotaRepositorio
{
    Task<List<Mascota>> ListarAsync(string? especie, string? nombre, bool soloActivas);
    Task<Mascota?> ObtenerAsync(int id);
    Task<bool> ExisteDuplicadoAsync(string nombre, string nombreDueno, int? excluirId = null);
    Task<bool> TieneCitasPendientesAsync(int id);
    Task AgregarAsync(Mascota mascota);
    Task EliminarAsync(Mascota mascota);
    Task GuardarAsync();
}
