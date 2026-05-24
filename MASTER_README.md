# TeejoshSystem
### Documentación Técnica Maestra — README Corporativo & Onboarding

---

> *"El dominio es estable. Los detalles técnicos son reemplazables."*
> *"La arquitectura no se diseñó para la versión actual — se diseñó para todas las versiones futuras."*

---

## Índice

1. [Contexto del Proyecto](#1-contexto-del-proyecto)
2. [Dominio del Negocio](#2-dominio-del-negocio)
3. [Línea Evolutiva](#3-línea-evolutiva)
4. [Etapa 1 — Fundación (0.0.1 → 0.1.1)](#4-etapa-1--fundación-001--011)
5. [Evento Crítico](#5-evento-crítico)
6. [Etapa 2 — Sistema Productivo (0.1.2 → 0.2.0)](#6-etapa-2--sistema-productivo-012--020-beta1)
7. [Arquitectura Actual](#7-arquitectura-actual)
8. [Módulos del Sistema](#8-módulos-del-sistema)
9. [Modelo de Base de Datos](#9-modelo-de-base-de-datos)
10. [Stack Tecnológico](#10-stack-tecnológico)
11. [Onboarding — Primeros Pasos](#11-onboarding--primeros-pasos)
12. [Convenciones y Reglas](#12-convenciones-y-reglas)
13. [Tests](#13-tests)
14. [Deuda Técnica Activa](#14-deuda-técnica-activa)
15. [Arquitectura Dual](#15-visión-objetivo-de-arquitectura-dual)

---

## 1. Contexto del Proyecto

**TeejoshSystem** es una aplicación de escritorio *offline-first* para la gestión de inventario y ventas de una tienda de coleccionables. Fue desarrollado por el equipo **ELEFANTE TECNOLÓGICO S.A.C.** para el cliente **Teejosh S.A.C.**, como proyecto académico de la asignatura Ingeniería de Software en la **UNJBG**.

| Campo | Detalle |
|---|---|
| **Versión actual** | `0.2.0` |
| **Estado** | Activo — desarrollo en curso |
| **Cliente** | Teejosh S.A.C. — tienda de coleccionables |
| **Interlocutor** | José Luis Franchescoly (encargado de tienda) |
| **Plataformas verificadas** | Windows, Linux (Fedora 43) |
| **Persistencia** | SQLite — Code-First, migraciones EF Core |
| **UI** | Avalonia 11 (multiplataforma) |
| **Inicio de proyecto** | Septiembre 2025 |

### Objetivo del sistema

Gestionar inventario y ventas de una tienda de coleccionables multimarca con las siguientes garantías arquitectónicas:

- Evolucionar la UI sin afectar el dominio
- Cambiar la base de datos sin reescribir lógica de negocio
- Incorporar nuevos canales (API, CLI, web) sin modificar el core
- Mantener reglas de negocio consistentes y centralizadas

---

## 2. Dominio del Negocio

Teejosh S.A.C. comercializa **cinco tipos de producto coleccionable**, cada uno con atributos específicos:

| Tipo | Atributos propios |
|---|---|
| **Hot Wheels** | Modelo, año, serie, categoría |
| **Funko** | Número de caja, licencia, subtipo, característica especial |
| **TCG** (cartas) | Franquicia, expansión, pack |
| **Toy** | Edad mínima, jugadores mín/máx, es juego de mesa |
| **Varios** | Marca, dimensiones, material, tiene ilustración |

### Reglas de negocio críticas

**Variantes de producto:** Un mismo producto de catálogo puede existir como múltiples ítems vendibles (distintas ediciones, idiomas, formatos). Esta distinción — heredada desde `0.0.1` — es central al modelo.

**Precio histórico en ventas:** Cuando se registra una venta, el precio unitario y el nombre del producto se guardan como *snapshot* en `VentaDetalle`. Si el precio del producto cambia después, el historial de ventas permanece correcto.

```csharp
// VentaDetalle preserva el estado al momento de la transacción
public class VentaDetalle
{
    public string NombreProducto { get; }   // snapshot — no FK al nombre actual
    public decimal PrecioUnitario { get; }  // snapshot — no FK al precio actual
    public int Cantidad { get; }
}
```

**Validación de stock:** `Unidades` es un Value Object que encapsula la regla de no-negatividad. Una venta que dejaría stock negativo falla en dominio antes de tocar la base de datos.

**Catálogos TCG en cascada:** Los catálogos de expansión y pack se filtran por franquicia. La UI desactiva los ComboBoxes dependientes hasta que el usuario selecciona una franquicia.

---

## 3. Línea Evolutiva

```
Sep 2025                                                        May 2026
    │                                                               │
  0.0.1          0.1.0          0.1.1    ⚡    0.1.2        0.2.0-beta.1
    │              │              │      │       │               │
  [PHP]         [C# WPF]      [patch]  [EC]  [Avalonia]    [Sistema
 [PostgreSQL]  [SQL Server]  [naming]        [SQLite]       completo]
    │              │              │             │               │
 Etapa 1 ────────────────────────┤             ├─── Etapa 2 ───┤
                                              inicio

⚡ EC = Evento Crítico (pérdida de datos + limitaciones operativas)
```

| Versión | Nombre interno | Etapa | Descripción | Fecha |
|---|---|---|---|---|
| `0.0.1` | V1 | 1 | PoC web — PHP + PostgreSQL | Sep–Dic 2025 |
| `0.1.0` | 0.0.2-beta | 1 | Reescritura completa — C# + WPF + Clean Architecture | Dic 2025 |
| `0.1.1` | 0.3-beta | 1 | Patch de consolidación — naming .NET + Obsidian | Ene 2026 |
| `0.1.2` | 0.4-beta | **2** | Transformación — Avalonia + SQLite + Code-First | Abr 2026 |
| `0.2.0` | 0.5-beta | **2** | Sistema completo — Auth + Ventas + APIs + Tests | May 2026 |

---

## 4. Etapa 1 — Fundación (0.0.1 → 0.1.1)

### 4.1 Versión 0.0.1 — Prueba de concepto

La primera versión fue una aplicación web PHP con PostgreSQL, desarrollada en el contexto académico de Ingeniería de Software I. Su propósito fue **validar funcionalidad**, no construir arquitectura sostenible.

**Lo que se logró:**
- Dominio del negocio comprendido correctamente desde el inicio
- CRUD de inventario y registro de ventas operativo
- Regla de restock TCG (apertura de sellados → unidades individuales) modelada con stored procedures
- Autenticación básica

**Lo que se acumuló como deuda:**
- Arquitectura script-per-page — mezcla de presentación, lógica y acceso a datos en cada `.php`
- Lógica de negocio en stored procedures PostgreSQL — acoplamiento al motor de BD
- Sin separación de capas — cualquier cambio tocaba múltiples archivos
- Descoordinación de equipo — inconsistencias entre validaciones PHP y BD

### 4.2 Versión 0.1.0 — Reescritura arquitectónica

La transición de `0.0.1` a `0.1.0` fue una **reescritura completa** motivada por la expansión del dominio (de solo TCG a 5 tipos de producto) y la insostenibilidad de la arquitectura anterior.

El equipo adoptó en una sola iteración: **Clean Architecture + Hexagonal Architecture + DDD + CQRS + MVVM**, pasando de PHP a C#, de navegador a escritorio WPF, y de PostgreSQL a SQL Server Express.

**Decisiones técnicas fundacionales establecidas en 0.1.0:**

```
Outbound Ports (Domain)              Inbound Ports (Application)
─────────────────────────            ─────────────────────────────
IProductoRepository                  CrearProductoCommand
ICatalogoRepository                  ActualizarProductoCommand
                                     EliminarProductoCommand
                                     ObtenerProductosQuery
                                     BuscarProductosQuery
```

Estas interfaces — definidas en `0.1.0` — sobrevivieron sin modificación hasta `0.2.0-beta.1`. Son la columna vertebral del sistema.

**Costo deliberado:** El módulo de Ventas y la regla de restock no fueron migrados. El equipo priorizó establecer la arquitectura correctamente antes de agregar funcionalidad.

### 4.3 Versión 0.1.1 — Consolidación

Patch de normalización sin nuevas funcionalidades. Los cambios más relevantes fueron el renombrado de proyectos a nombres .NET calificados (`TeejoshInventario.Domain`, etc.) y la adopción de **Obsidian** como herramienta de documentación con árboles versionados por fecha.

**Señal metodológica:** El equipo generó un patch de consolidación antes de continuar. Eso indica crecimiento en la gestión del proyecto.

---

## 5. Evento Crítico

Entre enero y abril de 2026 ocurrió el evento que disparó la Etapa 2. Las evidencias en `0.1.2` permiten reconstruir sus componentes:

**Vector principal:** SQL Server Express como servicio Windows fue el punto de falla. La instalación en el equipo del cliente era compleja y frágil. La dependencia de un proceso de servidor en background —en un sistema diseñado para ser offline-first— era una contradicción estructural.

**Consecuencias registradas:**
- Pérdida o corrupción de datos en la BD SQL Server local
- Imposibilidad de operar en entornos no-Windows
- Fricción de despliegue inaceptable para el cliente

**Respuesta del equipo:** Migración simultánea de cuatro dimensiones aprovechando la arquitectura hexagonal de `0.1.x`:

| Dimensión | Antes | Después |
|---|---|---|
| Base de datos | SQL Server Express (servicio Windows) | SQLite (archivo embebido) |
| UI Framework | WPF (Windows only) | Avalonia (multiplataforma) |
| Enfoque BD | Database-First, sin migraciones | Code-First + EF Core Migrations |
| Plataformas | Windows | Windows + Linux (macOS futuro) |

**Domain y Application no fueron modificados.** Esto validó retroactivamente cada decisión de `0.1.0`.

---

## 6. Etapa 2 — Sistema Productivo (0.1.2 → 0.2.0)

### 6.1 Versión 0.1.2 — Transformación de infraestructura

Primera versión de Etapa 2. Cuatro migraciones ejecutadas simultáneamente sin regresión funcional. El proyecto fue renombrado de `TeejoshInventario` a `TeejoshSystem` — señal de que el equipo anticipó un alcance mayor que solo el inventario.

Los tres árboles con fecha (`03-04`, `04-04`, `05-04`) evidencian trazabilidad interna activa: el equipo documentaba su propio progreso día a día.

**Adición arquitectónica nueva:** `ProductoBusquedaResult` en Domain — primer tipo de retorno específico para búsquedas, ubicado en los Outbound Ports. El repositorio retorna un tipo fuertemente tipado en lugar de un DTO genérico.

### 6.2 Versión 0.2.0-beta.1 — Sistema completo

El salto más grande en la historia funcional del proyecto. En términos de estructura: de 4 proyectos y 2 puertos, a 6 proyectos (+ 4 de tests) y 8 puertos.

**Deuda funcional saldada:**

| Funcionalidad | Pendiente desde | Implementada en |
|---|---|---|
| Módulo de Ventas | `0.0.1` (Sep 2025) | `0.2.0-beta.1` (May 2026) |
| Autenticación de usuarios | `0.0.1` | `0.2.0-beta.1` |
| ObtenerProductoPorId | `0.1.0` | `0.2.0-beta.1` |

---

## 7. Arquitectura Actual

### 7.1 Clasificación

| Patrón | Implementación |
|---|---|
| Clean Architecture | 4 capas con dependencias hacia el dominio |
| Hexagonal (Ports & Adapters) | 8 Outbound Ports, 12+ Inbound Ports via CQRS |
| DDD | Entidades, Value Objects, Aggregates, Invariantes |
| CQRS | Commands/Queries separados, MediatR como dispatcher |
| MVVM | CommunityToolkit.Mvvm, DataTemplates, binding |
| Repository | Contratos en Domain, implementaciones en Infrastructure |
| Result Pattern | Sin excepciones para flujo de control de negocio |

### 7.2 Flujo de dependencias

```
┌──────────────────────────────────────────────────────────────┐
│  UI — Avalonia (Inbound Adapter)                             │
│  Views ←binding→ ViewModels → IMediator.Send(command/query)  │
└──────────────────────────┬───────────────────────────────────┘
                           │ MediatR dispatch
┌──────────────────────────▼───────────────────────────────────┐
│  Application — Casos de Uso (Inbound Ports)                  │
│  CommandHandlers / QueryHandlers → orquestan Domain          │
│  Retornan Result<T> o DTOs                                   │
└────────────┬────────────────────────────┬────────────────────┘
             │ usa entidades              │ llama via interfaces
┌────────────▼──────────┐   ┌────────────▼────────────────────┐
│  Domain (Core)        │   │  Infrastructure (Outbound Adapter│
│  Entities, VOs, Rules │   │  EF Core, SQLite, APIs externas  │
│  Outbound Ports       │◄──┤  implementa Outbound Ports       │
│  (interfaces)         │   │                                  │
└───────────────────────┘   └─────────────────────────────────┘
```

### 7.3 Flujo de ejecución completo

```
View (XAML)
  → data binding
ViewModel
  → mediator.Send(new CrearProductoCommand(...))
Application Handler
  → valida con Value Objects del Dominio
  → llama IProductoRepository.AddAsync(producto)
Domain (Producto, NombreProducto, Precio, Unidades)
  → invariantes protegidos internamente
Infrastructure (ProductoRepository)
  → EF Core → SQLite
  → retorna Result<int>
Result<T> regresa encapsulado por toda la cadena
ViewModel
  → notifica UI vía ObservableProperty
```

### 7.4 Reglas de dependencia — no negociables

| Capa | Puede depender de | Nunca depende de |
|---|---|---|
| **Domain** | Nada externo | Todas las demás capas |
| **Application** | Domain | Infrastructure, UI |
| **Infrastructure** | Domain, Application (solo para implementar ports) | UI |
| **UI (Avalonia)** | Application (via MediatR) | Domain directamente |

**Violaciones que rompen la arquitectura:**
- Referenciar EF Core, SQLite o cualquier framework externo desde `Domain`
- Acceder a repositorios directamente desde un ViewModel
- Instanciar `DbContext` fuera de Infrastructure
- Lógica de negocio en ViewModels o code-behind

### 7.5 Outbound Ports — mapa completo

```csharp
// Domain/Ports/Outbound/

// Persistencia de inventario
IProductoRepository      // CRUD + búsqueda de productos
ICatalogoRepository      // Lectura de catálogos relacionales

// Persistencia de ventas y usuarios
IVentaRepository         // Registro + consulta de ventas
IUsuarioRepository       // CRUD de usuarios del sistema

// Servicios externos
IAuthService             // Autenticación — verifica y hashea contraseñas
IImageStorageService     // Almacenamiento de imágenes de productos
ITcgCatalogoApiService   // Sincronización con TCGdex, Scryfall, YGOPRODeck

// Soporte técnico
IAppLogger               // Logging de eventos técnicos del sistema
```

Cada interface tiene exactamente una implementación activa. Agregar una implementación alternativa (ej: `SupabaseAuthService`) no requiere modificar ningún archivo fuera de Infrastructure y DI.

### 7.6 NavigationService

En `0.2.0-beta.1` el Shell Pattern implícito fue reemplazado por un `INavigationService` explícito:

```csharp
public interface INavigationService
{
    void NavigateTo(object viewModel);
    void NavigateToMenu();
}
```

Los ViewModels reciben `INavigationService` por constructor DI. La navegación es testeable, intercambiable y no está acoplada a `MainViewModel`.

### 7.7 SesionContext

```csharp
// Singleton en memoria — no persiste entre reinicios
public class SesionContext
{
    public bool EstaAutenticado => _sesionActual is not null;
    public SesionDto? SesionActual => _sesionActual;

    public void IniciarSesion(SesionDto sesion) { ... }
    public void CerrarSesion() => _sesionActual = null;
}
```

Toda la lógica de sesión vive aquí. Los ViewModels que requieren sesión la reciben por DI. Login obligatorio en cada arranque.

---

## 8. Módulos del Sistema

### 8.1 Inventario

CRUD completo de los 5 tipos de producto con formularios dinámicos. El tipo seleccionado determina qué campos adicionales se muestran. Búsqueda y filtrado por nombre y tipo ejecutados en SQL (no en memoria).

**Casos de uso:** `CrearProductoCommand`, `ActualizarProductoCommand`, `EliminarProductoCommand`, `ObtenerProductosQuery`, `BuscarProductosQuery`, `ObtenerProductosPorIdQuery`

### 8.2 Ventas

Registro de ventas multi-producto con validación de stock y captura de precio histórico. Consulta de historial con filtro por rango de fechas.

**Flujo de RegistrarVenta:**

```
RegistrarVentaCommand (lista de items: productoId + cantidad)
  → Validar lista no vacía (guard en constructor)
  → Por cada item:
      → Cargar Producto + validar existencia
      → Verificar stock suficiente (Unidades.Decrementar())
      → Capturar snapshot: NombreProducto + PrecioUnitario
  → Crear Venta con Total calculado
  → Persistir en transacción atómica (Venta + VentaDetalles)
  → Retornar Result<int>(id de venta)
```

**Casos de uso:** `RegistrarVentaCommand`, `ObtenerVentasQuery`

### 8.3 Autenticación y Usuarios

Login con contraseña hasheada (BCrypt — hash de 60 caracteres). Gestión de usuarios con roles. Soft-delete (desactivar sin eliminar). Cambio de contraseña verificando la actual.

```csharp
// LocalAuthService verifica con BCrypt — nunca almacena la contraseña
PasswordHash maxLength: 60   // exactamente el tamaño de un hash BCrypt
```

**Casos de uso:** `AutenticarUsuarioCommand`, `RegistrarUsuarioCommand`, `CambiarPasswordCommand`, `DesactivarUsuarioCommand`, `ListarUsuariosQuery`

### 8.4 Catálogos

Consulta de catálogos relacionales con carga en cascada (Franquicia TCG → Expansiones/Packs). Sincronización bajo demanda con APIs externas gratuitas:

| API | Franquicia |
|---|---|
| TCGdex | Pokémon |
| Scryfall | Magic: The Gathering |
| YGOPRODeck | Yu-Gi-Oh! |
| Manual (seed) | One Piece, Bluey, otros sin API |

**Casos de uso:** `ObtenerCatalogosQuery`, `ObtenerExpansionesYPacksQuery`, `SincronizarCatalogosCommand`, `ObtenerImagenExpansionQuery`

### 8.5 Imágenes

Almacenamiento local de imágenes de productos. Ruta guardada en `product.image_path`. Conversión a `Bitmap` mediante `PathToImageConverter` en la UI. La implementación `LocalImageStorageService` es intercambiable con `SupabaseImageStorageService` (implementación futura).

---

## 9. Modelo de Base de Datos

### 9.1 Estrategia: Code-First con SQLite

La base de datos se genera y actualiza desde el código mediante EF Core Migrations. Una migración (`20260510005610_InitialCreate`) crea todo el esquema.

**Ubicación:**

```
Windows:  C:\Users\<usuario>\AppData\Local\TeejoshSystem\inventario.db
Linux:    ~/.local/share/TeejoshSystem/inventario.db
```

El startup aplica migraciones pendientes automáticamente:

```csharp
// App.axaml.cs
db.Database.Migrate();  // crea BD si no existe, aplica migraciones pendientes
```

### 9.2 Estrategia de herencia — Table-Per-Concrete (TPC)

Cada tipo de producto concreto tiene su propia tabla sin tabla base intermedia. La herencia existe en C# para compartir comportamiento, no se refleja en el esquema.

```csharp
// EF Core Fluent API — en cada configuración de detalle
builder.HasBaseType((Type?)null);  // TPC: sin tabla base para ProductoDetalle abstracta
```

### 9.3 Esquema completo

```sql
-- INVENTARIO
product         (id, type TEXT, image_path TEXT NULL, name, price, units)
hot_wheels      (product_id PK FK, model, year, serie, category_id)
funko           (product_id PK FK, box_number, license, subtype_id, special_feature_id)
tcg             (product_id PK FK, pack_id, expansion_id)
toy             (product_id PK FK, min_years, min_players, max_players, board_game)
                CHECK (max_players >= min_players)
varios          (product_id PK FK, brand, height, width, length, material, illustration)
                CHECK (height > 0 AND width > 0 AND (length IS NULL OR length > 0))

-- CATÁLOGOS
hot_wheels_category     (id, name)
funko_subtype           (id, name)
funko_special_feature   (id, name)
tcg_franchise           (id, name)
tcg_expansion           (id, name, franchise_id, image_url TEXT NULL)
tcg_pack                (id, name, franchise_id)

-- VENTAS
sale            (id, date, total decimal(10,2))
sale_detail     (id, sale_id FK, product_id, product_name, unit_price, quantity)

-- AUTENTICACIÓN
app_user        (id, username UNIQUE, password_hash VARCHAR(60), rol, active DEFAULT 1)

-- CONTROL
__EFMigrationsHistory
```

Todas las tablas de detalle tienen `ON DELETE CASCADE` sobre `product_id`.

---

## 10. Stack Tecnológico

### Backend

| Librería | Versión | Rol |
|---|---|---|
| .NET | 8.0 | Framework base |
| C# | 12 | Lenguaje |
| Entity Framework Core | 8.0.0 | ORM |
| EF Core SQLite | 8.0.0 | Proveedor de BD |
| MediatR | 12.2.0 | Dispatcher CQRS |
| FluentValidation | 11.9.0 | Validaciones de Application |
| BCrypt.Net-Next | — | Hasheo de contraseñas |
| Microsoft.Extensions.Hosting | 8.0.0 | DI container y ciclo de vida |
| Microsoft.Extensions.Configuration | 8.0.0 | `appsettings.json` |

### Frontend

| Librería | Versión | Rol |
|---|---|---|
| Avalonia | 11.3.12 | UI multiplataforma |
| Avalonia.Controls.DataGrid | 11.3.12 | DataGrid (requiere StyleInclude) |
| CommunityToolkit.Mvvm | 8.2.2 | ObservableObject, RelayCommand |

### Testing

| Herramienta | Rol |
|---|---|
| SpecFlow | BDD — `.feature` files como especificaciones ejecutables |
| Stryker.NET | Mutation testing — calidad de los tests |
| Cobertura XML | Reporte de cobertura de código |

---

## 11. Onboarding — Primeros Pasos

### 11.1 Prerrequisitos

- .NET 8 SDK
- IDE: Visual Studio 2022+ o Rider o VS Code con C# Dev Kit
- Git

**No se requiere instalar ningún motor de base de datos.** SQLite está embebido en el binario.

### 11.2 Clonar y compilar

```bash
git clone <repositorio>
cd TeejoshSystem
dotnet build
```

### 11.3 Ejecutar la aplicación

```bash
dotnet run --project TeejoshSystem.AvaloniaUI
```

La base de datos se crea automáticamente en el primer arranque con todas las migraciones aplicadas. `DatabaseSeeder` inicializa los datos base (catálogos, usuario administrador inicial).

### 11.4 Ejecutar los tests

```bash
# Todos los tests
dotnet test

# Solo una capa
dotnet test Tests/TeejoshSystem.Domain.Tests
dotnet test Tests/TeejoshSystem.Application.Tests
dotnet test Tests/TeejoshSystem.Infrastructure.Tests
dotnet test Tests/TeejoshSystem.AvaloniaUI.Tests
```

### 11.5 Gestión de migraciones

```bash
# Crear nueva migración
dotnet ef migrations add <NombreMigracion> \
    --project TeejoshSystem.Infrastructure \
    --startup-project TeejoshSystem.AvaloniaUI \
    --output-dir Adapters/Outbound/Persistence/Migrations

# Aplicar migraciones
dotnet ef database update \
    --project TeejoshSystem.Infrastructure \
    --startup-project TeejoshSystem.AvaloniaUI

# Revertir última migración
dotnet ef migrations remove \
    --project TeejoshSystem.Infrastructure \
    --startup-project TeejoshSystem.AvaloniaUI
```

### 11.6 Cómo agregar una nueva funcionalidad

El flujo estándar para cualquier feature nueva:

```
1. Domain (si aplica)
   → Agregar entidad, value object, o método en entidad existente
   → Agregar interface de Port si se necesita nuevo adaptador

2. Application
   → Crear Command o Query con su Handler
   → Crear DTO de respuesta si aplica
   → El handler orquesta Domain y llama Ports (nunca Infrastructure directamente)

3. Infrastructure (si aplica)
   → Implementar nuevo Port
   → Agregar configuración Fluent API si hay nueva entidad
   → Registrar en DependencyInjection
   → Generar migración si cambia el esquema

4. UI (AvaloniaUI)
   → Crear ViewModel que inyecta IMediator
   → Crear View (.axaml) con bindings
   → Registrar DataTemplate en App.axaml
   → Registrar ViewModel en DI si corresponde

5. Tests
   → Agregar tests unitarios en la capa correspondiente
   → Agregar feature BDD en Application.Tests si es caso de uso de negocio
```

### 11.7 Cómo agregar un nuevo tipo de producto

```
1. Domain/Entities/Detalles/
   → NuevoTipoDetalle.cs (hereda de ProductoDetalle)

2. Domain/Enums/TipoProducto.cs
   → Agregar valor al enum

3. Infrastructure/Configurations/
   → NuevoTipoConfiguration.cs (Fluent API + HasBaseType(null))

4. Infrastructure/Repositories/ProductoRepository.cs
   → Agregar AddNuevoTipoDetalleAsync()
   → Actualizar switch en GetByIdWithDetalleAsync()

5. Application/Commands/CrearProducto/CrearProductoCommandHandler.cs
   → Agregar case en switch de tipo

6. UI/ViewModels/Productos/CrearProductoViewModel.cs
   → Agregar fields para los nuevos atributos
   → Agregar visibilidad condicional

7. Migración de BD
   → dotnet ef migrations add AgregarNuevoTipo
```

---

## 12. Convenciones y Reglas

### 12.1 Nomenclatura

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases | PascalCase | `ProductoRepository` |
| Interfaces | `I` + PascalCase | `IProductoRepository` |
| Métodos | PascalCase | `GetByIdAsync` |
| Campos privados | `_` + camelCase | `_context` |
| Variables locales | camelCase | `producto` |
| Métodos async | Sufijo `Async` | `AddAsync` |

### 12.2 Reglas arquitectónicas

```
Ports     → siempre interfaces (nunca clases concretas)
Adapters  → siempre implementan Ports (no herencia de clases de Application)
DTOs      → solo en Application/Common/Dtos (nunca en Domain ni Infrastructure)
Entidades → nunca cruzan hacia la UI (solo via DTOs)
Validaciones de negocio → en Domain (Value Objects y métodos de entidad)
Validaciones de presentación → en ViewModels (INotifyDataErrorInfo)
```

### 12.3 Buenas prácticas de código

```csharp
// ✅ async/await en todas las operaciones I/O
public async Task<Result<int>> Handle(CrearProductoCommand request, CancellationToken ct)

// ✅ ConfigureAwait(false) en código de librería
var result = await _repository.AddAsync(producto).ConfigureAwait(false);

// ✅ sealed en clases que no serán heredadas
public sealed class ProductoRepository : IProductoRepository

// ✅ Propiedades string en entidades EF — null! para constructor privado
public string NombreUsuario { get; private set; } = null!;

// ✅ Propiedades string en DTOs — required
public required string Nombre { get; set; }

// ✅ Sin lógica de negocio en ViewModels
// ❌ INCORRECTO:
if (stock < cantidad) { mostrarError("Sin stock"); }

// ✅ CORRECTO:
var result = await _mediator.Send(new RegistrarVentaCommand(items));
if (!result.IsSuccess) { _notification.Show(result.Error); }
```

### 12.4 Result Pattern

Todos los Commands retornan `Result` o `Result<T>`. Los Queries retornan directamente DTOs (las lecturas no fallan silenciosamente).

```csharp
// Command exitoso
return Result.Success(producto.Id);

// Command fallido — sin lanzar excepción
return Result.Failure("Stock insuficiente para completar la venta.");

// En el ViewModel
var result = await _mediator.Send(command);
if (result.IsSuccess)
    _navigation.NavigateToMenu();
else
    await _notification.ShowAsync(result.Error);
```

---

## 13. Tests

### 13.1 Estructura

| Proyecto | Técnica | Qué cubre |
|---|---|---|
| `Domain.Tests` | Unit + Stryker | Value Objects, invariantes de entidad |
| `Application.Tests` | BDD (SpecFlow) + Unit | Casos de uso como comportamientos |
| `Infrastructure.Tests` | Integration (DatabaseFixture) | Repositorios contra SQLite in-memory |
| `AvaloniaUI.Tests` | Unit | Lógica de ViewModels sin levantar UI |

### 13.2 BDD con SpecFlow

Los casos de uso principales están especificados como features ejecutables:

```gherkin
# Features/registrar_venta.feature
Feature: Registrar venta
  Como encargado de la tienda
  Quiero registrar una venta con múltiples productos
  Para llevar control del inventario y las transacciones

  Scenario: Venta exitosa con stock suficiente
    Given un producto "Booster Box Scarlet & Violet" con 10 unidades en stock
    When registro una venta de 2 unidades
    Then el stock debe quedar en 8 unidades
    And la venta debe registrarse con precio histórico
```

### 13.3 Mutation Testing

`stryker-config.json` en `Domain.Tests` y `Application.Tests` habilita Stryker.NET para verificar que los tests detectan cambios en el código.

```bash
# Ejecutar mutation testing en Domain
dotnet stryker --project TeejoshSystem.Domain.Tests
```

---

## 14. Deuda Técnica Activa

| ID | Descripción | Severidad | Estado |
|---|---|---|---|
| DT-02 | Apertura de sellados (restock TCG) — regla crítica ausente desde `0.0.1` | Alta | Sin planificación |
| DT-05 | Lenguaje mixto: dominio en español, BD en inglés | Baja | Aceptada |
| DT-10 | Selección de proveedor de BD por variable de entorno (prerequisito Supabase) | Media | Implementación parcial |
| DT-11 | Importación/exportación Excel | Media | Roadmap Bloque 2 |
| DT-12 | Historial de cambios (audit log) | Media | Roadmap Bloque 2 |
| DT-13 | UI con colores hardcodeados — rompe modo oscuro | Baja | En progreso |
| DT-14 | API REST + WebUI Blazor | Media | Roadmap Bloque 3 |
| DT-15 | Migración a PostgreSQL / despliegue en VPS (Supabase) | Media | Roadmap Bloque 3 |

---

## 15. Visión Objetivo de Arquitectura Dual

```
┌──────────────────────────────────────────────────────────┐
│  Desktop (Avalonia)         Web (Blazor Server en VPS)   │
│         ↓                              ↓                 │
│         └─────────── Application ──────────────────────  │
│                       (MediatR/CQRS)                     │
│                            ↓                             │
│                    Infrastructure                        │
│               ┌────────────┴──────────┐                  │
│            SQLite                 Supabase               │
│          (desarrollo)           (producción)             │
│                                      ↓                   │
│                        APIs externas de catálogos        │
│                   (TCGdex, Scryfall, YGOPRODeck)         │
└──────────────────────────────────────────────────────────┘
```

Los mismos handlers de Application, los mismos DTOs, las mismas reglas de dominio — en desktop y en web, sin duplicar lógica.

---

## Apéndice — Equipo

### Ingeniería de Software I

| Nombre | Rol(es) |
|---|---|
| Victor Rodrigo Ticona Quispe | Product Owner, Scrum Team |
| Miguel Alessandro Meza Garabito | Product Owner, Scrum Team |
| Antony Ronald Chagua Chique | Scrum Team, Backend Developer |
| Erik Stephano Böttger Isidro | Scrum Team, Backend Developer |
| Sergio Helber Medina Arohuanca | Scrum Team, Scrum Master |

**Patrocinador académico:** Manuel Yuri Apaza Valencia
**Institución:** Universidad Nacional Jorge Basadre Grohmann (UNJBG)

#### Ingeniería de Software II

| Nombre | Rol(es) |
|---|---|
| Erik Stephano Böttger Isidro | Product Owner, Scrum Master, Test Developer |
| Miguel Alessandro Meza Garabito | Product Owner, Scrum Team, Backend Developer |
| Antony Fernando Yucra Choquecota | Scrum Team, Frontend Developer |
| Antony Ronald Chagua Chique | Scrum Team, Backend Developer |

**Patrocinador académico:** Gianfranco Alexey Málaga Tejada
**Institución:** Universidad Nacional Jorge Basadre Grohmann (UNJBG)
