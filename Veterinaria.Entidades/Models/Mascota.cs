namespace Veterinaria.Entidades.Models;

public partial class Mascota
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Especie { get; set; } = null!;
    public string? Raza { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string NombreDueno { get; set; } = null!;
    public string? Telefono { get; set; }
    public bool Activo { get; set; }

    public virtual ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
