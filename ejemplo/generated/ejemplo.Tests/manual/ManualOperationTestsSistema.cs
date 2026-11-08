using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Application.UseCases.Monedero;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Tests.Manual;

public sealed class ManualOperationTestsSistema
{
    private readonly ITestOutputHelper _output;

    public ManualOperationTestsSistema(ITestOutputHelper output)
    {
        _output = output;
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

    private static (SqliteConnection connection, DbContextOptions<ejemploDbContext> options) CreateInMemorySqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ejemploDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var db = new ejemploDbContext(options))
        {
            db.Database.EnsureCreated();
        }

        return (connection, options);
    }

    private static async Task<Usuario> CrearUsuarioAsync(
        UnitOfWork uow, string nombre, double saldo = 100, bool verificado = true,
        EstadoCuenta estado = EstadoCuenta.Activo, int edad = 25)
    {
        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            Correo = nombre + "@test.com",
            Contrasena = "pwd123",
            FechaNacimiento = DateTime.UtcNow.AddYears(-edad),
            Estado = estado,
            Verificado = verificado,
            Monedero = new Monedero
            {
                SaldoDisponible = new Moneda { Valor = saldo, Divisa = "EUR" },
                SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" }
            },
            Inventario = new InventarioPrivado
            {
                ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = "EUR" },
                EsPublico = true
            },
            Nivel = 1,
            Experiencia = 0
        };
        await uow.UsuarioRepository.SaveAsync(usuario);
        return usuario;
    }

    private static async Task<ActivoDigital> CrearActivoAsync(
        UnitOfWork uow, Usuario propietario, string nombre, string coleccion, double precio)
    {
        var activo = new ActivoDigital
        {
            Nombre = nombre,
            Coleccion = coleccion,
            PrecioEstimado = new Moneda { Valor = precio, Divisa = "EUR" },
            Atributos = new AtributosSkin { Rareza = Rareza.Raro, Condicion = Condicion.RecienFabricado, FloatVal = 0.05, Patron = 7 },
            EnVenta = false,
            IdentificadorUnico = Guid.NewGuid().ToString("N"),
            Inventario = propietario.Inventario
        };
        await uow.ActivoDigitalRepository.SaveAsync(activo);
        return activo;
    }

    // ------------------------------------------------------------------
    // Custom: Usuario.verificarUsuario
    // ------------------------------------------------------------------

    [Fact]
    public async Task Usuario_VerificarUsuario_Debe_Marcar_La_Identidad_Como_Verificada()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = await CrearUsuarioAsync(uow, "UsuarioPorVerificar", verificado: false);
            await uow.CommitAsync();

            var service = new UsuarioService(uow);
            await service.VerificarUsuarioAsync(usuario.Id);

            var actualizado = await uow.UsuarioRepository.ReadAsync(usuario.Id);
            Ensure(actualizado != null, "Usuario encontrado tras verificarlo.");
            Ensure(actualizado!.Verificado, "Identidad del usuario verificada.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Usuario_VerificarUsuario_Debe_Rechazar_Cuentas_Suspendidas_Y_Menores()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var suspendido = await CrearUsuarioAsync(uow, "UsuarioSuspendido", verificado: false, estado: EstadoCuenta.Suspendido);
            var menor = await CrearUsuarioAsync(uow, "UsuarioMenor", verificado: false, edad: 15);
            await uow.CommitAsync();

            var service = new UsuarioService(uow);
            await Assert.ThrowsAsync<ModelException>(() => service.VerificarUsuarioAsync(suspendido.Id));
            await Assert.ThrowsAsync<ModelException>(() => service.VerificarUsuarioAsync(menor.Id));

            Ensure(!suspendido.Verificado, "La cuenta suspendida no se verifica.");
            Ensure(!menor.Verificado, "El menor de edad no se verifica.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ------------------------------------------------------------------
    // Custom: ConfiguracionSistema.calcularComisionMercado
    // ------------------------------------------------------------------

    [Fact]
    public async Task ConfiguracionSistema_CalcularComisionMercado_Debe_Aplicar_La_Comision_Segun_El_Rango()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var configuracion = new ConfiguracionSistema
            {
                LimitePrecioBajo = 10,
                LimitePrecioMedio = 100,
                ComisionPrecioBajo = 5,
                ComisionPrecioMedio = 3,
                ComisionPrecioAlto = 1,
                RetiroMinimo = 5,
                RetiroMaximoDiario = 500
            };
            await uow.ConfiguracionSistemaRepository.SaveAsync(configuracion);
            await uow.CommitAsync();

            var service = new ConfiguracionSistemaService(uow);

            Ensure(await service.CalcularComisionMercadoAsync(configuracion.Id, 4.5) == 5, "Precio bajo: comisión del 5 %.");
            Ensure(await service.CalcularComisionMercadoAsync(configuracion.Id, 10) == 5, "El límite bajo es inclusivo.");
            Ensure(await service.CalcularComisionMercadoAsync(configuracion.Id, 50) == 3, "Precio medio: comisión del 3 %.");
            Ensure(await service.CalcularComisionMercadoAsync(configuracion.Id, 100) == 3, "El límite medio es inclusivo.");
            Ensure(await service.CalcularComisionMercadoAsync(configuracion.Id, 2500) == 1, "Precio alto: comisión del 1 %.");

            await Assert.ThrowsAsync<ModelException>(() => service.CalcularComisionMercadoAsync(configuracion.Id, 0));
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ------------------------------------------------------------------
    // Custom: ActivoDigital.generarReporteActividad
    // ------------------------------------------------------------------

    [Fact]
    public async Task ActivoDigital_GenerarReporteActividad_Debe_Incluir_Las_Metricas_Del_Activo()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var vendedor = await CrearUsuarioAsync(uow, "VendedorReporte");
            var comprador = await CrearUsuarioAsync(uow, "CompradorReporte");
            var activo = await CrearActivoAsync(uow, vendedor, "AK-47 | Redline", "Phoenix", 20);
            await CrearActivoAsync(uow, vendedor, "M4A4 | Howl", "Phoenix", 40);

            await uow.FavoritoRepository.SaveAsync(new Favorito { FechaMarcado = DateTime.UtcNow, Usuario = comprador, ActivoDigital = activo });
            await uow.FavoritoRepository.SaveAsync(new Favorito { FechaMarcado = DateTime.UtcNow, Usuario = vendedor, ActivoDigital = activo });
            await uow.VentaRepository.SaveAsync(new Venta
            {
                PrecioVenta = new Moneda { Valor = 25, Divisa = "EUR" },
                FechaPublicacion = DateTime.UtcNow,
                EstadoVenta = EstadoVenta.Completada,
                Vendedor = vendedor,
                Comprador = comprador,
                ActivoDigital = activo
            });
            await uow.CommitAsync();

            var service = new ActivoDigitalService(uow);
            var reporte = await service.GenerarReporteActividadAsync(activo.Id);
            _output.WriteLine(reporte);

            Ensure(reporte.Contains("AK-47 | Redline") && reporte.Contains("Phoenix"), "El reporte incluye el nombre y la colección.");
            Ensure(reporte.Contains("Completada: 1"), "El reporte cuenta las ventas por estado.");
            Ensure(reporte.Contains("+25.00%"), "El reporte calcula la variación del precio de venta frente al estimado.");
            Ensure(reporte.Contains("favorito: 2"), "El reporte cuenta las veces marcado como favorito.");
            Ensure(reporte.Contains("Activos en la colección: 2"), "El reporte compara con el resto de la colección.");

            await Assert.ThrowsAsync<ModelException>(() => service.GenerarReporteActividadAsync(987654));
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ------------------------------------------------------------------
    // CustomTransaction: Monedero.depositarFondos
    // ------------------------------------------------------------------

    [Fact]
    public async Task Monedero_DepositarFondos_Debe_Acreditar_Saldo_Y_Registrar_La_Transaccion()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = await CrearUsuarioAsync(uow, "UsuarioDeposito", saldo: 100);
            await uow.CommitAsync();

            var useCase = new MonederoUseCase(uow);
            var transaccion = await useCase.DepositarFondosAsync(
                usuario.Monedero.Id, new Moneda { Valor = 50, Divisa = "EUR" }, "PAY-001", true);

            Ensure(usuario.Monedero.SaldoDisponible.Valor == 150, "Saldo del monedero incrementado (100 + 50).");
            Ensure(transaccion.Tipo == TipoTransaccion.Deposito, "Transacción registrada como depósito.");
            Ensure(transaccion.Monto.Valor == 50, "Importe de la transacción correcto.");
            Ensure(transaccion.Monedero.Id == usuario.Monedero.Id, "Transacción asociada al monedero del usuario.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Monedero_DepositarFondos_Debe_Rechazar_Pagos_No_Confirmados_O_Repetidos()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = await CrearUsuarioAsync(uow, "UsuarioDepositoRechazado", saldo: 100);
            await uow.CommitAsync();

            var useCase = new MonederoUseCase(uow);
            int monederoId = usuario.Monedero.Id;

            await useCase.DepositarFondosAsync(monederoId, new Moneda { Valor = 10, Divisa = "EUR" }, "PAY-DUP", true);

            await Assert.ThrowsAsync<ModelException>(() =>
                useCase.DepositarFondosAsync(monederoId, new Moneda { Valor = 5, Divisa = "EUR" }, "PAY-NO", false));
            await Assert.ThrowsAsync<ModelException>(() =>
                useCase.DepositarFondosAsync(monederoId, new Moneda { Valor = 5, Divisa = "EUR" }, "PAY-DUP", true));
            await Assert.ThrowsAsync<ModelException>(() =>
                useCase.DepositarFondosAsync(monederoId, new Moneda { Valor = 0, Divisa = "EUR" }, "PAY-CERO", true));

            Ensure(usuario.Monedero.SaldoDisponible.Valor == 110, "Solo el primer depósito se acredita (100 + 10).");
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ------------------------------------------------------------------
    // CustomTransaction: Monedero.retirarSaldoWallet
    // ------------------------------------------------------------------

    [Fact]
    public async Task Monedero_RetirarSaldoWallet_Debe_Descontar_Saldo_Y_Registrar_La_Transaccion()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = await CrearUsuarioAsync(uow, "UsuarioRetirada", saldo: 500);
            await uow.ConfiguracionSistemaRepository.SaveAsync(new ConfiguracionSistema
            {
                LimitePrecioBajo = 10, LimitePrecioMedio = 100,
                ComisionPrecioBajo = 5, ComisionPrecioMedio = 3, ComisionPrecioAlto = 1,
                RetiroMinimo = 10, RetiroMaximoDiario = 300
            });
            await uow.CommitAsync();

            var useCase = new MonederoUseCase(uow);
            var transaccion = await useCase.RetirarSaldoWalletAsync(
                usuario.Monedero.Id, new Moneda { Valor = 100, Divisa = "EUR" }, "0xABC123");

            Ensure(usuario.Monedero.SaldoDisponible.Valor == 400, "Saldo del monedero decrementado (500 - 100).");
            Ensure(transaccion.Tipo == TipoTransaccion.Retiro, "Transacción registrada como retiro.");
            Ensure(transaccion.Monto.Valor == 100, "Importe de la transacción correcto.");
            Ensure(transaccion.Concepto.Contains("0xABC123"), "La transacción recoge el destino de la retirada.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Monedero_RetirarSaldoWallet_Debe_Respetar_El_Limite_Diario()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = await CrearUsuarioAsync(uow, "UsuarioLimite", saldo: 1000);
            await uow.ConfiguracionSistemaRepository.SaveAsync(new ConfiguracionSistema
            {
                LimitePrecioBajo = 10, LimitePrecioMedio = 100,
                ComisionPrecioBajo = 5, ComisionPrecioMedio = 3, ComisionPrecioAlto = 1,
                RetiroMinimo = 10, RetiroMaximoDiario = 300
            });
            await uow.CommitAsync();

            var useCase = new MonederoUseCase(uow);
            int monederoId = usuario.Monedero.Id;

            await useCase.RetirarSaldoWalletAsync(monederoId, new Moneda { Valor = 100, Divisa = "EUR" }, "0xABC123");
            await Assert.ThrowsAsync<ModelException>(() =>
                useCase.RetirarSaldoWalletAsync(monederoId, new Moneda { Valor = 250, Divisa = "EUR" }, "0xABC123"));   // 100 + 250 > 300
            await useCase.RetirarSaldoWalletAsync(monederoId, new Moneda { Valor = 200, Divisa = "EUR" }, "0xABC123"); // 100 + 200 = 300

            Ensure(usuario.Monedero.SaldoDisponible.Valor == 700, "Solo se descuentan las retiradas aceptadas (1000 - 100 - 200).");
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Monedero_RetirarSaldoWallet_Debe_Rechazar_Usuarios_No_Verificados_O_Sin_Saldo()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var noVerificado = await CrearUsuarioAsync(uow, "UsuarioNoVerificado", saldo: 500, verificado: false);
            var sinSaldo = await CrearUsuarioAsync(uow, "UsuarioSinSaldo", saldo: 20);
            await uow.CommitAsync();

            var useCase = new MonederoUseCase(uow);

            await Assert.ThrowsAsync<ModelException>(() =>
                useCase.RetirarSaldoWalletAsync(noVerificado.Monedero.Id, new Moneda { Valor = 50, Divisa = "EUR" }, "0xABC123"));
            await Assert.ThrowsAsync<ModelException>(() =>
                useCase.RetirarSaldoWalletAsync(sinSaldo.Monedero.Id, new Moneda { Valor = 50, Divisa = "EUR" }, "0xABC123"));

            Ensure(noVerificado.Monedero.SaldoDisponible.Valor == 500, "El usuario no verificado conserva su saldo.");
            Ensure(sinSaldo.Monedero.SaldoDisponible.Valor == 20, "El usuario sin saldo suficiente conserva su saldo.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ------------------------------------------------------------------
    // ReadFilter: CajaMisteriosa.filtrarCajasActivas
    // ------------------------------------------------------------------

    [Fact]
    public async Task CajaMisteriosa_FiltrarCajasActivas_Debe_Devolver_Solo_Las_Cajas_Activas()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var activa = new CajaMisteriosa { Nombre = "Caja Activa", PrecioApertura = new Moneda { Valor = 2, Divisa = "EUR" }, Activa = true };
            var inactiva = new CajaMisteriosa { Nombre = "Caja Inactiva", PrecioApertura = new Moneda { Valor = 2, Divisa = "EUR" }, Activa = false };
            await uow.CajaMisteriosaRepository.SaveAsync(activa);
            await uow.CajaMisteriosaRepository.SaveAsync(inactiva);
            await uow.CommitAsync();

            var service = new CajaMisteriosaService(uow);
            var resultado = await service.FiltrarCajasActivasAsync();

            Ensure(resultado.Any(c => c.Id == activa.Id), "Se incluye la caja activa.");
            Ensure(resultado.All(c => c.Activa), "No se devuelve ninguna caja inactiva.");
            Ensure(resultado.All(c => c.Id != inactiva.Id), "Se excluye la caja inactiva.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ------------------------------------------------------------------
    // ReadFilter: CajaMisteriosa.filtrarCajasPorPrecioApertura
    // ------------------------------------------------------------------

    [Fact]
    public async Task CajaMisteriosa_FiltrarCajasPorPrecioApertura_Debe_Devolver_Las_Cajas_Del_Rango()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var barata = new CajaMisteriosa { Nombre = "Caja Barata", PrecioApertura = new Moneda { Valor = 2, Divisa = "EUR" }, Activa = true };
            var media = new CajaMisteriosa { Nombre = "Caja Media", PrecioApertura = new Moneda { Valor = 5, Divisa = "EUR" }, Activa = true };
            var cara = new CajaMisteriosa { Nombre = "Caja Cara", PrecioApertura = new Moneda { Valor = 20, Divisa = "EUR" }, Activa = true };
            await uow.CajaMisteriosaRepository.SaveAsync(barata);
            await uow.CajaMisteriosaRepository.SaveAsync(media);
            await uow.CajaMisteriosaRepository.SaveAsync(cara);
            await uow.CommitAsync();

            var service = new CajaMisteriosaService(uow);
            var rangoBajo = await service.FiltrarCajasPorPrecioAperturaAsync(0, 5);
            var rangoAlto = await service.FiltrarCajasPorPrecioAperturaAsync(10, 100);
            var rangoVacio = await service.FiltrarCajasPorPrecioAperturaAsync(1000, 2000);

            Ensure(rangoBajo.Count == 2 && rangoBajo.Any(c => c.Id == barata.Id) && rangoBajo.Any(c => c.Id == media.Id),
                "El rango [0, 5] devuelve la caja barata y la media (límites inclusivos).");
            Ensure(rangoAlto.Count == 1 && rangoAlto[0].Id == cara.Id, "El rango [10, 100] devuelve solo la caja cara.");
            Ensure(rangoVacio.Count == 0, "Un rango sin cajas devuelve una lista vacía.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ------------------------------------------------------------------
    // ReadFilter: ResenaVendedor.filtrarResenasVisiblesPorVendedor
    // ------------------------------------------------------------------

    [Fact]
    public async Task ResenaVendedor_FiltrarResenasVisiblesPorVendedor_Debe_Excluir_Las_Ocultas_Y_Las_De_Otros_Vendedores()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var vendedor = await CrearUsuarioAsync(uow, "VendedorResenas");
            var otroVendedor = await CrearUsuarioAsync(uow, "OtroVendedorResenas");
            var comprador = await CrearUsuarioAsync(uow, "CompradorResenas");

            var visible = new ResenaVendedor { Puntuacion = 5, Comentario = "Todo perfecto", Fecha = DateTime.UtcNow, Oculta = false, Comprador = comprador, Vendedor = vendedor };
            var oculta = new ResenaVendedor { Puntuacion = 1, Comentario = "Comentario moderado", Fecha = DateTime.UtcNow, Oculta = true, Comprador = comprador, Vendedor = vendedor };
            var deOtro = new ResenaVendedor { Puntuacion = 4, Comentario = "Reseña de otro vendedor", Fecha = DateTime.UtcNow, Oculta = false, Comprador = comprador, Vendedor = otroVendedor };
            await uow.ResenaVendedorRepository.SaveAsync(visible);
            await uow.ResenaVendedorRepository.SaveAsync(oculta);
            await uow.ResenaVendedorRepository.SaveAsync(deOtro);
            await uow.CommitAsync();

            var service = new ResenaVendedorService(uow);
            var resultado = await service.FiltrarResenasVisiblesPorVendedorAsync(vendedor.Id);

            Ensure(resultado.Count == 1 && resultado[0].Id == visible.Id, "Solo se devuelve la reseña pública del vendedor.");
            Ensure(resultado.All(r => r.Id != oculta.Id), "No se devuelven las reseñas ocultas.");
            Ensure(resultado.All(r => r.Id != deOtro.Id), "No se devuelven las reseñas de otros vendedores.");
        }
        finally
        {
            connection.Dispose();
        }
    }
}
