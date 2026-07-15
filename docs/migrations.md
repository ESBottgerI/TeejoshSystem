# Migraciones por proveedor

Las migraciones de SQLite y PostgreSQL viven en assemblies independientes:

- `TeejoshSystem.Migrations.Sqlite`
- `TeejoshSystem.Migrations.PostgreSql`

Infrastructure selecciona el assembly con `Database:Provider`; no combine migraciones de ambos proveedores. WebUI Production tiene `Database:ApplyMigrationsOnStartup=false`: al iniciar valida conexión y migraciones pendientes, pero nunca modifica el esquema.

## Bundle PostgreSQL

Desde la raíz:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations bundle --project TeejoshSystem.Migrations.PostgreSql/TeejoshSystem.Migrations.PostgreSql.csproj --startup-project TeejoshSystem.Migrations.PostgreSql/TeejoshSystem.Migrations.PostgreSql.csproj --context TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.InventarioDbContext --configuration Release --output artifacts/efbundle-postgresql.exe
```

Ejecución contra una base PostgreSQL vacía o existente:

```powershell
./artifacts/efbundle-postgresql.exe --connection "$env:TEEJOSH_POSTGRES_CONNECTION"
```

Tras ejecutar el bundle, inicie WebUI con `ASPNETCORE_ENVIRONMENT=Production`. La aplicación debe iniciar únicamente cuando `GetPendingMigrationsAsync()` esté vacío.