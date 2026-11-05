namespace ejemplo.Application.Abstractions;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}