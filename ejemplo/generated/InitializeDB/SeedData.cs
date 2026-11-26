using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Services;
using ejemplo.Application.UseCases.CajaMisteriosa;
using ejemplo.Application.UseCases.Intercambio;
using ejemplo.Application.UseCases.Monedero;
using ejemplo.Application.UseCases.Usuario;
using ejemplo.Application.UseCases.Venta;
using ejemplo.Infrastructure.Data;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;
using ejemplo.Infrastructure;

using Microsoft.Extensions.Logging;

/*PROTECTED REGION ID(seedDataUsings) ENABLED START*/
// Add custom usings here.
/*PROTECTED REGION END*/

namespace InitializeDB;

public static class SeedData
{
    public static async Task InitializeDataAsync(DbContextOptions<ejemploDbContext> options, ILoggerFactory? loggerFactory = null)
    {
        try
        {
            _ = options;
            _ = loggerFactory;

            await using var db = new ejemploDbContext(options);
            await using var unitOfWork = new UnitOfWork(db, loggerFactory);

            // TODO: No seed generated for AtributosSkin; manual initialization required: New is missing or unsupported; use the initializeDataMethod protected region.

            // TODO: No seed generated for Moneda; manual initialization required: New is missing or unsupported; use the initializeDataMethod protected region.

            var articuloCatalogoService = new ArticuloCatalogoService(unitOfWork);
            var sampleArticuloCatalogo = await articuloCatalogoService.NewAsync("Sample ArticuloCatalogo Nombre", "Sample ArticuloCatalogo Coleccion", Rareza.Extraordinario, new Moneda { Valor = 8.5, Divisa = "Sample Moneda Divisa" }, true, 8.5, new DateTime(2026, 1, 8), string.Empty);
            var sampleArticuloCatalogoReadBack = await articuloCatalogoService.ReadOIDAsync(sampleArticuloCatalogo.Id);
            Ensure(sampleArticuloCatalogoReadBack is not null, "ArticuloCatalogo read by OID returns the generated entity.");
            var sampleArticuloCatalogoReadAll = await articuloCatalogoService.ReadAllAsync();
            Ensure(sampleArticuloCatalogoReadAll.Count > 0, "ArticuloCatalogo read all returns generated data.");
            await articuloCatalogoService.ModifyAsync(sampleArticuloCatalogo.Id, "Sample ArticuloCatalogo Nombre", "Sample ArticuloCatalogo Coleccion", Rareza.Extraordinario, new Moneda { Valor = 8.5, Divisa = "Sample Moneda Divisa" }, true, 8.5, new DateTime(2026, 1, 8), string.Empty);
            Ensure(await articuloCatalogoService.ReadOIDArticuloCatalogoDefaultAsync(sampleArticuloCatalogo.Id) is not null, "ArticuloCatalogo modify keeps the entity readable.");

            var configuracionSistemaService = new ConfiguracionSistemaService(unitOfWork);
            var sampleConfiguracionSistema = await configuracionSistemaService.NewAsync(16.5, 16.5, 16.5, 16.5, 16.5, 16.5, 16.5);
            var sampleConfiguracionSistemaReadBack = await configuracionSistemaService.ReadOIDAsync(sampleConfiguracionSistema.Id);
            Ensure(sampleConfiguracionSistemaReadBack is not null, "ConfiguracionSistema read by OID returns the generated entity.");
            var sampleConfiguracionSistemaReadAll = await configuracionSistemaService.ReadAllAsync();
            Ensure(sampleConfiguracionSistemaReadAll.Count > 0, "ConfiguracionSistema read all returns generated data.");
            await configuracionSistemaService.ModifyAsync(sampleConfiguracionSistema.Id, 16.5, 16.5, 16.5, 16.5, 16.5, 16.5, 16.5);
            Ensure(await configuracionSistemaService.ReadOIDConfiguracionSistemaDefaultAsync(sampleConfiguracionSistema.Id) is not null, "ConfiguracionSistema modify keeps the entity readable.");

            var usuarioService = new UsuarioService(unitOfWork);
            var sampleUsuario = await usuarioService.NewAsync("Sample Usuario NombreUsuario", "Sample Usuario Correo", "Password123!", new DateTime(2026, 1, 3), EstadoCuenta.Activo, 3, 3, new DateTime(2026, 1, 3), true, "Sample Usuario Idioma", true, new Monedero { SaldoDisponible = new Moneda { Valor = 5.5, Divisa = "Sample Moneda Divisa" }, SaldoPendiente = new Moneda { Valor = 5.5, Divisa = "Sample Moneda Divisa" } }, new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 7.5, Divisa = "Sample Moneda Divisa" }, EsPublico = true });
            var sampleUsuarioLoginToken = await usuarioService.LoginAsync(sampleUsuario.Id, "Password123!");
            Ensure(!string.IsNullOrWhiteSpace(sampleUsuarioLoginToken), "Usuario.login returns a login token for sampleUsuario.");
            var sampleMonedero = sampleUsuario.Monedero;
            var sampleInventarioPrivado = sampleUsuario.Inventario;
            Ensure(sampleUsuario.Monedero?.Equals(sampleMonedero) == true, "Usuario.new relates sampleUsuario and sampleMonedero.");
            Ensure(sampleUsuario.Inventario?.Equals(sampleInventarioPrivado) == true, "Usuario.new relates sampleUsuario and sampleInventarioPrivado.");
            Ensure(sampleMonedero.Usuario?.Equals(sampleUsuario) == true, "Usuario.new relates sampleMonedero and sampleUsuario.");
            Ensure(sampleInventarioPrivado.Usuario?.Equals(sampleUsuario) == true, "Usuario.new relates sampleInventarioPrivado and sampleUsuario.");
            var sampleUsuarioReadBack = await usuarioService.ReadOIDAsync(sampleUsuario.Id);
            Ensure(sampleUsuarioReadBack is not null, "Usuario read by OID returns the generated entity.");
            var sampleUsuarioReadAll = await usuarioService.ReadAllAsync();
            Ensure(sampleUsuarioReadAll.Count > 0, "Usuario read all returns generated data.");
            var sampleUsuarioFiltrarUsuariosPorEstadoResult = await usuarioService.FiltrarUsuariosPorEstadoAsync(EstadoCuenta.Activo);
            Ensure(sampleUsuarioFiltrarUsuariosPorEstadoResult is not null, "Usuario.filtrarUsuariosPorEstado filter executes.");
            await usuarioService.ModifyAsync(sampleUsuario.Id, "Sample Usuario NombreUsuario", "Sample Usuario Correo", "Password123!", new DateTime(2026, 1, 3), EstadoCuenta.Activo, 3, 3, new DateTime(2026, 1, 3), true, "Sample Usuario Idioma", true, default!, default!);
            Ensure(await usuarioService.ReadOIDUsuarioDefaultAsync(sampleUsuario.Id) is not null, "Usuario modify keeps the entity readable.");

