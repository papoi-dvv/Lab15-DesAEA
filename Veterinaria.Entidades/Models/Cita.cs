namespace Veterinaria.Entidades.Models;

public partial class Cita
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public DateTime FechaHora { get; set; }
    public string Motivo { get; set; } = null!;
    public decimal Costo { get; set; }
    public string Estado { get; set; } = null!;

    public virtual Mascota Mascota { get; set; } = null!;
}
