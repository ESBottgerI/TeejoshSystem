# TeejoshSystem

## Resumen

Sistema de gestión de inventario para coleccionables (Hot Wheels, Funkos, TCG, Toys y Varios) desarrollado bajo **Clean Architecture**, **Hexagonal Architecture**, **DDD**, **CQRS** y **MVVM**.

El sistema está diseñado como una aplicación de escritorio **offline-first**, con una arquitectura desacoplada que permite evolucionar la UI y la persistencia sin afectar el core del negocio.

---

## Descripción del Proyecto

**TeejoshInventario** permite gestionar productos coleccionables con estructuras variables según su tipo, manteniendo consistencia de dominio y separación estricta de responsabilidades.

### Capacidades clave

- Gestión de múltiples tipos de producto (Hot Wheels, Funko, TCG, Toy, Varios)
- Formularios dinámicos por tipo de producto
- Catálogos relacionales con carga en cascada
- Validaciones en tiempo real (INotifyDataErrorInfo)
- Búsqueda y filtrado por nombre y tipo
- CRUD completo con confirmaciones antes de acciones destructivas
- Arquitectura desacoplada (Ports & Adapters)

---

## Arquitectura

La arquitectura está modelada mediante **C4 Model** usando Structurizr.

### Niveles documentados

- **Context** → El usuario interactúa con el sistema de inventario
- **Container** → UI, Application, Domain, Infrastructure
- **Component** → Casos de uso, puertos y adaptadores internos

> La fuente de verdad arquitectónica es el archivo `.dsl` de Structurizr.

---

## Modelo Arquitectónico

### Clean Architecture

Separación estricta en 4 capas con dependencias unidireccionales hacia el núcleo:

|Capa|Rol|Dependencias|
|---|---|---|
|**Domain**|Núcleo del negocio: entidades, value objects, puertos|Ninguna|
|**Application**|Casos de uso, comandos, queries, DTOs|Solo Domain|
|**Infrastructure**|Persistencia con EF Core, implementaciones de repositorios|Domain + Application|
|**UI**|Interfaz WPF, ViewModels, vistas|Application + Infrastructure (solo DI)|

**¿Por qué esta separación?** El dominio concentra las reglas de negocio sin saber cómo se persisten ni cómo se presentan. Esto permite reemplazar la base de datos o la UI sin tocar la lógica central.

---

### Hexagonal Architecture (Ports & Adapters)

El sistema distingue entre lo que el negocio necesita y cómo esas necesidades se implementan:

- **Inbound Ports** → Interfaces que exponen casos de uso (Application)
- **Outbound Ports** → Contratos que el dominio define para sus dependencias
- **Inbound Adapters** → UI WPF (versión actual 0.4)
- **Outbound Adapters** → Persistencia con EF Core + SQL Server

**¿Por qué hexagonal?** Permite conectar nuevas interfaces (API REST, CLI, MAUI) o nuevos mecanismos de persistencia sin modificar Application ni Domain. El core permanece estable mientras los detalles evolucionan.

---

## Reglas de Dependencia

```
Domain ← Application ← Infrastructure
                  ↑
                  UI
```

|Capa|Puede depender de|No puede depender de|
|---|---|---|
|Domain|Nada|Todas|
|Application|Domain|Infrastructure, UI|
|Infrastructure|Domain|Application, UI|
|UI|Application|Domain directo|

> Infrastructure puede referenciar *Application* únicamente para implementar sus interfaces (outbound ports), nunca para invocar casos de uso.

### Violaciones que rompen la arquitectura

Estas situaciones se consideran errores arquitectónicos no negociables:

- Referenciar EF Core, SQL o frameworks externos desde `Domain`
- Acceder a repositorios directamente desde un ViewModel
- Instanciar `DbContext` fuera de Infrastructure
- Introducir lógica de negocio en ViewModels o code-behind

---

## Flujo de Ejecución

```
Inbound Adapter (UI)
  → Inbound Port (caso de uso via MediatR)
    → Application Handler
      → Domain (entidades, reglas, value objects)
        → Outbound Port (IProductoRepository)
          → Outbound Adapter (Infrastructure / EF Core)
            → Base de datos
```

1. La UI invoca un comando o query a través de MediatR
2. El handler de Application ejecuta la lógica coordinando entidades
3. El Domain aplica reglas de negocio y valida invariantes
4. Infrastructure implementa el acceso a datos sin que el dominio lo sepa
5. El resultado regresa encapsulado en un `Result<T>`

