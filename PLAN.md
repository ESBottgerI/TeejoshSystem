## Análisis previo

### Inconsistencias detectadas

- `REGLAS.md` exige `sealed` donde aplique, pero `MainViewModel`, varios ViewModels, Services y code-behind no están sellados.
- `GestionarProductosViewModel` instancia `EditarProductoViewModel` con `new`, evitando DI para ese flujo.
- `MainViewModel` recibe `IServiceProvider`, pero actualmente no lo usa.
- `App.axaml` fuerza `Light` en línea 14, por lo que cualquier recurso dinámico no resolverá modo oscuro mientras esa propiedad siga fija.
- `ValidatableViewModel` ya implementa `INotifyDataErrorInfo`, pero las Views no presentan `DataValidationErrors`.
- `CrearProductoViewModel` y `EditarProductoViewModel` validan `Nombre`, `Precio` y `Unidades`; el plan no debe añadir reglas nuevas de negocio ni mover validaciones a dominio.

### Información faltante no crítica

- No se especifica nombre del archivo de preferencia. El plan fija `theme-preference.txt` en `%LocalAppData%\TeejoshSystem\`.
- No se especifica si el tema inicial debe ser claro, oscuro o sistema. El plan usa `Light` como fallback para conservar comportamiento actual.
- No se especifica diseño visual exacto del selector. El plan lo ubica en el shell, esquina superior derecha, sin alterar navegación.

### Dependencias técnicas

- Avalonia 11.3.12 soporta `RequestedThemeVariant`, `ThemeVariant`, `DynamicResource` y `ResourceDictionary.ThemeDictionaries`.
- CommunityToolkit.Mvvm ya está disponible para `ObservableProperty` y `RelayCommand`.
- La carpeta `%LocalAppData%\TeejoshSystem\` ya se crea desde Infrastructure para SQLite; el servicio de tema debe reutilizar la misma ruta lógica sin tocar la DB.
- `App.axaml.cs` es composition root actual para DI y creación de `MainWindow`.
- `MainWindow.axaml` ya recibe `MainViewModel` como `DataContext`.

### Posibles violaciones a evitar

- No persistir tema desde code-behind.
- No acceder a repositorios, EF Core ni DB desde ViewModels.
- No introducir lógica de negocio en ViewModels.
- No mover validaciones de presentación fuera de UI.
- No crear servicios estáticos ni service locator para tema.
- No usar code-behind de Views para alternar tema.
- No duplicar reglas de validación existentes.

## Refactors opcionales detectados

### Opción 1

Descripción: Sellar clases UI no heredadas: `MainViewModel`, ViewModels concretos, Services concretos y code-behind de Views/Window donde no exista necesidad de herencia.

Beneficio: Alinea el código con `REGLAS.md` y reduce superficie de extensión accidental.

Impacto: Bajo-medio. Cambia firmas de clases, puede requerir ajustar tests si usan proxies o herencia.

¿Deseas incluirlo? No incluido en el flujo principal salvo confirmación.

### Opción 2

Descripción: Reemplazar el `new EditarProductoViewModel(...)` dentro de `GestionarProductosViewModel` por una fábrica inyectada o patrón equivalente mínimo.

Beneficio: Respeta mejor dependencias por constructor y evita construcción manual de ViewModels con dependencias.

Impacto: Medio. Afecta `GestionarProductosViewModel`, DI y tests existentes.

¿Deseas incluirlo? No incluido en el flujo principal salvo confirmación.

### Opción 3

Descripción: Extraer recursos visuales de tema desde `App.axaml` a un diccionario dedicado, por ejemplo `Resources/ThemeResources.axaml`.

Beneficio: Mejora mantenibilidad si crecerán estilos globales.

Impacto: Bajo. Agrega un archivo nuevo y una inclusión en `App.axaml`.

¿Deseas incluirlo? No incluido en el flujo principal; el plan usa recursos en `App.axaml` para minimizar cambios.

### Opción 4

Descripción: Validar estado inicial y preguardado de `CrearProductoViewModel` y `EditarProductoViewModel` para que campos requeridos no dependan de que el usuario los toque primero.

Beneficio: Corrige un riesgo funcional relacionado con validaciones UI.

Impacto: Medio. Cambia comportamiento de habilitación de `GuardarCommand`.

¿Deseas incluirlo? No incluido en el flujo principal porque el objetivo pedido es presentación visual de errores existentes.

# Plan de implementación

## Fase 1 - Preparar recursos visuales adaptativos

### Objetivo

Crear una base mínima de recursos dinámicos para superficies y validación visual compatible con claro/oscuro, sin crear lógica de negocio ni afectar capas Domain/Application/Infrastructure.

### Dependencias

- Debe ejecutarse antes de reemplazar `Background="White"`.
- Debe ejecutarse antes de estilizar errores de validación.
- Depende de `FluentTheme` ya incluido en `App.axaml`.

### Archivos afectados

- `TeejoshSystem.AvaloniaUI/App.axaml`

### Cambios

#### Paso 1 de 4 — Agregar recursos de superficie por tema en `App.axaml`

- Agregar `Application.Resources`.
- Definir `ResourceDictionary.ThemeDictionaries`.
- Definir clave `TeejoshSurfaceBrush`.
- Valor Light: superficie clara equivalente al blanco actual.
- Valor Dark: superficie oscura legible y coherente con Fluent.
- Definir clave `TeejoshValidationErrorBrush`.
- Definir clave `TeejoshValidationErrorTextBrush`.
- No modificar `FluentTheme`.
- No crear recursos en Views individuales.

#### Paso 2 de 4 — Agregar estilo global mínimo para `DataValidationErrors`

- Agregar estilo en `Application.Styles`.
- Selector objetivo: `DataValidationErrors`.
- Objetivo visual: conservar el control original y mostrar los errores debajo del input.
- Usar `DataValidationErrors.Errors` como fuente del mensaje.
- Usar `DataValidationErrors.HasErrors` para mostrar/ocultar contenido de error.
- Usar `TeejoshValidationErrorTextBrush` para texto de error.
- No introducir converters nuevos salvo que Avalonia lo exija para compilación.
- No poner mensajes hardcodeados en XAML.

#### Paso 3 de 4 — Agregar estilo de borde inválido

- Agregar estilos para controles con error de validación.
- Controles objetivo: `TextBox` y `NumericUpDown`.
- Propiedad visual esperada: borde resaltado con `TeejoshValidationErrorBrush`.
- No cambiar validaciones existentes.
- No envolver cada campo manualmente si el estilo global de Avalonia cubre el mecanismo.

#### Paso 4 de 4 — Validar aislamiento de capa

- Confirmar que los recursos quedan en AvaloniaUI.
- Confirmar que no se toca Domain.
- Confirmar que no se toca Application.
- Confirmar que no se toca Infrastructure para visuales.

### Validaciones esperadas

- La app compila con `App.axaml`.
- Los recursos se resuelven con `DynamicResource`.
- Cambiar `RequestedThemeVariant` cambia las superficies sin reiniciar.
- No aparecen excepciones de recursos inexistentes.

---

## Fase 2 - Reemplazar fondos blancos hardcodeados

### Objetivo

Eliminar todos los `Background="White"` que rompen modo oscuro y reemplazarlos por el recurso dinámico de superficie.

### Dependencias

- Requiere `TeejoshSurfaceBrush` creado en Fase 1.

### Archivos afectados

- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Views/Productos/CrearProductoView.axaml`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Views/Productos/EditarProductoView.axaml`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Views/Productos/GestionarProductosView.axaml`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Views/Productos/InventarioView.axaml`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Views/Ventas/HistorialVentasView.axaml`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Views/Ventas/RegistrarVentaView.axaml`

### Cambios

#### Paso 1 de 6 — Actualizar `CrearProductoView.axaml`

- Reemplazar `Background="White"` por `Background="{DynamicResource TeejoshSurfaceBrush}"`.
- Aplica en líneas actuales 18 y 257.
- No modificar headers `#FF9800`.
- No modificar overlay `#CC000000`.

