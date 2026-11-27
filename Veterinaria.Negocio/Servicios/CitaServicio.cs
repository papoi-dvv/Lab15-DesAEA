using AutoMapper;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Negocio.Servicios;

public class CitaServicio : ICitaServicio
{
    private readonly ICitaRepositorio _repo;
    private readonly IMascotaRepositorio _mascotas;
    private readonly IMapper _mapper;

    public CitaServicio(ICitaRepositorio repo, IMascotaRepositorio mascotas, IMapper mapper)
    {
        _repo = repo;
        _mascotas = mascotas;
        _mapper = mapper;
    }

    public async Task<List<CitaDto>> ListarAsync(string? estado, DateTime? desde, DateTime? hasta)
    {
        var citas = await _repo.ListarAsync(estado, desde, hasta);
        return _mapper.Map<List<CitaDto>>(citas);
    }

    public async Task<List<CitaDto>?> ListarPorMascotaAsync(int mascotaId)
    {
        if (await _mascotas.ObtenerAsync(mascotaId) is null) return null;
        var citas = await _repo.ListarPorMascotaAsync(mascotaId);
        return _mapper.Map<List<CitaDto>>(citas);
    }

    public async Task<CitaDto?> ObtenerAsync(int id)
    {
        var cita = await _repo.ObtenerAsync(id);
        return cita is null ? null : _mapper.Map<CitaDto>(cita);
    }

    public async Task<CitaDto?> CrearAsync(CitaCrearDto dto)
    {
        var mascota = await _mascotas.ObtenerAsync(dto.MascotaId);
        if (mascota is null) return null;

        // Regla de negocio: la mascota debe estar activa
        if (!mascota.Activo)
            throw new ReglaNegocioException("No se pueden agendar citas para una mascota inactiva.");

        // Regla de negocio: no se agenda en el pasado
        if (dto.FechaHora!.Value <= DateTime.Now)
            throw new ReglaNegocioException("La fecha de la cita debe ser futura.");

        // Regla de negocio: la mascota no puede tener dos citas a la misma hora
        if (await _repo.ExisteEnHorarioAsync(dto.MascotaId, dto.FechaHora.Value))
            throw new ReglaNegocioException("La mascota ya tiene una cita en esa fecha y hora.");

        var cita = _mapper.Map<Cita>(dto);
        await _repo.AgregarAsync(cita);
        await _repo.GuardarAsync();

        var creada = await _repo.ObtenerAsync(cita.Id); // recarga con la mascota
        return _mapper.Map<CitaDto>(creada);
    }

    public async Task<bool> ActualizarAsync(int id, CitaActualizarDto dto)
    {
        var cita = await _repo.ObtenerAsync(id);
        if (cita is null) return false;

        // Regla de negocio: una cita atendida no se modifica
        if (cita.Estado == "Atendida")
            throw new ReglaNegocioException("No se puede modificar una cita que ya fue atendida.");

        if (dto.Estado != "Cancelada" && await _repo.ExisteEnHorarioAsync(cita.MascotaId, dto.FechaHora!.Value, id))
            throw new ReglaNegocioException("La mascota ya tiene una cita en esa fecha y hora.");

        _mapper.Map(dto, cita); // actualiza el objeto existente
        await _repo.GuardarAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var cita = await _repo.ObtenerAsync(id);
        if (cita is null) return false;

        if (cita.Estado == "Atendida")
            throw new ReglaNegocioException("No se puede eliminar una cita que ya fue atendida.");

        await _repo.EliminarAsync(cita);
        await _repo.GuardarAsync();
        return true;
    }
}
