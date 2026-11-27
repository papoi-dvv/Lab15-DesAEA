using AutoMapper;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Entidades.Models;

namespace Veterinaria.Negocio;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entidad -> DTO de lectura (lo que la API devuelve)
        CreateMap<Mascota, MascotaDto>()
            .ForMember(d => d.TotalCitas, o => o.MapFrom(s => s.Citas.Count));

        // DTO de entrada -> Entidad (crear y, con Map(origen, destino), actualizar)
        CreateMap<MascotaCrearDto, Mascota>();
        CreateMap<MascotaActualizarDto, Mascota>();

        CreateMap<Cita, CitaDto>()
            .ForMember(d => d.MascotaNombre, o => o.MapFrom(s => s.Mascota.Nombre));

        CreateMap<CitaCrearDto, Cita>()
            .ForMember(d => d.Estado, o => o.MapFrom(_ => "Pendiente"));
        CreateMap<CitaActualizarDto, Cita>();
    }
}
