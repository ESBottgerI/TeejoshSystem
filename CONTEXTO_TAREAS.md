Tarea 01: Feature: Rediseño UI/UX — identidad visual y experiencia de uso
Descripción:
La UI actual es funcional pero genérica. Background="White" hardcodeados rompen el modo oscuro ya implementado. Formularios sin jerarquía visual, feedback de errores ausente o invisible, menú principal como StackPanel básico. Es deuda técnica activa — cada nueva View hereda los mismos problemas. Es prerequisito de WebUI Blazor porque establece el lenguaje visual del sistema que Blazor heredará traducido a CSS. Construir Blazor antes implica rediseñar dos superficies en lugar de una.
Dependencias:
-Depende de: ninguna -Bloquea: FEAT: WebUI Blazor Server
Archivos clave:
-AvaloniaUI/App.axaml (ResourceDictionary de colores) -AvaloniaUI/Adapters/Inbound/Views/ (todas las Views) -AvaloniaUI/Adapters/Inbound/Views/Menu/MenuPrincipalView.axaml
criterio de aceptacion de tarea 01:
-ResourceDictionary definido con paleta de colores del sistema -Todos los Background="White" y colores hardcodeados eliminados -Las Views usan DynamicResource en lugar de colores literales -Tipografía consistente — jerarquía entre títulos, etiquetas y valores -Mensajes de error de validación visibles junto al campo (no solo notificación global) -Menú principal muestra nombre del sistema, versión y usuario activo -Estados IsBusy con indicador visual diferenciado (no bloqueo silencioso) -Confirmaciones destructivas (eliminar) con color de alerta diferenciado -Espaciado, márgenes y alineación consistentes en todas las Views -UI legible en modo claro y modo oscuro (verificar con toggle ya implementado)
Tarea 02: Feature: WebUI con Blazor Server
Descripción:
Canal web que permite consultar inventario, registrar ventas y gestionar catálogos desde cualquier dispositivo sin instalar software. Los componentes Blazor llaman IMediator.Send() directamente — mismos handlers, DTOs y reglas de dominio que Avalonia. Sin API REST separada. Avalonia sigue funcionando en desktop sin cambios. Blazor Server sobre React/Angular porque elimina la necesidad de un proyecto http://ASP.NET Core adicional como intermediario y mantiene C# como único lenguaje del sistema.Criterios de Aceptación:
Dependencias:
Depende de: FEAT: Supabase, UI: Rediseño UI/UX Bloquea: TECH: Internacionalización i18n
Archivos clave:
TeejoshSystem.WebUI/ (proyecto nuevo) TeejoshSystem.WebUI/Program.cs TeejoshSystem.WebUI/Components/Pages/ docker-compose.yml (nuevo)
criterio de aceptacion de tarea 01:
-Proyecto TeejoshSystem.WebUI creado y agregado al solution -AddInfrastructure y AddMediatR registrados en Program.cs de WebUI -Autenticación con Supabase Auth + JWT configurada -Rutas protegidas con: Authorize -Componente LoginPage funcional -Componente InventarioPage equivalente a InventarioView de Avalonia -Componente GestionarProductosPage con editar y eliminar -Componente CrearProductoPage con formulario dinámico por tipo -Componente VentasPage para registrar y consultar historial -Deploy funcional en VPS: 1. docker-compose.yml con app + PostgreSQL (o Supabase) 2. Nginx como reverse proxy (opcional) -El cliente desktop Avalonia sigue funcionando sin cambios