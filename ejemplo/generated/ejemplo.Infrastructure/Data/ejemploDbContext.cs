using Microsoft.EntityFrameworkCore;
using ejemplo.Domain.Entities;

namespace ejemplo.Infrastructure.Data;

public sealed class ejemploDbContext : DbContext
{
    public ejemploDbContext(DbContextOptions<ejemploDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Administrador> Administradors => Set<Administrador>();
    public DbSet<Monedero> Monederos => Set<Monedero>();
    public DbSet<Transaccion> Transaccions => Set<Transaccion>();
    public DbSet<InventarioPrivado> InventarioPrivados => Set<InventarioPrivado>();
    public DbSet<ArticuloCatalogo> ArticuloCatalogos => Set<ArticuloCatalogo>();
    public DbSet<ActivoDigital> ActivoDigitals => Set<ActivoDigital>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<Intercambio> Intercambios => Set<Intercambio>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<ResenaVendedor> ResenaVendedors => Set<ResenaVendedor>();
    public DbSet<CajaMisteriosa> CajaMisteriosas => Set<CajaMisteriosa>();
    public DbSet<ProbabilidadDrop> ProbabilidadDrops => Set<ProbabilidadDrop>();
    public DbSet<RegistroAuditoria> RegistroAuditorias => Set<RegistroAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.NombreUsuario)
                .HasColumnName("nombreUsuario")
                .IsRequired();
            entity.Property(e => e.Correo)
                .HasColumnName("correo")
                .IsRequired();
            entity.Property(e => e.Contrasena)
                .HasColumnName("contrasena")
                .IsRequired();
            entity.Property(e => e.FechaNacimiento)
                .HasColumnName("fechaNacimiento")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(e => e.Estado)
                .HasColumnName("estado")
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.Nivel)
                .HasColumnName("nivel")
                .IsRequired();
            entity.Property(e => e.Experiencia)
                .HasColumnName("experiencia")
                .IsRequired();
            entity.Property(e => e.FechaUltimoLogin)
                .HasColumnName("fechaUltimoLogin")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(e => e.TemaModoOscuro)
                .HasColumnName("temaModoOscuro")
                .IsRequired();
            entity.Property(e => e.Idioma)
                .HasColumnName("idioma")
                .IsRequired();
        });

        modelBuilder.Entity<Administrador>(entity =>
        {
            entity.ToTable("Administrador");
            entity.HasBaseType<Usuario>();
            entity.Property(e => e.CodigoAuditor)
                .HasColumnName("codigoAuditor")
                .IsRequired();
        });

        modelBuilder.Entity<Monedero>(entity =>
        {
            entity.ToTable("Monedero");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.OwnsOne(e => e.SaldoDisponible, ownedSaldoDisponible =>
            {
                ownedSaldoDisponible.Property(e => e.Valor)
                    .HasColumnName("saldoDisponible_valor")
                    .IsRequired();
                ownedSaldoDisponible.Property(e => e.Divisa)
                    .HasColumnName("saldoDisponible_divisa")
                    .IsRequired();
            });
            entity.OwnsOne(e => e.SaldoPendiente, ownedSaldoPendiente =>
            {
                ownedSaldoPendiente.Property(e => e.Valor)
                    .HasColumnName("saldoPendiente_valor")
                    .IsRequired();
                ownedSaldoPendiente.Property(e => e.Divisa)
                    .HasColumnName("saldoPendiente_divisa")
                    .IsRequired();
            });
        });

        modelBuilder.Entity<Transaccion>(entity =>
        {
            entity.ToTable("Transaccion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Tipo)
                .HasColumnName("tipo")
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(e => e.Concepto)
                .HasColumnName("concepto")
                .IsRequired();
            entity.OwnsOne(e => e.Monto, ownedMonto =>
            {
                ownedMonto.Property(e => e.Valor)
                    .HasColumnName("monto_valor")
                    .IsRequired();
                ownedMonto.Property(e => e.Divisa)
                    .HasColumnName("monto_divisa")
                    .IsRequired();
            });
        });

        modelBuilder.Entity<InventarioPrivado>(entity =>
        {
            entity.ToTable("InventarioPrivado");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.EsPublico)
                .HasColumnName("esPublico")
                .IsRequired();
            entity.OwnsOne(e => e.ValorEstimadoTotal, ownedValorEstimadoTotal =>
            {
                ownedValorEstimadoTotal.Property(e => e.Valor)
                    .HasColumnName("valorEstimadoTotal_valor")
                    .IsRequired();
                ownedValorEstimadoTotal.Property(e => e.Divisa)
                    .HasColumnName("valorEstimadoTotal_divisa")
                    .IsRequired();
            });
        });

        modelBuilder.Entity<ArticuloCatalogo>(entity =>
        {
            entity.ToTable("ArticuloCatalogo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .IsRequired();
            entity.Property(e => e.Coleccion)
                .HasColumnName("coleccion")
                .IsRequired();
            entity.Property(e => e.Rareza)
                .HasColumnName("rareza")
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.Habilitado)
                .HasColumnName("habilitado")
                .IsRequired();
            entity.Property(e => e.PorcentajeDescuento)
                .HasColumnName("porcentajeDescuento")
                .IsRequired(false);
            entity.Property(e => e.FechaFinDescuento)
                .HasColumnName("fechaFinDescuento")
                .HasColumnType("date")
                .IsRequired(false);
            entity.OwnsOne(e => e.PrecioReferencia, ownedPrecioReferencia =>
            {
                ownedPrecioReferencia.Property(e => e.Valor)
                    .HasColumnName("precioReferencia_valor")
                    .IsRequired();
                ownedPrecioReferencia.Property(e => e.Divisa)
                    .HasColumnName("precioReferencia_divisa")
                    .IsRequired();
            });
        });

        modelBuilder.Entity<ActivoDigital>(entity =>
        {
            entity.ToTable("ActivoDigital");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .IsRequired();
            entity.Property(e => e.Coleccion)
                .HasColumnName("coleccion")
                .IsRequired();
            entity.Property(e => e.EnVenta)
                .HasColumnName("enVenta")
                .IsRequired();
            entity.Property(e => e.IdentificadorUnico)
                .HasColumnName("identificadorUnico")
                .IsRequired();
            entity.OwnsOne(e => e.PrecioEstimado, ownedPrecioEstimado =>
            {
                ownedPrecioEstimado.Property(e => e.Valor)
                    .HasColumnName("precioEstimado_valor")
                    .IsRequired();
                ownedPrecioEstimado.Property(e => e.Divisa)
                    .HasColumnName("precioEstimado_divisa")
                    .IsRequired();
            });
            entity.OwnsOne(e => e.Atributos, ownedAtributos =>
            {
                ownedAtributos.Property(e => e.Rareza)
                    .HasColumnName("atributos_rareza")
                    .HasConversion<int>()
                    .IsRequired();
                ownedAtributos.Property(e => e.Condicion)
                    .HasColumnName("atributos_condicion")
                    .HasConversion<int>()
                    .IsRequired();
                ownedAtributos.Property(e => e.FloatVal)
                    .HasColumnName("atributos_floatVal")
                    .IsRequired();
                ownedAtributos.Property(e => e.Patron)
                    .HasColumnName("atributos_patron")
                    .IsRequired();
            });
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.ToTable("Favorito");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.FechaMarcado)
                .HasColumnName("fechaMarcado")
                .HasColumnType("date")
                .IsRequired();
        });

        modelBuilder.Entity<Intercambio>(entity =>
        {
            entity.ToTable("Intercambio");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Estado)
                .HasColumnName("estado")
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.FechaCreacion)
                .HasColumnName("fechaCreacion")
                .HasColumnType("date")
                .IsRequired();
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable("Venta");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.FechaPublicacion)
                .HasColumnName("fechaPublicacion")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(e => e.EstadoVenta)
                .HasColumnName("estadoVenta")
                .HasConversion<int>()
                .IsRequired();
            entity.OwnsOne(e => e.PrecioVenta, ownedPrecioVenta =>
            {
                ownedPrecioVenta.Property(e => e.Valor)
                    .HasColumnName("precioVenta_valor")
                    .IsRequired();
                ownedPrecioVenta.Property(e => e.Divisa)
                    .HasColumnName("precioVenta_divisa")
                    .IsRequired();
            });
        });

        modelBuilder.Entity<ResenaVendedor>(entity =>
        {
            entity.ToTable("ResenaVendedor");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Puntuacion)
                .HasColumnName("puntuacion")
                .IsRequired();
            entity.Property(e => e.Comentario)
                .HasColumnName("comentario")
                .IsRequired();
            entity.Property(e => e.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("date")
                .IsRequired();
            entity.Property(e => e.Oculta)
                .HasColumnName("oculta")
                .IsRequired();
        });

        modelBuilder.Entity<CajaMisteriosa>(entity =>
        {
            entity.ToTable("CajaMisteriosa");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .IsRequired();
            entity.Property(e => e.Activa)
                .HasColumnName("activa")
                .IsRequired();
            entity.OwnsOne(e => e.PrecioApertura, ownedPrecioApertura =>
            {
                ownedPrecioApertura.Property(e => e.Valor)
                    .HasColumnName("precioApertura_valor")
                    .IsRequired();
                ownedPrecioApertura.Property(e => e.Divisa)
                    .HasColumnName("precioApertura_divisa")
                    .IsRequired();
            });
        });

        modelBuilder.Entity<ProbabilidadDrop>(entity =>
        {
            entity.ToTable("ProbabilidadDrop");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.PorcentajeDrop)
                .HasColumnName("porcentajeDrop")
                .IsRequired();
        });

        modelBuilder.Entity<RegistroAuditoria>(entity =>
        {
            entity.ToTable("RegistroAuditoria");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Accion)
                .HasColumnName("accion")
                .IsRequired();
            entity.Property(e => e.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("date")
                .IsRequired();
        });

        modelBuilder.Entity<Usuario>()
            .HasOne(e => e.Monedero)
            .WithOne(e => e.Usuario)
            .HasForeignKey<Usuario>("MonederoId")
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .HasOne(e => e.Inventario)
            .WithOne(e => e.Usuario)
            .HasForeignKey<Usuario>("InventarioId")
            .IsRequired();

        modelBuilder.Entity<InventarioPrivado>()
            .HasMany(e => e.Activos)
            .WithOne(e => e.Inventario)
            .HasForeignKey("InventarioId")
            .IsRequired();

        modelBuilder.Entity<Monedero>()
            .HasMany(e => e.Transacciones)
            .WithOne(e => e.Monedero)
            .HasForeignKey("MonederoId")
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .HasMany(e => e.VentasPublicadas)
            .WithOne(e => e.Vendedor)
            .HasForeignKey("VendedorId")
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .HasMany(e => e.ComprasRealizadas)
            .WithOne(e => e.Comprador)
            .HasForeignKey("CompradorId")
            .IsRequired(false);

        modelBuilder.Entity<Venta>()
            .HasOne(e => e.ActivoDigital)
            .WithOne(e => e.Venta)
            .HasForeignKey<Venta>("ActivoDigitalId")
            .IsRequired();

        modelBuilder.Entity<Venta>()
            .HasOne(e => e.TransaccionCobro)
            .WithOne(e => e.VentaCobro)
            .HasForeignKey<Venta>("TransaccionCobroId")
            .IsRequired(false);

        modelBuilder.Entity<Venta>()
            .HasOne(e => e.TransaccionPago)
            .WithOne(e => e.VentaPago)
            .HasForeignKey<Venta>("TransaccionPagoId")
            .IsRequired(false);

        modelBuilder.Entity<Usuario>()
            .HasMany(e => e.ResenasRealizadas)
            .WithOne(e => e.Comprador)
            .HasForeignKey("CompradorId")
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .HasMany(e => e.ResenasRecibidas)
            .WithOne(e => e.Vendedor)
            .HasForeignKey("VendedorId")
            .IsRequired();

        modelBuilder.Entity<CajaMisteriosa>()
            .HasMany(e => e.Probabilidades)
            .WithOne(e => e.Caja)
            .HasForeignKey("CajaId")
            .IsRequired();

        modelBuilder.Entity<ActivoDigital>()
            .HasMany(e => e.Probabilidad)
            .WithOne(e => e.ActivoRecompensa)
            .HasForeignKey("ActivoRecompensaId")
            .IsRequired();

        modelBuilder.Entity<Administrador>()
            .HasMany(e => e.RegistrosAuditoria)
            .WithOne(e => e.Administrador)
            .HasForeignKey("AdministradorId")
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .HasMany(e => e.Favoritos)
            .WithOne(e => e.Usuario)
            .HasForeignKey("UsuarioId")
            .IsRequired();

        modelBuilder.Entity<ActivoDigital>()
            .HasMany(e => e.FavoritosActivo)
            .WithOne(e => e.ActivoDigital)
            .HasForeignKey("ActivoDigitalId")
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .HasMany(e => e.IntercambiosPropuestos)
            .WithOne(e => e.Proponente)
            .HasForeignKey("ProponenteId")
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .HasMany(e => e.IntercambiosRecibidos)
            .WithOne(e => e.Receptor)
            .HasForeignKey("ReceptorId")
            .IsRequired();

        modelBuilder.Entity<Intercambio>()
            .HasMany(e => e.ActivosOfrecidos)
            .WithOne(e => e.IntercambioOfrecido)
            .HasForeignKey("IntercambioOfrecidoId")
            .IsRequired(false);

        modelBuilder.Entity<Intercambio>()
            .HasMany(e => e.ActivosSolicitados)
            .WithOne(e => e.IntercambioSolicitado)
            .HasForeignKey("IntercambioSolicitadoId")
            .IsRequired(false);

    }
}
