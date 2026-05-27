using TeejoshSystem.Application.Common;

namespace TeejoshSystem.Application.Tests.Common;

/// <summary>
/// Tests de Result y Result&lt;T&gt;.
///
/// Mutantes objetivo:
///   - Result(true, null)  → IsSuccess=true, Error=null
///   - Result(false, msg)  → IsSuccess=false, Error=msg
///   - Result&lt;T&gt;.Value    → no reemplazado por default(T)
///   - Success&lt;T&gt;         → delega a Result&lt;T&gt; con isSuccess=true
///   - Failure&lt;T&gt;         → delega con isSuccess=false y error
/// </summary>
public class ResultTests
{
    // ── Result (no genérico) ──────────────────────────────────────────────────

    [Fact]
    public void Success_DebeCrearResultadoExitoso()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_DebeCrearResultadoFallido()
    {
        var result = Result.Failure("algo salió mal");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("algo salió mal");
    }

    [Fact]
    public void Failure_IsSuccess_EsFalseNoTrue()
    {
        // Mata mutante que cambia false → true en el constructor
        var result = Result.Failure("error");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Success_Error_EsNull()
    {
        // Mata mutante que asigna error en Success
        var result = Result.Success();

        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_Error_ContieneElMensajeExacto()
    {
        const string mensaje = "El nombre de usuario es obligatorio.";
        var result = Result.Failure(mensaje);

        result.Error.Should().Be(mensaje);
    }

    // ── Result<T> — factory genérica en Result base ───────────────────────────

    [Fact]
    public void SuccessGenerico_DebeCrearResultadoExitosoConValor()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void SuccessGenerico_Value_EsElValorPasado_NoDefault()
    {
        // Mata mutante que reemplaza Value por default(T) = 0
        var result = Result.Success(99);

        result.Value.Should().Be(99);
        result.Value.Should().NotBe(0);
    }

    [Fact]
    public void FailureGenerico_DebeCrearResultadoFallidoConError()
    {
        var result = Result.Failure<int>("Stock insuficiente.");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Stock insuficiente.");
    }

    [Fact]
    public void FailureGenerico_IsSuccess_EsFalse()
    {
        var result = Result.Failure<string>("error");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void SuccessGenerico_ConString_PreservaElString()
    {
        var result = Result.Success("sesion_token");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("sesion_token");
    }

    [Fact]
    public void SuccessGenerico_ConObjeto_PreservaLaReferencia()
    {
        var obj = new object();
        var result = Result.Success(obj);

        result.Value.Should().BeSameAs(obj);
    }

    // ── Distinción entre Success y Failure ────────────────────────────────────

    [Fact]
    public void Success_YFailure_SonOpuestos()
    {
        var exito = Result.Success();
        var fallo = Result.Failure("err");

        exito.IsSuccess.Should().BeTrue();
        fallo.IsSuccess.Should().BeFalse();

        // Nunca ambos verdaderos ni ambos falsos
        exito.IsSuccess.Should().NotBe(fallo.IsSuccess);
    }

    [Fact]
    public void Success_NoTieneError_Failure_SiTieneError()
    {
        var exito = Result.Success();
        var fallo = Result.Failure("fallo");

        exito.Error.Should().BeNull();
        fallo.Error.Should().NotBeNull();
    }
}