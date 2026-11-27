using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Veterinaria.Datos;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Negocio;
using Veterinaria.Negocio.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configuración JWT (sección "Jwt"). La clave va en appsettings.Development.json / user-secrets / variable de entorno.
builder.Services.Configure<JwtOpciones>(builder.Configuration.GetSection(JwtOpciones.Seccion));
var jwt = builder.Configuration.GetSection(JwtOpciones.Seccion).Get<JwtOpciones>() ?? new JwtOpciones();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Falta la clave JWT (Jwt:Key, mínimo 32 caracteres). Configúrala en appsettings.Development.json, " +
        "con 'dotnet user-secrets set \"Jwt:Key\" \"<clave>\"' o con la variable de entorno Jwt__Key.");

// OpenAPI con esquema Bearer (botón Authorize en Swagger)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Pega el token obtenido en POST api/auth/login (sin la palabra 'Bearer')."
            }
        };
        return Task.CompletedTask;
    });

    // Candado solo en los endpoints que exigen autenticación
    options.AddOperationTransformer((operation, context, ct) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any())
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });
        }
        return Task.CompletedTask;
    });
});

// DbContext: SQL Server en el equipo local, PostgreSQL en la nube (Database:Provider)
var proveedor = builder.Configuration["Database:Provider"] ?? "SqlServer";
var usaPostgres = proveedor.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase);
var cadena = builder.Configuration.GetConnectionString("VeterinariaDB");
if (string.IsNullOrWhiteSpace(cadena))
    throw new InvalidOperationException(
        "Falta la cadena de conexión (ConnectionStrings:VeterinariaDB). Configúrala en appsettings.Development.json " +
        "o con la variable de entorno ConnectionStrings__VeterinariaDB.");

builder.Services.AddDbContext<VeterinariaContext>(options =>
{
    if (usaPostgres) options.UseNpgsql(CadenaPostgres.Normalizar(cadena));
    else options.UseSqlServer(cadena);
});

// Repositorios y servicios (por interfaz)
builder.Services.AddScoped<IMascotaRepositorio, MascotaRepositorio>();
builder.Services.AddScoped<ICitaRepositorio, CitaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IMascotaServicio, MascotaServicio>();
builder.Services.AddScoped<ICitaServicio, CitaServicio>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();

// AutoMapper: un único MappingProfile en la capa de Negocio
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Autenticación JWT: valida emisor, audiencia, vencimiento y firma
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // conserva los nombres sub, email, role
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "email",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Veterinaria API v1"));

app.UseHttpsRedirection();
app.UseDefaultFiles();   // wwwroot/index.html: panel de demostración
app.UseStaticFiles();

app.UseAuthentication(); // siempre antes de UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
