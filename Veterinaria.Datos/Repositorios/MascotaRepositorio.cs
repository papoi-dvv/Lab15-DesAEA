using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public class MascotaRepositorio : IMascotaRepositorio
{
    private readonly VeterinariaContext _context;
    public MascotaRepositorio(VeterinariaContext context) => _context = context;

    public async Task<List<Mascota>> ListarAsync(string? especie, string? nombre, bool soloActivas)
    {
        var query = _context.Mascotas.Include(m => m.Citas).AsQueryable();
        if (!string.IsNullOrWhiteSpace(especie)) query = query.Where(m => m.Especie == especie);
        if (!string.IsNullOrWhiteSpace(nombre)) query = query.Where(m => m.Nombre.Contains(nombre));
        if (soloActivas) query = query.Where(m => m.Activo);
        return await query.OrderBy(m => m.Nombre).ToListAsync();
    }

    public async Task<Mascota?> ObtenerAsync(int id) =>
        await _context.Mascotas.Include(m => m.Citas).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<bool> ExisteDuplicadoAsync(string nombre, string nombreDueno, int? excluirId = null) =>
        await _context.Mascotas.AnyAsync(m => m.Nombre == nombre && m.NombreDueno == nombreDueno
                                              && (excluirId == null || m.Id != excluirId));

    public async Task<bool> TieneCitasPendientesAsync(int id) =>
        await _context.Citas.AnyAsync(c => c.MascotaId == id && c.Estado == "Pendiente");

    public async Task AgregarAsync(Mascota mascota) => await _context.Mascotas.AddAsync(mascota);

    public Task EliminarAsync(Mascota mascota)
    {
        _context.Mascotas.Remove(mascota);
        return Task.CompletedTask;
    }

    public async Task GuardarAsync() => await _context.SaveChangesAsync();
}
