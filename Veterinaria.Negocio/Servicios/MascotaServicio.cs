using AutoMapper;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Negocio.Servicios;

public class MascotaServicio : IMascotaServicio
{
    private readonly IMascotaRepositorio _repo;
    private readonly IMapper _mapper;

    public MascotaServicio(IMascotaRepositorio repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<MascotaDto>> ListarAsync(string? especie, string? nombre, bool soloActivas)
    {
        var mascotas = await _repo.ListarAsync(especie, nombre, soloActivas);
        return _mapper.Map<List<MascotaDto>>(mascotas);
    }

    public async Task<MascotaDto?> ObtenerAsync(int id)
    {
        var mascota = await _repo.ObtenerAsync(id);
        return mascota is null ? null : _mapper.Map<MascotaDto>(mascota);
    }

    public async Task<MascotaDto> CrearAsync(MascotaCrearDto dto)
    {
        // Regla de negocio 1: no se duplica la misma mascota del mismo dueño
        if (await _repo.ExisteDuplicadoAsync(dto.Nombre, dto.NombreDueno))
            throw new ReglaNegocioException($"Ya existe una mascota '{dto.Nombre}' registrada para {dto.NombreDueno}.");

        var mascota = _mapper.Map<Mascota>(dto);
        mascota.Activo = true;
        await _repo.AgregarAsync(mascota);
        await _repo.GuardarAsync();
        return _mapper.Map<MascotaDto>(mascota);
    }

    public async Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto)
    {
        var mascota = await _repo.ObtenerAsync(id);
        if (mascota is null) return false;

        if (await _repo.ExisteDuplicadoAsync(dto.Nombre, dto.NombreDueno, id))
            throw new ReglaNegocioException($"Ya existe otra mascota '{dto.Nombre}' registrada para {dto.NombreDueno}.");

        _mapper.Map(dto, mascota); // actualiza el objeto existente
        await _repo.GuardarAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var mascota = await _repo.ObtenerAsync(id);
        if (mascota is null) return false;

        // Regla de negocio 2: no se elimina una mascota con citas pendientes
        if (await _repo.TieneCitasPendientesAsync(id))
            throw new ReglaNegocioException("No se puede eliminar la mascota: tiene citas pendientes.");

        await _repo.EliminarAsync(mascota);
        await _repo.GuardarAsync();
        return true;
    }
}
