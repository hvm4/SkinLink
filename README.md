# SkinLink

Plataforma de comercialización de activos digitales coleccionables (*skins*), con compraventa, intercambios entre usuarios, cajas misteriosas y gamificación. Está desarrollada con el lenguaje de modelado y la arquitectura **HARIA** (C# / .NET 9, Entity Framework Core y SQLite).

## Estructura del repositorio

```
SkinLink/
├── README.md
└── SkinLink/                      <- proyecto HARIA
    ├── SkinLink.tdm               <- modelo (fuente de verdad)
    ├── SkinLink.puml              <- diagrama PlantUML del modelo
    ├── haria.config.json          <- configuración de generación (nombre, ORM, BD)
    └── generated/
        ├── SkinLink.sln
        ├── SkinLink.Domain/       <- entidades, enumerados, value objects
        ├── SkinLink.Application/  <- servicios y casos de uso
        │   ├── Services/Generated    (CRUD, filtros: no editar)
        │   ├── Services/manual       (operaciones Custom)
        │   ├── UseCases/Generated
        │   └── UseCases/manual       (operaciones CustomTransaction)
        ├── SkinLink.Infrastructure/ <- repositorios, UnitOfWork, DbContext
        ├── SkinLink.Tests/
        │   ├── Generated/            (pruebas generadas)
        │   └── manual/               (pruebas de las operaciones a mano)
        └── InitializeDB/          <- crea y rellena la base de datos SQLite
```

## Requisitos

- .NET 9 SDK
- Visual Studio Code con la extensión **HARIA** (alisoftua.haria) 1.1.3 o superior (necesaria para `filterLinQ`)

## Puesta en marcha

Desde `SkinLink/generated`:

```powershell
# 1) Crear (o recrear) la base de datos con el esquema actual y los datos de ejemplo
cd InitializeDB
dotnet run
cd ..

# 2) Compilar y ejecutar todas las pruebas
dotnet test
```

La base de datos es `InitializeDB/SkinLinkBDD.db`. Hay que volver a ejecutar `InitializeDB` cada vez que cambie el modelo (`SkinLink.tdm`); si no, las pruebas que usan la base de datos de archivo fallan con errores del tipo `table ... has no column named ...`.

Si `dotnet test` da `UnauthorizedAccessException` sobre `.dotnet` en Windows:

```powershell
$env:DOTNET_CLI_HOME = "$env:TEMP\dotnet-home"
New-Item -ItemType Directory -Force $env:DOTNET_CLI_HOME | Out-Null
```

## Modelo

**Enumerados:** `EstadoCuenta`, `Rareza`, `Condicion`, `TipoTransaccion`, `EstadoVenta`, `EstadoIntercambio`.

**Value objects:** `AtributosSkin`, `Moneda`.

**Entidades:** `Usuario` (con `Login`), `Administrador` (hereda de `Usuario`), `Monedero`, `Transaccion`, `InventarioPrivado`, `ArticuloCatalogo`, `ActivoDigital`, `Favorito`, `Intercambio`, `Venta`, `ResenaVendedor`, `CajaMisteriosa`, `ProbabilidadDrop`, `ConfiguracionSistema`, `RegistroAuditoria`.

Todas las entidades tienen las operaciones CRUD generadas (`new`, `modify`, `destroy`, `readOID`, `readAll`).

### Operaciones Custom (servicios)

| Entidad | Operación | Qué hace |
|---|---|---|
| ActivoDigital | `actualizarPrecio` | Actualiza el precio estimado |
| ActivoDigital | `bloquearParaVenta` / `desbloquear` | Marca o libera el activo para la venta |
| ActivoDigital | `marcarComoFavorito` | Añade o retira el favorito |
| ActivoDigital | `verificarAutenticidad` | Comprueba el identificador único |
| ActivoDigital | `generarReporteActividad` | Informe de ventas, favoritos y posición en la colección |
| ArticuloCatalogo | `actualizarDescuentoArticulo` | Descuento temporal con porcentaje y fecha de fin |
| InventarioPrivado | `cambiarVisibilidadInventario` | Alterna inventario público/privado |
| Usuario | `suspenderCuentaUsuario` | Suspende la cuenta |
| Usuario | `verificarUsuario` | Marca la identidad como verificada (mayor de edad, cuenta no suspendida) |
| Venta | `cancelarVenta` | Cancela la venta |
| ConfiguracionSistema | `calcularComisionMercado` | Comisión según el tramo de precio |

### Operaciones CustomTransaction (casos de uso)

| Entidad | Operación | Qué hace |
|---|---|---|
| Venta | `procesarCompraSkin` | Transfiere el activo y los saldos entre comprador y vendedor |
| Usuario | `reclamarRecompensaDiaria` | Entrega una skin y suma experiencia |
| Intercambio | `iniciarIntercambio` / `aceptarIntercambio` / `cancelarIntercambio` | Ciclo completo de una propuesta de intercambio |
| CajaMisteriosa | `abrirCajaSorpresa` | Cobra la apertura y entrega un activo según probabilidades |
| Monedero | `depositarFondos` | Acredita un pago confirmado (sin referencias repetidas) |
| Monedero | `retirarSaldoWallet` | Retirada con usuario verificado, mínimo y límite diario |

### Filtros (`readFilter`)

`filtrarUsuariosPorEstado`, `filtrarTransaccionesPorTipo`, `filtrarTransaccionesPorRangoFechas`, `filtrarActivosPorRareza`, `filtrarActivosPorColeccion`, `filtrarActivosPorCondicion`, `filtrarActivosEnVenta`, `filtrarVentasPorEstado`, `filtrarVentasPorPrecioMaximo`, `filtrarResenasVisiblesPorVendedor`, `filtrarCajasActivas`, `filtrarCajasPorPrecioApertura`.

### Operaciones aún sin implementar

Siguen con el código generado por HARIA, que lanza `ModelException("... is pending")`:

`incrementarSaldo`, `decrementarSaldo`, `validarSaldoMonedero`, `sumarExperiencia`, `actualizarEstadoKYC`, `publicarVenta`, `editarPrecio`, `moderarResena`, `ocultarResena`, `simularApertura`.

## Pruebas

- `SkinLink.Tests/Generated/ModelGeneratedTests.cs`: pruebas generadas (CRUD y asociaciones contra la base de datos, literales de enumerados y contrato de los filtros).
- `SkinLink.Tests/manual/`: pruebas de las operaciones Custom, CustomTransaction y filtros implementados a mano, con SQLite en memoria.

## Normas de trabajo con HARIA

- Nunca se edita nada fuera de las regiones `/*PROTECTED REGION ID(...) ENABLED START*/ ... /*PROTECTED REGION END*/`: al regenerar se pierde.
- La lógica propia va en `Services/manual` y `UseCases/manual`, un archivo por operación (`<Entidad>Service_<Operacion>.cs` o `<Entidad>UseCase_<Operacion>.cs`).
- Tras cambiar `SkinLink.tdm`: regenerar con HARIA, ejecutar `InitializeDB` y lanzar `dotnet test` antes de subir los cambios.
- Si una región protegida desaparece del modelo, HARIA guarda su código en `generated/.haria/orphans`.