---

## Patrones Implementados

|Patrón|Descripción|Ubicación|
|---|---|---|
|**Hexagonal / Clean Architecture**|Separación por capas con puertos y adaptadores|Todo el proyecto|
|**DDD**|Entidades, Value Objects, Agregados|`Domain/`|
|**CQRS**|Comandos (escritura) separados de Queries (lectura)|`Application/Ports/Inbound/`|
|**MVVM**|Separación vista-lógica-estado|`WPF/Adapters/Inbound/`|
|**Repository**|Abstracción de persistencia|`Domain/Ports/Outbound/`|
|**Result Pattern**|Manejo de errores sin excepciones|`Application/Common/Result.cs`|
|**Mediator**|Desacoplamiento entre handlers y casos de uso|MediatR via Application|
|**Dependency Injection**|Composición del sistema en startup|App.xaml.cs|

---

## Decisiones Técnicas

### CQRS sin Event Sourcing

Los comandos (escritura) y las queries (lectura) están separados en handlers distintos bajo `Application/Ports/Inbound/`. MediatR actúa como dispatcher interno.

**¿Por qué CQRS sin eventos?** El sistema no requiere auditoría de estados intermedios ni reconstrucción de estado a partir de eventos. CQRS simplificado aporta la separación de intenciones (leer vs. modificar) sin la complejidad operativa del Event Sourcing.

---

### Result Pattern en lugar de excepciones

Todos los casos de uso retornan `Result.Success()` o `Result.Failure(error)` en lugar de lanzar excepciones para el flujo de control.

**¿Por qué?** Las excepciones tienen overhead de captura de stack trace y dificultan el razonamiento sobre los caminos posibles de ejecución. Con `Result<T>`, el caller está obligado a manejar explícitamente el caso de error, los mensajes son más descriptivos para el usuario final, y no existe costo de excepciones en rutas normales.

---

### Value Objects en el Dominio

`NombreProducto`, `Precio` y `Unidades` encapsulan sus propias reglas de validación y son inmutables.

**¿Por qué?** Centralizar la validación en el value object garantiza que un `Precio` nunca puede existir en estado inválido en ninguna parte del sistema. Evita validaciones duplicadas dispersas en handlers, ViewModels o repositorios. Se mapean con `OwnsOne()` en EF Core sin necesidad de tablas adicionales.

---

### Database-First con Fluent API

La base de datos preexiste. EF Core mapea las tablas mediante 12 archivos de configuración Fluent API. No se usan migraciones.

**¿Por qué?** El esquema de base de datos fue definido antes que la aplicación y tiene dependencias externas. Usar Database-First permite trabajar sobre él sin riesgo de que EF genere cambios destructivos. Las configuraciones Fluent API otorgan control total sobre el mapeo sin atarse a convenciones de nombres.

**Limitación conocida:** cambios en el esquema requieren intervención manual tanto en la BD como en las configuraciones.

---

### Table-Per-Type (TPT)

Cada subtipo de producto (HotWheels, Funko, TCG, Toy, Varios) tiene su propia tabla relacionada 1:1 con `product` mediante `product_id`.

**¿Por qué TPT y no Table-Per-Hierarchy (TPH)?** TPH almacena todos los subtipos en una sola tabla con columnas nullable para los campos específicos de cada tipo. Con 5 tipos de producto con campos muy distintos entre sí, TPH generaría una tabla con una gran cantidad de NULLs, dificultando consultas y comprometiendo la integridad. TPT mantiene el esquema normalizado y cada tabla tiene sentido por sí misma.

---

### Navegación por estado (Shell Pattern) sin NavigationService

`MainViewModel.CurrentView` controla la vista activa. Los DataTemplates en XAML mapean automáticamente cada ViewModel a su View. Los ViewModels se resuelven desde el contenedor DI.

**¿Por qué?** Introducir un NavigationService cuando no hay navegación compleja (historial, deep linking, parámetros de ruta) es sobreingeniería. El Shell Pattern resuelve la navegación de forma simple, testeable y consistente con MVVM: la vista solo reacciona al estado del ViewModel central.

---

### Modo Offline

La aplicación opera completamente sin conexión. La base de datos es local (SQL Server Express). No existen dependencias de red en el flujo operativo normal.

La conectividad se reserva únicamente para operaciones explícitas iniciadas por el usuario (ej.: importación desde APIs externas). Si no hay red, la aplicación continúa sin errores ni degradación de funcionalidad.

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

