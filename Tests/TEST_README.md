# TeejoshSystem — Tests

## Resumen

La suite de tests cubre las cuatro capas de la arquitectura de forma aislada e independiente.  
El criterio de separación es directo: cuanto más interno el componente, más determinista y barato debe ser su test.

---

## Estado

| Capa | Proyecto | Tipo | Tests | Estado |
|---|---|---|---|---|
| Domain | `TeejoshSystem.Domain.Tests` | Unit | 46 | ✓ |
| Application | `TeejoshSystem.Application.Tests` | Unit | 17 | ✓ |
| Infrastructure | `TeejoshSystem.Infrastructure.Tests` | Integration | 15 | ✓ |
| UI | `TeejoshSystem.AvaloniaUI.Tests` | Unit | 32 | ✓ |
| **Total** | | | **110** | **✓** |

---

## Estructura

```
tests/
├── TeejoshSystem.Domain.Tests/
│   ├── GlobalUsings.cs
│   ├── Entities/
│   │   └── ProductoTests.cs
│   └── ValueObjects/
│       ├── PrecioTests.cs
│       └── UnidadesTests.cs
│
├── TeejoshSystem.Application.Tests/
│   ├── GlobalUsings.cs
│   ├── Productos/
│   │   └── ProductoHandlerTests.cs
│   └── Ventas/
│       └── VentaHandlerTests.cs
│
├── TeejoshSystem.Infrastructure.Tests/
│   ├── GlobalUsings.cs
│   ├── Fixtures/
│   │   └── DatabaseFixture.cs
│   └── Repositories/
│       └── ProductoRepositoryTests.cs
│
└── TeejoshSystem.AvaloniaUI.Tests/
    ├── GlobalUsings.cs
    ├── Productos/
    │   └── ProductoViewModelTests.cs
    └── Ventas/
        └── VentaViewModelTests.cs
```

---

## Comandos

### Ejecutar todos los tests desde la raíz del proyecto

```bash
dotnet test
```

### Ejecutar una capa específica desde la raíz del proyecto

```bash
dotnet test tests/TeejoshSystem.Domain.Tests/
dotnet test tests/TeejoshSystem.Application.Tests/
dotnet test tests/TeejoshSystem.Infrastructure.Tests/
dotnet test tests/TeejoshSystem.AvaloniaUI.Tests/
```

### Ejecutar con reporte de cobertura

Desde el directorio del proyecto de tests correspondiente:

```bash
cd tests/TeejoshSystem.Domain.Tests
dotnet test --collect:"XPlat Code Coverage"
```

Los reportes XML quedan en `TestResults/` dentro de cada proyecto.  
Para generar un reporte HTML a partir del XML:

```bash
# Instalar la herramienta globalmente (una sola vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generar el reporte desde la raíz del proyecto
reportgenerator \
  -reports:"tests/**/TestResults/**/coverage.cobertura.xml" \
  -targetdir:"tests/CoverageReport" \
  -reporttypes:Html
```

El reporte queda en `tests/CoverageReport/index.html`.

---

## Tecnologías

| Librería | Versión | Propósito |
|---|---|---|
| xUnit | 2.9.0 | Framework de tests |
| FluentAssertions | 6.12.0 | Asserts expresivos |
| NSubstitute | 5.1.0 | Mocks (Application y UI) |
| Microsoft.Data.Sqlite | 8.0.0 | SQLite real en Infrastructure |
| coverlet.collector | 6.0.2 | Reporte de cobertura |

> **Por qué NSubstitute y no Moq:** Moq tuvo un incidente de supply chain en 2023 (SponsorLink). NSubstitute es más limpio en sintaxis y sin controversia.

---

## Estrategia por Capa

### Domain.Tests — Unit Tests puros

Sin mocks, sin base de datos, sin framework externo. Los value objects y entidades tienen lógica pura — son deterministas por definición.

**Qué se verifica:**

- `Precio` rechaza valores negativos y redondea a dos decimales
- `Unidades.Decrementar` lanza `InvalidOperationException` si la cantidad supera el stock — invariante que garantiza que el stock nunca quede negativo
- `Producto.AsignarDescripcion` rechaza detalles incompatibles con el tipo del producto (`FunkoDetalle` en un `Producto` de tipo `HotWheels` lanza `InvalidOperationException`)
- `Producto.ReducirStock` delega correctamente a `Unidades.Decrementar`

