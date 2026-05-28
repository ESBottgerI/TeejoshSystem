using FluentAssertions;
using NSubstitute;
using Reqnroll;

using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.StepDefinitions;

[Binding]
public class BuscarProductoSteps
{
    private readonly IProductoRepository _repository;
    private readonly BuscarProductosQueryHandler _handler;

    private readonly IImageStorageService _imageStorageMock;

    private List<ProductoBusquedaDto> _resultado = new();

    public BuscarProductoSteps()
    {
        _repository = Substitute.For<IProductoRepository>();
        
        _imageStorageMock = Substitute.For<IImageStorageService>();

        _handler = new BuscarProductosQueryHandler(
            _repository,
            _imageStorageMock
        );
    }

    [Given("existen productos registrados")]
    public void GivenExistenProductosRegistrados()
    {
        _repository.SearchWithDetalleAsync(
                Arg.Any<string?>(),
                Arg.Any<TipoProducto?>())
            .Returns(Task.FromResult<IReadOnlyList<ProductoBusquedaResult>>(
            [
                new ProductoBusquedaResult(
                    1,
                    TipoProducto.Funko,
                    "Funko Batman",
                    80,
                    10,
                    "Funko POP",
                    null)
            ]));
    }

    [Given("no existen productos registrados")]
    public void GivenNoExistenProductosRegistrados()
    {
        _repository.SearchWithDetalleAsync(
                Arg.Any<string?>(),
                Arg.Any<TipoProducto?>())
            .Returns(Task.FromResult<IReadOnlyList<ProductoBusquedaResult>>(
                []));
    }

    [When("el administrador busca productos")]
    public async Task WhenElAdministradorBuscaProductos()
    {
        _resultado = await _handler.Handle(
            new BuscarProductosQuery(null, null),
            CancellationToken.None);
    }

    [Then("el sistema debe devolver resultados")]
    public void ThenElSistemaDebeDevolverResultados()
    {
        _resultado.Should().NotBeEmpty();
    }

    [Then("el sistema no debe devolver resultados")]
    public void ThenElSistemaNoDebeDevolverResultados()
    {
        _resultado.Should().BeEmpty();
    }
}