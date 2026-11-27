# Veterinaria · API Gateway con Ocelot (Semana 15)

Dos microservicios detrás de un único punto de entrada. Laboratorio S15 – Desarrollo de Aplicaciones Empresariales Avanzadas (Tecsup).

## 1. Tema

| Proyecto | Puerto | Qué hace | Datos |
|---|---|---|---|
| `GatewayApi` | **5000** | Único punto de entrada (Ocelot 25). Valida el JWT en las rutas protegidas y reenvía. | — |
| `Veterinaria.Api` (microservicio 1) | 5206 | API en capas de la S14: registro y login con JWT, mascotas y citas con roles (Cliente, Recepcionista, Veterinario). | SQL Server local / PostgreSQL en la nube |
| `Propietarios.Api` (microservicio 2) | 5207 | Directorio de dueños de las mascotas: lista y detalle por id. Exige token en todos sus endpoints. | En memoria |

```
Cliente ──► GatewayApi :5000 ──┬──► Veterinaria.Api  :5206 ──► Base de datos
                               └──► Propietarios.Api :5207 (memoria)
```

Los microservicios no comparten base de datos ni clases. Solo comparten la clave, el emisor y la audiencia del JWT.

La lógica de la API de la S14 no cambió. Respecto a la S14 se agregó únicamente: el proveedor de base de datos configurable (`Database:Provider`) para poder usar PostgreSQL en la nube, y la cadena de conexión salió del repositorio.

## 2. Tabla de rutas

| Upstream (cliente → gateway) | Método | Downstream local (gateway → servicio) | Downstream en la nube | Token en el gateway |
|---|---|---|---|---|
| `/gateway/auth/{everything}` | POST | `localhost:5206/api/auth/{everything}` | `vetlab15-api.onrender.com:443` | No |
| `/gateway/auth/perfil` | GET | `localhost:5206/api/auth/perfil` | `vetlab15-api.onrender.com:443` | Sí |
| `/gateway/mascotas{everything}` | GET | `localhost:5206/api/mascotas{everything}` | `vetlab15-api.onrender.com:443` | No ¹ |
| `/gateway/mascotas{everything}` | POST, PUT, DELETE | `localhost:5206/api/mascotas{everything}` | `vetlab15-api.onrender.com:443` | Sí |
| `/gateway/citas{everything}` | GET | `localhost:5206/api/citas{everything}` | `vetlab15-api.onrender.com:443` | No ¹ |
| `/gateway/citas{everything}` | POST, PUT, DELETE | `localhost:5206/api/citas{everything}` | `vetlab15-api.onrender.com:443` | Sí |
| `/gateway/propietarios{everything}` | GET | `localhost:5207/api/propietarios{everything}` | `vetlab15-propietarios.onrender.com:443` | Sí |

¹ El gateway deja pasar los GET, pero la API conserva sus reglas de la S14: solo `GET /api/mascotas` es anónimo; el detalle y las citas responden 401 sin token. El rol (403) también lo exige la API.

Las direcciones de la nube son las previstas en `render.yaml`; confírmelas después de publicar (ver [DESPLIEGUE.md](DESPLIEGUE.md)).

En las rutas de escritura, `DownstreamHeaderTransform` reescribe la cabecera `Location` de los 201 para que el cliente siga viendo solo la dirección del gateway.

## 3. Puesta en marcha local

Requisitos: .NET 10 SDK y SQL Server (Express).

1. **Base de datos** (si aún no existe `VeterinariaDB` de la S14). El script borra y recrea las tablas:
   ```
   sqlcmd -S localhost\SQLEXPRESS -E -C -f 65001 -i script.sql
   ```
2. **Secretos** (no van al repositorio). En cada uno de los tres proyectos copie `appsettings.Development.example.json` como `appsettings.Development.json` y ponga **la misma** clave JWT de 32 caracteres o más. La API además necesita ahí la cadena de conexión.
3. **Ejecutar los tres**:
   - Visual Studio: abra `Veterinaria.slnx` y elija el perfil de inicio **Gateway y microservicios** (o clic derecho en la solución → Configurar proyectos de inicio → Varios proyectos).
   - VS Code / terminal, cada uno en su propia terminal:
     ```
     dotnet run --project Veterinaria.Api
     dotnet run --project Propietarios.Api
     dotnet run --project GatewayApi
     ```

**Swagger** está solo en la API: <http://localhost:5206/swagger> (pegue el token en **Authorize**, sin la palabra `Bearer`). En <http://localhost:5206> sigue el panel de demostración de la S14. Ambos llaman a la API directamente, sin pasar por el gateway; para las pruebas del laboratorio use el puerto 5000.

## 4. Pruebas (todo por el gateway)

- `.\pruebas.ps1` ejecuta los casos 200, 201, 204, 401, 403, 404 y 409, más la llamada directa sin token al microservicio 2. Crea y borra sus propios datos; el usuario del caso de registro queda en la tabla `Usuario`.
- Para el **502**: detenga `Propietarios.Api` y ejecute `.\pruebas.ps1 -Solo502`.
- `requests.http` tiene las peticiones principales para enviarlas una por una y tomar capturas. Antes de enviar el `PUT` y los `DELETE`, ponga en `@mascotaId` el id que devolvió el `POST`; si no, modificará otra mascota.

Si PowerShell bloquea el script: `powershell -ExecutionPolicy Bypass -File .\pruebas.ps1`.

Usuarios de prueba (clave `Clave123*`): `cliente@vet.com`, `recepcion@vet.com`, `veterinario@vet.com`.

## 5. Dónde se valida el token

| Lugar | Qué valida |
|---|---|
| Gateway (`AuthenticationProviderKeys: ["Bearer"]`) | Firma, emisor, audiencia y vencimiento en las rutas protegidas. Sin token válido responde 401 y no reenvía. |
| `Veterinaria.Api` | Lo mismo, y además el rol (`[Authorize(Roles = ...)]` → 403). |
| `Propietarios.Api` | Lo mismo, en todos sus endpoints, aunque se le llame sin pasar por el gateway. |

## 6. Archivos

| Archivo | Para qué |
|---|---|
| `GatewayApi/ocelot.json` | Rutas locales |
| `GatewayApi/ocelot.Production.json` | Rutas publicadas (se carga con `ASPNETCORE_ENVIRONMENT=Production`) |
| `script.sql` / `script.postgres.sql` | Base de datos en SQL Server / PostgreSQL |
| `pruebas.ps1`, `requests.http` | Pruebas por el gateway |
| `Veterinaria.slnLaunch` | Perfil de Visual Studio que inicia los tres proyectos |
| `render.yaml`, `*/Dockerfile`, `.dockerignore` | Despliegue en la nube |
| `DESPLIEGUE.md` | Elección de proveedores y pasos para publicar |

## Autor
Arévalo Sermeño, Edwin William – Sección C, Tecsup.
