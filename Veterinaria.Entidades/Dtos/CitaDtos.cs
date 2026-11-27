using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

public class CitaDto
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public string MascotaNombre { get; set; } = null!;
    public DateTime FechaHora { get; set; }
    public string Motivo { get; set; } = null!;
    public decimal Costo { get; set; }
    public string Estado { get; set; } = null!;
}

public class CitaCrearDto
{
    [Range(1, int.MaxValue)]
    public int MascotaId { get; set; }

    [Required]
    public DateTime? FechaHora { get; set; }

    [Required, StringLength(200, MinimumLength = 3)]
    public string Motivo { get; set; } = null!;

    [Range(0, 100000)]
    public decimal Costo { get; set; }
}

public class CitaActualizarDto
{
    [Required]
    public DateTime? FechaHora { get; set; }

    [Required, StringLength(200, MinimumLength = 3)]
    public string Motivo { get; set; } = null!;

    [Range(0, 100000)]
    public decimal Costo { get; set; }

    [Required, RegularExpression("^(Pendiente|Atendida|Cancelada)$", ErrorMessage = "Estado debe ser Pendiente, Atendida o Cancelada.")]
    public string Estado { get; set; } = null!;
}
