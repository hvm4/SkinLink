# SkinLink

Plataforma de comercialización de activos digitales coleccionables (*skins*), con compraventa, intercambios entre usuarios, cajas misteriosas y gamificación. Está desarrollada con el lenguaje de modelado y la arquitectura **HARIA** (C# / .NET 9, Entity Framework Core y SQLite).

## Estructura del repositorio

```
SkinLink/
├── README.md
└── ejemplo/                         <- proyecto HARIA (el nombre interno del proyecto es "ejemplo")
    ├── ejemplo.tdm                  <- modelo (fuente de verdad)
    ├── ejemplo.puml                 <- diagrama PlantUML del modelo
    ├── haria.config.json            <- configuración de generación (nombre, ORM, BD)
    ├── SkinLink.sln                 <- solución completa (incluye Skinlink.Web)
    └── generated/
        ├── ejemplo.sln              <- solución generada por HARIA (sin la web; no editar)
        ├── ejemplo.Domain/          <- entidades, enumerados, value objects
        ├── ejemplo.Application/     <- servicios y casos de uso
        │   ├── Services/Generated      (CRUD, filtros: no editar)
        │   ├── Services/manual         (operaciones Custom)
        │   ├── UseCases/Generated
        │   └── UseCases/manual         (operaciones CustomTransaction)
        ├── ejemplo.Infrastructure/  <- repositorios, UnitOfWork, DbContext
        ├── ejemplo.Tests/
        │   ├── Generated/              (pruebas generadas)
        │   └── manual/                 (pruebas de las operaciones a mano)
        ├── InitializeDB/            <- crea la base de datos SQLite y carga los datos de demostración
        └── Skinlink.Web/            <- capa web ASP.NET Core MVC generada con HARIA Web
```

## Requisitos

- .NET 9 SDK
- Visual Studio Code con la extensión **HARIA** (alisoftua.haria) 1.1.3 o superior (necesaria para `filterLinQ`)

## Puesta en marcha

Desde `ejemplo/generated`:

```powershell
# 1) Crear (o recrear) la base de datos con el esquema actual y los datos de demostración (~20 s)
cd InitializeDB
dotnet run
cd ..

# 2) Compilar y ejecutar todas las pruebas
dotnet test ejemplo.sln

# 3) Volver a crear la demo: la prueba generada Generated_Crud_And_Associations_Should_Work_Against_Database
#    usa esta misma base de datos y deja monederos e inventarios "Sample" sueltos
cd InitializeDB
dotnet run
cd ..

# 4) Arrancar la web (con HTTPS: la cookie de sesión es segura y no viaja por http)
dotnet run --project Skinlink.Web --launch-profile https
```

La web se abre en https://localhost:7046. La base de datos es `InitializeDB/ejemploBDD.db` y la web la usa directamente (`Skinlink.Web/appsettings.Haria.json`). Hay que volver a ejecutar `InitializeDB` cada vez que cambie el modelo (`ejemplo.tdm`); si no, las pruebas que usan la base de datos de archivo fallan con errores del tipo `table ... has no column named ...`. Ejecutarlo también devuelve los datos de la demo a su estado inicial.

Si `InitializeDB` va muy lento, borra `InitializeDB/logs/ejemplo.json`: el registrador generado por HARIA reescribe ese fichero entero en cada mensaje y, cuando pesa varios MB, cada paso tarda segundos.

Si `dotnet test` da `UnauthorizedAccessException` sobre `.dotnet` en Windows:

```powershell
$env:DOTNET_CLI_HOME = "$env:TEMP\dotnet-home"
New-Item -ItemType Directory -Force $env:DOTNET_CLI_HOME | Out-Null
```

## Datos de demostración (`InitializeDB/SeedData.cs`)

El login es con el **correo** y la contraseña. Las credenciales salen por consola al final de `InitializeDB`:

| Correo | Contraseña | Para qué sirve en la demo |
|---|---|---|
| `admin@skinlink.gg` (Administrador) | `Admin123!` | Paneles de administración, moderación, auditoría, comisión |
| `neorift@skinlink.gg` | `Demo1234!` | Comprador y coleccionista: compras, cajas abiertas, retirada, favoritos, intercambio pendiente |
| `novatrade@skinlink.gg` | `Demo1234!` | Vendedor con buena reputación, varias ventas publicadas, inventario público |
| `pixelwolf@skinlink.gg` | `Demo1234!` | Comprador y vendedor; tiene un intercambio por responder |
| `lunabyte@skinlink.gg` | `Demo1234!` | Ha usado la recompensa diaria; un intercambio aceptado y otro cancelado |
| `zerolag@skinlink.gg` | `Demo1234!` | Ha abierto una caja; venta cancelada |
| `xraptor@skinlink.gg` | `Demo1234!` | Cuenta sin verificar (no puede retirar); autor de una reseña moderada |
| `tramposo99@skinlink.gg` | `Demo1234!` | Cuenta suspendida por el administrador (queda en auditoría) |

Todo se crea con las operaciones reales del modelo, así que los saldos, comisiones, transacciones y auditoría son coherentes:

- Configuración del sistema: comisión del 5 % hasta 50 €, 4 % hasta 500 € y 3 % por encima; retirada mínima 10 € y máximo diario 2.000 €.
- 10 artículos de catálogo (uno con descuento del 10 %), 23 activos de todas las rarezas y condiciones.
- 5 cajas con probabilidades que suman 100 % (una inactiva y una gratuita para la recompensa diaria).
- 7 depósitos, 7 ventas (3 completadas, 3 disponibles, 1 cancelada, 1 con el precio rebajado), 1 retirada, 3 aperturas de caja.
- 8 reseñas (una moderada por el admin y otra ocultada por el vendedor), 5 favoritos, 3 intercambios (pendiente, aceptado y cancelado).

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
| Monedero | `incrementarSaldo` / `decrementarSaldo` | Suma o resta saldo disponible (importe positivo, misma divisa, nunca negativo) |
| Transaccion | `validarSaldoMonedero` | Indica si un monedero cubre un importe en su divisa |
| Usuario | `sumarExperiencia` | Suma XP y recalcula el nivel (nivel = XP / 100 + 1). La usan compra (+50 comprador, +30 vendedor), publicación (+10), caja (+20) y recompensa diaria (+25) |
| Administrador | `actualizarEstadoKYC` | Cambia el estado de una cuenta y lo registra en `RegistroAuditoria` |
| Venta | `publicarVenta` | Publica un activo del inventario del vendedor (propiedad, no bloqueado, precio válido) |
| Venta | `editarPrecio` | Cambia el precio de una venta disponible |
| ResenaVendedor | `moderarResena` | El administrador oculta/restaura la reseña y queda en auditoría |
| ResenaVendedor | `ocultarResena` | Oculta la reseña del perfil del vendedor |
| CajaMisteriosa | `simularApertura` | Simula la apertura sin coste ni persistencia |

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

Todas las operaciones Custom y CustomTransaction del modelo están implementadas.

## Seguridad y roles (Skinlink.Web)

Los roles se asignan al iniciar sesión en `Utils/Security/UsuarioClaims.cs` (punto `AddTrustedClaims` de HARIA Web), según la entidad cargada en el servidor:

| Cuenta | Rol |
|---|---|
| `Usuario` activo | `Usuario` |
| `Administrador` activo (hereda de `Usuario`) | `Administrador` |
| Cuenta suspendida | ninguno: solo ve la portada y puede cerrar sesión |

- **Solo `Administrador`**:
  - Controladores `Administrador`, `RegistroAuditoria`, `ConfiguracionSistema` y `ProbabilidadDrop`.
  - Listado, alta, edición, borrado y filtro de `Usuario`; `sumarExperiencia`, `suspenderCuentaUsuario`, `verificarUsuario` y `actualizarEstadoKYC`.
  - Alta, edición y borrado de `ArticuloCatalogo`, `CajaMisteriosa`, `Transaccion`, `Monedero` e `InventarioPrivado`; `incrementarSaldo`, `decrementarSaldo` y `validarSaldoMonedero`.
- **Cualquier cuenta activa** (`Roles.Todos`): el resto. Un usuario solo ve su propia ficha (`Usuario/Details`) y solo reclama su propia recompensa diaria.
- **CRUD genérico de HARIA (Crear, Editar, Eliminar)**: solo el administrador, en todas las entidades. La única alta genérica abierta a usuarios es la de `ResenaVendedor`, que para ellos se muestra como la pantalla propia "Valorar a un vendedor" (`Views/ResenaVendedor/Valorar.cshtml`). Los usuarios operan con sus pantallas: Poner a la venta, Editar precio, Cancelar venta, Proponer intercambio, ★ Favorito en Buscar activos, etc.
- **Listados completos** de `Monedero`, `Transaccion` e `InventarioPrivado`: solo el administrador. En el detalle, un usuario solo ve su monedero, sus transacciones y su inventario (o un inventario público). Los listados genéricos de `ActivoDigital` y `ResenaVendedor` redirigen al usuario a Buscar activos y a la reputación pública.
- `Utils/Security/AccionesSegunRolTagHelper.cs` (registrado en `Views/_ViewImports.cshtml`): oculta a quien no es administrador los enlaces Crear/Editar/Eliminar y los "Volver al listado" de listados reservados, también en las vistas generadas, sin modificarlas. Si una regeneración de HARIA Web rehace `_ViewImports.cshtml`, hay que volver a añadir `@addTagHelper *, Skinlink.Web`.
- **Poner a la venta** (`/Venta/PonerALaVenta`, `Controllers/manual/VentaController.PonerALaVenta.cs`): el usuario elige un activo suyo y un precio; se crea la venta con `Venta.new` (vendedor = sesión) y se publica con `publicarVenta`. El modelo solo admite una venta por activo (relación 1 a 1 con clave única): una venta cancelada propia se reutiliza y un activo que ya se vendió no se puede volver a vender.
- Las acciones con `[AllowAnonymous]` (búsqueda de activos y cajas) siguen siendo públicas.
- En el código: `[Authorize(Roles = Roles.Administrador)]` o `Roles.Todos`; `User.GetUsuarioId()`, `User.EsAdministrador()`, `User.PuedeAccederAUsuario(id)`.
- El login es con el **correo** y la contraseña (operación `login` del modelo con `p_correo`).
- **La pantalla de login muestra la etiqueta "id" en el campo del correo, y se ha dejado así a propósito.** `Views/Account/Login.cshtml` y `ViewModels/LoginViewModel.cs` los genera HARIA Web enteros, sin ninguna región protegida: la etiqueta sale de la propiedad `Id` del modelo de login generado. Si se cambiara a mano, la siguiente regeneración de la web lo sobrescribiría y habría que repetirlo cada vez, con el riesgo de que alguien del grupo regenere y el cambio se pierda sin que nadie lo note. El campo acepta el correo y el login funciona; solo es el texto de la etiqueta.
- Los roles van en la cookie: si se suspende a alguien con la sesión abierta, pierde el acceso al volver a iniciar sesión.
- `Views/Home/Index.cshtml` es generado: si se regenera con "Update Home Page", hay que volver a poner el `if` que separa la portada del administrador (`_BusinessNavigation`) de la del resto (`_InicioUsuario`).

## Páginas de social y sistema (Skinlink.Web)

En todas estas operaciones el usuario que actúa sale de la sesión, nunca del formulario: aunque las acciones generadas sigan recibiendo `p_proponente_oid`, `p_admin_oid`, etc., esos valores se ignoran.

| URL | Quién | Operación |
|---|---|---|
| `/ResenaVendedor/FiltrarResenasVisiblesPorVendedor?p_vendedorId=5` | Público | `filtrarResenasVisiblesPorVendedor`: reseñas visibles y nota media del vendedor; el propio vendedor puede `ocultarResena` |
| `/ResenaVendedor/Moderacion` | Administrador | `moderarResena`: ocultar o restaurar cualquier reseña (queda en auditoría) |
| `/Intercambio/MisIntercambios` | Usuario | Recibidos (aceptar o rechazar), enviados (cancelar) e historial |
| `/Intercambio/Proponer` | Usuario | `iniciarIntercambio`: elegir usuario y activos de cada lado |
| `/Favorito/MisFavoritos` | Usuario | Favoritos propios; `POST /Favorito/Alternar` (`activoId`, `returnUrl`) marca o quita desde cualquier vista |
| `/InventarioPrivado/Visibilidad` | Usuario / Administrador | `cambiarVisibilidadInventario` del inventario propio (el admin, de cualquiera) |
| `/ConfiguracionSistema/Comision` | Administrador | `calcularComisionMercado`: tramos y cálculo de comisión e importe neto |

`/Intercambio` y `/Favorito` (listados completos) redirigen a las páginas "Mis…" salvo para el administrador. La nota de una reseña se valida entre 1 y 5 y nadie puede valorarse a sí mismo (`ResenaVendedorViewModel`, región manual).

## Mensajes al usuario (Skinlink.Web)

Toda la web sigue el mismo estándar para avisar del resultado de una operación:

| Dónde | Qué se usa |
|---|---|
| Controlador, operación correcta | `TempData["Exito"] = "...";` |
| Controlador, operación fallida | `TempData["Error"] = this.MensajeDeError(ex);` dentro de `catch (Exception ex) when (ex is not OperationCanceledException)` |
| Controlador, error en un formulario que se vuelve a mostrar | `ModelState.AddModelError(string.Empty, this.MensajeDeError(ex));` |
| Vista | `<partial name="_MensajesOperacion" />` (recuadro verde o rojo y resumen de validación) |

`MensajeDeError` está en `Utils/MensajesUsuario.cs`. Los errores de negocio (`ModelException` lanzadas por los servicios y casos de uso) se muestran tal cual; cualquier otro error es técnico: se registra en el log del servidor y el usuario ve "No se ha podido completar la operación. Inténtalo de nuevo más tarde.". Así nunca se enseñan errores internos (SQL, nulos, trazas) en pantalla. Los mensajes que deba ver el usuario se lanzan como `ModelException` y en castellano.

### Plantilla de las páginas

Todas las pantallas propias siguen la misma tarjeta que las vistas generadas por HARIA Web (ejemplo de referencia: `Views/Venta/PublicarVenta.cshtml`):

```cshtml
<div class="card border-0 shadow-sm">
    <div class="card-header bg-body p-4">
        <h1 class="h3 mb-1">Título</h1>
        <p class="text-body-secondary mb-0">Una frase que explica qué se hace en la página.</p>
    </div>
    <div class="card-body p-4">
        <partial name="_MensajesOperacion" />
        ...formulario, tabla o contenido...
    </div>
    <div class="card-footer bg-body p-4 d-flex flex-column flex-sm-row gap-2">
        <button type="submit" class="btn btn-primary">Acción principal</button>
        <a asp-action="..." class="btn btn-outline-secondary">Volver a ...</a>
    </div>
</div>
```

- Si la página es un formulario, el `<form>` envuelve el cuerpo y el pie para que los botones queden en el pie.
- Los botones de la página que no son la acción del formulario (crear, ir a otra sección) van a la derecha de la cabecera.
- Las tablas usan `table table-hover align-middle` con `thead class="table-light"`; los textos secundarios, `text-body-secondary`.
- Una sección secundaria con mucho contenido (p. ej. "Descuentos actuales") va en otra tarjeta igual debajo, con un `h2 class="h5"` en la cabecera.
- `@section Scripts { ... }` va siempre fuera de la tarjeta, al final del archivo.

## Cobertura de requisitos de la propuesta

| Requisito | Dónde está |
|---|---|
| Navegar y clasificar activos por precio, rareza, tipo y condición | Filtros de `ActivoDigital`, `filtrarVentasPorPrecioMaximo`, `filtrarCajasPorPrecioApertura`; skins y cajas son entidades separadas |
| Registro, perfil e inventario privado | `Usuario` (registro y login), `InventarioPrivado`, `cambiarVisibilidadInventario` |
| Depósitos, retiros y transacciones de saldo | `depositarFondos`, `retirarSaldoWallet`, `incrementarSaldo`/`decrementarSaldo`, filtros de `Transaccion` |
| Compra y venta directa | `publicarVenta`, `editarPrecio`, `cancelarVenta`, `procesarCompraSkin` |
| Cajas basadas en el azar | `abrirCajaSorpresa`, `simularApertura`, `ProbabilidadDrop` |
| Historial de transacciones | Filtros de `Transaccion` por tipo y por fechas |
| Historial de resultados de azar | **Parcial**: queda la transacción `CosteCaja`, pero no qué activo salió |
| Roles Administrador / Usuario y paneles de administración | `[Authorize(Roles = ...)]`, CRUD de usuarios, catálogo y cajas solo para el admin |
| Búsquedas con filtros | 12 `readFilter` |
| Validación en cliente y servidor | Data annotations de los ViewModels, `ModelState` y validaciones de los servicios |
| Niveles de usuario y bonificación diaria | `sumarExperiencia`, `reclamarRecompensaDiaria` |
| Valoración numérica de vendedores | `ResenaVendedor.puntuacion` (1–5), nota media en la página del vendedor, moderación |
| Notificar al usuario las transacciones | **Pendiente**: no hay entidad `Notificacion` (requiere cambiar el `.tdm`) |
| Recomendaciones en la portada según valoraciones y actividad | **Pendiente**: no hay operación de recomendación (requiere cambiar el `.tdm`) |

## Pruebas

- `ejemplo.Tests/Generated/ModelGeneratedTests.cs`: pruebas generadas (CRUD y asociaciones contra la base de datos, literales de enumerados y contrato de los filtros).
- `ejemplo.Tests/manual/`: pruebas de las operaciones Custom, CustomTransaction y filtros implementados a mano, con SQLite en memoria (`OperacionesPendientesTests.cs` cubre las 10 operaciones completadas en esta entrega y la experiencia por actividad).

## Normas de trabajo con HARIA

- Nunca se edita nada fuera de las regiones `/*PROTECTED REGION ID(...) ENABLED START*/ ... /*PROTECTED REGION END*/`: al regenerar se pierde.
- La lógica propia va en `Services/manual` y `UseCases/manual`, un archivo por operación (`<Entidad>Service_<Operacion>.cs` o `<Entidad>UseCase_<Operacion>.cs`).
- Tras cambiar `ejemplo.tdm`: regenerar con HARIA, ejecutar `InitializeDB` y lanzar `dotnet test` antes de subir los cambios. Después de `dotnet test`, ejecutar otra vez `InitializeDB` para dejar la demo limpia.
- Si una región protegida desaparece del modelo, HARIA guarda su código en `generated/.haria/orphans`.