### Tablas de detalles (1:1 con `product`)

```sql
hot_wheels (
    product_id  INT PRIMARY KEY,
    model       NVARCHAR(50),
    year        INT,
    serie       NVARCHAR(50),
    category_id INT,
    FOREIGN KEY (product_id) REFERENCES product(id) ON DELETE CASCADE
)

funko (
    product_id          INT PRIMARY KEY,
    box_number          INT,
    license             NVARCHAR(50),
    subtype_id          INT,
    special_feature_id  INT NULL,
    FOREIGN KEY (product_id) REFERENCES product(id) ON DELETE CASCADE
)

tcg (
    product_id   INT PRIMARY KEY,
    pack_id      INT,
    expansion_id INT,
    FOREIGN KEY (product_id) REFERENCES product(id) ON DELETE CASCADE
)

toy (
    product_id   INT PRIMARY KEY,
    min_years    INT,
    min_players  INT,
    max_players  INT,
    board_game   BIT,
    FOREIGN KEY (product_id) REFERENCES product(id) ON DELETE CASCADE
)

varios (
    product_id   INT PRIMARY KEY,
    brand        NVARCHAR(50),
    height       DECIMAL(5,2),
    width        DECIMAL(5,2),
    length       DECIMAL(5,2) NULL,
    material     NVARCHAR(50),
    illustration BIT,
    FOREIGN KEY (product_id) REFERENCES product(id) ON DELETE CASCADE
)
```

### Tablas de catálogos

```sql
hot_wheels_category    (id, name)
funko_subtype          (id, name)
funko_special_feature  (id, name)
tcg_franchise          (id, name)
tcg_expansion          (id, name, franchise_id)
tcg_pack               (id, name, franchise_id)
```

La carga de expansiones y packs de TCG es en cascada: primero se selecciona la franquicia, y solo entonces se habilitan y cargan los combos dependientes.

---

## Estructura del Proyecto

```
Domain/           # Núcleo: entidades, value objects, puertos
Application/      # Casos de uso: commands, queries, DTOs
Infrastructure/   # Persistencia: EF Core, repositorios, DbContext
WPF/              # UI: ViewModels, Views, Services, Converters
```

### Domain

```
Domain/
├── Entities/
│   ├── Producto.cs
│   ├── Catalogos/          # FunkoSubtipo, TcgFranquicia, TcgExpansion...
│   └── Detalles/           # FunkoDetalle, HotWheelsDetalle, TcgDetalle...
├── ValueObjects/
│   ├── NombreProducto.cs
│   ├── Precio.cs
│   └── Unidades.cs
├── Enums/
│   └── TipoProducto.cs
└── Ports/
    └── Outbound/
        ├── IProductoRepository.cs
        └── ICatalogoRepository.cs
```

### Application

```
Application/
├── Common/
│   └── Result.cs
└── Ports/
    └── Inbound/
        ├── Productos/
        │   ├── Commands/   # Crear, Actualizar, Eliminar
        │   └── Queries/    # ObtenerProductos, BuscarProductos, ObtenerPorId
        └── Catalogos/
            └── Queries/    # ObtenerCatalogos, ObtenerExpansionesYPacks
```

### Infrastructure

```
Infrastructure/
├── DependencyInjection/
└── Adapters/
    └── Outbound/
        └── Persistence/
            ├── InventarioDbContext.cs
            ├── Configurations/     # Fluent API por entidad (12 archivos)
            └── Repositories/
                ├── ProductoRepository.cs
                └── CatalogoRepository.cs
```

### WPF

```
WPF/
├── App.xaml / App.xaml.cs          # DI y startup
├── appsettings.json
├── MainWindow.xaml
└── Adapters/
    └── Inbound/
        ├── ViewModels/             # Shell, Menu, Productos
        ├── Views/                  # Menu, Productos
        ├── Services/               # Notificaciones, confirmaciones
        ├── Behaviors/              # SelectedItemsBehavior (DataGrid)
        └── Converters/             # InverseBool, ObjectToBool
```

---

## Tecnologías

### Backend (.NET 8)

|Librería|Versión|Propósito|
|---|---|---|
|.NET|8.0|Framework base|
|Entity Framework Core|8.0.0|ORM|
|SQL Server Express|—|Base de datos local|
|MediatR|12.2.0|Dispatcher CQRS|
|FluentValidation|11.9.0|Validaciones de Application (opcional)|

### Frontend (WPF)

