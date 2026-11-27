# Pruebas de extremo a extremo por el gateway (Windows PowerShell 5.1 o superior).
#   Local:  .\pruebas.ps1
#   Nube:   .\pruebas.ps1 -Gateway https://<gateway> -Propietarios https://<servicio-propietarios>
#   502:    detener Propietarios.Api y ejecutar  .\pruebas.ps1 -Solo502
param(
    [string]$Gateway = "http://localhost:5000",
    [string]$Propietarios = "http://localhost:5207",
    [switch]$Solo502
)

$ErrorActionPreference = "Stop"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$script:fallos = 0

function Invoke-Prueba {
    param([string]$Nombre, [int]$Esperado, [string]$Metodo, [string]$Url, [string]$Token, $Cuerpo)

    $parametros = @{ Method = $Metodo; Uri = $Url; UseBasicParsing = $true; TimeoutSec = 120; Headers = @{} }
    if ($Token) { $parametros.Headers.Authorization = "Bearer $Token" }
    if ($Cuerpo) {
        $parametros.ContentType = "application/json; charset=utf-8"
        $parametros.Body = [Text.Encoding]::UTF8.GetBytes(($Cuerpo | ConvertTo-Json))
    }

    $respuesta = $null
    try {
        $respuesta = Invoke-WebRequest @parametros
        $codigo = [int]$respuesta.StatusCode
    }
    catch {
        if ($_.Exception.Response) { $codigo = [int]$_.Exception.Response.StatusCode }
        else { $codigo = 0; Write-Host "      $($_.Exception.Message)" -ForegroundColor DarkYellow }
    }

    if ($codigo -eq $Esperado) { $marca = "OK   "; $color = "Green" }
    else { $marca = "FALLA"; $color = "Red"; $script:fallos++ }
    $ruta = ([Uri]$Url).PathAndQuery
    Write-Host ("{0}  {1} (esperado {2})  {3,-6} {4,-38} {5}" -f $marca, $codigo, $Esperado, $Metodo, $ruta, $Nombre) -ForegroundColor $color
    return $respuesta
}

function Get-Token([string]$Correo) {
    $r = Invoke-Prueba "Login $Correo" 200 POST "$Gateway/gateway/auth/login" -Cuerpo @{ correo = $Correo; clave = "Clave123*" }
    if (-not $r) { throw "No se pudo iniciar sesion con $Correo. Revise que el gateway, la API y la base de datos respondan." }
    return ($r.Content | ConvertFrom-Json).token
}

Write-Host "`nGateway: $Gateway`n"

$cliente = Get-Token "cliente@vet.com"

