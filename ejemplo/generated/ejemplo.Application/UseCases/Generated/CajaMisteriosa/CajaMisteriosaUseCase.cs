using ejemplo.Application.Abstractions;

namespace ejemplo.Application.UseCases.CajaMisteriosa;

public sealed partial class CajaMisteriosaUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnitOfWorkUtils? _utils;

    public CajaMisteriosaUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public CajaMisteriosaUseCase(IUnitOfWork unitOfWork, IUnitOfWorkUtils utils)
        : this(unitOfWork)
    {
        _utils = utils;
    }
}
