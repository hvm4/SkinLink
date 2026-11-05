using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Services;
using ejemplo.Application.UseCases.CajaMisteriosa;
using ejemplo.Application.UseCases.Intercambio;
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

            // TODO: No seed generated for AtributosSkin; define a non-customized New operation in the service to initialize it.

            // TODO: No seed generated for Moneda; define a non-customized New operation in the service to initialize it.

            var administradorService = new AdministradorService(unitOfWork);
            var sampleAdministrador = await administradorService.NewAsync("Sample Administrador NombreUsuario", "Sample Administrador Correo", "Password123!", new DateTime(2026, 1, 4), EstadoCuenta.Activo, 4, 4, new DateTime(2026, 1, 4), true, "Sample Administrador Idioma", new Monedero { SaldoDisponible = new Moneda { Valor = 5.5, Divisa = "Sample Moneda Divisa" }, SaldoPendiente = new Moneda { Valor = 5.5, Divisa = "Sample Moneda Divisa" } }, new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 7.5, Divisa = "Sample Moneda Divisa" }, EsPublico = true }, "Sample Administrador CodigoAuditor");
            var sampleMonedero = sampleAdministrador.Monedero;
            var sampleInventarioPrivado = sampleAdministrador.Inventario;
            Ensure(sampleAdministrador.Monedero?.Equals(sampleMonedero) == true, "Administrador.new relates sampleAdministrador and sampleMonedero.");
            Ensure(sampleAdministrador.Inventario?.Equals(sampleInventarioPrivado) == true, "Administrador.new relates sampleAdministrador and sampleInventarioPrivado.");
            Ensure(sampleMonedero.Usuario?.Equals(sampleAdministrador) == true, "Administrador.new relates sampleMonedero and sampleAdministrador.");
            Ensure(sampleInventarioPrivado.Usuario?.Equals(sampleAdministrador) == true, "Administrador.new relates sampleInventarioPrivado and sampleAdministrador.");
            var sampleAdministradorReadBack = await administradorService.ReadOIDAsync(sampleAdministrador.Id);
            Ensure(sampleAdministradorReadBack is not null, "Administrador read by OID returns the generated entity.");
            var sampleAdministradorReadAll = await administradorService.ReadAllAsync();
            Ensure(sampleAdministradorReadAll.Count > 0, "Administrador read all returns generated data.");
            await administradorService.ModifyAsync(sampleAdministrador.Id, "Sample Administrador NombreUsuario", "Sample Administrador Correo", "Password123!", new DateTime(2026, 1, 4), EstadoCuenta.Activo, 4, 4, new DateTime(2026, 1, 4), true, "Sample Administrador Idioma", default!, default!, "Sample Administrador CodigoAuditor");
            Ensure(await administradorService.ReadOIDAdministradorDefaultAsync(sampleAdministrador.Id) is not null, "Administrador modify keeps the entity readable.");

            var articuloCatalogoService = new ArticuloCatalogoService(unitOfWork);
            var sampleArticuloCatalogo = await articuloCatalogoService.NewAsync("Sample ArticuloCatalogo Nombre", "Sample ArticuloCatalogo Coleccion", Rareza.Extraordinario, new Moneda { Valor = 8.5, Divisa = "Sample Moneda Divisa" }, true, 8.5, new DateTime(2026, 1, 8));
            var sampleArticuloCatalogoReadBack = await articuloCatalogoService.ReadOIDAsync(sampleArticuloCatalogo.Id);
            Ensure(sampleArticuloCatalogoReadBack is not null, "ArticuloCatalogo read by OID returns the generated entity.");
            var sampleArticuloCatalogoReadAll = await articuloCatalogoService.ReadAllAsync();
            Ensure(sampleArticuloCatalogoReadAll.Count > 0, "ArticuloCatalogo read all returns generated data.");
            await articuloCatalogoService.ModifyAsync(sampleArticuloCatalogo.Id, "Sample ArticuloCatalogo Nombre", "Sample ArticuloCatalogo Coleccion", Rareza.Extraordinario, new Moneda { Valor = 8.5, Divisa = "Sample Moneda Divisa" }, true, 8.5, new DateTime(2026, 1, 8));
            Ensure(await articuloCatalogoService.ReadOIDArticuloCatalogoDefaultAsync(sampleArticuloCatalogo.Id) is not null, "ArticuloCatalogo modify keeps the entity readable.");

            var registroAuditoriaService = new RegistroAuditoriaService(unitOfWork);
            var sampleRegistroAuditoria = await registroAuditoriaService.NewAsync("Sample RegistroAuditoria Accion", new DateTime(2026, 1, 16), sampleAdministrador.Id);
            Ensure(sampleRegistroAuditoria.Administrador?.Equals(sampleAdministrador) == true, "RegistroAuditoria.new relates sampleRegistroAuditoria and sampleAdministrador.");
            var sampleRegistroAuditoriaReadBack = await registroAuditoriaService.ReadOIDAsync(sampleRegistroAuditoria.Id);
            Ensure(sampleRegistroAuditoriaReadBack is not null, "RegistroAuditoria read by OID returns the generated entity.");
            var sampleRegistroAuditoriaReadAll = await registroAuditoriaService.ReadAllAsync();
            Ensure(sampleRegistroAuditoriaReadAll.Count > 0, "RegistroAuditoria read all returns generated data.");
            await registroAuditoriaService.ModifierRegistroAuditoriaDefaultAsync(sampleRegistroAuditoria);
            Ensure(await registroAuditoriaService.ReadOIDRegistroAuditoriaDefaultAsync(sampleRegistroAuditoria.Id) is not null, "RegistroAuditoria modify keeps the entity readable.");

            var usuarioService = new UsuarioService(unitOfWork);
            var sampleUsuario = await usuarioService.NewAsync("Sample Usuario NombreUsuario", "Sample Usuario Correo", "Password123!", new DateTime(2026, 1, 3), EstadoCuenta.Activo, 3, 3, new DateTime(2026, 1, 3), true, "Sample Usuario Idioma", sampleMonedero, sampleInventarioPrivado);
            var sampleUsuarioLoginToken = await usuarioService.LoginAsync(sampleUsuario.Id, "Password123!");
            Ensure(!string.IsNullOrWhiteSpace(sampleUsuarioLoginToken), "Usuario.login returns a login token for sampleUsuario.");
            Ensure(sampleUsuario.Monedero?.Equals(sampleMonedero) == true, "Usuario.new relates sampleUsuario and sampleMonedero.");
            Ensure(sampleUsuario.Inventario?.Equals(sampleInventarioPrivado) == true, "Usuario.new relates sampleUsuario and sampleInventarioPrivado.");
            var sampleUsuarioReadBack = await usuarioService.ReadOIDAsync(sampleUsuario.Id);
            Ensure(sampleUsuarioReadBack is not null, "Usuario read by OID returns the generated entity.");
            var sampleUsuarioReadAll = await usuarioService.ReadAllAsync();
            Ensure(sampleUsuarioReadAll.Count > 0, "Usuario read all returns generated data.");
            var sampleUsuarioFiltrarUsuariosPorEstadoResult = await usuarioService.FiltrarUsuariosPorEstadoAsync(EstadoCuenta.Activo);
            Ensure(sampleUsuarioFiltrarUsuariosPorEstadoResult is not null, "Usuario.filtrarUsuariosPorEstado filter executes.");
            await usuarioService.ModifyAsync(sampleUsuario.Id, "Sample Usuario NombreUsuario", "Sample Usuario Correo", "Password123!", new DateTime(2026, 1, 3), EstadoCuenta.Activo, 3, 3, new DateTime(2026, 1, 3), true, "Sample Usuario Idioma", default!, default!);
            Ensure(await usuarioService.ReadOIDUsuarioDefaultAsync(sampleUsuario.Id) is not null, "Usuario modify keeps the entity readable.");

            var transaccionService = new TransaccionService(unitOfWork);
            var sampleTransaccion = await transaccionService.NewAsync(TipoTransaccion.Deposito, new Moneda { Valor = 6.5, Divisa = "Sample Moneda Divisa" }, new DateTime(2026, 1, 6), "Sample Transaccion Concepto", sampleMonedero.Id);
            Ensure(sampleTransaccion.Monedero?.Equals(sampleMonedero) == true, "Transaccion.new relates sampleTransaccion and sampleMonedero.");
            var sampleTransaccionReadBack = await transaccionService.ReadOIDAsync(sampleTransaccion.Id);
            Ensure(sampleTransaccionReadBack is not null, "Transaccion read by OID returns the generated entity.");
            var sampleTransaccionReadAll = await transaccionService.ReadAllAsync();
            Ensure(sampleTransaccionReadAll.Count > 0, "Transaccion read all returns generated data.");
            var sampleTransaccionFiltrarTransaccionesPorTipoResult = await transaccionService.FiltrarTransaccionesPorTipoAsync(TipoTransaccion.Deposito);
            Ensure(sampleTransaccionFiltrarTransaccionesPorTipoResult is not null, "Transaccion.filtrarTransaccionesPorTipo filter executes.");
            var sampleTransaccionFiltrarTransaccionesPorRangoFechasResult = await transaccionService.FiltrarTransaccionesPorRangoFechasAsync(new DateTime(2026, 1, 6), new DateTime(2026, 1, 6));
            Ensure(sampleTransaccionFiltrarTransaccionesPorRangoFechasResult is not null, "Transaccion.filtrarTransaccionesPorRangoFechas filter executes.");
            await transaccionService.ModifyAsync(sampleTransaccion.Id, TipoTransaccion.Deposito, new Moneda { Valor = 6.5, Divisa = "Sample Moneda Divisa" }, new DateTime(2026, 1, 6), "Sample Transaccion Concepto");
            Ensure(await transaccionService.ReadOIDTransaccionDefaultAsync(sampleTransaccion.Id) is not null, "Transaccion modify keeps the entity readable.");

            var activoDigitalService = new ActivoDigitalService(unitOfWork);
            var sampleActivoDigital = await activoDigitalService.NewAsync("Sample ActivoDigital Nombre", "Sample ActivoDigital Coleccion", new Moneda { Valor = 9.5, Divisa = "Sample Moneda Divisa" }, new AtributosSkin { Rareza = Rareza.Extraordinario, Condicion = Condicion.RecienFabricado, FloatVal = 9.5, Patron = 9 }, true, "Sample ActivoDigital IdentificadorUnico", sampleInventarioPrivado.Id);
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
            await activoDigitalService.ModifyAsync(sampleActivoDigital.Id, "Sample ActivoDigital Nombre", "Sample ActivoDigital Coleccion", new Moneda { Valor = 9.5, Divisa = "Sample Moneda Divisa" }, new AtributosSkin { Rareza = Rareza.Extraordinario, Condicion = Condicion.RecienFabricado, FloatVal = 9.5, Patron = 9 }, true, "Sample ActivoDigital IdentificadorUnico");
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
            await cajaMisteriosaService.ModifyAsync(sampleCajaMisteriosa.Id, "Sample CajaMisteriosa Nombre", new Moneda { Valor = 14.5, Divisa = "Sample Moneda Divisa" }, true, new List<ProbabilidadDrop> { default! });
            Ensure(await cajaMisteriosaService.ReadOIDCajaMisteriosaDefaultAsync(sampleCajaMisteriosa.Id) is not null, "CajaMisteriosa modify keeps the entity readable.");

            await articuloCatalogoService.DestroyAsync(sampleArticuloCatalogo.Id);
            try
            {
                await registroAuditoriaService.DestroyAsync(sampleRegistroAuditoria.Id);
            }
            catch (Exception)
            {
                // Ya eliminado en cascada por el Administrador
            }
            try
            {
                await administradorService.DestroyAsync(sampleAdministrador.Id);
            }
            catch (Exception)
            {
                // Ya gestionado por el ciclo de borrado o herencia con Usuario
            }
            await transaccionService.DestroyAsync(sampleTransaccion.Id);
            await favoritoService.DestroyAsync(sampleFavorito.Id);
            await ventaService.DestroyAsync(sampleVenta.Id);
            await resenaVendedorService.DestroyAsync(sampleResenaVendedor.Id);
            await cajaMisteriosaService.DestroyAsync(sampleCajaMisteriosa.Id);
            await activoDigitalService.DestroyAsync(sampleActivoDigital.Id);
            await intercambioService.DestroyAsync(sampleIntercambio.Id);
            await usuarioService.DestroyAsync(sampleUsuario.Id);

            Ensure(await administradorService.ReadOIDAdministradorDefaultAsync(sampleAdministrador.Id) is null, "Administrador seed cleanup completed.");
            Ensure(await articuloCatalogoService.ReadOIDArticuloCatalogoDefaultAsync(sampleArticuloCatalogo.Id) is null, "ArticuloCatalogo seed cleanup completed.");
            Ensure(await registroAuditoriaService.ReadOIDRegistroAuditoriaDefaultAsync(sampleRegistroAuditoria.Id) is null, "RegistroAuditoria seed cleanup completed.");
            Ensure(await usuarioService.ReadOIDUsuarioDefaultAsync(sampleUsuario.Id) is null, "Usuario seed cleanup completed.");
            Ensure(await transaccionService.ReadOIDTransaccionDefaultAsync(sampleTransaccion.Id) is null, "Transaccion seed cleanup completed.");
            Ensure(await activoDigitalService.ReadOIDActivoDigitalDefaultAsync(sampleActivoDigital.Id) is null, "ActivoDigital seed cleanup completed.");
            Ensure(await favoritoService.ReadOIDFavoritoDefaultAsync(sampleFavorito.Id) is null, "Favorito seed cleanup completed.");
            Ensure(await intercambioService.ReadOIDIntercambioDefaultAsync(sampleIntercambio.Id) is null, "Intercambio seed cleanup completed.");
            Ensure(await ventaService.ReadOIDVentaDefaultAsync(sampleVenta.Id) is null, "Venta seed cleanup completed.");
            Ensure(await resenaVendedorService.ReadOIDResenaVendedorDefaultAsync(sampleResenaVendedor.Id) is null, "ResenaVendedor seed cleanup completed.");
            Ensure(await cajaMisteriosaService.ReadOIDCajaMisteriosaDefaultAsync(sampleCajaMisteriosa.Id) is null, "CajaMisteriosa seed cleanup completed.");
            /*PROTECTED REGION ID(initializeDataMethod) ENABLED START*/
            // Add custom seed data here.
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