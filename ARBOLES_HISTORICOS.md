# TeejoshSystem — Árboles Históricos de Proyecto
prubea para el ations p

Registro cronológico de la estructura del proyecto en cada versión documentada.

---

## 0.0.1 — Etapa 1 (Septiembre–Diciembre 2025)

> Designación interna: V1 | Plataforma: Web (PHP + PostgreSQL)

```
Etapa1_V1/
├── busqueda/
│   ├── buscar.php
│   └── formulario_busqueda.php
├── css/
│   ├── reset.css
│   └── styles.css
├── includes/
│   ├── db_connect.php
│   └── navbar.php
├── inventario/
│   ├── inventario.php
│   ├── inventario_insertar.php
│   ├── inventario_eliminar.php
│   ├── items.php
│   ├── modificar_producto.php
│   ├── op_modificar.php
│   ├── reabastecimiento.php
│   └── restock_item.php
├── ventas/
│   ├── venta.php
│   └── registrar_venta.php
├── index.php
├── login.php
├── logout.php
└── README.md
```

---

## 0.1.0 — Etapa 1 (Diciembre 2025)

> Designación interna: 0.0.2-beta | Plataforma: Escritorio (C# + WPF + SQL Server)

```
TeejoshInventario/
├── TeejoshInventario.Domain/
│   ├── Entities/
│   │   ├── Producto.cs
│   │   ├── Catalogos/
│   │   │   ├── FunkoCaracteristica.cs
│   │   │   ├── FunkoSubtipo.cs
│   │   │   ├── HotWheelsCategoria.cs
│   │   │   ├── TcgExpansion.cs
│   │   │   ├── TcgFranquicia.cs
│   │   │   └── TcgPack.cs
│   │   └── Detalles/
│   │       ├── ProductoDetalle.cs
│   │       ├── FunkoDetalle.cs
│   │       ├── HotWheelsDetalle.cs
│   │       ├── TcgDetalle.cs
│   │       ├── ToyDetalle.cs
│   │       └── VariosDetalle.cs
│   ├── ValueObjects/
│   │   ├── NombreProducto.cs
│   │   ├── Precio.cs
│   │   └── Unidades.cs
│   ├── Enums/
│   │   └── TipoProducto.cs
│   └── Ports/
│       └── Outbound/
│           ├── IProductoRepository.cs
│           └── ICatalogoRepository.cs
│
├── TeejoshInventario.Application/
│   ├── Common/
│   │   └── Result.cs
│   └── Ports/
│       └── Inbound/
│           ├── Productos/
│           │   ├── Commands/
│           │   │   ├── CrearProducto/
│           │   │   ├── ActualizarProducto/
│           │   │   └── EliminarProducto/
│           │   └── Queries/
│           │       ├── ObtenerProductos/
│           │       ├── BuscarProductos/
│           │       └── ObtenerProductosPorId/
│           └── Catalogos/
│               └── Queries/
│                   ├── ObtenerCatalogos/
│                   └── ObtenerExpansionesYPacks/
│
├── TeejoshInventario.Infrastructure/
│   ├── DependencyInjection/
│   │   ├── InfrastructureServiceRegistration.cs
│   │   └── PersistenceServiceRegistration.cs
│   └── Adapters/
│       └── Outbound/
│           └── Persistence/
│               ├── InventarioDbContext.cs
│               ├── Configurations/             (12 archivos Fluent API)
│               └── Repositories/
│                   ├── ProductoRepository.cs
│                   └── CatalogoRepository.cs
│
└── TeejoshInventario.WPF/
    ├── App.xaml / App.xaml.cs
    ├── appsettings.json
    ├── MainWindow.xaml / MainWindow.xaml.cs
    └── Adapters/
        └── Inbound/
            ├── ViewModels/
            │   ├── Common/     (ViewModelBase, ValidatableViewModel)
            │   ├── Shell/      (MainViewModel)
            │   ├── Menu/       (MenuPrincipalViewModel)
            │   └── Productos/  (Inventario, Gestionar, Crear, Editar)
            ├── Views/
            │   ├── Menu/
            │   └── Productos/
            ├── Services/       (Notification, Confirmation)
            ├── Behaviors/      (SelectedItemsBehavior)
            └── Converters/     (InverseBoolConverter, ObjectToBoolConverter)
```

---

## 0.1.1 — Etapa 1 · Árbol · 06-01-2026

> Designación interna: 0.3-beta | Patch de consolidación — renombrado a nombres .NET calificados

```
TeejoshInventario/
├── TeejoshInventario.Domain/
│   ├── Entities/
│   │   ├── Producto.cs
│   │   ├── Catalogos/
│   │   │   ├── FunkoCaracteristica.cs
│   │   │   ├── FunkoSubtipo.cs
│   │   │   ├── HotWheelsCategoria.cs
│   │   │   ├── TcgExpansion.cs
│   │   │   ├── TcgFranquicia.cs
│   │   │   └── TcgPack.cs
│   │   └── Detalles/
│   │       ├── FunkoDetalle.cs
│   │       ├── HotWheelsDetalle.cs
│   │       ├── ProductoDetalle.cs
│   │       ├── TcgDetalle.cs
│   │       ├── ToyDetalle.cs
│   │       └── VariosDetalle.cs
│   ├── ValueObjects/
│   │   ├── NombreProducto.cs
│   │   ├── Precio.cs
│   │   └── Unidades.cs
│   ├── Enums/
│   │   └── TipoProducto.cs
│   └── Ports/
│       └── Outbound/
│           ├── ICatalogoRepository.cs
│           └── IProductoRepository.cs
│
├── TeejoshInventario.Application/
│   ├── Common/
│   │   └── Result.cs
│   └── Ports/
│       └── Inbound/
│           ├── Productos/
│           │   ├── Commands/
│           │   │   ├── ActualizarProducto/
│           │   │   ├── CrearProducto/
│           │   │   └── EliminarProducto/
│           │   └── Queries/
│           │       ├── BuscarProductos/
│           │       ├── ObtenerProductos/
│           │       └── ObtenerProductosPorId/
│           └── Catalogos/
│               └── Queries/
│                   ├── ObtenerCatalogos/
│                   └── ObtenerExpansionesYPacks/
│
├── TeejoshInventario.Infrastructure/
│   ├── DependencyInjection/
│   │   ├── InfrastructureServiceRegistration.cs
│   │   └── PersistenceServiceRegistration.cs
│   └── Adapters/
│       └── Outbound/
│           └── Persistence/
│               ├── InventarioDbContext.cs
│               ├── Configurations/             (12 archivos Fluent API)
│               └── Repositories/
│                   ├── CatalogoRepository.cs
│                   └── ProductoRepository.cs
│
└── TeejoshInventario.WPF/
    ├── App.xaml / App.xaml.cs
    ├── appsettings.json
    ├── AssemblyInfo.cs
    ├── MainWindow.xaml / MainWindow.xaml.cs
    └── Adapters/
        └── Inbound/
            ├── ViewModels/
            │   ├── Common/     (ViewModelBase, ValidatableViewModel)
            │   ├── Menu/       (MenuPrincipalViewModel)
            │   ├── Productos/  (Inventario, Gestionar, Crear, Editar)
            │   └── Shell/      (MainViewModel)
            ├── Views/
            │   ├── Menu/
            │   └── Productos/
            ├── Services/       (Notification, Confirmation)
            ├── Behaviors/      (SelectedItemsBehavior)
            └── Converters/
                └── InverseBoolConverter.cs     ← ObjectToBoolConverter.cs ELIMINADO
```

---

## 0.1.2 — Etapa 2 · Árbol · 03-04-2026

> Designación interna: 0.4-beta | Primer snapshot — migración en curso

```
TeejoshSystem/
├── README.md
├── TeejoshSystem.slnx
│
├── TeejoshSystem.Domain/
│   ├── Entities/
│   │   ├── Producto.cs
│   │   ├── Catalogos/              (sin cambios)
│   │   └── Detalles/               (sin cambios)
│   ├── ValueObjects/               (sin cambios)
│   ├── Enums/
│   │   └── TipoProducto.cs
│   └── Ports/
│       └── Outbound/
│           └── Repositories/
│               ├── ICatalogoRepository.cs
│               └── IProductoRepository.cs
│
├── TeejoshSystem.Application/
│   ├── Common/
│   │   └── Result.cs
│   └── Ports/
│       └── Inbound/
│           ├── Catalogos/Queries/  (ObtenerCatalogos, ObtenerExpansionesYPacks)
│           └── Productos/
│               ├── Commands/       (Crear, Actualizar, Eliminar)
│               └── Queries/        (ObtenerProductos, BuscarProductos, ObtenerPorId)
│
├── TeejoshSystem.Infrastructure/
│   ├── Adapters/
│   │   └── Outbound/
│   │       └── Persistence/
│   │           ├── InventarioDbContext.cs
│   │           ├── Configurations/             (12 archivos)
│   │           └── Repositories/
│   │               ├── CatalogoRepository.cs
│   │               └── ProductoRepository.cs
│   └── DependencyInjection/
│
└── TeejoshSystem.AvaloniaUI/       ← reemplaza WPF
    ├── App.axaml / App.axaml.cs
    ├── app.manifest
    ├── appsettings.json
    ├── MainWindow.axaml / MainWindow.axaml.cs
    ├── Program.cs                  ← NUEVO (entry point Avalonia)
    └── Adapters/
        └── Inbound/
            ├── Services/           (Notification, Confirmation)
            ├── ViewModels/         (Common, Menu, Productos, Shell)
            └── Views/
                ├── Menu/
                └── Productos/      ⚠️ VACÍO — migración en curso
```

---

## 0.1.2 — Etapa 2 · Árbol · 04-04-2026

> Segundo snapshot — Code-First activado, DTOs centralizados

```
TeejoshSystem/
├── TeejoshSystem.Domain/           (sin cambios vs 03-04)
│
├── TeejoshSystem.Application/
│   ├── Common/
│   │   ├── Result.cs
│   │   └── Dtos/                   ← NUEVO — centralización de DTOs
│   │       ├── CatalogoItemDto.cs
│   │       ├── ProductoDetalladoDto.cs
│   │       └── ProductoDto.cs
│   └── Ports/                      (sin cambios)
│
├── TeejoshSystem.Infrastructure/
│   └── Adapters/
│       └── Outbound/
│           └── Persistence/
│               ├── InventarioDbContextFactory.cs   ← NUEVO
│               ├── Migrations/                     ← NUEVO (Code-First)
│               └── (resto sin cambios)
│
└── TeejoshSystem.AvaloniaUI/       (sin cambios vs 03-04)
```

---

## 0.1.2 — Etapa 2 · Árbol · 05-04-2026 (estado final)

> Tercer snapshot — estado final de la versión 0.1.2

```
TeejoshSystem/
├── README.md
├── TeejoshSystem.slnx
│
├── TeejoshSystem.Domain/
│   ├── Entities/
│   │   ├── Producto.cs
│   │   ├── Catalogos/
│   │   └── Detalles/
│   ├── ValueObjects/
│   ├── Enums/
│   │   └── TipoProducto.cs
│   └── Ports/
│       └── Outbound/
│           └── Repositories/
│               ├── ProductoBusquedaResult.cs   ← NUEVO (05-04)
│               ├── IProductoRepository.cs
│               └── ICatalogoRepository.cs
│
├── TeejoshSystem.Application/
│   ├── Common/
│   │   ├── Result.cs
│   │   └── Dtos/
│   │       ├── CatalogoItemDto.cs
│   │       ├── ProductoDetalladoDto.cs
│   │       └── ProductoDto.cs
│   └── Ports/
│       └── Inbound/
│           ├── Productos/
│           │   ├── Commands/   (Crear, Actualizar, Eliminar)
│           │   └── Queries/    (ObtenerProductos, BuscarProductos, ObtenerPorId)
│           └── Catalogos/
│               └── Queries/    (ObtenerCatalogos, ObtenerExpansionesYPacks)
│
├── TeejoshSystem.Infrastructure/
│   ├── Adapters/
│   │   └── Outbound/
│   │       └── Persistence/
│   │           ├── InventarioDbContext.cs
│   │           ├── InventarioDbContextFactory.cs
│   │           ├── Configurations/   (12 archivos Fluent API)
│   │           ├── Migrations/       (Code-First activo)
│   │           └── Repositories/
│   │               ├── ProductoRepository.cs
│   │               └── CatalogoRepository.cs
│   └── DependencyInjection/
│       ├── InfrastructureServiceRegistration.cs
│       └── PersistenceServiceRegistration.cs
│
└── TeejoshSystem.AvaloniaUI/
    ├── Program.cs
    ├── App.axaml / App.axaml.cs
    ├── MainWindow.axaml / MainWindow.axaml.cs
    ├── appsettings.json
    └── Adapters/
        └── Inbound/
            ├── ViewModels/     (Common, Shell, Menu, Productos)
            ├── Views/          (Menu, Productos — migración completada)
            └── Services/       (Notification, Confirmation)
```

---

## 0.2.0 — Etapa 2 · Estado actual (Mayo 2026)

> Designación interna: 0.5-beta | Estado de producción activo

```
TeejoshSystem/
├── README.md
├── TeejoshSystem.sln
│
├── TeejoshSystem.Domain/
│   ├── Entities/
│   │   ├── Producto.cs
│   │   ├── Venta.cs                  ← NUEVO
│   │   ├── Usuario.cs                ← NUEVO
│   │   ├── Catalogos/
│   │   │   ├── FunkoCaracteristica.cs
│   │   │   ├── FunkoSubtipo.cs
│   │   │   ├── HotWheelsCategoria.cs
│   │   │   ├── TcgExpansion.cs
│   │   │   ├── TcgFranquicia.cs
│   │   │   └── TcgPack.cs
│   │   └── Detalles/
│   │       ├── ProductoDetalle.cs
│   │       ├── FunkoDetalle.cs
│   │       ├── HotWheelsDetalle.cs
│   │       ├── TcgDetalle.cs
│   │       ├── ToyDetalle.cs
│   │       ├── VariosDetalle.cs
│   │       └── VentaDetalle.cs       ← NUEVO
│   ├── ValueObjects/
│   │   ├── NombreProducto.cs
│   │   ├── Precio.cs
│   │   └── Unidades.cs
│   ├── Enums/
│   │   ├── TipoProducto.cs
│   │   └── RolUsuario.cs             ← NUEVO
│   └── Ports/
│       └── Outbound/
│           ├── Repositories/
│           │   ├── IProductoRepository.cs
│           │   ├── ICatalogoRepository.cs
│           │   ├── IVentaRepository.cs         ← NUEVO
│           │   └── ProductoBusquedaResult.cs
│           ├── Auth/
│           │   ├── IAuthService.cs             ← NUEVO
│           │   └── IUsuarioRepository.cs       ← NUEVO
│           ├── IImageStorageService.cs         ← NUEVO
│           ├── IAppLogger.cs                   ← NUEVO
│           └── ITcgCatalogoApiService.cs       ← NUEVO
│
├── TeejoshSystem.Application/
│   ├── Common/
│   │   ├── Result.cs
│   │   └── Dtos/
│   │       ├── CatalogoItemDto.cs
│   │       ├── ProductoDetalladoDto.cs
│   │       ├── ProductoDto.cs
│   │       ├── VentaDto.cs           ← NUEVO
│   │       ├── SessionDto.cs         ← NUEVO
│   │       └── UsuarioListaDto.cs    ← NUEVO
│   └── Ports/
│       └── Inbound/
│           ├── Auth/                 ← NUEVO módulo completo
│           │   ├── Commands/
│           │   │   ├── AutenticarUsuario/
│           │   │   ├── CambiarPassword/
│           │   │   ├── DesactivarUsuario/
│           │   │   └── RegistrarUsuario/
│           │   └── Queries/
│           │       └── ListarUsuarios/
│           ├── Catalogos/
│           │   ├── Commands/
│           │   │   └── SincronizarCatalogos/   ← NUEVO
│           │   └── Queries/
│           │       ├── ObtenerCatalogos/
│           │       ├── ObtenerExpansionesYPacks/
│           │       └── ObtenerImagenExpansion/  ← NUEVO
│           ├── Productos/
│           │   ├── Commands/         (Crear, Actualizar, Eliminar — sin cambios)
│           │   └── Queries/
│           │       ├── BuscarProductos/
│           │       ├── ObtenerProductos/
│           │       └── ObtenerProductosPorId/  ← COMPLETADO
│           └── Ventas/               ← NUEVO módulo completo
│               ├── Commands/
│               │   └── RegistrarVenta/
│               └── Queries/
│                   └── ObtenerVentas/
│
├── TeejoshSystem.Infrastructure/
│   ├── Adapters/
│   │   └── Outbound/
│   │       ├── Apis/                 ← NUEVO
│   │       │   ├── ScryfallAdapter.cs
│   │       │   ├── TcgdexAdapter.cs
│   │       │   └── YgoprodeckAdapter.cs
│   │       ├── Auth/                 ← NUEVO
│   │       │   ├── LocalAuthService.cs
│   │       │   └── UsuarioRepository.cs
│   │       ├── Backup/               ← NUEVO
│   │       │   └── BackupService.cs
│   │       ├── Logging/              ← NUEVO
│   │       │   └── AppLogger.cs
│   │       ├── Persistence/
│   │       │   ├── Configurations/   (15 archivos — +3 nuevos)
│   │       │   ├── Repositories/
│   │       │   │   ├── CatalogoRepository.cs
│   │       │   │   ├── ProductoRepository.cs
│   │       │   │   └── VentaRepository.cs      ← NUEVO
│   │       │   ├── DatabaseSeeder.cs            ← NUEVO
│   │       │   ├── InventarioDbContext.cs
│   │       │   └── InventarioDbContextFactory.cs
│   │       └── Storage/              ← NUEVO
│   │           └── LocalImageStorageService.cs
│   ├── DependencyInjection/
│   │   ├── InfrastructureServiceRegistration.cs
│   │   └── PersistenceServiceRegistration.cs
│   └── Migrations/
│       ├── 20260510005610_InitialCreate.cs
│       ├── 20260510005610_InitialCreate.Designer.cs
│       └── InventarioDbContextModelSnapshot.cs
│
├── TeejoshSystem.AvaloniaUI/
│   ├── Program.cs
│   ├── App.axaml / App.axaml.cs
│   ├── MainWindow.axaml / MainWindow.axaml.cs
│   ├── appsettings.json
│   ├── appsettings.Production.json   ← NUEVO
│   └── Adapters/
│       └── Inbound/
│           ├── Converters/
│           │   ├── InverseBoolConverter.cs
│           │   └── PathToImageConverter.cs     ← NUEVO
│           ├── Helpers/
│           │   └── ControlExtensions.cs        ← NUEVO
│           ├── Services/
│           │   ├── IConfirmationService / ConfirmationService
│           │   ├── INotificationService / NotificationService
│           │   ├── INavigationService / NavigationService  ← NUEVO
│           │   ├── ILoadable.cs                ← NUEVO
│           │   ├── SesionContext.cs            ← NUEVO
│           │   └── IThemePreferenceService / ThemePreferenceService ← NUEVO
│           ├── ViewModels/
│           │   ├── Admin/
│           │   │   ├── CambiarPasswordViewModel.cs  ← NUEVO
│           │   │   └── GestionarUsuariosViewModel.cs ← NUEVO
│           │   ├── Auth/
│           │   │   └── LoginViewModel.cs       ← NUEVO
│           │   ├── Catalogos/
│           │   │   └── SincronizarCatalogosViewModel.cs ← NUEVO
│           │   ├── Common/
│           │   │   ├── TipoProductoFiltroItem.cs ← NUEVO
│           │   │   ├── ValidatableViewModel.cs
│           │   │   └── ViewModelBase.cs
│           │   ├── Menu/MenuPrincipalViewModel.cs
│           │   ├── Productos/        (Inventario, Gestionar, Crear, Editar)
│           │   ├── Shell/MainViewModel.cs
│           │   └── Ventas/
│           │       ├── HistorialVentasViewModel.cs  ← NUEVO
│           │       └── RegistrarVentaViewModel.cs   ← NUEVO
│           └── Views/                (espejo completo de ViewModels)
│               ├── Admin/   ├── Auth/   ├── Catalogos/
│               ├── Menu/    ├── Productos/   └── Ventas/
│
└── Tests/                            ← NUEVO proyecto raíz
    ├── TEST_README.md
    ├── TeejoshSystem.Domain.Tests/
    │   ├── Entities
    │   │   ├── ProductoTests.cs
    │   │   ├── Venta.cs
    │   │   └── Detalles/
    │   ├── ValueObjects/
    │   ├── TestResults/coverage.cobertura.xml
    │   ├── stryker-config.json
    │   └── Ports/Outbound/Auth/AutenticacionResultadoTests.cs
    ├── TeejoshSystem.Application.Tests/
    │   ├── stryker-config.json
    │   ├── Gherkin/ (BDD SpecFlow)
    │   │   ├── Features/ (BDD SpecFlow)
    │   │   │   ├── buscar_productos.feature
    │   │   │   ├── crear_producto.feature
    │   │   │   └── registrar_venta.feature
    │   │   ├── StepDefinitions/
    │   │   │   ├── BuscarProductoSteps.cs
    │   │   │   ├── CrearProductoSteps.cs
    │   │   │   └── RegistrarVentaSteps.cs
    │   │   └── Support/
    │   │       └── TestContext.cs
    │   └── Ports/Inbound
    │        ├── Auth/
    │        ├── Catalogos/
    │        ├── Productos/
    │        └── Ventas/
    ├── TeejoshSystem.Infrastructure.Tests/
    │   ├── Fixtures/DatabaseFixture.cs
    │   └── Repositories/ProductoRepositoryTests.cs
    └── TeejoshSystem.AvaloniaUI.Tests/
        ├── Productos/ProductoViewModelTests.cs
        └── Ventas/VentaViewModelTests.cs
```

---

## Resumen de evolución estructural

| Versión | Proyectos | Entidades | Puertos | Adaptadores Infra | Tests |
|---|---|---|---|---|---|
| `0.0.1` | 1 (monolito PHP) | 9 tablas SQL | — | — | — |
| `0.1.0` | 4 (.NET) | 1 + 5 detalles | 2 | 2 repos | — |
| `0.1.1` | 4 (.NET) | Sin cambios | Sin cambios | Sin cambios | — |
| `0.1.2` | 4 (.NET) | Sin cambios | +1 resultado | +DbContextFactory | — |
| `0.2.0` | **6 + 4 tests** | +3 nuevas | **8 total** | **+10 nuevos** | **4 proyectos** |