            var administradorService = new AdministradorService(unitOfWork);
            var sampleAdministrador = await administradorService.NewAsync("Sample Administrador NombreUsuario", "Sample Administrador Correo", "Password123!", new DateTime(2026, 1, 4), EstadoCuenta.Activo, 4, 4, new DateTime(2026, 1, 4), true, "Sample Administrador Idioma", true, new Monedero { SaldoDisponible = new Moneda { Valor = 5.5, Divisa = "Sample Moneda Divisa" }, SaldoPendiente = new Moneda { Valor = 5.5, Divisa = "Sample Moneda Divisa" } }, new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 7.5, Divisa = "Sample Moneda Divisa" }, EsPublico = true }, "Sample Administrador CodigoAuditor");
            Ensure(!sampleAdministrador.Monedero.Equals(sampleMonedero), "Administrador.new creates its own Monedero.");
            Ensure(!sampleAdministrador.Inventario.Equals(sampleInventarioPrivado), "Administrador.new creates its own Inventario.");
            Ensure(sampleAdministrador.Monedero.Usuario?.Equals(sampleAdministrador) == true, "Administrador.new relates sampleAdministrador.Monedero and sampleAdministrador.");
            Ensure(sampleAdministrador.Inventario.Usuario?.Equals(sampleAdministrador) == true, "Administrador.new relates sampleAdministrador.Inventario and sampleAdministrador.");
            var sampleAdministradorReadBack = await administradorService.ReadOIDAsync(sampleAdministrador.Id);
            Ensure(sampleAdministradorReadBack is not null, "Administrador read by OID returns the generated entity.");
            var sampleAdministradorReadAll = await administradorService.ReadAllAsync();
            Ensure(sampleAdministradorReadAll.Count > 0, "Administrador read all returns generated data.");
            await administradorService.ModifyAsync(sampleAdministrador.Id, "Sample Administrador NombreUsuario", "Sample Administrador Correo", "Password123!", new DateTime(2026, 1, 4), EstadoCuenta.Activo, 4, 4, new DateTime(2026, 1, 4), true, "Sample Administrador Idioma", true, default!, default!, "Sample Administrador CodigoAuditor");
            Ensure(await administradorService.ReadOIDAdministradorDefaultAsync(sampleAdministrador.Id) is not null, "Administrador modify keeps the entity readable.");

            var transaccionService = new TransaccionService(unitOfWork);
            var sampleTransaccion = await transaccionService.NewAsync(TipoTransaccion.Deposito, new Moneda { Valor = 6.5, Divisa = "Sample Moneda Divisa" }, new DateTime(2026, 1, 6), "Sample Transaccion Concepto", sampleMonedero.Id);
            Ensure(sampleTransaccion.Monedero?.Equals(sampleMonedero) == true, "Transaccion.new relates sampleTransaccion and sampleMonedero.");
            var sampleTransaccionReadBack = await transaccionService.ReadOIDAsync(sampleTransaccion.Id);
            Ensure(sampleTransaccionReadBack is not null, "Transaccion read by OID returns the generated entity.");
            var sampleTransaccionReadAll = await transaccionService.ReadAllAsync();
            Ensure(sampleTransaccionReadAll.Count > 0, "Transaccion read all returns generated data.");
            var sampleTransaccionFiltrarTransaccionesPorTipoResult = await transaccionService.FiltrarTransaccionesPorTipoAsync(sampleUsuario.Id, TipoTransaccion.Deposito);
            Ensure(sampleTransaccionFiltrarTransaccionesPorTipoResult is not null, "Transaccion.filtrarTransaccionesPorTipo filter executes.");
            var sampleTransaccionFiltrarTransaccionesPorRangoFechasResult = await transaccionService.FiltrarTransaccionesPorRangoFechasAsync(sampleUsuario.Id, new DateTime(2026, 1, 6), new DateTime(2026, 1, 6));
            Ensure(sampleTransaccionFiltrarTransaccionesPorRangoFechasResult is not null, "Transaccion.filtrarTransaccionesPorRangoFechas filter executes.");
            await transaccionService.ModifyAsync(sampleTransaccion.Id, TipoTransaccion.Deposito, new Moneda { Valor = 6.5, Divisa = "Sample Moneda Divisa" }, new DateTime(2026, 1, 6), "Sample Transaccion Concepto");
            Ensure(await transaccionService.ReadOIDTransaccionDefaultAsync(sampleTransaccion.Id) is not null, "Transaccion modify keeps the entity readable.");

            var activoDigitalService = new ActivoDigitalService(unitOfWork);
            var sampleActivoDigital = await activoDigitalService.NewAsync("Sample ActivoDigital Nombre", "Sample ActivoDigital Coleccion", new Moneda { Valor = 9.5, Divisa = "Sample Moneda Divisa" }, new AtributosSkin { Rareza = Rareza.Extraordinario, Condicion = Condicion.RecienFabricado, FloatVal = 9.5, Patron = 9 }, true, "Sample ActivoDigital IdentificadorUnico", string.Empty, sampleInventarioPrivado.Id);
            Ensure(sampleActivoDigital.Inventario?.Equals(sampleInventarioPrivado) == true, "ActivoDigital.new relates sampleActivoDigital and sampleInventarioPrivado.");
            var sampleActivoDigitalReadBack = await activoDigitalService.ReadOIDAsync(sampleActivoDigital.Id);
            Ensure(sampleActivoDigitalReadBack is not null, "ActivoDigital read by OID returns the generated entity.");
            var sampleActivoDigitalReadAll = await activoDigitalService.ReadAllAsync();
            Ensure(sampleActivoDigitalReadAll.Count > 0, "ActivoDigital read all returns generated data.");
            var sampleActivoDigitalFiltrarActivosPorRarezaResult = await activoDigitalService.FiltrarActivosPorRarezaAsync(Rareza.Extraordinario);
            Ensure(sampleActivoDigitalFiltrarActivosPorRarezaResult is not null, "ActivoDigital.filtrarActivosPorRareza filter executes.");
            var sampleActivoDigitalFiltrarActivosPorColeccionResult = await activoDigitalService.FiltrarActivosPorColeccionAsync("Sample ActivoDigital Coleccion");
            Ensure(sampleActivoDigitalFiltrarActivosPorColeccionResult is not null, "ActivoDigital.filtrarActivosPorColeccion filter executes.");
            var sampleActivoDigitalFiltrarActivosPorCondicionResult = await activoDigitalService.FiltrarActivosPorCondicionAsync(Condicion.RecienFabricado);
            Ensure(sampleActivoDigitalFiltrarActivosPorCondicionResult is not null, "ActivoDigital.filtrarActivosPorCondicion filter executes.");
            var sampleActivoDigitalFiltrarActivosEnVentaResult = await activoDigitalService.FiltrarActivosEnVentaAsync(sampleUsuario.Id, true);
            Ensure(sampleActivoDigitalFiltrarActivosEnVentaResult is not null, "ActivoDigital.filtrarActivosEnVenta filter executes.");
            await activoDigitalService.ModifyAsync(sampleActivoDigital.Id, "Sample ActivoDigital Nombre", "Sample ActivoDigital Coleccion", new Moneda { Valor = 9.5, Divisa = "Sample Moneda Divisa" }, new AtributosSkin { Rareza = Rareza.Extraordinario, Condicion = Condicion.RecienFabricado, FloatVal = 9.5, Patron = 9 }, true, "Sample ActivoDigital IdentificadorUnico", string.Empty);
            Ensure(await activoDigitalService.ReadOIDActivoDigitalDefaultAsync(sampleActivoDigital.Id) is not null, "ActivoDigital modify keeps the entity readable.");

            var favoritoService = new FavoritoService(unitOfWork);
            var sampleFavorito = await favoritoService.NewAsync(new DateTime(2026, 1, 10), sampleUsuario.Id, sampleActivoDigital.Id);
            Ensure(sampleFavorito.Usuario?.Equals(sampleUsuario) == true, "Favorito.new relates sampleFavorito and sampleUsuario.");
            Ensure(sampleFavorito.ActivoDigital?.Equals(sampleActivoDigital) == true, "Favorito.new relates sampleFavorito and sampleActivoDigital.");
            var sampleFavoritoReadBack = await favoritoService.ReadOIDAsync(sampleFavorito.Id);
            Ensure(sampleFavoritoReadBack is not null, "Favorito read by OID returns the generated entity.");
            var sampleFavoritoReadAll = await favoritoService.ReadAllAsync();
            Ensure(sampleFavoritoReadAll.Count > 0, "Favorito read all returns generated data.");
            await favoritoService.ModifyAsync(sampleFavorito.Id, new DateTime(2026, 1, 10));
            Ensure(await favoritoService.ReadOIDFavoritoDefaultAsync(sampleFavorito.Id) is not null, "Favorito modify keeps the entity readable.");

            var intercambioService = new IntercambioService(unitOfWork);
            var sampleIntercambio = await intercambioService.NewAsync(EstadoIntercambio.Pendiente, new DateTime(2026, 1, 11), sampleUsuario.Id, sampleUsuario.Id);
            Ensure(sampleIntercambio.Proponente?.Equals(sampleUsuario) == true, "Intercambio.new relates sampleIntercambio and sampleUsuario.");
            Ensure(sampleIntercambio.Receptor?.Equals(sampleUsuario) == true, "Intercambio.new relates sampleIntercambio and sampleUsuario.");
            var sampleIntercambioReadBack = await intercambioService.ReadOIDAsync(sampleIntercambio.Id);
            Ensure(sampleIntercambioReadBack is not null, "Intercambio read by OID returns the generated entity.");
            var sampleIntercambioReadAll = await intercambioService.ReadAllAsync();
            Ensure(sampleIntercambioReadAll.Count > 0, "Intercambio read all returns generated data.");
            await intercambioService.ModifyAsync(sampleIntercambio.Id, EstadoIntercambio.Pendiente, new DateTime(2026, 1, 11));
            Ensure(await intercambioService.ReadOIDIntercambioDefaultAsync(sampleIntercambio.Id) is not null, "Intercambio modify keeps the entity readable.");

            var ventaService = new VentaService(unitOfWork);
            var sampleVenta = await ventaService.NewAsync(new Moneda { Valor = 12.5, Divisa = "Sample Moneda Divisa" }, new DateTime(2026, 1, 12), EstadoVenta.Disponible, sampleUsuario.Id, sampleUsuario.Id, sampleActivoDigital.Id, sampleTransaccion.Id, sampleTransaccion.Id);
            Ensure(sampleVenta.Vendedor?.Equals(sampleUsuario) == true, "Venta.new relates sampleVenta and sampleUsuario.");
            Ensure(sampleVenta.ActivoDigital?.Equals(sampleActivoDigital) == true, "Venta.new relates sampleVenta and sampleActivoDigital.");
            var sampleVentaReadBack = await ventaService.ReadOIDAsync(sampleVenta.Id);
            Ensure(sampleVentaReadBack is not null, "Venta read by OID returns the generated entity.");
            var sampleVentaReadAll = await ventaService.ReadAllAsync();
            Ensure(sampleVentaReadAll.Count > 0, "Venta read all returns generated data.");
            var sampleVentaFiltrarVentasPorEstadoResult = await ventaService.FiltrarVentasPorEstadoAsync(EstadoVenta.Disponible);
            Ensure(sampleVentaFiltrarVentasPorEstadoResult is not null, "Venta.filtrarVentasPorEstado filter executes.");
            var sampleVentaFiltrarVentasPorPrecioMaximoResult = await ventaService.FiltrarVentasPorPrecioMaximoAsync(12.5);
            Ensure(sampleVentaFiltrarVentasPorPrecioMaximoResult is not null, "Venta.filtrarVentasPorPrecioMaximo filter executes.");
            await ventaService.ModifyAsync(sampleVenta.Id, new Moneda { Valor = 12.5, Divisa = "Sample Moneda Divisa" }, new DateTime(2026, 1, 12), EstadoVenta.Disponible, sampleActivoDigital.Id);
            Ensure(await ventaService.ReadOIDVentaDefaultAsync(sampleVenta.Id) is not null, "Venta modify keeps the entity readable.");

            var resenaVendedorService = new ResenaVendedorService(unitOfWork);
            var sampleResenaVendedor = await resenaVendedorService.NewAsync(13, "Sample ResenaVendedor Comentario", new DateTime(2026, 1, 13), true, sampleUsuario.Id, sampleUsuario.Id);
            Ensure(sampleResenaVendedor.Comprador?.Equals(sampleUsuario) == true, "ResenaVendedor.new relates sampleResenaVendedor and sampleUsuario.");
            Ensure(sampleResenaVendedor.Vendedor?.Equals(sampleUsuario) == true, "ResenaVendedor.new relates sampleResenaVendedor and sampleUsuario.");
            var sampleResenaVendedorReadBack = await resenaVendedorService.ReadOIDAsync(sampleResenaVendedor.Id);
            Ensure(sampleResenaVendedorReadBack is not null, "ResenaVendedor read by OID returns the generated entity.");
            var sampleResenaVendedorReadAll = await resenaVendedorService.ReadAllAsync();
            Ensure(sampleResenaVendedorReadAll.Count > 0, "ResenaVendedor read all returns generated data.");
            var sampleResenaVendedorFiltrarResenasVisiblesPorVendedorResult = await resenaVendedorService.FiltrarResenasVisiblesPorVendedorAsync(13);
            Ensure(sampleResenaVendedorFiltrarResenasVisiblesPorVendedorResult is not null, "ResenaVendedor.filtrarResenasVisiblesPorVendedor filter executes.");
            await resenaVendedorService.ModifyAsync(sampleResenaVendedor.Id, 13, "Sample ResenaVendedor Comentario", new DateTime(2026, 1, 13), true);
            Ensure(await resenaVendedorService.ReadOIDResenaVendedorDefaultAsync(sampleResenaVendedor.Id) is not null, "ResenaVendedor modify keeps the entity readable.");

            var cajaMisteriosaService = new CajaMisteriosaService(unitOfWork);
            var sampleCajaMisteriosa = await cajaMisteriosaService.NewAsync("Sample CajaMisteriosa Nombre", new Moneda { Valor = 14.5, Divisa = "Sample Moneda Divisa" }, true, new List<ProbabilidadDrop> { new ProbabilidadDrop { PorcentajeDrop = 15.5, ActivoRecompensa = sampleActivoDigital } });
            var sampleProbabilidadDrop = sampleCajaMisteriosa.Probabilidades.First();
            Ensure(sampleCajaMisteriosa.Probabilidades.Contains(sampleProbabilidadDrop), "CajaMisteriosa.new relates sampleCajaMisteriosa and sampleProbabilidadDrop.");
            Ensure(sampleProbabilidadDrop.Caja?.Equals(sampleCajaMisteriosa) == true, "CajaMisteriosa.new relates sampleProbabilidadDrop and sampleCajaMisteriosa.");
            Ensure(sampleProbabilidadDrop.ActivoRecompensa?.Equals(sampleActivoDigital) == true, "CajaMisteriosa.new relates sampleProbabilidadDrop and sampleActivoDigital.");
            var sampleCajaMisteriosaReadBack = await cajaMisteriosaService.ReadOIDAsync(sampleCajaMisteriosa.Id);
            Ensure(sampleCajaMisteriosaReadBack is not null, "CajaMisteriosa read by OID returns the generated entity.");
            var sampleCajaMisteriosaReadAll = await cajaMisteriosaService.ReadAllAsync();
            Ensure(sampleCajaMisteriosaReadAll.Count > 0, "CajaMisteriosa read all returns generated data.");
            var sampleCajaMisteriosaFiltrarCajasActivasResult = await cajaMisteriosaService.FiltrarCajasActivasAsync();
            Ensure(sampleCajaMisteriosaFiltrarCajasActivasResult is not null, "CajaMisteriosa.filtrarCajasActivas filter executes.");
            var sampleCajaMisteriosaFiltrarCajasPorPrecioAperturaResult = await cajaMisteriosaService.FiltrarCajasPorPrecioAperturaAsync(14.5, 14.5);
            Ensure(sampleCajaMisteriosaFiltrarCajasPorPrecioAperturaResult is not null, "CajaMisteriosa.filtrarCajasPorPrecioApertura filter executes.");
            await cajaMisteriosaService.ModifyAsync(sampleCajaMisteriosa.Id, "Sample CajaMisteriosa Nombre", new Moneda { Valor = 14.5, Divisa = "Sample Moneda Divisa" }, true, new List<ProbabilidadDrop> { default! });
            Ensure(await cajaMisteriosaService.ReadOIDCajaMisteriosaDefaultAsync(sampleCajaMisteriosa.Id) is not null, "CajaMisteriosa modify keeps the entity readable.");

            var registroAuditoriaService = new RegistroAuditoriaService(unitOfWork);
            var sampleRegistroAuditoria = await registroAuditoriaService.NewAsync("Sample RegistroAuditoria Accion", new DateTime(2026, 1, 17), sampleAdministrador.Id);
            Ensure(sampleRegistroAuditoria.Administrador?.Equals(sampleAdministrador) == true, "RegistroAuditoria.new relates sampleRegistroAuditoria and sampleAdministrador.");
            var sampleRegistroAuditoriaReadBack = await registroAuditoriaService.ReadOIDAsync(sampleRegistroAuditoria.Id);
            Ensure(sampleRegistroAuditoriaReadBack is not null, "RegistroAuditoria read by OID returns the generated entity.");
            var sampleRegistroAuditoriaReadAll = await registroAuditoriaService.ReadAllAsync();
            Ensure(sampleRegistroAuditoriaReadAll.Count > 0, "RegistroAuditoria read all returns generated data.");
            await registroAuditoriaService.ModifierRegistroAuditoriaDefaultAsync(sampleRegistroAuditoria);
            Ensure(await registroAuditoriaService.ReadOIDRegistroAuditoriaDefaultAsync(sampleRegistroAuditoria.Id) is not null, "RegistroAuditoria modify keeps the entity readable.");

            await articuloCatalogoService.DestroyAsync(sampleArticuloCatalogo.Id);
            await configuracionSistemaService.DestroyAsync(sampleConfiguracionSistema.Id);
            await favoritoService.DestroyAsync(sampleFavorito.Id);
            await ventaService.DestroyAsync(sampleVenta.Id);
            await transaccionService.DestroyAsync(sampleTransaccion.Id);
            await resenaVendedorService.DestroyAsync(sampleResenaVendedor.Id);
            await cajaMisteriosaService.DestroyAsync(sampleCajaMisteriosa.Id);
            await activoDigitalService.DestroyAsync(sampleActivoDigital.Id);
            await intercambioService.DestroyAsync(sampleIntercambio.Id);
            await registroAuditoriaService.DestroyAsync(sampleRegistroAuditoria.Id);
            await administradorService.DestroyAsync(sampleAdministrador.Id);
            await usuarioService.DestroyAsync(sampleUsuario.Id);

            Ensure(await articuloCatalogoService.ReadOIDArticuloCatalogoDefaultAsync(sampleArticuloCatalogo.Id) is null, "ArticuloCatalogo seed cleanup completed.");
            Ensure(await configuracionSistemaService.ReadOIDConfiguracionSistemaDefaultAsync(sampleConfiguracionSistema.Id) is null, "ConfiguracionSistema seed cleanup completed.");
            Ensure(await usuarioService.ReadOIDUsuarioDefaultAsync(sampleUsuario.Id) is null, "Usuario seed cleanup completed.");
            Ensure(await administradorService.ReadOIDAdministradorDefaultAsync(sampleAdministrador.Id) is null, "Administrador seed cleanup completed.");
            Ensure(await transaccionService.ReadOIDTransaccionDefaultAsync(sampleTransaccion.Id) is null, "Transaccion seed cleanup completed.");
            Ensure(await activoDigitalService.ReadOIDActivoDigitalDefaultAsync(sampleActivoDigital.Id) is null, "ActivoDigital seed cleanup completed.");
            Ensure(await favoritoService.ReadOIDFavoritoDefaultAsync(sampleFavorito.Id) is null, "Favorito seed cleanup completed.");
            Ensure(await intercambioService.ReadOIDIntercambioDefaultAsync(sampleIntercambio.Id) is null, "Intercambio seed cleanup completed.");
            Ensure(await ventaService.ReadOIDVentaDefaultAsync(sampleVenta.Id) is null, "Venta seed cleanup completed.");
            Ensure(await resenaVendedorService.ReadOIDResenaVendedorDefaultAsync(sampleResenaVendedor.Id) is null, "ResenaVendedor seed cleanup completed.");
            Ensure(await cajaMisteriosaService.ReadOIDCajaMisteriosaDefaultAsync(sampleCajaMisteriosa.Id) is null, "CajaMisteriosa seed cleanup completed.");
            Ensure(await registroAuditoriaService.ReadOIDRegistroAuditoriaDefaultAsync(sampleRegistroAuditoria.Id) is null, "RegistroAuditoria seed cleanup completed.");
            /*PROTECTED REGION ID(initializeDataMethod) ENABLED START*/
            // ============================================================
            // DATOS DE DEMOSTRACIÓN (miembro 4)
            // Se crean usando las operaciones reales del modelo (depositar,
            // publicar, comprar, abrir caja, intercambiar, moderar...), cada
            // paso con su propia UnitOfWork, como si fuera una petición web.
            // Contraseña de todos los usuarios de la demo: Demo1234!
            // Administrador: admin / Admin123! (su id se muestra al final)
            // ============================================================
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Cargando datos de demostración de SkinLink...");
            Console.WriteLine("-------------------------------------------------------");

            static Moneda Eur(double valor) => new Moneda { Valor = valor, Divisa = "EUR" };

            // Misma base de datos pero sin registrador: el FileLoggerProvider generado reescribe
            // logs/ejemplo.json entero con cada mensaje y con cientos de pasos el seed tardaba minutos.
            var cadenaConexion = options.Extensions
                .OfType<Microsoft.EntityFrameworkCore.Infrastructure.RelationalOptionsExtension>()
                .Select(e => e.ConnectionString)
                .First(c => !string.IsNullOrWhiteSpace(c));
            var opcionesDemo = new DbContextOptionsBuilder<ejemploDbContext>().UseSqlite(cadenaConexion).Options;

            async Task<T> Paso<T>(Func<UnitOfWork, Task<T>> accion)
            {
                await using var uow = new UnitOfWork(new ejemploDbContext(opcionesDemo));
                return await accion(uow);
            }

            async Task Hacer(Func<UnitOfWork, Task> accion)
            {
                await using var uow = new UnitOfWork(new ejemploDbContext(opcionesDemo));
                await accion(uow);
            }

            const string ClaveDemo = "Demo1234!";

            // --- Configuración del sistema (comisiones en %, límites de retirada en EUR) ---
            var configuracion = await Paso(u => new ConfiguracionSistemaService(u)
                .NewAsync(50, 500, 5, 4, 3, 10, 2000));

            // --- Catálogo de artículos de referencia ---
            var catalogo = new (string Nombre, string Coleccion, Rareza Rareza, double Precio)[]
            {
                ("AK-47 | Emperatriz", "Spectrum 2", Rareza.Raro, 101.24),
                ("M4A1-S | Printstream", "Operación Fractura", Rareza.Raro, 318.50),
                ("Guantes | Vice", "Guantes Broken Fang", Rareza.Extraordinario, 1048.00),
                ("AWP | Asiimov", "Operación Phoenix", Rareza.Especial, 141.25),
                ("Desert Eagle | Blaze", "Colección Dust", Rareza.Especial, 548.90),
                ("USP-S | Kill Confirmed", "Shadow", Rareza.Raro, 82.10),
                ("M4A4 | Neo-Noir", "Spectrum 2", Rareza.Especial, 28.60),
                ("Karambit | Autotrónico", "Chroma 2", Rareza.Extraordinario, 1124.30),
                ("P250 | Circuito", "Revolution", Rareza.PocoComun, 4.80),
                ("MP9 | Pulso", "Revolution", Rareza.Comun, 1.20),
            };
            int primerArticuloId = 0;
            foreach (var art in catalogo)
            {
                var creado = await Paso(u => new ArticuloCatalogoService(u).NewAsync(
                    art.Nombre, art.Coleccion, art.Rareza, Eur(art.Precio), true, 0, DateTime.UtcNow, string.Empty));
                if (primerArticuloId == 0) primerArticuloId = creado.Id;
            }
            // Descuento temporal del 10 % en el primer artículo durante 15 días.
            await Hacer(u => new ArticuloCatalogoService(u)
                .ActualizarDescuentoArticuloAsync(primerArticuloId, 10, DateTime.UtcNow.AddDays(15)));

            // --- Usuarios ---
            Monedero MonederoVacio() => new Monedero { SaldoDisponible = Eur(0), SaldoPendiente = Eur(0) };
            InventarioPrivado InventarioVacio(bool publico) => new InventarioPrivado { ValorEstimadoTotal = Eur(0), EsPublico = publico };

            var admin = await Paso(u => new AdministradorService(u).NewAsync(
                "admin", "admin@skinlink.gg", "Admin123!", new DateTime(1995, 3, 14), EstadoCuenta.Activo,
                1, 0, DateTime.UtcNow, true, "es", true, MonederoVacio(), InventarioVacio(false), "AUD-0001"));

            Task<Usuario> NuevoUsuario(string nombre, DateTime nacimiento, int experiencia, bool verificado, bool inventarioPublico) =>
                Paso(u => new UsuarioService(u).NewAsync(
                    nombre, $"{nombre.ToLowerInvariant()}@skinlink.gg", ClaveDemo, nacimiento, EstadoCuenta.Activo,
                    experiencia / 100 + 1, experiencia, DateTime.UtcNow.AddDays(-1), true, "es", verificado,
                    MonederoVacio(), InventarioVacio(inventarioPublico)));

            var neoRift = await NuevoUsuario("NeoRift", new DateTime(2000, 6, 12), 420, true, false);
            var novaTrade = await NuevoUsuario("NovaTrade", new DateTime(1997, 11, 2), 860, true, false);
            var pixelWolf = await NuevoUsuario("PixelWolf", new DateTime(2002, 1, 25), 150, true, true);
            var lunaByte = await NuevoUsuario("LunaByte", new DateTime(1999, 8, 9), 230, true, true);
            var zeroLag = await NuevoUsuario("ZeroLag", new DateTime(2003, 4, 30), 60, true, false);
            var xRaptor = await NuevoUsuario("xRaptor", new DateTime(2004, 9, 17), 0, false, false);
            var tramposo = await NuevoUsuario("Tramposo99", new DateTime(2001, 2, 3), 10, false, false);

            // --- Activos en los inventarios ---
            Task<ActivoDigital> NuevoActivo(Usuario dueno, string nombre, string coleccion, double precio,
                Rareza rareza, Condicion condicion, double floatVal, int patron) =>
                Paso(u => new ActivoDigitalService(u).NewAsync(
                    nombre, coleccion, Eur(precio),
                    new AtributosSkin { Rareza = rareza, Condicion = condicion, FloatVal = floatVal, Patron = patron },
                    false, Guid.NewGuid().ToString(), string.Empty, dueno.Inventario.Id));

            // NovaTrade: vendedor con buena reputación
            var akEmperatriz = await NuevoActivo(novaTrade, "AK-47 | Emperatriz", "Spectrum 2", 101.24, Rareza.Raro, Condicion.DesgasteMinimo, 0.078421, 661);
            var awpAsiimov = await NuevoActivo(novaTrade, "AWP | Asiimov", "Operación Phoenix", 141.25, Rareza.Especial, Condicion.AlgoDesgastado, 0.214, 318);
            var deagleBlaze = await NuevoActivo(novaTrade, "Desert Eagle | Blaze", "Colección Dust", 548.90, Rareza.Especial, Condicion.RecienFabricado, 0.011, 77);
            var guantesVice = await NuevoActivo(novaTrade, "Guantes | Vice", "Guantes Broken Fang", 1048.00, Rareza.Extraordinario, Condicion.DesgasteMinimo, 0.092, 412);
            // NeoRift: coleccionista
            var printstream = await NuevoActivo(neoRift, "M4A1-S | Printstream", "Operación Fractura", 318.50, Rareza.Raro, Condicion.RecienFabricado, 0.031, 905);
            var karambit = await NuevoActivo(neoRift, "Karambit | Autotrónico", "Chroma 2", 1124.30, Rareza.Extraordinario, Condicion.DesgasteMinimo, 0.071, 233);
            var uspKill = await NuevoActivo(neoRift, "USP-S | Kill Confirmed", "Shadow", 82.10, Rareza.Raro, Condicion.BastanteDesgastado, 0.402, 154);
            // PixelWolf
            var m4a4NeoNoir = await NuevoActivo(pixelWolf, "M4A4 | Neo-Noir", "Spectrum 2", 28.60, Rareza.Especial, Condicion.AlgoDesgastado, 0.187, 590);
            var mp9Pulso = await NuevoActivo(pixelWolf, "MP9 | Pulso", "Revolution", 1.20, Rareza.Comun, Condicion.RecienFabricado, 0.044, 12);
            // LunaByte
            var p250Circuito = await NuevoActivo(lunaByte, "P250 | Circuito", "Revolution", 4.80, Rareza.PocoComun, Condicion.DesgasteMinimo, 0.120, 731);
            var awpNeoNoir = await NuevoActivo(lunaByte, "AWP | Neo-Noir", "Spectrum 2", 39.90, Rareza.Especial, Condicion.AlgoDesgastado, 0.256, 47);
            // ZeroLag
            var glockFade = await NuevoActivo(zeroLag, "Glock-18 | Fade", "Colección Assault", 1250.00, Rareza.Extraordinario, Condicion.RecienFabricado, 0.009, 1);
            var famasRoll = await NuevoActivo(zeroLag, "FAMAS | Roll Cage", "Wildfire", 3.40, Rareza.PocoComun, Condicion.Deplorable, 0.812, 600);
            // xRaptor (cuenta sin verificar)
            await NuevoActivo(xRaptor, "Nova | Antique", "Phoenix", 2.10, Rareza.Comun, Condicion.BastanteDesgastado, 0.430, 88);

            // --- Activos base de las cajas (en el inventario de la plataforma, el del admin) ---
            var dropKarambit = await NuevoActivo(admin, "Karambit | Autotrónico", "Chroma 2", 1124.30, Rareza.Extraordinario, Condicion.RecienFabricado, 0.05, 1);
            var dropPrintstream = await NuevoActivo(admin, "M4A1-S | Printstream", "Operación Fractura", 318.50, Rareza.Raro, Condicion.RecienFabricado, 0.05, 1);
            var dropAsiimov = await NuevoActivo(admin, "AWP | Asiimov", "Operación Phoenix", 141.25, Rareza.Especial, Condicion.AlgoDesgastado, 0.25, 1);
            var dropCircuito = await NuevoActivo(admin, "P250 | Circuito", "Revolution", 4.80, Rareza.PocoComun, Condicion.DesgasteMinimo, 0.10, 1);
            var dropPulso = await NuevoActivo(admin, "MP9 | Pulso", "Revolution", 1.20, Rareza.Comun, Condicion.RecienFabricado, 0.05, 1);

            // --- Cajas misteriosas con sus probabilidades (suman 100 %) ---
            async Task<CajaMisteriosa> NuevaCaja(string nombre, double precio, bool activa, params (double Porcentaje, ActivoDigital Activo)[] drops)
            {
                return await Paso(async u =>
                {
                    var probabilidades = new List<ProbabilidadDrop>();
                    foreach (var d in drops)
                    {
                        var activoBase = await u.ActivoDigitalRepository.ReadAsync(d.Activo.Id)
                            ?? throw new InvalidOperationException($"Activo base {d.Activo.Id} no encontrado.");
                        probabilidades.Add(new ProbabilidadDrop { PorcentajeDrop = d.Porcentaje, ActivoRecompensa = activoBase });
                    }
                    return await new CajaMisteriosaService(u).NewAsync(nombre, Eur(precio), activa, probabilidades);
                });
            }

            var fuegoIgneo = await NuevaCaja("Fuego Ígneo", 3.90, true,
                (0.26, dropKarambit), (0.64, dropPrintstream), (3.20, dropAsiimov), (15.90, dropCircuito), (80.00, dropPulso));
            await NuevaCaja("Neón Nocturno", 5.50, true,
                (0.50, dropKarambit), (1.50, dropPrintstream), (8.00, dropAsiimov), (30.00, dropCircuito), (60.00, dropPulso));
            await NuevaCaja("Colección 2026", 8.00, true,
                (1.00, dropKarambit), (4.00, dropPrintstream), (15.00, dropAsiimov), (30.00, dropCircuito), (50.00, dropPulso));
            await NuevaCaja("Órbita Cero", 12.40, false,
                (2.00, dropKarambit), (8.00, dropPrintstream), (20.00, dropAsiimov), (70.00, dropCircuito));
            var cajaBienvenida = await NuevaCaja("Caja de bienvenida", 0, true,
                (5.00, dropAsiimov), (25.00, dropCircuito), (70.00, dropPulso));

            // --- Depósitos (pasarela de pago confirmada) ---
            async Task Depositar(Usuario usuario, double importe, string referencia) =>
                await Paso(u => new ejemplo.Application.UseCases.Monedero.MonederoUseCase(u)
                    .DepositarFondosAsync(usuario.Monedero.Id, Eur(importe), referencia, true));

            await Depositar(neoRift, 500, "PAY-DEMO-0001");
            await Depositar(neoRift, 1000, "PAY-DEMO-0002");
            await Depositar(pixelWolf, 800, "PAY-DEMO-0003");
            await Depositar(lunaByte, 400, "PAY-DEMO-0004");
            await Depositar(zeroLag, 300, "PAY-DEMO-0005");
            await Depositar(novaTrade, 150, "PAY-DEMO-0006");
            await Depositar(xRaptor, 50, "PAY-DEMO-0007");

            // --- Ventas publicadas en el mercado ---
            async Task<Venta> Publicar(Usuario vendedor, ActivoDigital activo, double precio)
            {
                var venta = await Paso(u => new VentaService(u).NewAsync(
                    Eur(precio), DateTime.UtcNow, EstadoVenta.Disponible, vendedor.Id, 0, activo.Id, 0, 0));
                await Hacer(u => new VentaService(u).PublicarVentaAsync(venta.Id, activo.Id));
                return venta;
            }

            var ventaEmperatriz = await Publicar(novaTrade, akEmperatriz, 96.40);
            var ventaAsiimov = await Publicar(novaTrade, awpAsiimov, 141.25);
            await Publicar(novaTrade, deagleBlaze, 548.90);
            var ventaGuantes = await Publicar(novaTrade, guantesVice, 1100.00);
            await Publicar(neoRift, printstream, 312.80);
            var ventaNeoNoir = await Publicar(pixelWolf, m4a4NeoNoir, 28.60);
            var ventaGlock = await Publicar(zeroLag, glockFade, 1290.00);

            // Rebaja de precio y una venta retirada del mercado.
            await Hacer(u => new VentaService(u).EditarPrecioAsync(ventaGuantes.Id, Eur(1048.00)));
            await Hacer(u => new VentaService(u).CancelarVentaAsync(ventaGlock.Id));

            // --- Compras completadas (mueven saldos, activos y comisiones) ---
            async Task Comprar(Venta venta, Usuario comprador) =>
                await Hacer(u => new ejemplo.Application.UseCases.Venta.VentaUseCase(u)
                    .ProcesarCompraSkinAsync(venta.Id, comprador.Id));

            await Comprar(ventaEmperatriz, neoRift);
            await Comprar(ventaAsiimov, pixelWolf);
            await Comprar(ventaNeoNoir, lunaByte);

            // --- Retirada de saldo (usuario verificado) ---
            await Paso(u => new ejemplo.Application.UseCases.Monedero.MonederoUseCase(u)
                .RetirarSaldoWalletAsync(neoRift.Monedero.Id, Eur(120), "Visa •••• 4281"));

            // --- Aperturas de cajas y recompensa diaria ---
            async Task AbrirCaja(CajaMisteriosa caja, Usuario usuario) =>
                await Paso(u => new ejemplo.Application.UseCases.CajaMisteriosa.CajaMisteriosaUseCase(u)
                    .AbrirCajaSorpresaAsync(caja.Id, usuario.Id));

            await AbrirCaja(fuegoIgneo, neoRift);
            await AbrirCaja(fuegoIgneo, neoRift);
            await AbrirCaja(fuegoIgneo, zeroLag);
            await Paso(u => new ejemplo.Application.UseCases.Usuario.UsuarioUseCase(u)
                .ReclamarRecompensaDiariaAsync(lunaByte.Id, cajaBienvenida.Id));

            // --- Reseñas de vendedores (nota de 1 a 5) ---
            Task<ResenaVendedor> Resenar(Usuario comprador, Usuario vendedor, int nota, string comentario, int haceDias) =>
                Paso(u => new ResenaVendedorService(u).NewAsync(
                    nota, comentario, DateTime.UtcNow.AddDays(-haceDias), false, comprador.Id, vendedor.Id));

            await Resenar(neoRift, novaTrade, 5, "Entrega rapidísima y el artículo coincidía exactamente con las capturas.", 1);
            await Resenar(pixelWolf, novaTrade, 5, "Todo perfecto. La skin coincide con la descripción.", 2);
            await Resenar(zeroLag, novaTrade, 4, "Buen vendedor, aunque tardó un poco en responder.", 9);
            await Resenar(lunaByte, pixelWolf, 5, "Buena comunicación y proceso sencillo. Volvería a comprar.", 3);
            await Resenar(pixelWolf, neoRift, 5, "Vendedor de confianza. Artículo recibido sin incidencias.", 12);
            await Resenar(lunaByte, neoRift, 4, "Todo correcto.", 20);
            var resenaOfensiva = await Resenar(xRaptor, novaTrade, 1, "ESTAFADORES!!! no compréis aquí", 5);
            var resenaRetirada = await Resenar(zeroLag, pixelWolf, 2, "El precio me pareció alto.", 6);

            // Moderación del administrador (queda en auditoría) y reseña ocultada.
            await Hacer(u => new ResenaVendedorService(u).ModerarResenaAsync(resenaOfensiva.Id, admin.Id));
            await Hacer(u => new ResenaVendedorService(u).OcultarResenaAsync(resenaRetirada.Id));

            // --- Favoritos ---
            await Paso(u => new ActivoDigitalService(u).MarcarComoFavoritoAsync(deagleBlaze.Id, neoRift.Id));
            await Paso(u => new ActivoDigitalService(u).MarcarComoFavoritoAsync(guantesVice.Id, neoRift.Id));
            await Paso(u => new ActivoDigitalService(u).MarcarComoFavoritoAsync(guantesVice.Id, pixelWolf.Id));
            await Paso(u => new ActivoDigitalService(u).MarcarComoFavoritoAsync(karambit.Id, zeroLag.Id));
            await Paso(u => new ActivoDigitalService(u).MarcarComoFavoritoAsync(deagleBlaze.Id, lunaByte.Id));

            // --- Intercambios: uno pendiente, uno aceptado y uno cancelado ---
            Task<Intercambio> Proponer(Usuario proponente, Usuario receptor, ActivoDigital ofrecido, ActivoDigital solicitado) =>
                Paso(u => new ejemplo.Application.UseCases.Intercambio.IntercambioUseCase(u)
                    .IniciarIntercambioAsync(proponente.Id, receptor.Id, new List<int> { ofrecido.Id }, new List<int> { solicitado.Id }));

            await Proponer(neoRift, pixelWolf, uspKill, mp9Pulso);
            var intercambioAceptado = await Proponer(zeroLag, lunaByte, famasRoll, p250Circuito);
            await Hacer(u => new ejemplo.Application.UseCases.Intercambio.IntercambioUseCase(u)
                .AceptarIntercambioAsync(intercambioAceptado.Id, lunaByte.Id));
            var intercambioCancelado = await Proponer(lunaByte, neoRift, awpNeoNoir, karambit);
            await Hacer(u => new ejemplo.Application.UseCases.Intercambio.IntercambioUseCase(u)
                .CancelarIntercambioAsync(intercambioCancelado.Id, lunaByte.Id));

            // --- Inventario público de NovaTrade (escaparate del vendedor) ---
            await Paso(u => new InventarioPrivadoService(u).CambiarVisibilidadInventarioAsync(novaTrade.Inventario.Id));

            // --- Administración: suspensión de una cuenta (queda en auditoría) ---
            await Hacer(u => new AdministradorService(u).ActualizarEstadoKYCAsync(admin.Id, tramposo.Id, EstadoCuenta.Suspendido));

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Credenciales de la demo (el login es con el id):");
            Console.WriteLine($"  Administrador  id={admin.Id,-4} admin      / Admin123!");
            foreach (var demo in new[] { neoRift, novaTrade, pixelWolf, lunaByte, zeroLag, xRaptor, tramposo })
            {
                Console.WriteLine($"  Usuario        id={demo.Id,-4} {demo.NombreUsuario,-10} / {ClaveDemo}");
            }
            Console.WriteLine($"  ConfiguracionSistema id={configuracion.Id}");
            Console.WriteLine("-------------------------------------------------------");
            /*PROTECTED REGION END*/
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException ?? ex);
            throw;
        }
    }

    private static void Ensure(bool condition, string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = condition ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine((condition ? "[OK] " : "[FAIL] ") + message);
        Console.ForegroundColor = previousColor;

        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}