#### Paso 2 de 6 — Actualizar `EditarProductoView.axaml`

- Reemplazar `Background="White"` por `Background="{DynamicResource TeejoshSurfaceBrush}"`.
- Aplica en líneas actuales 17 y 47.
- No modificar headers `#FF9800`.

#### Paso 3 de 6 — Actualizar `GestionarProductosView.axaml`

- Reemplazar `Background="White"` por `Background="{DynamicResource TeejoshSurfaceBrush}"`.
- Aplica en líneas actuales 18 y 80.
- No modificar botones ni acentos existentes.

#### Paso 4 de 6 — Actualizar `InventarioView.axaml`

- Reemplazar `Background="White"` por `Background="{DynamicResource TeejoshSurfaceBrush}"`.
- Aplica en líneas actuales 24 y 89.
- No modificar estructura de DataGrid.

#### Paso 5 de 6 — Actualizar `HistorialVentasView.axaml`

- Reemplazar `Background="White"` por `Background="{DynamicResource TeejoshSurfaceBrush}"`.
- Aplica en líneas actuales 18 y 105.
- Mantener acento azul `#1565C0`.

#### Paso 6 de 6 — Actualizar `RegistrarVentaView.axaml`

- Reemplazar `Background="White"` por `Background="{DynamicResource TeejoshSurfaceBrush}"`.
- Aplica en líneas actuales 21, 63 y 144.
- Mantener acento verde `#43A047`.

