using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Tabla de rutas: ocelot.json (equipo local) u ocelot.Production.json (nube).
// Las variables de entorno van al final para poder sobrescribir un valor sin editar el archivo,
// por ejemplo Routes__6__DownstreamHostAndPorts__0__Host.
var archivoOcelot = builder.Environment.IsProduction() ? "ocelot.Production.json" : "ocelot.json";
builder.Configuration
    .AddJsonFile(archivoOcelot, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Misma clave, emisor y audiencia que la API de la S14. La clave nunca se guarda en el repositorio.
var clave = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(clave) || clave.Length < 32)
    throw new InvalidOperationException(
        "Falta la clave JWT (Jwt:Key, mínimo 32 caracteres). Configúrala en appsettings.Development.json " +
        "o con la variable de entorno Jwt__Key.");

// El esquema "Bearer" es el que usan las rutas protegidas de ocelot.json (AuthenticationOptions)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "email",
            RoleClaimType = "role"
        };
    });

builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();

app.Run();
