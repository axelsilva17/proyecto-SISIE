#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Test suite para la Web API SISIE
.DESCRIPTION
    Ejecuta casos de prueba contra la API REST en http://localhost:5000
    Requiere: PowerShell 5.1+ o PowerShell 7+
    Uso: .\tests.ps1
    Si hay error de permisos: Set-ExecutionPolicy -Scope CurrentUser Unrestricted
#>

# ══════════════════════════════════════════════════════════════════════
# CONFIGURACION
# ══════════════════════════════════════════════════════════════════════
$script:BaseUrl      = "http://localhost:5000"
$script:LoginEmail   = "test@sisie.com"
$script:LoginPassword = "Admin123!"

# ══════════════════════════════════════════════════════════════════════
# INICIALIZACION
# ══════════════════════════════════════════════════════════════════════
$script:Token   = $null
$script:Passed  = 0
$script:Failed  = 0
$script:Results = @()

[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

# ══════════════════════════════════════════════════════════════════════
# HELPERS
# ══════════════════════════════════════════════════════════════════════

function Get-JwtToken {
    param([string]$Email, [string]$Password)
    $body = @{ email = $Email; password = $Password } | ConvertTo-Json
    $response = Invoke-WebRequest -Uri "$script:BaseUrl/api/auth/login" `
        -Method Post -Body $body -ContentType "application/json"
    if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300) {
        $result = $response.Content | ConvertFrom-Json
        return $result.token
    }
    throw "Login failed: $($response.StatusCode) $($response.Content)"
}

function Invoke-Api {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [string]$Body   = $null,
        [bool]$Auth     = $true
    )
    $headers = @{ "Content-Type" = "application/json" }
    if ($Auth -and $script:Token) {
        $headers["Authorization"] = "Bearer $script:Token"
    }
    $params = @{ Uri = "$script:BaseUrl$Uri"; Method = $Method; Headers = $headers }
    if ($Body) { $params["Body"] = $Body }
    try {
        $response = Invoke-WebRequest @params
        $json = if ($response.Content) { $response.Content | ConvertFrom-Json } else { $null }
        return @{ StatusCode = [int]$response.StatusCode; Content = $json; Success = $true }
    } catch {
        $statusCode = [int]$_.Exception.Response.StatusCode
        $raw = $null
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            if ($stream -and $stream.CanRead) {
                $reader = [System.IO.StreamReader]::new($stream)
                $raw    = $reader.ReadToEnd()
                $reader.Close()
            }
        } catch { $raw = $_.ErrorDetails.Message }
        $json = if ($raw) { try { $raw | ConvertFrom-Json } catch { $null } } else { $null }
        return @{ StatusCode = $statusCode; Content = $json; Success = $false; Raw = $raw }
    }
}

function Get-Message {
    param($Content)
    if (-not $Content) { return "" }
    if ($Content.message) { return "$($Content.message)" }
    if ($Content.Message) { return "$($Content.Message)" }
    return ""
}

function Write-TestResult {
    param(
        [string]$Id,
        [string]$Description,
        [int]$StatusCode,
        [string]$Message,
        [bool]$Pass
    )
    $color   = if ($Pass) { "Green" } else { "Red" }
    $verdict = if ($Pass) { "PASA"  } else { "FALLA" }
    Write-Host ""
    Write-Host ("[$Id] $Description") -ForegroundColor $color
    Write-Host "  Status : $StatusCode"
    if ($Message) { Write-Host "  Mensaje: $Message" }
    Write-Host ("  >>> $verdict <<<") -ForegroundColor $color
}

function Write-Summary {
    $total = $script:Passed + $script:Failed
    Write-Host ""
    Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  RESUMEN" -ForegroundColor Cyan
    Write-Host "  Total : $total"  -ForegroundColor Cyan
    Write-Host ("  PASA  : $($script:Passed)") -ForegroundColor Green
    Write-Host ("  FALLA : $($script:Failed)") -ForegroundColor Red
    if ($script:Failed -gt 0) {
        Write-Host ""
        Write-Host "  FALLOS:" -ForegroundColor Red
        foreach ($r in $script:Results) {
            if (-not $r.Pass) {
                Write-Host ("    [$($r.Id)] $($r.Description) - esperado: $($r.ExpectedStr), obtenido: $($r.StatusCode)") -ForegroundColor Red
            }
        }
    }
    Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
}

function Run-TestCase {
    param(
        [string]$Id,
        [string]$Description,
        [string]$Endpoint,
        [string]$Method           = "POST",
        [string]$Body             = $null,
        [int[]]$ExpectedStatus    = @(200),
        [scriptblock]$ExtraValidation = $null,
        [bool]$Auth               = $true
    )
    $result     = Invoke-Api -Uri $Endpoint -Method $Method -Body $Body -Auth $Auth
    $statusCode = $result.StatusCode
    $message    = Get-Message $result.Content
    $statusOk   = $statusCode -in $ExpectedStatus

    $extraOk  = $true
    $extraMsg = ""
    if ($ExtraValidation) {
        $extraResult = & $ExtraValidation $result
        $extraOk  = $extraResult.Ok
        $extraMsg = $extraResult.Message
    }

    $pass       = $statusOk -and $extraOk
    $displayMsg = if ($extraMsg) { "$message | $extraMsg" } else { $message }

    Write-TestResult -Id $Id -Description $Description -StatusCode $statusCode -Message $displayMsg -Pass $pass

    if ($pass) { $script:Passed++ } else { $script:Failed++ }
    $script:Results += @{
        Id          = $Id
        Description = $Description
        StatusCode  = $statusCode
        Message     = $message
        Pass        = $pass
        ExpectedStr = ($ExpectedStatus -join " o ")
    }
    return $result
}

# ══════════════════════════════════════════════════════════════════════
# MAIN
# ══════════════════════════════════════════════════════════════════════

Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SISIE Web API - Test Suite"               -ForegroundColor Cyan
Write-Host "  $($script:BaseUrl)"                       -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan

# ── REGISTER + LOGIN ──────────────────────────────────────────────────
Write-Host ""
Write-Host ">>> Registrando usuario de prueba (si no existe)..." -ForegroundColor Yellow
try {
    $regBody = @{
        nombreUsuario = "admin"
        email         = $script:LoginEmail
        password      = $script:LoginPassword
    } | ConvertTo-Json
    Invoke-WebRequest -Uri "$script:BaseUrl/api/auth/register" -Method Post -Body $regBody -ContentType "application/json" | Out-Null
    Write-Host "  OK - usuario registrado" -ForegroundColor Green
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "  $code - usuario ya existe o registro no disponible" -ForegroundColor Gray
}

Write-Host ""
Write-Host ">>> Obteniendo token JWT..." -ForegroundColor Yellow
try {
    $script:Token = Get-JwtToken -Email $script:LoginEmail -Password $script:LoginPassword
    Write-Host "  OK - token obtenido" -ForegroundColor Green
} catch {
    Write-Host "  ERROR al obtener token: $_" -ForegroundColor Red
    exit 1
}

# ══════════════════════════════════════════════════════════════════════
# PRODUCTOS  (POST /api/productos)
# ══════════════════════════════════════════════════════════════════════
Write-Host ""
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CASOS DE PRUEBA — registrarProducto"     -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan

# Sufijo unico por ejecucion para evitar duplicados entre corridas
$ts = Get-Date -Format "HHmmss"

# CP-P01 — Registro exitoso (Taladro + timestamp)
$nombreP01 = "Taladro$ts"
Run-TestCase -Id "CP-P01" -Description "Registro exitoso de producto con datos validos" `
    -Endpoint "/api/productos" -Method POST `
    -Body "{`"nombreProducto`":`"$nombreP01`",`"descripcion`":`"Taladro electrico`",`"precioUnitario`":1500,`"stock`":10,`"idCategoria`":1}" `
    -ExpectedStatus @(200, 201)

# CP-P02 — Nombre duplicado (mismo nombre que CP-P01)
Run-TestCase -Id "CP-P02" -Description "El nombre del producto ya existe" `
    -Endpoint "/api/productos" -Method POST `
    -Body "{`"nombreProducto`":`"$nombreP01`",`"descripcion`":`"Taladro electrico`",`"precioUnitario`":1500,`"stock`":10,`"idCategoria`":1}" `
    -ExpectedStatus @(400)

# CP-P03 — Precio invalido (cero) — Serrucho
Run-TestCase -Id "CP-P03" -Description "Precio invalido — valor cero" `
    -Endpoint "/api/productos" -Method POST `
    -Body '{"nombreProducto":"Serrucho","descripcion":"Serrucho manual","precioUnitario":0,"stock":5,"idCategoria":1}' `
    -ExpectedStatus @(400)

# CP-P04 — Precio invalido (negativo) — Pinza
Run-TestCase -Id "CP-P04" -Description "Precio invalido — valor negativo" `
    -Endpoint "/api/productos" -Method POST `
    -Body '{"nombreProducto":"Pinza","descripcion":"Pinza universal","precioUnitario":-100,"stock":5,"idCategoria":1}' `
    -ExpectedStatus @(400)

# CP-P05 — Stock negativo — Cierra
Run-TestCase -Id "CP-P05" -Description "Stock negativo — valor -1" `
    -Endpoint "/api/productos" -Method POST `
    -Body '{"nombreProducto":"Cierra","descripcion":"Cierra circular","precioUnitario":5000,"stock":-1,"idCategoria":1}' `
    -ExpectedStatus @(400)

# CP-P06 — Error al guardar (nombre null)
Run-TestCase -Id "CP-P06" -Description "Error al guardar en la base de datos (nombre null)" `
    -Endpoint "/api/productos" -Method POST `
    -Body '{"nombreProducto":null,"descripcion":"Martillo de goma","precioUnitario":80000,"stock":3,"idCategoria":1}' `
    -ExpectedStatus @(400, 500)

# ══════════════════════════════════════════════════════════════════════
# VENTAS  (POST /api/ventas/registrar)
# ══════════════════════════════════════════════════════════════════════
Write-Host ""
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CASOS DE PRUEBA — registrarVenta"        -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan

# Body base valido para ventas — incluye todos los campos requeridos por VentaCreateDTO
$ventaValida = '{"dniCliente":"12345678","nombreCliente":"Juan Perez","telefonoCliente":"3794000000","emailCliente":"juan@mail.com","esEnvio":false,"detalles":[{"idProducto":1,"cantidad":2}],"metodoPago":"Efectivo","tipoEntrega":"Mostrador"}'

# CP-V01 — Registro exitoso
Run-TestCase -Id "CP-V01" -Description "Registro exitoso de venta con datos validos" `
    -Endpoint "/api/ventas/registrar" -Method POST `
    -Body $ventaValida `
    -ExpectedStatus @(200, 201)

# CP-V02 — Stock insuficiente (cantidad 9999)
Run-TestCase -Id "CP-V02" -Description "Stock insuficiente — stock disponible es 1" `
    -Endpoint "/api/ventas/registrar" -Method POST `
    -Body '{"dniCliente":"12345678","nombreCliente":"Juan Perez","telefonoCliente":"3794000000","emailCliente":"juan@mail.com","esEnvio":false,"detalles":[{"idProducto":1,"cantidad":9999}],"metodoPago":"Efectivo","tipoEntrega":"Mostrador"}' `
    -ExpectedStatus @(400)

# CP-V03 — Producto inexistente
Run-TestCase -Id "CP-V03" -Description "Producto inexistente — Id no existe en BD" `
    -Endpoint "/api/ventas/registrar" -Method POST `
    -Body '{"dniCliente":"12345678","nombreCliente":"Juan Perez","telefonoCliente":"3794000000","emailCliente":"juan@mail.com","esEnvio":false,"detalles":[{"idProducto":99999,"cantidad":1}],"metodoPago":"Efectivo","tipoEntrega":"Mostrador"}' `
    -ExpectedStatus @(400, 404)

# CP-V04 — Lista de productos vacia
Run-TestCase -Id "CP-V04" -Description "Lista de productos vacia" `
    -Endpoint "/api/ventas/registrar" -Method POST `
    -Body '{"dniCliente":"12345678","nombreCliente":"Juan Perez","telefonoCliente":"3794000000","emailCliente":"juan@mail.com","esEnvio":false,"detalles":[],"metodoPago":"Efectivo","tipoEntrega":"Mostrador"}' `
    -ExpectedStatus @(400)

# CP-V05 — Error al guardar: nombre null (campo obligatorio)
# dniCliente presente pero nombreCliente null — debe rechazarlo con 400
Run-TestCase -Id "CP-V05" -Description "Error al guardar en la base de datos (nombreCliente null)" `
    -Endpoint "/api/ventas/registrar" -Method POST `
    -Body '{"dniCliente":"12345678","nombreCliente":null,"telefonoCliente":"3794000000","emailCliente":"juan@mail.com","esEnvio":false,"detalles":[{"idProducto":1,"cantidad":1}],"metodoPago":"Efectivo","tipoEntrega":"Mostrador"}' `
    -ExpectedStatus @(400, 500)

# CP-V06 — Postcondicion stock actualizado (2 productos)
Write-Host ""
Write-Host ">>> CP-V06: consultando stock inicial de idProducto=1 e idProducto=2..." -ForegroundColor Yellow
$p1Before = (Invoke-Api -Uri "/api/productos/1" -Method GET).Content.stock
$p2Before = (Invoke-Api -Uri "/api/productos/2" -Method GET).Content.stock
Write-Host "  Stock producto 1: $p1Before | Stock producto 2: $p2Before" -ForegroundColor Gray

$extraV06 = {
    $p1After = (Invoke-Api -Uri "/api/productos/1" -Method GET).Content.stock
    $p2After = (Invoke-Api -Uri "/api/productos/2" -Method GET).Content.stock
    $exp1 = $p1Before - 2
    $exp2 = $p2Before - 3
    $ok1  = $p1After -eq $exp1
    $ok2  = $p2After -eq $exp2
    $msg  = "Prod1: $p1Before->$p1After (esp $exp1) | Prod2: $p2Before->$p2After (esp $exp2)"
    return @{ Ok = ($ok1 -and $ok2); Message = $msg }
}

Run-TestCase -Id "CP-V06" -Description "Postcondicion — stock actualizado por producto (ActualizarStockAsync x2)" `
    -Endpoint "/api/ventas/registrar" -Method POST `
    -Body '{"dniCliente":"12345678","nombreCliente":"Juan Perez","telefonoCliente":"3794000000","emailCliente":"juan@mail.com","esEnvio":false,"detalles":[{"idProducto":1,"cantidad":2},{"idProducto":2,"cantidad":3}],"metodoPago":"Efectivo","tipoEntrega":"Mostrador"}' `
    -ExpectedStatus @(200, 201) `
    -ExtraValidation $extraV06

# ══════════════════════════════════════════════════════════════════════
# RESUMEN
# ══════════════════════════════════════════════════════════════════════
Write-Summary

if ($script:Failed -gt 0) { exit 1 }