if ($Solo502) {
    $null = Invoke-Prueba "Propietarios detenido" 502 GET "$Gateway/gateway/propietarios" $cliente
    $null = Invoke-Prueba "La API sigue respondiendo" 200 GET "$Gateway/gateway/mascotas"
    $null = Invoke-Prueba "Ruta protegida de la API" 200 GET "$Gateway/gateway/mascotas/1" $cliente
}
else {
    $recep = Get-Token "recepcion@vet.com"
    $vet = Get-Token "veterinario@vet.com"
    $sello = Get-Date -Format "yyyyMMddHHmmss"

    Write-Host "`n-- Autenticacion"
    $null = Invoke-Prueba "Clave incorrecta" 401 POST "$Gateway/gateway/auth/login" -Cuerpo @{ correo = "cliente@vet.com"; clave = "incorrecta" }
    $null = Invoke-Prueba "Registro" 201 POST "$Gateway/gateway/auth/registro" -Cuerpo @{ correo = "prueba$sello@vet.com"; clave = "Clave123*" }
    $null = Invoke-Prueba "Perfil sin token (gateway)" 401 GET "$Gateway/gateway/auth/perfil"
    $null = Invoke-Prueba "Perfil con token" 200 GET "$Gateway/gateway/auth/perfil" $vet

    Write-Host "`n-- Lectura de la API"
    $null = Invoke-Prueba "Lista publica de mascotas" 200 GET "$Gateway/gateway/mascotas"
    $null = Invoke-Prueba "Lista con filtro" 200 GET "$Gateway/gateway/mascotas?especie=perro&soloActivas=true"
    $null = Invoke-Prueba "Detalle sin token (lo exige la API)" 401 GET "$Gateway/gateway/mascotas/1"
    $null = Invoke-Prueba "Detalle con token" 200 GET "$Gateway/gateway/mascotas/1" $cliente
    $null = Invoke-Prueba "Citas de una mascota" 200 GET "$Gateway/gateway/mascotas/1/citas" $cliente
    $null = Invoke-Prueba "Citas filtradas" 200 GET "$Gateway/gateway/citas?estado=Pendiente&desde=2030-01-01" $cliente
    $null = Invoke-Prueba "Mascota inexistente" 404 GET "$Gateway/gateway/mascotas/999999" $cliente

    Write-Host "`n-- Escritura de la API"
    $nueva = @{ nombre = "Prueba $sello"; especie = "Perro"; nombreDueno = "Pedro Gil" }
    $null = Invoke-Prueba "Crear sin token (gateway)" 401 POST "$Gateway/gateway/mascotas" -Cuerpo $nueva
    $null = Invoke-Prueba "Token falsificado (gateway)" 401 POST "$Gateway/gateway/mascotas" "$($recep)x" $nueva
    $null = Invoke-Prueba "Cliente no puede crear (API)" 403 POST "$Gateway/gateway/mascotas" $cliente $nueva
    $creada = Invoke-Prueba "Recepcionista crea mascota" 201 POST "$Gateway/gateway/mascotas" $recep $nueva
    if ($creada) {
        $id = ($creada.Content | ConvertFrom-Json).id
        Write-Host "      Location: $($creada.Headers.Location)"
        if ("$($creada.Headers.Location)" -notlike "$Gateway/gateway/*") {
            Write-Host "FALLA  La cabecera Location no apunta al gateway" -ForegroundColor Red; $script:fallos++
        }
        $null = Invoke-Prueba "Mascota duplicada" 409 POST "$Gateway/gateway/mascotas" $recep $nueva
        $cambio = @{ nombre = "Prueba $sello"; especie = "Perro"; raza = "Beagle"; nombreDueno = "Pedro Gil"; activo = $true }
        $null = Invoke-Prueba "Actualizar mascota" 204 PUT "$Gateway/gateway/mascotas/$id" $recep $cambio

        $cita = @{ mascotaId = $id; fechaHora = "2031-01-01T10:00:00"; motivo = "Chequeo general"; costo = 40 }
        $citaCreada = Invoke-Prueba "Agendar cita" 201 POST "$Gateway/gateway/citas" $recep $cita
        if ($citaCreada) {
            $citaId = ($citaCreada.Content | ConvertFrom-Json).id
            $edicion = @{ fechaHora = "2031-01-02T10:00:00"; motivo = "Chequeo general"; costo = 45; estado = "Pendiente" }
            $null = Invoke-Prueba "Veterinario reprograma" 204 PUT "$Gateway/gateway/citas/$citaId" $vet $edicion
            $null = Invoke-Prueba "Eliminar con cita pendiente" 409 DELETE "$Gateway/gateway/mascotas/$id" $vet
            $null = Invoke-Prueba "Eliminar cita" 204 DELETE "$Gateway/gateway/citas/$citaId" $vet
        }
        $null = Invoke-Prueba "Recepcionista no elimina" 403 DELETE "$Gateway/gateway/mascotas/$id" $recep
        $null = Invoke-Prueba "Veterinario elimina" 204 DELETE "$Gateway/gateway/mascotas/$id" $vet
    }

    Write-Host "`n-- Microservicio de propietarios"
    $null = Invoke-Prueba "Sin token (gateway)" 401 GET "$Gateway/gateway/propietarios"
    $null = Invoke-Prueba "Lista" 200 GET "$Gateway/gateway/propietarios" $cliente
    $null = Invoke-Prueba "Por id" 200 GET "$Gateway/gateway/propietarios/1" $cliente
    $null = Invoke-Prueba "Inexistente" 404 GET "$Gateway/gateway/propietarios/999" $cliente
    $null = Invoke-Prueba "DIRECTO al servicio, sin token" 401 GET "$Propietarios/api/propietarios"
    $null = Invoke-Prueba "DIRECTO por id, sin token" 401 GET "$Propietarios/api/propietarios/1"
    Write-Host "`nCorreo creado por la prueba de registro: prueba$sello@vet.com"
}

if ($script:fallos -eq 0) { Write-Host "`nTodas las pruebas pasaron.`n" -ForegroundColor Green; exit 0 }
Write-Host "`n$($script:fallos) prueba(s) fallaron.`n" -ForegroundColor Red
exit 1
