namespace Veterinaria.Entidades;

/// <summary>Roles propios de la clínica veterinaria. El rol lo asigna siempre el servidor.</summary>
public static class Roles
{
    public const string Cliente = "Cliente";               // dueño de mascota: registro público, solo consulta
    public const string Recepcionista = "Recepcionista";   // registra mascotas y agenda citas
    public const string Veterinario = "Veterinario";       // atiende citas y puede eliminar registros

    public const string RecepcionistaOVeterinario = Recepcionista + "," + Veterinario;
}
