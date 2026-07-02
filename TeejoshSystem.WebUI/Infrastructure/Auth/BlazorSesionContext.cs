using TeejoshSystem.Application.Common.Dtos;

namespace TeejoshSystem.WebUI.Infrastructure.Auth;

public sealed class BlazorSesionContext
{
    private SesionDto? _sesionActual;

    public bool EstaAutenticado => _sesionActual is not null;
    public SesionDto? SesionActual => _sesionActual;

    public void IniciarSesion(SesionDto sesion)
    {
        ArgumentNullException.ThrowIfNull(sesion);
        _sesionActual = sesion;
    }

    public void CerrarSesion() => _sesionActual = null;
}
