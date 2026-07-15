Requisitos de aceptacion en fases:
Checklist — Fase 0: Integración con Application/Infrastructure
Registrar AddInfrastructure(configuration) en Program.cs
Registrar AddMediatR apuntando al assembly de TeejoshSystem.Application
Definir estrategia de migraciones EF Core (evitar colisión Avalonia/Blazor sobre la misma DB)
Smoke test: página temporal que ejecute ObtenerProductosQuery y muestre resultado real
Confirmar appsettings.Production.json con la misma connection string que Avalonia
Checklist — Fase 1: Shell y Navegación
Layout propio (reemplazar plantilla Bootstrap por defecto)
Definir mapa de rutas (/login, /inventario, /productos, /ventas, etc.)
Menú lateral con misma nomenclatura que Avalonia
Eliminar páginas de plantilla (Counter, Weather)
Checklist — Fase 2: Autenticación
Login.razor invocando AutenticarUsuarioCommand
AuthenticationStateProvider propio (por circuito, no JWT)
Servicio de sesión scoped (no copiar el Singleton de Avalonia — riesgo de fuga de sesión entre usuarios)
Logout
Manejo de error de credenciales inválidas vía Result<SesionDto>
Checklist — Fase 3: Autorización
[Authorize] en rutas privadas + redirect a /login
Autorización por rol (RolUsuario)
Componente de "acceso denegado"
Checklist — Fase 4: Inventario
Listado real vía ObtenerProductosQuery
Búsqueda/filtro vía BuscarProductosQuery
Detalle de producto vía ObtenerProductosPorIdQuery
Checklist — Fase 5: Gestión de Productos
Crear producto (CrearProductoCommand)
Editar producto (ActualizarProductoCommand)
Eliminar producto con confirmación (EliminarProductoCommand)
Estrategia de manejo de imágenes (ImagePath) — no reutilizable de Avalonia tal cual
Checklist — Fase 6: Ventas
Registrar venta (RegistrarVentaCommand)
Historial de ventas (ObtenerVentasQuery)
Checklist — Fase 7: Pulido final
Loading states consistentes
Manejo centralizado de Result<T> fallido
Ajustes responsive (uso probable en tablet/POS)
Confirmar que Avalonia sigue funcionando sin regresiones
