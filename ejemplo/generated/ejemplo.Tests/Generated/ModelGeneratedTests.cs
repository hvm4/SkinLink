using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;
using Xunit.Abstractions;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Tests.Generated;

public sealed class ModelGeneratedTests
{
    private readonly ITestOutputHelper _output;

    public ModelGeneratedTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void EstadoCuenta_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)EstadoCuenta.Activo);
        Assert.Equal(2, (int)EstadoCuenta.Suspendido);
    }

    [Fact]
    public void Rareza_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)Rareza.Extraordinario);
        Assert.Equal(2, (int)Rareza.Raro);
        Assert.Equal(3, (int)Rareza.Especial);
        Assert.Equal(4, (int)Rareza.PocoComun);
        Assert.Equal(5, (int)Rareza.Comun);
    }

    [Fact]
    public void Condicion_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)Condicion.RecienFabricado);
        Assert.Equal(2, (int)Condicion.DesgasteMinimo);
        Assert.Equal(3, (int)Condicion.AlgoDesgastado);
        Assert.Equal(4, (int)Condicion.BastanteDesgastado);
        Assert.Equal(5, (int)Condicion.Deplorable);
    }

    [Fact]
    public void TipoTransaccion_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)TipoTransaccion.Deposito);
        Assert.Equal(2, (int)TipoTransaccion.Retiro);
        Assert.Equal(3, (int)TipoTransaccion.CobroVenta);
        Assert.Equal(4, (int)TipoTransaccion.PagoVenta);
        Assert.Equal(5, (int)TipoTransaccion.CosteCaja);
    }

    [Fact]
    public void EstadoVenta_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)EstadoVenta.Disponible);
        Assert.Equal(2, (int)EstadoVenta.EnProceso);
        Assert.Equal(3, (int)EstadoVenta.Completada);
        Assert.Equal(4, (int)EstadoVenta.Cancelada);
    }

    [Fact]
    public void EstadoIntercambio_Should_Define_Model_Literals()
    {
        Assert.Equal(1, (int)EstadoIntercambio.Pendiente);
        Assert.Equal(2, (int)EstadoIntercambio.Aceptado);
        Assert.Equal(3, (int)EstadoIntercambio.Rechazado);
        Assert.Equal(4, (int)EstadoIntercambio.Cancelado);
    }

    [Fact]
    public void Usuario_FiltrarUsuariosPorEstado_Filter_Contract()
    {
        // <haria-protected id="test:filter:Usuario:filtrarUsuariosPorEstado:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarUsuariosPorEstado";
        var filterExpression = "entity => entity.Estado == p_estado";

        // <haria-protected id="test:filter:Usuario:filtrarUsuariosPorEstado:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Usuario.filtrarUsuariosPorEstado declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Usuario.filtrarUsuariosPorEstado declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Transaccion_FiltrarTransaccionesPorTipo_Filter_Contract()
    {
        // <haria-protected id="test:filter:Transaccion:filtrarTransaccionesPorTipo:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarTransaccionesPorTipo";
        var filterExpression = "entity => entity.Tipo == p_tipo";

        // <haria-protected id="test:filter:Transaccion:filtrarTransaccionesPorTipo:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Transaccion.filtrarTransaccionesPorTipo declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Transaccion.filtrarTransaccionesPorTipo declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Transaccion_FiltrarTransaccionesPorRangoFechas_Filter_Contract()
    {
        // <haria-protected id="test:filter:Transaccion:filtrarTransaccionesPorRangoFechas:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarTransaccionesPorRangoFechas";
        var filterExpression = "entity => entity.Fecha >= p_fechaInicio && entity.Fecha <= p_fechaFin";

        // <haria-protected id="test:filter:Transaccion:filtrarTransaccionesPorRangoFechas:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Transaccion.filtrarTransaccionesPorRangoFechas declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Transaccion.filtrarTransaccionesPorRangoFechas declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void ActivoDigital_FiltrarActivosPorRareza_Filter_Contract()
    {
        // <haria-protected id="test:filter:ActivoDigital:filtrarActivosPorRareza:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarActivosPorRareza";
        var filterExpression = "entity => entity.Atributos != null && entity.Atributos.Rareza == p_rareza";

        // <haria-protected id="test:filter:ActivoDigital:filtrarActivosPorRareza:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "ActivoDigital.filtrarActivosPorRareza declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "ActivoDigital.filtrarActivosPorRareza declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void ActivoDigital_FiltrarActivosPorColeccion_Filter_Contract()
    {
        // <haria-protected id="test:filter:ActivoDigital:filtrarActivosPorColeccion:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarActivosPorColeccion";
        var filterExpression = "entity => entity.Coleccion == p_coleccion";

        // <haria-protected id="test:filter:ActivoDigital:filtrarActivosPorColeccion:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "ActivoDigital.filtrarActivosPorColeccion declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "ActivoDigital.filtrarActivosPorColeccion declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void ActivoDigital_FiltrarActivosPorCondicion_Filter_Contract()
    {
        // <haria-protected id="test:filter:ActivoDigital:filtrarActivosPorCondicion:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarActivosPorCondicion";
        var filterExpression = "entity => entity.Atributos != null && entity.Atributos.condicion == p_condicion";

        // <haria-protected id="test:filter:ActivoDigital:filtrarActivosPorCondicion:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "ActivoDigital.filtrarActivosPorCondicion declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "ActivoDigital.filtrarActivosPorCondicion declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void ActivoDigital_FiltrarActivosEnVenta_Filter_Contract()
    {
        // <haria-protected id="test:filter:ActivoDigital:filtrarActivosEnVenta:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarActivosEnVenta";
        var filterExpression = "entity => entity.Inventario.Usuario.Id == p_usuarioId && entity.EnVenta == p_enVenta";

        // <haria-protected id="test:filter:ActivoDigital:filtrarActivosEnVenta:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "ActivoDigital.filtrarActivosEnVenta declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "ActivoDigital.filtrarActivosEnVenta declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Venta_FiltrarVentasPorEstado_Filter_Contract()
    {
        // <haria-protected id="test:filter:Venta:filtrarVentasPorEstado:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarVentasPorEstado";
        var filterExpression = "entity => entity.EstadoVenta == p_estado";

        // <haria-protected id="test:filter:Venta:filtrarVentasPorEstado:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Venta.filtrarVentasPorEstado declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Venta.filtrarVentasPorEstado declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    public void Venta_FiltrarVentasPorPrecioMaximo_Filter_Contract()
    {
        // <haria-protected id="test:filter:Venta:filtrarVentasPorPrecioMaximo:arrange">
        // extra user setup
        // </haria-protected>

        var operationName = "filtrarVentasPorPrecioMaximo";
        var filterExpression = "entity => (double)entity.PrecioVenta.Valor <= p_precioMaximo";

        // <haria-protected id="test:filter:Venta:filtrarVentasPorPrecioMaximo:assert">
        Ensure(!string.IsNullOrWhiteSpace(operationName), "Venta.filtrarVentasPorPrecioMaximo declares the generated operation name.");
        Ensure(!string.IsNullOrWhiteSpace(filterExpression), "Venta.filtrarVentasPorPrecioMaximo declares the generated filter expression.");
        // </haria-protected>
    }

    [Fact]
    [Trait("Category", "Database")]
    public async Task Generated_Crud_And_Associations_Should_Work_Against_Database()
    {
        var options = CreateDbContextOptions();
        using var loggerFactory = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Warning));
        await using (var schemaDb = new ejemploDbContext(options))
        {
            await schemaDb.Database.EnsureCreatedAsync();
        }

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
            catch (ejemplo.Application.Exceptions.ModelException)
            {
                // Ya eliminado en cascada por el Administrador
            }
            try
            {
                await administradorService.DestroyAsync(sampleAdministrador.Id);
            }
            catch (Exception)
            {
                // Ya eliminado o gestionado por el ciclo de pruebas
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
    }

    private static DbContextOptions<ejemploDbContext> CreateDbContextOptions()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DbConnection")
            ?? throw new InvalidOperationException("No database connection string found for generated database tests. Configure ConnectionStrings:DbConnection or provide InitializeDB/appsettings.json.");
        connectionString = ResolveSqliteConnectionString(connectionString);

        return new DbContextOptionsBuilder<ejemploDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    private static string ResolveSqliteConnectionString(string connectionString)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrWhiteSpace(connectionStringBuilder.DataSource) &&
            !Path.IsPathRooted(connectionStringBuilder.DataSource) &&
            !string.Equals(connectionStringBuilder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase) &&
            !connectionStringBuilder.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            connectionStringBuilder.DataSource = Path.GetFullPath(Path.Combine(GetInitializeDbDirectory(), connectionStringBuilder.DataSource));
        }

        return connectionStringBuilder.ConnectionString;
    }

    private static string GetInitializeDbDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "InitializeDB.csproj")))
            {
                return directory.FullName;
            }

            var candidate = Path.Combine(directory.FullName, "InitializeDB");
            if (File.Exists(Path.Combine(candidate, "InitializeDB.csproj")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }

    private const string SuccessColor = "\u001b[32m";
    private const string ErrorColor = "\u001b[31m";
    private const string ResetColor = "\u001b[0m";

    private void Ensure(bool condition, string message)
    {
        WriteTestMessage(condition, message);
        Assert.True(condition, message);
    }

    private void WriteTestMessage(bool condition, string message)
    {
        var color = condition ? SuccessColor : ErrorColor;
        var status = condition ? "[OK] " : "[FAIL] ";
        _output.WriteLine($"{color}{status}{message}{ResetColor}");
    }

}
