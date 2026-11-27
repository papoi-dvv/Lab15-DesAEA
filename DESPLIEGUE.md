# Despliegue en la nube (pasos manuales)

Todo el código y la configuración están preparados. Falta crear las cuentas, subir el repositorio y publicar.

## 1. Proveedores elegidos

| Pieza | Proveedor | Plan |
|---|---|---|
| Gateway, API y microservicio 2 | **Render** (contenedores Docker) | Free |
| Base de datos | **Neon** (PostgreSQL) | Free |

**Por qué Render.** Admite ASP.NET Core 10 con Docker, publica los tres servicios desde un solo repositorio con `render.yaml`, entrega HTTPS y genera la clave JWT compartida sin escribirla en ningún archivo. Límites: el servicio se duerme tras 15 minutos sin tráfico, tarda cerca de un minuto en despertar y las 750 horas gratuitas al mes se comparten entre todos los servicios del espacio de trabajo ([Render](https://render.com/articles/platforms-with-a-real-free-tier-for-developers-in-2026), [anuncio del plan gratuito](https://render.com/blog/free-tier)).

**Por qué Neon.** Plan gratuito permanente con 1 GB de almacenamiento y 100 CU-horas por proyecto; la base se suspende tras 5 minutos sin uso y vuelve sola con la siguiente conexión ([límites de Neon](https://neon.com/faqs/free-plan-limits-and-quotas)).

**Alternativas descartadas.**

| Alternativa | Motivo |
|---|---|
| Supabase (PostgreSQL) | Pausa los proyectos gratuitos tras una semana sin actividad y hay que reactivarlos a mano ([precios](https://supabase.com/pricing)). |
| PostgreSQL de Render | La base gratuita caduca a los 30 días según [reseñas de terceros](https://toolfreebie.com/render-hosting-review/). |
| MonsterASP.NET (permite SQL Server) | El plan gratuito ofrece un solo sitio web y aquí se necesitan tres ([planes](https://www.monsterasp.net/ASP.NET-Freehosting/)). |
| MyASP.NET | Gratis solo 60 días ([página](https://www.myasp.net/freeaspnethosting)). |

Los límites se consultaron en noviembre de 2026; revíselos al crear las cuentas.

## 2. Cambio de SQL Server a PostgreSQL (para las conclusiones)

- Se agregó `Npgsql.EntityFrameworkCore.PostgreSQL`; `Database__Provider=PostgreSQL` activa `UseNpgsql`. En local sigue SQL Server.
- `script.postgres.sql` adapta el script: `IDENTITY` estándar, `BOOLEAN`, `NUMERIC`, `TIMESTAMP` y nombres entre comillas.
- PostgreSQL distingue mayúsculas al comparar; las columnas de texto que se buscan o comparan usan `citext` para conservar el comportamiento de la S14.
- Npgsql no acepta fechas marcadas como UTC en columnas sin zona horaria; el contexto quita esa marca en `FechaHora`.
- `CadenaPostgres` convierte la URI `postgresql://...` de Neon al formato de Npgsql.

## 3. Pasos

### 3.1 Base de datos (hacerlo primero)
1. En <https://neon.tech> cree un proyecto en la región **AWS US East (N. Virginia)**.
2. Abra **SQL Editor**, pegue todo `script.postgres.sql` y ejecútelo.
3. En **Connect**, copie la cadena de conexión (`postgresql://...?sslmode=require...`). No la guarde en ningún archivo del proyecto.

### 3.2 Repositorio
Cree en GitHub un repositorio vacío y, desde la carpeta `Lab15`:

```powershell
git init -b main
```
```powershell
git add .
```
```powershell
git status
```

En la lista **no** deben aparecer archivos `appsettings.Development.json` (solo los `.example.json`).

```powershell
git commit -m "S15: API Gateway con Ocelot y dos microservicios"
```
```powershell
git remote add origin https://github.com/<usuario>/<repositorio>.git
```
```powershell
git push -u origin main
```

### 3.3 Render
1. En <https://render.com>: **New → Blueprint** y conecte el repositorio.
2. Render lee `render.yaml`. Cuando pida `ConnectionStrings__VeterinariaDB`, pegue la cadena de Neon.
3. Aplique y espere a que los tres servicios queden en **Live**. `Jwt__Key` se genera sola y es la misma en los tres.

### 3.4 Ajustar las direcciones públicas
Compare la URL de cada servicio en Render con las previstas:

- `https://vetlab15-gateway.onrender.com`
- `https://vetlab15-api.onrender.com`
- `https://vetlab15-propietarios.onrender.com`

Si alguna es distinta (Render agrega un sufijo cuando el nombre está ocupado), edite `GatewayApi/ocelot.Production.json` reemplazando **todas** las apariciones: `Host`, `Location` y `BaseUrl`. Haga commit y push; Render vuelve a publicar. Actualice también la tabla del README.

### 3.5 Comprobar
1. Abra las tres URL en el navegador para despertarlas. El gateway y la raíz del microservicio 2 no tienen página de inicio, así que un 404 o 401 ahí es normal. La API muestra su panel y `/swagger`.
2. Ejecute las pruebas:

```powershell
.\pruebas.ps1 -Gateway https://vetlab15-gateway.onrender.com -Propietarios https://vetlab15-propietarios.onrender.com
```

Si la primera ejecución falla por el arranque en frío, espere un minuto y repita.

### 3.6 El 502 en la nube
Suspender el servicio en Render devuelve **503** (página de servicio suspendido), no 502 ([documentación](https://render.com/docs/troubleshooting-deploys)). Para provocar el 502 que pide la guía, haga que el gateway no pueda conectarse:

1. En el servicio **gateway → Environment**, agregue `Routes__6__DownstreamHostAndPorts__0__Host` = `no-existe.invalid` y guarde (vuelve a desplegar).
2. Ejecute:

```powershell
.\pruebas.ps1 -Gateway https://vetlab15-gateway.onrender.com -Solo502
```

3. Tome la captura y **borre la variable**.

El índice `6` es la ruta de propietarios, la séptima de `ocelot.Production.json`.

## 4. Variables de entorno

| Servicio | Variable | Valor |
|---|---|---|
| Los tres | `Jwt__Key` | Generada por Render (grupo `vetlab15-jwt`) |
| API | `ConnectionStrings__VeterinariaDB` | Cadena de Neon (se pega en el panel) |
| API | `Database__Provider` | `PostgreSQL` |
| API | `TZ` | `America/Lima` |
| API | `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` |

## 5. Qué está comprobado y qué no

- Comprobado en local: los tres proyectos publicados en modo `Production`, con `ocelot.Production.json` (redirigido a localhost mediante variables de entorno) y la API sobre un PostgreSQL 18 temporal creado con `script.postgres.sql`.
- Sin comprobar: la construcción de las imágenes Docker, el Blueprint en Render, la conexión a Neon y la comunicación entre servicios por HTTPS público.