**Por qué es la capa más importante:** si una invariante de dominio falla, ninguna capa superior puede compensarlo. Un `Precio` negativo que llega a la BD es un error que el compilador no puede detectar.

---

### Application.Tests — Unit Tests con repositorios mockeados

Los handlers son el núcleo de los casos de uso. Se testean con `IProductoRepository` e `IVentaRepository` mockeados mediante NSubstitute — no hay base de datos real.

**Qué se verifica:**

- `CrearProductoCommandHandler` construye la entidad correctamente y llama `AddAsync`
- `CrearProductoCommandHandler` retorna `Result.Failure` sin persistir si el nombre es inválido o el precio es negativo
- `ObtenerProductosQueryHandler` mapea entidades a DTOs y retorna `IReadOnlyList<ProductoDto>`
- `EliminarProductoCommandHandler` delega a `DeleteRangeAsync` y captura excepciones de repositorio como `Result.Failure`
- `BuscarProductosQueryHandler` delega a `SearchWithDetalleAsync` con los parámetros correctos
- `RegistrarVentaCommandHandler` valida stock antes de persistir — si stock < cantidad retorna `Result.Failure` sin llamar `AddAsync`
- `RegistrarVentaCommandHandler` llama `UpdateAsync` por cada producto tras decrementar su stock
- `RegistrarVentaCommand` rechaza listas vacías en su constructor

**Nota sobre `ObtenerProductosQueryHandler` y `ObtenerVentasQueryHandler`:** ambos retornan `IReadOnlyList<T>` directamente, sin envolver en `Result`. Los asserts son sobre la lista, no sobre `.IsSuccess`.

---

### Infrastructure.Tests — Integration Tests con SQLite real

Estos tests levantan una base de datos SQLite en archivo temporal, aplican las migraciones reales y verifican que las queries y configuraciones Fluent funcionan correctamente.

**Por qué SQLite real y no el provider InMemory de EF Core:**

El provider InMemory no ejecuta SQL real, no valida foreign keys y no soporta las queries raw de `SearchWithDetalleAsync`. Usarlo daría falsos positivos en los tests más críticos. El archivo temporal garantiza el mismo comportamiento que producción.

**Qué se verifica:**

- `AddAsync` persiste el `Producto` y su `ProductoDetalle` en la misma operación
- `SearchWithDetalleAsync` filtra correctamente por nombre y tipo usando el SQL raw con la columna discriminadora `type` y JOINs a las tablas de detalle
- `DeleteAsync` elimina el producto y su detalle por cascade (configuración Fluent `ON DELETE CASCADE`)
- `UpdateAsync` persiste los cambios en BD (verificado con `ChangeTracker.Clear()` antes de re-leer)
- `ExistsAsync` retorna `true`/`false` correctamente

**`DatabaseFixture`:** implementa `IClassFixture<DatabaseFixture>` — xUnit comparte una instancia por clase de test. `LimpiarDatos()` elimina todos los registros entre tests para evitar contaminación sin recrear la BD completa.

**Nota para Windows:** `DatabaseFixture.Dispose()` llama `SqliteConnection.ClearAllPools()` antes de eliminar el archivo. Sin esto, Windows retiene el file handle y lanza `IOException` al intentar borrar el `.db` temporal.

---

### AvaloniaUI.Tests — Unit Tests con IMediator mockeado

Los ViewModels son clases C# puras — no se levanta Avalonia. `IMediator`, `INotificationService`, `IConfirmationService` e `INavigationService` se mockean con NSubstitute.

**Qué se verifica:**

`GestionarProductosViewModel`
- El constructor dispara `BuscarAsync()` automáticamente
- `BuscarCommand` popula `Productos` con los resultados del mediator
- `EliminarCommand` no puede ejecutarse sin `ProductoSeleccionado`
- `EliminarCommand` envía `EliminarProductoCommand` solo si el usuario confirma
- `EliminarCommand` no envía nada si el usuario rechaza la confirmación

`CrearProductoViewModel`
- La visibilidad de paneles (`MostrarHotWheels`, `MostrarFunko`, etc.) responde al `TipoSeleccionado`
- Las validaciones inline rechazan nombre vacío, nombre mayor a 50 caracteres, precio negativo y unidades negativas
- `GuardarCommand` no puede ejecutarse hasta que `CatalogosCargados = true`

`HistorialVentasViewModel`
- `BuscarCommand` popula `Ventas` y respeta los filtros de fecha
- Las excepciones del mediator se capturan y notifican via `INotificationService`
- `LimpiarFiltrosCommand` resetea `FechaDesde` y `FechaHasta` a null

