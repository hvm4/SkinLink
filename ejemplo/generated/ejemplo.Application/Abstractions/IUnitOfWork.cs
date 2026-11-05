namespace ejemplo.Application.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    IUsuarioRepository UsuarioRepository { get; }
    IAdministradorRepository AdministradorRepository { get; }
    IMonederoRepository MonederoRepository { get; }
    ITransaccionRepository TransaccionRepository { get; }
    IInventarioPrivadoRepository InventarioPrivadoRepository { get; }
    IArticuloCatalogoRepository ArticuloCatalogoRepository { get; }
    IActivoDigitalRepository ActivoDigitalRepository { get; }
    IFavoritoRepository FavoritoRepository { get; }
    IIntercambioRepository IntercambioRepository { get; }
    IVentaRepository VentaRepository { get; }
    IResenaVendedorRepository ResenaVendedorRepository { get; }
    ICajaMisteriosaRepository CajaMisteriosaRepository { get; }
    IProbabilidadDropRepository ProbabilidadDropRepository { get; }
    IRegistroAuditoriaRepository RegistroAuditoriaRepository { get; }

    IHariaPasswordHasher PasswordHasher { get; }
    IHariaTokenGenerator TokenGenerator { get; }

    Task BeginTransactionAsync(string? businessOperation = null);
    Task CommitAsync();
    Task RollbackAsync();
}