### Validaciones esperadas

- Búsqueda posterior de `Background="White"` en `.axaml` devuelve cero resultados.
- Todas las Views siguen cargando.
- En modo claro, la apariencia se mantiene equivalente al estado actual.
- En modo oscuro, ningún panel queda blanco puro.

---

## Fase 3 - Persistir preferencia de tema

### Objetivo

Agregar un servicio UI para cargar y guardar la preferencia de tema en archivo separado de la base de datos, con interfaz y dependencia por constructor.

### Dependencias

- No depende de cambios visuales.
- Debe completarse antes de inicializar `MainViewModel` con tema persistido.

### Archivos afectados

- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Services/IThemePreferenceService.cs`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Services/ThemePreferenceService.cs`
- `TeejoshSystem.AvaloniaUI/App.axaml.cs`

### Cambios

#### Paso 1 de 4 — Crear puerto UI `IThemePreferenceService`

- Ubicación: `Adapters/Inbound/Services`.
- Responsabilidad única: cargar y guardar tema.
- Métodos async.
- No depender de EF Core.
- No depender de repositorios.
- No mezclar preferencia de tema con configuración de DB.

#### Paso 2 de 4 — Crear implementación `ThemePreferenceService`

- Clase concreta `sealed`.
- Ruta base: `Environment.SpecialFolder.LocalApplicationData`.
- Carpeta: `TeejoshSystem`.
- Archivo: `theme-preference.txt`.
- Valores válidos: `Light` y `Dark`.
- Fallback ante archivo inexistente: `Light`.
- Fallback ante contenido inválido: `Light`.
- Crear carpeta si no existe.
- Usar I/O async.
- No lanzar error al usuario por preferencia corrupta; recuperar con fallback.

#### Paso 3 de 4 — Registrar servicio en DI

- Archivo: `App.axaml.cs`.
- Registrar `IThemePreferenceService` con `ThemePreferenceService`.
- Lifetime recomendado: singleton.
- Mantener registros existentes de `INotificationService`, `IConfirmationService`, `NavigationService` e `INavigationService`.

#### Paso 4 de 4 — Verificar separación

