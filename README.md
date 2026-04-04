# TeejoshSystem

## Resumen

**TeejoshSystem** es una aplicación de escritorio para gestionar inventarios de coleccionables (Hot Wheels, Funkos, TCG, Toys y Varios).

El sistema está diseñado con **arquitectura hexagonal** sobre **Clean Architecture**, aplicando **DDD**, **CQRS** y **MVVM**. Estas no son decisiones decorativas — cada patrón responde a un problema concreto del dominio y se explican en detalle en la sección [[#Patrones y Decisiones]].

Funciona **offline-first**: sin conexión a red, sin servicios externos en runtime, con base de datos local. La UI es **Avalonia**, verificada en Windows y Linux (Fedora 43).

> La arquitectura no se diseñó para la versión actual — se diseñó para todas las versiones futuras.

---

## Estado del Proyecto

|Campo|Valor|
|---|---|
|**Versión**|0.4 — beta|
|**Estado**|Activo|
|**UI**|Avalonia (migración desde WPF parcial)|
|**Plataformas verificadas**|Windows, Linux (Fedora 43)|
|**Persistencia**|SQL Server Express — Database-First (migración a Code-First pendiente)|
|**Arquitectura**|Estable|
|**Última actualización**|Abril 2026|

---

## Capacidades

- Gestión de 5 tipos de producto con campos distintos entre sí
- Formularios dinámicos que adaptan sus campos según el tipo seleccionado
- Catálogos relacionales con carga en cascada (TCG: Franquicia → Expansiones/Packs)
- Validaciones en tiempo real sobre cada campo
- Búsqueda y filtrado por nombre y tipo
- CRUD completo con confirmaciones antes de acciones destructivas
- Operación completamente offline

---

## Arquitectura

### Modelo C4

La arquitectura está modelada mediante **C4 Model** usando Structurizr DSL.

> Los diagramas se generan desde el archivo `.dsl` — no están incrustados en este documento.

Niveles documentados:

- **System Context** → El usuario interactúa con TeejoshSystem
- **Container** → UI, Application, Domain, Infrastructure, Database
- **Component** → ViewModels, casos de uso, puertos, repositorios, DbContext

---

### Capas y reglas de dependencia

```
UI (Avalonia)
      ↓
Infrastructure ──→ Domain
      ↓                ↑
  Application ─────────┘
```

|Capa|Rol|Depende de|NO Depende de|
|---|---|---|---|
|**Domain**|Núcleo: entidades, value objects, contratos|Nada|Todas|
|**Application**|Casos de uso, comandos, queries, DTOs|Domain|Infrastructure, UI|
|**Infrastructure**|Persistencia, EF Core, repositorios|Domain, Application| UI|
|**UI (Avalonia)**|Presentación: ViewModels, Views, Services|Application|Domain|

> Infrastructure puede referenciar Application únicamente para implementar sus interfaces (outbound ports). Nunca para invocar casos de uso.

La dirección de las dependencias no es una convención — es una garantía. Significa que Domain compila sin conocer que existe EF Core, Avalonia, o SQL Server. Si eso deja de ser cierto, la arquitectura está rota.

### Violaciones no aceptables

- Referenciar EF Core, SQL o cualquier framework externo desde `Domain`
- Acceder a repositorios directamente desde un ViewModel
- Instanciar `DbContext` fuera de Infrastructure
- Introducir lógica de negocio en ViewModels o code-behind

---

### Flujo de ejecución (C4 - Component)

```
Views (Avalonia XAML)
  ↓ data binding
ViewModels
  ↓ MediatR.Send(command/query)
Application Handler
  ↓ orquesta entidades y value objects
Domain (reglas de negocio, invariantes)
  ↓ IProductoRepository / ICatalogoRepository
Infrastructure (EF Core)
  ↓
Base de datos
  ↑
Result<T> regresa encapsulado por toda la cadena
```

---

## Patrones y Decisiones

Esta sección explica qué resuelve cada patrón, por qué se eligió, y qué problema concreto habría sin él.

---

### Clean Architecture

**Qué es:** Organización del sistema en capas concéntricas donde las dependencias siempre apuntan hacia adentro (hacia el dominio). Ninguna capa interna conoce las externas.

**Qué problema resuelve:** Sin esta separación, la lógica de negocio termina acoplada a la base de datos, al framework de UI, o a ambos. Cualquier cambio en EF Core, en WPF/Avalonia, o en el esquema SQL arrastraría cambios en las reglas del negocio — que son la parte más estable y valiosa del sistema.

**Por qué importa aquí:** La migración de WPF a Avalonia fue posible sin tocar Domain ni Application. Si la arquitectura estuviera acoplada, esa migración habría requerido reescribir lógica de negocio junto con la UI.

---

### Hexagonal Architecture (Ports & Adapters)

**Qué es:** El dominio define contratos (Ports) para todo lo que necesita del exterior. El exterior (UI, base de datos, APIs) los implementa mediante Adapters. El dominio nunca llama directamente a ninguna implementación concreta.

**Qué problema resuelve:** Sin esto, el dominio está atado a tecnologías específicas. Si mañana se reemplaza SQL Server por SQLite, o se agrega una API REST, o se migra de Avalonia a MAUI, el dominio no debería necesitar cambios.

**Inbound Adapters actuales:** UI Avalonia  
**Outbound Adapters actuales:** EF Core + SQL Server  
**Puertos definidos:** `IProductoRepository`, `ICatalogoRepository`

**Por qué importa aquí:** La sección [[#Evolución Planeada]] lista cambios de infraestructura que no requieren tocar el dominio, exactamente porque los ports están bien definidos.

---

### Domain-Driven Design (DDD)

**Qué es:** Modelar el software a partir del dominio del problema — en este caso, el negocio de coleccionables. Las entidades representan conceptos reales del negocio, los value objects encapsulan reglas, y los invariantes se protegen desde adentro.

**Qué problema resuelve:** Sin DDD, las reglas de negocio se dispersan: validaciones en ViewModels, lógica de cálculo en handlers, restricciones en la base de datos. El resultado es que no hay un único lugar donde confiar para saber "qué es válido".

**Aplicación concreta:**

- `Producto` es la entidad raíz del agregado
- `NombreProducto`, `Precio`, `Unidades` son value objects inmutables que validan sus propias reglas — un `Precio` inválido no puede existir en ningún punto del sistema
- `ProductoDetalle` (abstracta) y sus subtipos modelan la variabilidad estructural de cada tipo de coleccionable
- Los catálogos (`TcgFranquicia`, `TcgExpansion`, etc.) son entidades de referencia con identidad propia

---

### CQRS (Command Query Responsibility Segregation)

**Qué es:** Separar explícitamente las operaciones que modifican estado (Commands) de las que solo leen (Queries). Cada intención tiene su propio objeto y su propio handler.

**Qué problema resuelve:** Sin CQRS, un mismo método hace demasiado: valida, persiste, y devuelve datos para la UI al mismo tiempo. Esto dificulta mantener, testear y razonar sobre el código. Con CQRS, `CrearProductoCommand` solo sabe crear, y `ObtenerProductosQuery` solo sabe leer.

**Implementación:** CQRS simplificado — sin Event Sourcing, sin event store. MediatR actúa como dispatcher. Los comandos retornan `Result<T>`, las queries retornan DTOs.

**¿Por qué sin Event Sourcing?** El sistema no requiere reconstruir estado a partir de eventos ni mantener historial de cambios intermedios. Event Sourcing agregaría complejidad operativa sin beneficio concreto en este contexto. Si el módulo de auditoría (roadmap) lo requiere en el futuro, es un cambio de Infrastructure, no de Domain.

---

### MVVM (Model-View-ViewModel)

**Qué es:** Separar la vista (XAML) de su lógica de presentación (ViewModel) mediante data binding. La vista no tiene lógica; el ViewModel no conoce la vista.

**Qué problema resuelve:** Sin MVVM, la lógica de presentación vive en code-behind: está atada al framework de UI, no es testeable, y mezcla estado con comportamiento visual. Con MVVM, los ViewModels son clases C# puras que pueden testearse sin levantar la UI.

**Por qué importa en la migración WPF → Avalonia:** Los ViewModels no cambiaron. Solo cambiaron los archivos `.xaml` y la reimplementación de `INotificationService` e `IConfirmationService`. Esto es exactamente el contrato que MVVM garantiza.

---

### Repository Pattern

**Qué es:** Abstraer el acceso a datos detrás de una interfaz. El dominio define qué necesita (`IProductoRepository`), la infraestructura decide cómo lo obtiene.

**Qué problema resuelve:** Sin este patrón, los handlers de Application usarían `DbContext` directamente — acoplando la capa de casos de uso a EF Core. Cambiar la base de datos requeriría modificar Application.

**Implementación:** Los repositorios viven en Infrastructure. Los ports (interfaces) viven en Domain. Application solo conoce los ports.

---

### Result Pattern

**Qué es:** Todos los casos de uso retornan `Result.Success(valor)` o `Result.Failure(error)` en lugar de lanzar excepciones para el flujo de control.

**Qué problema resuelve:** Las excepciones tienen overhead (captura de stack trace) y hacen implícitos los caminos de error — el caller puede ignorarlos o capturarlos incorrectamente. Con `Result<T>`, el caller está obligado a manejar el caso de error, los mensajes son descriptivos para el usuario, y no hay costo de excepciones en rutas normales.

---

### Mediator (MediatR)

**Qué es:** Un despachador central que recibe un comando o query y lo dirige al handler correcto, sin que el emisor conozca al receptor.

**Qué problema resuelve:** Sin Mediator, los ViewModels tendrían referencias directas a múltiples servicios de Application. Con MediatR, el ViewModel solo conoce `IMediator`. Agregar un nuevo caso de uso no requiere modificar la UI.

---

### Shell Pattern (navegación por estado)

**Qué es:** `MainViewModel.CurrentView` controla qué vista está activa. Los DataTemplates en XAML mapean cada ViewModel a su View automáticamente. Los ViewModels se resuelven desde el contenedor DI.

**Por qué no NavigationService:** Introducir un NavigationService implica historial, parámetros de ruta, y deep linking. Ninguna de esas necesidades existe aquí. El Shell Pattern resuelve la navegación de forma simple, sin overhead, y consistente con MVVM.

---

## Estructura del Proyecto

Estructura real según el repositorio:

```
TeejoshSystem/
├── README.md
├── TeejoshSystem.slnx
│
├── TeejoshSystem.Domain/
│   ├── Entities/
│   │   ├── Producto.cs
│   │   ├── Catalogos/        # FunkoSubtipo, TcgFranquicia, TcgExpansion...
│   │   └── Detalles/         # FunkoDetalle, HotWheelsDetalle, TcgDetalle...
│   ├── ValueObjects/
│   │   ├── NombreProducto.cs
│   │   ├── Precio.cs
│   │   └── Unidades.cs
│   ├── Enums/
│   │   └── TipoProducto.cs
│   └── Ports/
│       └── Outbound/
│           └── Repositories/
│               ├── IProductoRepository.cs
│               └── ICatalogoRepository.cs
│
├── TeejoshSystem.Application/
│   ├── Common/
│   │   └── Result.cs
│   └── Ports/
│       └── Inbound/
│           ├── Productos/
│           │   ├── Commands/   # Crear, Actualizar, Eliminar
│           │   └── Queries/    # ObtenerProductos, BuscarProductos, ObtenerPorId
│           └── Catalogos/
│               └── Queries/    # ObtenerCatalogos, ObtenerExpansionesYPacks
│
├── TeejoshSystem.Infrastructure/
│   ├── Adapters/
│   │   └── Outbound/
│   │       └── Persistence/
│   │           ├── InventarioDbContext.cs
│   │           ├── Configurations/   # Fluent API — 12 archivos
│   │           └── Repositories/
│   │               ├── ProductoRepository.cs
│   │               └── CatalogoRepository.cs
│   └── DependencyInjection/
│       ├── InfrastructureServiceRegistration.cs
│       └── PersistenceServiceRegistration.cs
│
└── TeejoshSystem.AvaloniaUI/
    ├── App.axaml / App.axaml.cs
    ├── MainWindow.axaml / MainWindow.axaml.cs
    ├── Program.cs
    ├── appsettings.json
    └── Adapters/
        └── Inbound/
            ├── ViewModels/     # Shell, Menu, Productos, Common
            ├── Views/          # Menu, Productos
            └── Services/       # INotificationService, IConfirmationService
```

---

## Modelo de Base de Datos

### Tabla principal

```sql
product (
    id    INT PRIMARY KEY IDENTITY,
    name  NVARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    units INT           NOT NULL
)
```

### Tablas de detalles (1:1 con `product`, TPT)

```sql
hot_wheels (product_id PK, model, year, serie, category_id)
funko      (product_id PK, box_number, license, subtype_id, special_feature_id)
tcg        (product_id PK, pack_id, expansion_id)
toy        (product_id PK, min_years, min_players, max_players, board_game)
varios     (product_id PK, brand, height, width, length, material, illustration)
```

Todas con `FOREIGN KEY (product_id) REFERENCES product(id) ON DELETE CASCADE`.

### Tablas de catálogos

```sql
hot_wheels_category    (id, name)
funko_subtype          (id, name)
funko_special_feature  (id, name)
tcg_franchise          (id, name)
tcg_expansion          (id, name, franchise_id)
tcg_pack               (id, name, franchise_id)
```

### Por qué Table-Per-Type (TPT) y no Table-Per-Hierarchy (TPH)

TPH almacena todos los subtipos en una sola tabla con columnas nullable para los campos que no aplican. Con 5 tipos de coleccionable con estructuras muy diferentes entre sí, TPH generaría una tabla con mayoría de NULLs, consultas con muchos `IS NULL`, e integridad referencial débil. TPT mantiene cada tabla coherente y normalizada.

---

## Tecnologías

### Backend (.NET 8)

|Librería|Versión|Propósito|
|---|---|---|
|.NET|8.0|Framework base|
|Entity Framework Core|8.0.0|ORM|
|MediatR|12.2.0|Dispatcher CQRS|
|FluentValidation|11.9.0|Validaciones de Application (opcional)|

### Frontend (Avalonia)

|Librería|Versión|Propósito|
|---|---|---|
|Avalonia|-|UI multiplataforma (Windows, Linux, macOS)|
|CommunityToolkit.Mvvm|8.2.2|ObservableObject, RelayCommand|
|Microsoft.Extensions.Hosting|8.0.0|DI container|
|Microsoft.Extensions.Configuration|8.0.0|appsettings.json|

---

## Evolución Planeada

### Database-First → Code-First

**Estado actual:** La base de datos preexiste. EF Core mapea las tablas mediante Fluent API. No hay migraciones.

**Por qué migrar:** Con Database-First, los cambios de esquema requieren intervención manual en dos lugares (BD y configuraciones). Es frágil y no versionable. Code-First invierte esto: las clases del dominio y sus configuraciones Fluent definen el esquema, las migraciones versionan la BD junto con el código.

**Impacto por capa al migrar:**

|Capa|Cambio|
|---|---|
|Domain|Ninguno|
|Application|Ninguno|
|Infrastructure|Se agregan `Migrations/`. DbContext pasa a ser la fuente de verdad del esquema. Las configuraciones Fluent se mantienen y evolucionan.|
|Base de datos|Generada y versionada desde código|

---

### Elección de base de datos: SQLite sobre SQL Server Express

Para un sistema de escritorio offline-first, de usuario único, con importación/exportación de Excel en el roadmap, **SQLite es la opción más adecuada**.

|Criterio|SQL Server Express|SQLite|
|---|---|---|
|Proceso de servidor|Requiere instalación y servicio en background|No — archivo único|
|Multiplataforma|Windows only (sin configuración extra)|Windows, Linux, macOS nativo|
|Instalación para el usuario|Compleja|Ninguna — incluida en el binario|
|EF Core + Code-First + Migrations|✓|✓|
|Importación/exportación Excel|Indiferente|Indiferente|
|Backup|Requiere herramientas SQL|Copiar el archivo `.db`|
|Concurrencia multiusuario|✓|No necesaria en este contexto|
|Tamaño para coleccionables|Sobredimensionado|Ajustado|

El único escenario donde SQL Server gana es acceso concurrente de múltiples usuarios sobre red — que no aplica aquí. El resto de características de SQL Server Express son overhead innecesario para una aplicación de escritorio de usuario único.

**Cambio de proveedor con Code-First:** reemplazar el proveedor en `PersistenceServiceRegistration.cs` y regenerar migraciones. Domain y Application no se tocan.

---

### UI: Migración a Avalonia — Completada

La migración de WPF a Avalonia está **completada**. El proyecto `TeejoshSystem.AvaloniaUI` reemplaza al anterior `WPF/`.

**Verificado en:** Windows y Linux (Fedora 43).

**Qué cambió:** Solo la capa de presentación — Views reescritas en XAML de Avalonia, reimplementación de `INotificationService` e `IConfirmationService`, ajuste de bindings.

**Qué no cambió:** Domain, Application, Infrastructure — exactamente como garantiza la arquitectura.

> Si la migración de UI hubiera requerido cambios en capas internas, habría indicado una violación arquitectónica.

---

## Problemas Conocidos

### Filtrado por tipo ejecutado en memoria

**Causa:** La tabla `product` no tiene columna discriminadora `type`. El filtrado se hace mediante joins en memoria a las tablas de detalle.  
**Solución futura:** Agregar columna `type` en `product`. Con Code-First esto se resuelve en la configuración Fluent y una migración, sin tocar el dominio.

---

### "Invalid object name 'ProductoDetalle'"

**Causa:** EF Core intentaba crear una tabla para la clase base abstracta `ProductoDetalle`.  
**Solución:** `builder.HasBaseType((Type)null)` en la configuración Fluent de cada clase derivada.

---

### Detalles no se guardaban al crear producto

**Causa:** Faltaban los métodos `Add...DetalleAsync()` en el repositorio.  
**Solución:** Implementados en `ProductoRepository` e invocados desde `CrearProductoCommandHandler`.

---

## Extensibilidad

La arquitectura hexagonal permite conectar nuevos adaptadores sin modificar el core.

### Nuevos Inbound Adapters (sin tocar Domain ni Application)

- API REST con ASP.NET Core
- CLI
- MAUI para mobile

Solo se necesita un nuevo proyecto que invoque los inbound ports existentes vía MediatR.

### Nuevos Outbound Adapters (sin tocar Domain ni Application)

- SQLite (recomendado — ver sección anterior)
- APIs externas de precios (ej.: TCGPlayer API)
- Cache en memoria para catálogos

Solo se necesita implementar `IProductoRepository` o `ICatalogoRepository` con la nueva tecnología y registrarla en DI.

---

## Convenciones

### Nomenclatura

|Elemento|Convención|Ejemplo|
|---|---|---|
|Clases|PascalCase|`ProductoRepository`|
|Interfaces|I + PascalCase|`IProductoRepository`|
|Métodos|PascalCase|`GetByIdAsync`|
|Variables privadas|`_` + camelCase|`_context`|
|Variables locales|camelCase|`producto`|
|Métodos async|Sufijo `Async`|`AddAsync`|

### Reglas arquitectónicas

- Ports → siempre interfaces
- Adapters → siempre implementan Ports
- DTOs → solo en Application
- Entidades de dominio → nunca cruzan hacia la UI
- Validaciones de negocio → en Domain (value objects)
- Validaciones de presentación → en la UI (INotifyDataErrorInfo)

### Buenas prácticas de código

- `async/await` en todas las operaciones I/O
- `ConfigureAwait(false)` en código de librería
- Cada método tiene una sola responsabilidad (SRP)
- Dependencias siempre inyectadas por constructor
- Sin lógica de negocio en ViewModels ni code-behind
- Clases que no serán heredadas: `sealed`

---

## Roadmap

### Alta prioridad 🔴

- [ ] Migración a Code-First + SQLite
- [ ] Módulo de ventas
- [ ] Validaciones adicionales (stock no negativo tras venta)
- [ ] Columna discriminadora `type` en tabla `product`

### Media prioridad 🟡

- [ ] Importación y exportación desde Excel
- [ ] Historial de cambios (audit log)
- [ ] Imágenes de productos

### Baja prioridad 🟢

- [ ] Autenticación y roles
- [ ] Backup automático de BD
- [ ] Temas claro/oscuro
- [ ] Internacionalización (i18n)
- [ ] API REST para consumo externo

---

## Nota Final

El dominio es estable. Los detalles técnicos son reemplazables.

> La arquitectura no se diseñó para la versión actual — se diseñó para todas las versiones futuras.