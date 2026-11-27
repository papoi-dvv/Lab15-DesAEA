using Npgsql;

namespace Veterinaria.Datos;

/// <summary>
/// Neon, Supabase y Render entregan la conexión como URI (postgresql://usuario:clave@host/base),
/// pero Npgsql espera el formato "Host=...;Username=...". Se aceptan ambos.
/// </summary>
public static class CadenaPostgres
{
    public static string Normalizar(string cadena)
    {
        cadena = cadena.Trim();
        if (!cadena.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !cadena.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return cadena;

        var uri = new Uri(cadena);
        var credenciales = uri.UserInfo.Split(':', 2);
        var resultado = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credenciales[0]),
            Password = credenciales.Length > 1 ? Uri.UnescapeDataString(credenciales[1]) : null,
            // En la nube la conexión va cifrada salvo que la URI indique otra cosa
            SslMode = uri.IsLoopback ? SslMode.Prefer : SslMode.Require
        };

        foreach (var parametro in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var partes = parametro.Split('=', 2);
            if (partes.Length == 2 && partes[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse<SslMode>(partes[1].Replace("-", ""), ignoreCase: true, out var modo))
                resultado.SslMode = modo;
        }

        return resultado.ConnectionString;
    }
}