- Confirmar que `ThemePreferenceService` queda solo en AvaloniaUI.
- Confirmar que no se modifica `PersistenceServiceRegistration`.
- Confirmar que no se modifica `InventarioDbContextFactory`.
- Confirmar que el archivo de preferencia no comparte nombre ni extensión con `inventario.db`.

### Validaciones esperadas

- Al iniciar sin archivo, tema inicial es claro.
- Al guardar oscuro, existe `%LocalAppData%\TeejoshSystem\theme-preference.txt`.
- El archivo contiene solo el valor de tema persistido.
- El archivo no afecta migraciones ni creación de SQLite.

---

## Fase 4 - Exponer estado y cambio de tema desde `MainViewModel`

### Objetivo

Centralizar estado global de tema en el shell ViewModel, sin code-behind de UI y sin lógica de negocio.

### Dependencias

- Requiere `IThemePreferenceService`.
- Debe completarse antes de enlazar `App.axaml` y `MainWindow.axaml`.

### Archivos afectados

- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/ViewModels/Shell/MainViewModel.cs`
- `TeejoshSystem.AvaloniaUI/App.axaml.cs`
- Tests UI si se agregan o ajustan en fase de verificación.

### Cambios

#### Paso 1 de 5 — Ajustar constructor de `MainViewModel`

- Inyectar `IThemePreferenceService`.
- Eliminar dependencia no usada de `IServiceProvider` si sigue sin propósito.
- Mantener dependencia por constructor.
- No acceder a archivos directamente desde `MainViewModel`.

#### Paso 2 de 5 — Agregar propiedad observable `ThemeVariant`

- Tipo: `Avalonia.Styling.ThemeVariant`.
- Valor inicial seguro: `ThemeVariant.Light`.
- Debe notificar cambios para que `App.axaml` actualice `RequestedThemeVariant`.

#### Paso 3 de 5 — Agregar opciones para ComboBox

- Exponer colección de opciones de tema en `MainViewModel`.
- Opciones visibles: `Claro`, `Oscuro`.
- Cada opción debe mapear a `ThemeVariant.Light` o `ThemeVariant.Dark`.
- No usar strings mágicos desde `MainWindow.axaml`.
- No localizar textos fuera del alcance actual.

#### Paso 4 de 5 — Agregar comando de cambio de tema

- Exponer comando desde `MainViewModel`.
- El comando actualiza `ThemeVariant`.
- El comando persiste mediante `IThemePreferenceService`.
- El comando no toca Views.
- El comando no toca `Application.Current` directamente.

#### Paso 5 de 5 — Agregar inicialización async de tema

- Agregar método de inicialización en `MainViewModel`.
- Cargar preferencia antes de mostrar `MainWindow` o antes de completar composición inicial.
- Actualizar opción seleccionada y `ThemeVariant` de forma consistente.
- Si falla carga, conservar `Light`.

### Validaciones esperadas

- `MainViewModel` puede construirse con mocks en tests.
- Cambiar opción actualiza `ThemeVariant`.
- Cambiar opción escribe archivo.
- Reiniciar app restaura el tema guardado.
- No hay llamadas a servicios de persistencia de dominio.

---

## Fase 5 - Enlazar tema global en `App.axaml`

### Objetivo

Eliminar el modo claro forzado y enlazar `RequestedThemeVariant` al estado global expuesto por `MainViewModel`.

### Dependencias

- Requiere `MainViewModel.ThemeVariant`.
- Requiere que `App.axaml.cs` asigne el `MainViewModel` como contexto de binding de la aplicación o enlace equivalente compatible con Avalonia.

### Archivos afectados

- `TeejoshSystem.AvaloniaUI/App.axaml`
- `TeejoshSystem.AvaloniaUI/App.axaml.cs`

### Cambios

#### Paso 1 de 3 — Reemplazar tema hardcodeado

- Quitar el valor fijo `Light` de `Application.RequestedThemeVariant`.
- Enlazar `RequestedThemeVariant` a `ThemeVariant`.
- Mantener `FluentTheme`.
- Mantener `StyleInclude` de DataGrid.

#### Paso 2 de 3 — Asignar contexto de aplicación

- En `App.axaml.cs`, resolver `MainViewModel` desde DI.
- Inicializar tema desde `MainViewModel`.
- Asignar el contexto necesario para que el binding de `App.axaml` resuelva `ThemeVariant`.
- Hacerlo antes de crear o mostrar `MainWindow`.

#### Paso 3 de 3 — Mantener composition root limpio

- No mover lógica de tema a `MainWindow.axaml.cs`.
- No usar eventos visuales.
- Mantener `MainWindow.DataContext = MainViewModel`.
- Mantener navegación existente.

### Validaciones esperadas

- `RequestedThemeVariant` cambia cuando cambia `MainViewModel.ThemeVariant`.
- No queda `<Application.RequestedThemeVariant>Light</Application.RequestedThemeVariant>`.
- La app arranca en el tema persistido.
- Si no hay preferencia, arranca en claro.

---

## Fase 6 - Agregar selector de tema al shell

### Objetivo

Exponer el cambio de tema desde un ComboBox siempre visible en `MainWindow.axaml`, esquina superior derecha.

### Dependencias

- Requiere opciones y comando/estado en `MainViewModel`.
- Requiere binding de tema global funcionando.

### Archivos afectados

- `TeejoshSystem.AvaloniaUI/MainWindow.axaml`

### Cambios

#### Paso 1 de 3 — Reestructurar barra superior

- Mantener `Border` superior en `Grid.Row="0"`.
- Reemplazar contenido simple por layout con título a la izquierda y selector a la derecha.
- Mantener altura actual de 50.
- Mantener título actual.
- No mover `ContentControl`.

#### Paso 2 de 3 — Agregar ComboBox de tema

- Ubicar en esquina superior derecha.
- Enlazar ItemsSource a opciones de tema de `MainViewModel`.
- Enlazar selección a la opción seleccionada de `MainViewModel`.
- Mostrar texto legible: `Claro` y `Oscuro`.
- No añadir lógica en `MainWindow.axaml.cs`.

#### Paso 3 de 3 — Ajustar legibilidad de shell

- Mantener texto de título en blanco.
- Verificar que ComboBox sea legible sobre `#2C3E50`.
- Si el ComboBox queda ilegible en modo oscuro, ajustar solo propiedades visuales del ComboBox en XAML, no lógica.

