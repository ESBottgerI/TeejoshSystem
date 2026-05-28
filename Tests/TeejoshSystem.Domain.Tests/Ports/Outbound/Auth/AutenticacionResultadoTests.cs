using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Domain.Tests.Ports.Outbound.Auth
{
    // ═════════════════════════════════════════════════════════════════════════
    // AutenticacionResultado
    // ═════════════════════════════════════════════════════════════════════════

    public class AutenticacionResultadoTests
    {
        // ── Valido: mutante true → false en Exitoso; null → valor en MensajeError ─

        [Fact]
        public void Valido_ExitosoEsTrue()
        {
            // Mata el mutante: new(true, ...) → new(false, ...)
            var resultado = AutenticacionResultado.Valido(1, "admin", RolUsuario.Administrador);

            resultado.Exitoso.Should().BeTrue();
        }

        [Fact]
        public void Valido_MensajeErrorEsNull()
        {
            // Mata el mutante: null → string.Empty en MensajeError
            var resultado = AutenticacionResultado.Valido(1, "admin", RolUsuario.Administrador);

            resultado.MensajeError.Should().BeNull();
        }

        [Fact]
        public void Valido_AsignaCamposCorrectamente()
        {
            // Mata mutantes de UsuarioId, NombreUsuario y Rol en Valido
            var resultado = AutenticacionResultado.Valido(7, "operador", RolUsuario.Operador);

            resultado.UsuarioId.Should().Be(7);
            resultado.NombreUsuario.Should().Be("operador");
            resultado.Rol.Should().Be(RolUsuario.Operador);
        }

        // ── Invalido: mutante false → true en Exitoso; motivo → null ─────────────

        [Fact]
        public void Invalido_ExitosoEsFalse()
        {
            // Mata el mutante: new(false, ...) → new(true, ...)
            var resultado = AutenticacionResultado.Invalido("Credenciales incorrectas.");

            resultado.Exitoso.Should().BeFalse();
        }

        [Fact]
        public void Invalido_MensajeErrorContieneElMotivo()
        {
            // Mata el mutante: motivo → null en MensajeError
            var resultado = AutenticacionResultado.Invalido("Usuario inactivo.");

            resultado.MensajeError.Should().Be("Usuario inactivo.");
        }

        [Fact]
        public void Invalido_CamposNullables_SonNull()
        {
            // Mata mutantes en los tres null del constructor de Invalido
            var resultado = AutenticacionResultado.Invalido("Error.");

            resultado.UsuarioId.Should().BeNull();
            resultado.NombreUsuario.Should().BeNull();
            resultado.Rol.Should().BeNull();
        }

        // ── Distinción entre ambos estados ───────────────────────────────────────

        [Fact]
        public void Valido_YInvalido_ExitosoSonOpuestos()
        {
            // Test de regresión: los dos factory methods son complementarios
            var valido = AutenticacionResultado.Valido(1, "u", RolUsuario.Administrador);
            var invalido = AutenticacionResultado.Invalido("err");

            valido.Exitoso.Should().BeTrue();
            invalido.Exitoso.Should().BeFalse();
            valido.Exitoso.Should().NotBe(invalido.Exitoso);
        }
    }
}
