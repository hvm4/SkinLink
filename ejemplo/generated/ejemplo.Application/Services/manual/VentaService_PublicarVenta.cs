using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Venta_publicarVenta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class VentaService
{
    public async Task PublicarVentaAsync(int p_oid, int p_activoDigital_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Venta_publicarVenta) ENABLED START*/
        // Publica en el mercado un activo del inventario del vendedor de la venta (RF-16, CU-13).
        await _unitOfWork.BeginTransactionAsync("PublicarVenta");
        try
        {
            var venta = await _repository.ReadAsync(p_oid)
                ?? throw new ModelException("Venta not found.");
            if (venta.EstadoVenta != EstadoVenta.Disponible)
                throw new ModelException("Solo se puede publicar una venta en estado Disponible.");

            var vendedor = venta.Vendedor
                ?? throw new ModelException("La venta no tiene un vendedor asociado.");
            if (vendedor.Estado != EstadoCuenta.Activo)
                throw new ModelException("La cuenta del vendedor no está activa.");

            // Carga inventarios (con su usuario), activos y sus ventas/intercambios en el contexto compartido.
            await _unitOfWork.InventarioPrivadoRepository.ReadAllAsync();
            await _unitOfWork.ActivoDigitalRepository.ReadAllAsync();

            var activo = await _unitOfWork.ActivoDigitalRepository.ReadAsync(p_activoDigital_oid)
                ?? throw new ModelException("ActivoDigital not found.");

            // Propiedad: el activo debe estar en el inventario del vendedor.
            if (activo.Inventario?.Usuario?.Id != vendedor.Id)
                throw new ModelException("El activo no pertenece al inventario del vendedor.");
            // No puede estar ya publicado ni bloqueado por otra operación.
            if (activo.EnVenta && venta.ActivoDigital?.Id != activo.Id)
                throw new ModelException("El activo ya está en venta o bloqueado.");
            if (activo.Venta is not null && activo.Venta.Id != venta.Id)
                throw new ModelException("El activo ya tiene otra venta asociada.");
            if (activo.IntercambioOfrecido is not null || activo.IntercambioSolicitado is not null)
                throw new ModelException("El activo participa en un intercambio y no puede publicarse.");

            // Precio válido.
            var precio = venta.PrecioVenta;
            if (precio is null || double.IsNaN(precio.Valor) || double.IsInfinity(precio.Valor) || precio.Valor <= 0)
                throw new ModelException("El precio de venta debe ser un número positivo.");
            if (string.IsNullOrWhiteSpace(precio.Divisa))
                throw new ModelException("El precio de venta debe indicar la divisa.");

            // Si la venta apuntaba a otro activo, este queda libre.
            var anterior = venta.ActivoDigital;
            if (anterior is not null && anterior.Id != activo.Id && anterior.EnVenta)
            {
                anterior.EnVenta = false;
                await _unitOfWork.ActivoDigitalRepository.SaveAsync(anterior);
            }

            activo.EnVenta = true;
            await _unitOfWork.ActivoDigitalRepository.SaveAsync(activo);

            venta.ActivoDigital = activo;
            venta.FechaPublicacion = DateTime.UtcNow;
            await _repository.SaveAsync(venta);

            // Experiencia por publicar (RF-04).
            await new UsuarioService(_unitOfWork).SumarExperienciaAsync(vendedor.Id, 10);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        /*PROTECTED REGION END*/
    }
}
