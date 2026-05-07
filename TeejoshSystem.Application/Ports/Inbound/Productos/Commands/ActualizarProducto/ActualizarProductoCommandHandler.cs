using MediatR;
using System;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Commands.ActualizarProducto
{
    public class ActualizarProductoCommandHandler : IRequestHandler<ActualizarProductoCommand, Result>
    {
        private readonly IProductoRepository _repository;
        private readonly IImageStorageService _imageStorage;

        public ActualizarProductoCommandHandler(
            IProductoRepository repository,
            IImageStorageService imageStorage)
        {
            _repository = repository;
            _imageStorage = imageStorage;
        }

        public async Task<Result> Handle(
            ActualizarProductoCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var producto = await _repository.GetByIdAsync(request.Id);

                if (producto is null)
                    return Result.Failure("El producto no existe.");

                producto.ActualizarDatos(
                    new NombreProducto(request.Nombre),
                    new Precio(request.Precio),
                    new Unidades(request.Unidades));

                // Guardar nueva imagen si viene una
                if (request.ImagePath is not null)
                {
                    var imageName = await _imageStorage.SaveImageAsync(request.ImagePath);
                    producto.AsignarImagePath(imageName);
                }

                await _repository.UpdateAsync(producto);

                return Result.Success();
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Result.Failure(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Result.Failure("No se pudo guardar el producto. Intente nuevamente.");
            }
        }
    }
}