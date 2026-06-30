# LogiticaOC — Sistema de Gestión de Órdenes de Compra

Sistema interno de seguimiento logístico de OC. Reemplaza el control en Excel.

## Requisitos

| Herramienta | Versión mínima |
|---|---|
| .NET SDK | 8.0 |
| SQL Server | 2019 / LocalDB |
| dotnet-ef (global tool) | 8.0.11 |

## Instalación rápida

```bash
# 1. Clonar / descomprimir el proyecto
cd LogiticaOC

# 2. Configurar la connection string (si no usás LocalDB)
# Editar: src/LogiticaOC.Presentation/appsettings.json
# Cambiar: "Server=(localdb)\mssqllocaldb" por tu instancia SQL Server

# 3. Aplicar schema a la BD (opción A — automático al arrancar)
dotnet run --project src/LogiticaOC.Presentation
# La app aplica migrations automáticamente al iniciar

# 3. Aplicar schema a la BD (opción B — script SQL manual)
# Ejecutar en SSMS o Azure Data Studio:
scripts/OC_Logistica_schema.sql

# 4. Navegar a
https://localhost:5001
```

## Arquitectura

```
LogiticaOC/
├── src/
│   ├── LogiticaOC.Domain/           # Entidades, interfaces (sin dependencias)
│   ├── LogiticaOC.Application/      # DTOs, contratos de use cases
│   ├── LogiticaOC.Infrastructure/   # EF Core, repositorios, ExportService
│   └── LogiticaOC.Presentation/     # ASP.NET MVC, Controllers, Views
└── scripts/
    └── OC_Logistica_schema.sql      # Script SQL idempotente (migrations)
```

## Funcionalidades

- **Dashboard** con métricas: pendientes, entregadas, críticas
- **Alertas por color**: rojo (≤20% de días), amarillo (≤40%), verde
- **CRUD completo** de OC con detalle de productos (líneas)
- **Historial automático** de cambios de estado y responsable
- **Guías de despacho** agregables por modal AJAX
- **Autocomplete** de entidades (crea nuevas automáticamente)
- **Upload/Download de PDF** adjunto por OC (guardado en BD)
- **Export PDF** de OC individual (QuestPDF)
- **Hoja de Ruta Excel** agrupada por responsable (ClosedXML) + cambio automático de estado a "Listo para Despacho"

## Estados de OC

| Estado | Color |
|---|---|
| Pendiente | Azul |
| Listo para Despacho | Violeta |
| Entrega Parcial | Amarillo |
| Cerrada | Verde |
| Cancelada | Gris |

## Connection String

Por defecto usa **LocalDB** (incluida con Visual Studio). Para SQL Server Express:

```json
"Server=.\\SQLEXPRESS;Database=OC_Logistica;Trusted_Connection=True;"
```
