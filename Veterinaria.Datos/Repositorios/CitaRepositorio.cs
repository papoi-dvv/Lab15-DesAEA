using Microsoft.EntityFrameworkCore;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Datos.Repositorios;

public class CitaRepositorio : ICitaRepositorio
{
    private readonly VeterinariaContext _context;
    public CitaRepositorio(VeterinariaContext context) => _context = context;

    public async Task<List<Cita>> ListarAsync(string? estado, DateTime? desde, DateTime? hasta)
    {
        var query = _context.Citas.Include(c => c.Mascota).AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(c => c.Estado == estado);
        if (desde.HasValue) query = query.Where(c => c.FechaHora >= desde.Value);
        if (hasta.HasValue) query = query.Where(c => c.FechaHora <= hasta.Value);
        return await query.OrderBy(c => c.FechaHora).ToListAsync();
    }

    public async Task<List<Cita>> ListarPorMascotaAsync(int mascotaId) =>
        await _context.Citas.Include(c => c.Mascota)
            .Where(c => c.MascotaId == mascotaId).OrderBy(c => c.FechaHora).ToListAsync();

    public async Task<Cita?> ObtenerAsync(int id) =>
        await _context.Citas.Include(c => c.Mascota).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<bool> ExisteEnHorarioAsync(int mascotaId, DateTime fechaHora, int? excluirId = null) =>
        await _context.Citas.AnyAsync(c => c.MascotaId == mascotaId && c.FechaHora == fechaHora
                                           && c.Estado != "Cancelada" && (excluirId == null || c.Id != excluirId));

    public async Task AgregarAsync(Cita cita) => await _context.Citas.AddAsync(cita);

    public Task EliminarAsync(Cita cita)
    {
        _context.Citas.Remove(cita);
        return Task.CompletedTask;
    }

    public async Task GuardarAsync() => await _context.SaveChangesAsync();
}
