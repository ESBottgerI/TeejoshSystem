Voy a revisar la solución y los documentos fuente sin modificar archivos, para convertir el encargo en un plan ejecutable y detectar restricciones reales del código existente.

Encontré una solución .NET con proyectos Domain/Application/Infrastructure/AvaloniaUI y pruebas; ahora voy a leer los puntos de integración y estilos existentes.

La infraestructura ya referencia Npgsql además de SQLite, así que el plan debe apoyarse en registros existentes o en selección por configuración sin tocar migraciones. Voy a inspeccionar esos registros y los CQRS disponibles para evitar duplicar lógica.

Detecté que los servicios de notificación/confirmación parecen vivir en Avalonia, no en Application; eso puede impedir registrarlos desde WebUI sin acoplarse al cliente desktop. Voy a verificar si existe una abstracción compartida antes de decidir el plan.

Hay una tensión importante: AddPersistence ya selecciona SQLite/PostgreSQL por configuración, pero ProductoRepository.SearchWithDetalleAsync usa Microsoft.Data.Sqlite.SqliteParameter y SQL específico de SQLite. Con la restricción de no tocar Infrastructure, el despliegue PostgreSQL completo no parece alcanzable sin riesgo.

Antes de cerrar el plan, voy a comprobar si ya existe algún proyecto WebUI parcial para no proponer sobreescrituras.

Revisé la solución en modo solo lectura. No he modificado archivos.

Hallazgos Clave

TeejoshSystem.WebUI/ no existe todavía.
AddPersistence() ya soporta Database:Provider = sqlite|postgresql en TeejoshSystem.Infrastructure.
Hay un bloqueo real para PostgreSQL: ProductoRepository.SearchWithDetalleAsync() usa SqliteParameter y SQL específico de SQLite, por lo que una WebUI con PostgreSQL fallaría sin tocar Infrastructure.
Las interfaces INotificationService e IConfirmationService están dentro de TeejoshSystem.AvaloniaUI, no en Application/Core compartido. WebUI no debería referenciar Avalonia.
No encontré IUserContext; BlazorUserContext no puede implementar una abstracción inexistente sin crearla en una capa compartida, lo cual chocaría con la regla de no modificar Core.
appsettings.Production.json contiene una cadena Supabase con contraseña en claro. El plan debe mover eso a variables de entorno y no copiar secretos al nuevo proyecto.
Plan Propuesto

Crear TeejoshSystem.WebUI como Blazor Server .NET 8 con AddInteractiveServerComponents, rutas protegidas y referencias solo a Application e Infrastructure.
Agregar el proyecto a TeejoshSystem.sln.
Centralizar DI en Extensions/ServiceCollectionExtensions.cs:
AddInfrastructure(configuration)
AddMediatR(...) apuntando al assembly de Application
AddAuthorization(), no AddAuthorizationCore()
CustomAuthenticationStateProvider
servicios web propios para notificaciones/confirmaciones sin depender de Avalonia
Traducir tokens visuales desde App.axaml y AppStyles.axaml a design-tokens.css, con modo claro/oscuro.
Implementar estructura Blazor solicitada:
Login accesible
Layout, menú, header de usuario/versión
Inventario con <Virtualize>
Gestión/creación de productos vía IMediator.Send()
Ventas con carrito en CircuitStateStore
Toasts, confirm dialog, inputs y botones accesibles
Implementar Supabase JWT de forma aislada en WebUI:
Validación de token en CustomAuthenticationStateProvider
Claims Name, Role, NameIdentifier
Sin modificar Usuario ni BCrypt local
Dockerizar:
Dockerfile multi-stage
docker-compose.yml con WebUI, PostgreSQL y Nginx opcional
Secretos vía variables de entorno
Validación final:
dotnet build TeejoshSystem.sln
verificar que Avalonia sigue compilando
revisar criterios de CONTEXTO_TAREAS.md
Decisión Necesaria Para cumplir PostgreSQL real en VPS hay que elegir una opción:

Recomendada: autorizar una modificación mínima en Infrastructure para hacer SearchWithDetalleAsync() compatible con PostgreSQL/SQLite sin tocar Domain ni Application.
Estricta sin tocar Core: crear WebUI, pero el modo PostgreSQL quedará bloqueado por el repositorio actual; solo sería seguro con SQLite hasta resolver esa deuda.
Confírmame cuál opción quieres y paso a implementación cuando salgas de modo plan.

Procedamos con la Opción Recomendada. Autorizo la modificación mínima en Infrastructure para que ProductoRepository.SearchWithDetalleAsync() sea compatible con ambos proveedores (PostgreSQL/SQLite) sin alterar las capas de Domain ni Application. Con esto aprobado, puedes iniciar la implementación
Voy a implementar primero el andamiaje WebUI y la corrección mínima de proveedor en Infrastructure, luego compilaré para iterar sobre errores reales.

Voy a crear el proyecto con la plantilla oficial de Blazor .NET 8 para partir de una base compilable y luego reemplazar la estructura por la arquitectura solicitada.

El proyecto base fue creado correctamente. Ahora agregaré referencias, paquetes y lo registraré en la solución antes de hacer los cambios de código.

La restauración detectó un downgrade porque Supabase exige System.IdentityModel.Tokens.Jwt >= 7.5.1; ajustaré el paquete explícito a esa versión para mantener el grafo consistente.

El proyecto quedó registrado y restaurado. Siguiente cambio: haré que SearchWithDetalleAsync use parámetros y concatenación compatibles según proveedor EF activo.

La corrección será acotada a ProductoRepository: conservará el contrato y elegirá SQL compatible por proveedor usando interpolación de EF Core, sin migraciones ni cambios en Domain/Application.

La compatibilidad de búsqueda quedó implementada con ramas SQLite/PostgreSQL y parámetros interpolados por EF Core. Ahora reemplazaré la plantilla Blazor por la estructura WebUI acordada.

Voy a añadir primero la infraestructura web interna: autenticación Supabase por JWT, servicios de estado/notificación y registro centralizado de dependencias.