|Librería|Versión|Propósito|
|---|---|---|
|WPF|.NET 8|Interfaz de usuario|
|CommunityToolkit.Mvvm|8.2.2|ObservableObject, RelayCommand|
|Microsoft.Extensions.Hosting|8.0.0|DI container|
|Microsoft.Extensions.Configuration|8.0.0|appsettings.json|

---

## Problemas Conocidos

### Filtrado por tipo no funciona a nivel SQL

**Causa:** La tabla `product` no tiene columna discriminadora `type`.  
**Estado actual:** El filtrado se realiza en memoria mediante joins a las tablas de detalle.  
**Solución futura:** Agregar columna `type` en `product` como discriminador.

---

### "Invalid object name 'ProductoDetalle'"

**Causa:** EF Core generaba una tabla para la clase base abstracta `ProductoDetalle`.  
**Solución:** Usar `builder.HasBaseType((Type)null)` en la configuración Fluent de cada clase derivada.

---

### Detalles no se guardaban al crear producto

**Causa:** Faltaban los métodos `Add...DetalleAsync()` en el repositorio.  
**Solución:** Implementados en `ProductoRepository` e invocados desde `CrearProductoCommandHandler`.

---

## Evolución Planeada

### Database-First → Code-First

El esquema actual fue heredado. La evolución natural es que las clases del dominio pasen a definir el esquema.

**Cambios:**

- Las entidades y configuraciones Fluent pasan a ser la fuente de verdad
- Se introducen migraciones (`Migrations/`) en Infrastructure
- Se elimina la dependencia del esquema preexistente
- Domain y Application no requieren cambios

---

### WPF → Avalonia

Avalonia permite ejecutar la misma aplicación en Windows, macOS y Linux.

**Qué cambia:** Solo el proyecto `WPF/` se reescribe.  
**Qué no cambia:** Domain, Application, Infrastructure permanecen virtualmente intactos.

> Si un cambio de UI requiere modificar completamente capas internas, es indicador de una violación arquitectónica.

**Ajustes necesarios en la migración:**

- Reescritura de Views en XAML compatible con Avalonia
- Adaptación de bindings y triggers
- Reimplementación de `INotificationService` e `IConfirmationService`

---

## Extensibilidad

Gracias a la arquitectura hexagonal, el sistema admite nuevos adaptadores sin modificar el core.

### Nuevos Inbound Adapters

- API REST (ASP.NET Core)
- CLI
- Avalonia / MAUI

Solo se necesita crear el nuevo adapter que invoque los inbound ports existentes.

### Nuevos Outbound Adapters

- APIs externas (ej.: precios TCG via API pública)
- SQLite para contextos más ligeros
- Cache en memoria

Solo se necesita implementar `IProductoRepository` o `ICatalogoRepository` con la nueva tecnología.

> El dominio define contratos. La infraestructura los cumple. Cambiar la infraestructura no requiere cambiar el dominio.

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

### Buenas prácticas

- `async/await` en todas las operaciones I/O
- `ConfigureAwait(false)` en código de librería
- Cada método tiene una sola responsabilidad (SRP)
- Dependencias siempre por constructor
- Sin lógica de negocio en ViewModels ni code-behind
- Clases que no serán heredadas: `sealed`

### Reglas arquitectónicas

- Los Ports siempre son interfaces
- Los Adapters siempre implementan Ports
- Los DTOs existen solo en Application
- Las entidades de dominio nunca cruzan hacia la UI
- Las validaciones de negocio viven en Domain
- Las validaciones de presentación viven en la UI

---

## Roadmap

### Alta prioridad 🔴

- [ ] Módulo de ventas
- [ ] Validaciones de negocio adicionales (stock no negativo tras venta)
- [ ] Columna discriminadora `type` en tabla `product`

### Media prioridad 🟡

- [ ] Importación desde Excel
- [ ] Historial de cambios (audit log)
- [ ] Imágenes de productos

### Baja prioridad 🟢

- [ ] Autenticación y roles
- [ ] Backup automático de BD
- [ ] Temas claro/oscuro
- [ ] Internacionalización (i18n)
- [ ] API REST para consumo externo

---

## Estado del Proyecto

**Versión:** 0.4 — beta  
**Estado:** Activo  
**Arquitectura:** Estable  
**Última actualización:** Abril 2026

---

## Nota Final

El dominio es estable. Los detalles técnicos son reemplazables.

> La arquitectura no se diseñó para la versión actual — se diseñó para todas las versiones futuras.