`RegistrarVentaViewModel`
- `AgregarItemCommand` agrega al carrito o suma cantidad si el producto ya existe
- `AgregarItemCommand` notifica error si la cantidad supera el stock disponible
- `TotalVenta` es la suma de todos los subtotales del carrito
- `QuitarItemCommand` elimina el item y recalcula el total
- `ConfirmarVentaCommand` no puede ejecutarse con carrito vacío
- `ConfirmarVentaCommand` navega al menú tras éxito
- `ConfirmarVentaCommand` no envía nada si el usuario rechaza la confirmación

**Nota sobre el constructor async:** todos los ViewModels disparan operaciones async en el constructor via fire-and-forget (`_ = BuscarAsync()`). Los mocks de mediator deben estar configurados **antes** de instanciar el ViewModel para evitar excepciones en esa llamada inicial.

---

## Patrones de Test

### Nomenclatura

```
Clase:  [ClaseTesteada]Tests
Método: [Metodo]_[Escenario]_[ResultadoEsperado]
```

Ejemplos:
```
Handle_NombreInvalido_DebeRetornarFailureSinPersistir
ReducirStock_CantidadMayorAlStock_DebeArrojarInvalidOperationException
EliminarCommand_RechazadoPorUsuario_NuncaEnviaCommand
```

### GlobalUsings por proyecto

Cada proyecto de tests tiene un `GlobalUsings.cs` en su raíz que declara los usings compartidos por todos los archivos del proyecto. Esto evita repetir los mismos `using` en cada archivo.

```csharp
// Domain.Tests y Infrastructure.Tests
global using Xunit;
global using FluentAssertions;

// Application.Tests y AvaloniaUI.Tests (agregan NSubstitute)
global using Xunit;
global using FluentAssertions;
global using NSubstitute;
```

### Mocks con NSubstitute

```csharp
// Crear mock
var repo = Substitute.For<IProductoRepository>();

// Configurar retorno
repo.GetByIdAsync(1).Returns(producto);

// Configurar excepción
repo.When(x => x.DeleteRangeAsync(Arg.Any<IEnumerable<int>>()))
    .Throw(new Exception("Error de BD"));

// Verificar llamada exacta
await repo.Received(1).AddAsync(Arg.Any<Producto>());

// Verificar que NO se llamó
await repo.DidNotReceive().DeleteAsync(Arg.Any<Producto>());
```

---

## Qué NO se testea

| Qué | Por qué |
|---|---|
| Constructores triviales de DTOs | Sin lógica |
| Propiedades auto-implementadas | Sin lógica |
| Migraciones de EF Core | EF Core las garantiza |
| Código de framework (MediatR, EF Core, Avalonia) | Ya tienen sus propios tests |
| Views XAML | No son testeables unitariamente sin Avalonia headless |
| `INotificationService` e `IConfirmationService` concretos | Son adapters de UI — su test es manual |

---

## Convenciones Arquitectónicas en Tests

Las mismas reglas de dependencia del proyecto principal se aplican a los tests:

- `Domain.Tests` no referencia Application, Infrastructure ni AvaloniaUI
- `Application.Tests` no referencia Infrastructure ni AvaloniaUI
- `Infrastructure.Tests` no referencia AvaloniaUI
- Ningún test instancia `DbContext` fuera de `DatabaseFixture`
- Ningún test accede a repositorios concretos desde Application.Tests — solo a través de interfaces mockeadas

---

## Próximas Coberturas (Roadmap)

A medida que el sistema evoluciona según el roadmap, los tests deben crecer en paralelo:

| Módulo | Capa prioritaria | Tests pendientes |
|---|---|---|
| Autenticación | Domain + Application | `AutenticarUsuarioCommandHandler`, invariantes de `PasswordHash` |
| APIs de catálogos | Application + Infrastructure | Adapters de TCGdex, Scryfall, YGOPRODeck mockeados |
| Excel import/export | Application | `ImportarProductosCommandHandler` con validación fila por fila |
| Audit log | Infrastructure | Interceptor de `SaveChangesAsync` registra cambios |
| Blazor UI | AvaloniaUI.Tests → BlazorUI.Tests | Mismo patrón, nuevo proyecto paralelo |

> Los tests de Infrastructure de repositorios nuevos (ventas, catálogos) siguen el mismo patrón de `DatabaseFixture` — reutilizar el fixture existente o crear uno específico si el módulo requiere seed data propio.