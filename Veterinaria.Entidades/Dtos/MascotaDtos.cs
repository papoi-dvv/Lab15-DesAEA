using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

public class MascotaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string NombreDueno { get; set; } = null!;
    public string? Telefono { get; set; }
    public bool Activo { get; set; }
    public int TotalCitas { get; set; }
}

public class MascotaCrearDto
{
    [Required, StringLength(60, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;

    [Required, StringLength(30)]
    public string Especie { get; set; } = null!;

    [StringLength(60)]
    public string? Raza { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [Required, StringLength(100, MinimumLength = 3)]
    public string NombreDueno { get; set; } = null!;

    [StringLength(20)]
    public string? Telefono { get; set; }
}

public class MascotaActualizarDto
{
    [Required, StringLength(60, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;

    [Required, StringLength(30)]
    public string Especie { get; set; } = null!;

    [StringLength(60)]
    public string? Raza { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [Required, StringLength(100, MinimumLength = 3)]
    public string NombreDueno { get; set; } = null!;

    [StringLength(20)]
    public string? Telefono { get; set; }

    public bool Activo { get; set; } = true;
}
