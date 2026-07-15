using FluentAssertions;
using NSubstitute;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands.RegistrarVenta;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;
using Xunit;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Ventas.Commands;

public sealed class RegistrarVentaTransactionTests
{
    [Fact]
    public async Task FailureAfterSaleInsert_RequestsRollbackAndDoesNotCommit()
    {
        var saleRepository = Substitute.For<IVentaRepository>();
        var productRepository = Substitute.For<IProductoRepository>();
        var transaction = new RecordingTransaction();
        var product = Product(id: 1, stock: 5);
        productRepository.GetByIdAsync(1).Returns(product);
        saleRepository.AddAsync(Arg.Any<Venta>()).Returns(91);
        productRepository.UpdateAsync(Arg.Any<Producto>())
            .Returns(_ => Task.FromException(new InvalidOperationException("falló stock")));
        var handler = new RegistrarVentaCommandHandler(saleRepository, productRepository, transaction);

        var result = await handler.Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(1, 2) }),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        transaction.RolledBack.Should().BeTrue();
        transaction.Committed.Should().BeFalse();
    }

    [Fact]
    public async Task SuccessfulSale_CommitsSingleTransaction()
    {
        var saleRepository = Substitute.For<IVentaRepository>();
        var productRepository = Substitute.For<IProductoRepository>();
        var transaction = new RecordingTransaction();
        productRepository.GetByIdAsync(1).Returns(Product(id: 1, stock: 5));
        saleRepository.AddAsync(Arg.Any<Venta>()).Returns(92);
        var handler = new RegistrarVentaCommandHandler(saleRepository, productRepository, transaction);

        var result = await handler.Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(1, 1) }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        transaction.Committed.Should().BeTrue();
        transaction.RolledBack.Should().BeFalse();
    }

    private static Producto Product(int id, int stock)
    {
        var product = new Producto(TipoProducto.HotWheels, new NombreProducto("Producto"), new Precio(10m), new Unidades(stock));
        typeof(Producto).GetProperty(nameof(Producto.Id))!.SetValue(product, id);
        return product;
    }

    private sealed class RecordingTransaction : IApplicationTransaction
    {
        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, Func<T, bool> shouldCommit, CancellationToken cancellationToken = default)
        {
            var result = await operation();
            Committed = shouldCommit(result);
            RolledBack = !Committed;
            return result;
        }
    }
}