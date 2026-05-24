# TeejoshSystem

<div align="center">

**Aplicación de escritorio offline-first para gestión de inventario y ventas de coleccionables**

![Version](https://img.shields.io/badge/versión-0.2.0--beta.1-blue)
![Platform](https://img.shields.io/badge/plataforma-Windows%20%7C%20Linux-lightgrey)
![Framework](https://img.shields.io/badge/.NET-8.0-purple)
![UI](https://img.shields.io/badge/UI-Avalonia%2011-blueviolet)
![DB](https://img.shields.io/badge/BD-SQLite-003B57)
![Status](https://img.shields.io/badge/estado-activo-brightgreen)

</div>

---

## ¿Qué es?

TeejoshSystem es una aplicación de escritorio desarrollada para **Teejosh S.A.C.**, una tienda de coleccionables multimarca. Gestiona inventario y ventas de cinco tipos de producto: **Hot Wheels, Funkos, TCG (cartas), Toys y Varios**.

Funciona completamente **offline** — sin servicios externos en runtime, sin instalación de servidor de base de datos. Todo el sistema vive en un único archivo `.db`.

> No es un proyecto orientado a frameworks — es un proyecto orientado a estabilidad, evolución y desacoplamiento.

---

## Características

| Módulo | Capacidades |
|---|---|
| **Inventario** | CRUD de 5 tipos de producto · Formularios dinámicos · Búsqueda y filtrado · Imágenes |
| **Ventas** | Registro multi-producto · Precio histórico · Historial con filtros de fecha |
| **Catálogos** | Carga en cascada TCG · Sincronización con TCGdex, Scryfall, YGOPRODeck |
| **Auth** | Login con BCrypt · Gestión de usuarios y roles · Cambio de contraseña |
| **Sistema** | Temas claro/oscuro · Backup automático · Operación 100% offline |

---

## Stack

```
Lenguaje      C# 12 / .NET 8
UI            Avalonia 11 · CommunityToolkit.Mvvm
Base de datos SQLite · Entity Framework Core 8 (Code-First)
CQRS          MediatR 12
Validaciones  FluentValidation 11 · BCrypt.Net
Tests         SpecFlow (BDD) · Stryker.NET (Mutation) · xUnit
```

---

## Arquitectura

El sistema implementa **Clean Architecture + Hexagonal Architecture**, con capas que solo dependen hacia adentro:

```
UI (Avalonia)
      ↓
Infrastructure ──→ Domain
      ↓                ↑
  Application ─────────┘
```

| Capa | Responsabilidad |
|---|---|
| `Domain` | Entidades, Value Objects, Ports (interfaces) — sin dependencias externas |
| `Application` | Casos de uso — Commands y Queries via MediatR |
| `Infrastructure` | EF Core, SQLite, APIs externas, BCrypt, sistema de archivos |
| `AvaloniaUI` | ViewModels, Views, Servicios de UI |

**La regla es simple:** Domain no sabe que existe EF Core, Avalonia ni SQLite. Esto se validó en la práctica: la migración de WPF → Avalonia y SQL Server → SQLite se realizó sin tocar una sola línea de Domain ni Application.

### Puertos definidos

```
IProductoRepository    IVentaRepository       IAuthService
ICatalogoRepository    IUsuarioRepository     IImageStorageService
ITcgCatalogoApiService IAppLogger
```

Cada uno tiene una implementación local activa. Agregar adaptadores alternativos (ej: Supabase) no requiere tocar el dominio.

---

## Estructura del Proyecto

```
TeejoshSystem/
├── TeejoshSystem.Domain/           # Core — sin dependencias
│   ├── Entities/                   # Producto, Venta, Usuario, Detalles, Catálogos
│   ├── ValueObjects/               # NombreProducto, Precio, Unidades
│   ├── Enums/                      # TipoProducto, RolUsuario
│   └── Ports/Outbound/             # 8 interfaces de contratos
│
├── TeejoshSystem.Application/      # Casos de uso
│   ├── Common/                     # Result Pattern + DTOs
│   └── Ports/Inbound/              # Auth · Catalogos · Productos · Ventas
│
├── TeejoshSystem.Infrastructure/   # Implementaciones técnicas
│   └── Adapters/Outbound/
│       ├── Apis/                   # ScryfallAdapter, TcgdexAdapter, YgoprodeckAdapter
│       ├── Auth/                   # LocalAuthService, UsuarioRepository
│       ├── Backup/                 # BackupService
│       ├── Persistence/            # EF Core · SQLite · Migrations · Repositories
│       └── Storage/                # LocalImageStorageService
│
├── TeejoshSystem.AvaloniaUI/       # Presentación MVVM
│   └── Adapters/Inbound/
│       ├── ViewModels/             # Auth · Admin · Productos · Ventas · Catálogos
│       ├── Views/                  # Espejo de ViewModels
│       └── Services/               # Navigation · Session · Theme · Notification
│
└── Tests/                          # 4 proyectos de tests
    ├── Domain.Tests                # Unit + Stryker
    ├── Application.Tests           # BDD (SpecFlow) + Unit
    ├── Infrastructure.Tests        # Integration (SQLite in-memory)
    └── AvaloniaUI.Tests            # Unit (ViewModels)
```

---

## Inicio Rápido

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- No se requiere instalar ningún motor de base de datos

### Clonar y ejecutar

```bash
git clone <repositorio>
cd TeejoshSystem
dotnet run --project TeejoshSystem.AvaloniaUI
```

La base de datos se crea automáticamente en el primer arranque. El seeder inicializa catálogos y el usuario administrador inicial.

### Ubicación de la base de datos

```
Windows:  %LOCALAPPDATA%\TeejoshSystem\inventario.db
Linux:    ~/.local/share/TeejoshSystem/inventario.db
```

### Ejecutar los tests

```bash
dotnet test
```

---

## Base de Datos

Esquema generado por EF Core Code-First. Una sola migración (`InitialCreate`) construye todo el esquema desde cero.

```sql
product      (id, type, image_path, name, price, units)
hot_wheels   (product_id PK FK, model, year, serie, category_id)
funko        (product_id PK FK, box_number, license, subtype_id, special_feature_id)
tcg          (product_id PK FK, pack_id, expansion_id)
toy          (product_id PK FK, min_years, min_players, max_players, board_game)
varios       (product_id PK FK, brand, height, width, length, material, illustration)

sale         (id, date, total)
sale_detail  (id, sale_id, product_id, product_name, unit_price, quantity)

app_user     (id, username UNIQUE, password_hash VARCHAR(60), rol, active)
```

`product_name` y `unit_price` en `sale_detail` son **snapshots** del momento de la venta. El historial no se altera si el producto cambia de precio o nombre.

Los detalles usan **Table-Per-Concrete (TPC)** — cada tipo tiene su propia tabla completa, sin tabla base intermediaria. Implementado con `builder.HasBaseType((Type)null)` en cada configuración Fluent.

---

## Tests

| Proyecto | Técnica | Alcance |
|---|---|---|
| `Domain.Tests` | Unit + Mutation (Stryker) | Value Objects · invariantes de entidad |
| `Application.Tests` | BDD (SpecFlow) + Unit | Casos de uso como especificaciones ejecutables |
| `Infrastructure.Tests` | Integration (DatabaseFixture) | Repositorios contra SQLite in-memory |
| `AvaloniaUI.Tests` | Unit | Lógica de ViewModels sin levantar UI |

```bash
# Por capa
dotnet test Tests/TeejoshSystem.Domain.Tests
dotnet test Tests/TeejoshSystem.Application.Tests
dotnet test Tests/TeejoshSystem.Infrastructure.Tests
dotnet test Tests/TeejoshSystem.AvaloniaUI.Tests
```

---

## Migraciones EF Core

```bash
# Nueva migración
dotnet ef migrations add <Nombre> \
  --project TeejoshSystem.Infrastructure \
  --startup-project TeejoshSystem.AvaloniaUI \
  --output-dir Adapters/Outbound/Persistence/Migrations

# Aplicar
dotnet ef database update \
  --project TeejoshSystem.Infrastructure \
  --startup-project TeejoshSystem.AvaloniaUI
```

---

## Problemas Conocidos

| Estado | Problema | Solución |
|---|---|---|
| ✅ Resuelto | Filtrado por tipo en memoria | Columna `type` en `product` — filtro en SQL |
| ✅ Resuelto | `"Invalid object name 'ProductoDetalle'"` | `HasBaseType(null)` en configuraciones TPC |
| ✅ Resuelto | Detalles no se guardaban al crear | Métodos `Add...DetalleAsync()` implementados |
| ✅ Resuelto | `ObtenerProductoPorId` sin implementar | Query + Handler + `OnLoaded()` completados |
| ✅ Resuelto | DataGrid sin StyleInclude | `StyleInclude` en `App.axaml` |
| ✅ Resuelto | `.slnx` incompatible con Stryker y Linux | Migrado a `.sln` clásico |
| ⚠️ Pendiente | BD inválida tras cambio de esquema | Eliminar `inventario.db` manualmente y reiniciar |

---

## Roadmap

### Alta 🔴
- [x] Code-First + SQLite
- [x] Migración WPF → Avalonia (multiplataforma)
- [x] Módulo de ventas con precio histórico
- [x] Validaciones de stock en dominio
- [x] `ObtenerProductoPorId`

### Media 🟡
- [x] Autenticación BCrypt + gestión de usuarios
- [x] APIs de catálogos (TCGdex · Scryfall · YGOPRODeck)
- [x] Imágenes de productos
- [x] Historial de cambios · Mejoras de UI
- [ ] Importación / exportación Excel

### Baja 🟢
- [x] Backup automático · Temas claro/oscuro
- [ ] Despliegue en VPS con Supabase (PostgreSQL)
- [ ] API REST / WebUI Blazor Server
- [ ] Internacionalización (i18n)

---

## Extensibilidad

La arquitectura hexagonal permite agregar nuevos canales y tecnologías sin modificar Domain ni Application:

**Nuevos inbound adapters** (nuevos canales de acceso):
- API REST con ASP.NET Core
- WebUI Blazor Server
- CLI · MAUI mobile

**Nuevos outbound adapters** (nuevas tecnologías de soporte):
- `SupabaseAuthService` implementando `IAuthService`
- `SupabaseImageStorageService` implementando `IImageStorageService`
- PostgreSQL vía `UseNpgsql` en `PersistenceServiceRegistration`

---

## Equipo

Desarrollado por **ELEFANTE TECNOLÓGICO S.A.C.** como proyecto académico de Ingeniería de Software — UNJBG.

---

*El dominio es estable. Los detalles técnicos son reemplazables.*