### Validaciones esperadas

- Selector visible en todas las pantallas.
- Selector no depende de `MenuPrincipalView`.
- Cambiar selección cambia tema inmediatamente.
- La selección permanece tras reiniciar.

---

## Fase 7 - Mostrar errores de validación en formularios

### Objetivo

Hacer visibles los errores existentes de `INotifyDataErrorInfo` en `CrearProductoView` y `EditarProductoView`, debajo de campos inválidos y con borde resaltado.

### Dependencias

- Requiere estilo global `DataValidationErrors` de Fase 1.
- Requiere validaciones existentes en `CrearProductoViewModel` y `EditarProductoViewModel`.

### Archivos afectados

- `TeejoshSystem.AvaloniaUI/App.axaml`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Views/Productos/CrearProductoView.axaml`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/Views/Productos/EditarProductoView.axaml`
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/ViewModels/Productos/CrearProductoViewModel.cs` solo si el binding necesita notificación adicional; no cambiar reglas.
- `TeejoshSystem.AvaloniaUI/Adapters/Inbound/ViewModels/Productos/EditarProductoViewModel.cs` solo si el binding necesita notificación adicional; no cambiar reglas.

### Cambios

#### Paso 1 de 4 — Validar bindings de campos en `CrearProductoView.axaml`

- Campo `Nombre`: usa `Text="{Binding Nombre}"`.
- Campo `Precio`: usa `Value="{Binding Precio}"`.
- Campo `Unidades`: usa `Value="{Binding Unidades}"`.
- Confirmar que el binding participa en validación Avalonia.
- No añadir TextBlocks manuales si `DataValidationErrors` global muestra el error.

#### Paso 2 de 4 — Validar bindings de campos en `EditarProductoView.axaml`

- Campo `Nombre`: usa `Text="{Binding Nombre}"`.
- Campo `Precio`: usa `Value="{Binding Precio}"`.
- Campo `Unidades`: usa `Value="{Binding Unidades}"`.
- Confirmar que el binding participa en validación Avalonia.
- No añadir mensajes duplicados.

#### Paso 3 de 4 — Mantener validaciones en ViewModels

- No mover `AddError`.
- No mover `ClearErrors`.
- No cambiar textos existentes.
- No agregar validaciones de negocio.
- No llamar repositorios.
- No consultar Domain desde View.

#### Paso 4 de 4 — Confirmar desaparición de errores

- Al volver `Nombre` a valor no vacío y máximo 50 caracteres, debe desaparecer mensaje y borde.
- Al volver `Precio` a valor válido, debe desaparecer mensaje y borde.
- Al volver `Unidades` a valor válido, debe desaparecer mensaje y borde.
- El botón Guardar debe seguir dependiendo de `HasErrors`.

### Validaciones esperadas

- En `CrearProductoView`, `Nombre` vacío muestra `El nombre es obligatorio.`
- En `CrearProductoView`, `Nombre` mayor a 50 muestra `Máximo 50 caracteres.`
- En `EditarProductoView`, errores equivalentes se muestran debajo del campo.
- El borde del input inválido se resalta.
- Al corregir, error visual desaparece sin reiniciar ni navegar.

---

## Fase 8 - Verificación técnica

### Objetivo

Confirmar que los cambios cumplen `REGLAS.md`, no rompen build y no introducen regresiones visuales básicas.

### Dependencias

- Todas las fases anteriores completas.

### Archivos afectados

- Ninguno obligatorio.
- Tests opcionales en `Tests/TeejoshSystem.AvaloniaUI.Tests` si se decide cubrir `MainViewModel` y `ThemePreferenceService`.

### Cambios

#### Paso 1 de 5 — Verificación estática

- Buscar `Background="White"` en `.axaml`; resultado esperado: cero.
- Buscar `RequestedThemeVariant>Light`; resultado esperado: cero.
- Buscar acceso directo a `theme-preference.txt` fuera de `ThemePreferenceService`; resultado esperado: cero.
- Buscar lógica de tema en code-behind de Views; resultado esperado: cero.

#### Paso 2 de 5 — Build

- Ejecutar build de solución.
- Corregir errores de XAML binding o recursos.
- No relajar `Nullable`.
- No desactivar compiled bindings globalmente.

#### Paso 3 de 5 — Tests existentes

- Ejecutar tests del proyecto `TeejoshSystem.AvaloniaUI.Tests`.
- Ejecutar tests de solución si el tiempo lo permite.
- Ajustar mocks si `MainViewModel` gana nueva dependencia.

#### Paso 4 de 5 — Prueba manual de tema

- Iniciar app sin archivo de preferencia.
- Verificar tema claro.
- Seleccionar oscuro.
- Verificar cambio inmediato.
- Cerrar app.
- Reabrir app.
- Verificar tema oscuro restaurado.
- Seleccionar claro.
- Reabrir app.
- Verificar tema claro restaurado.

#### Paso 5 de 5 — Prueba manual de validación

- Abrir crear producto.
- Vaciar `Nombre`.
- Verificar mensaje bajo campo.
- Verificar borde resaltado.
- Corregir `Nombre`.
- Verificar desaparición.
- Repetir en editar producto.
- Confirmar que no aparecen errores visuales duplicados.

### Validaciones esperadas

- Build exitoso.
- Tests existentes exitosos o fallos documentados si son previos.
- Cero fondos blancos hardcodeados.
- Tema persistente en archivo separado.
- Validaciones visibles y reactivas.
- Sin cambios en Domain/Application/Infrastructure salvo ninguno requerido.