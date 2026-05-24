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
        private readonly IAppLogger _logger;                 // NUEVO

        public ActualizarProductoCommandHandler(
            IProductoRepository repository,
            IImageStorageService imageStorage,
            IAppLogger logger)                               // NUEVO
        {
            _repository = repository;
            _imageStorage = imageStorage;
            _logger = logger;
        }

        public async Task<Result> Handle(
            ActualizarProductoCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.Debug($"Iniciando actualización de producto: Id={request.Id}");

                var producto = await _repository.GetByIdAsync(request.Id);

                if (producto is null)
                {
                    _logger.Warning($"Intento de actualizar producto inexistente: Id={request.Id}");
                    return Result.Failure("El producto no existe.");
                }

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

                _logger.Info($"Producto actualizado exitosamente: Id={request.Id}, Nombre={request.Nombre}");
                return Result.Success();
            }
            catch (ArgumentException ex)
            {
                _logger.Warning($"Datos inválidos al actualizar producto Id={request.Id}: {ex.Message}");
                return Result.Failure(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning($"Operación inválida al actualizar producto Id={request.Id}: {ex.Message}");
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inesperado al actualizar producto Id={request.Id}", ex);
                return Result.Failure("No se pudo guardar el producto. Intente nuevamente.");
            }
        }
    }
}