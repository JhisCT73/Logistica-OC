# Guía de Configuración y Funcionamiento — LogiticaOC

## Contenido
- [Requisitos previos](#requisitos-previos)
- [Configuración de la base de datos](#configuración-de-la-base-de-datos)
- [Primera ejecución](#primera-ejecución)
- [Cómo funciona el sistema](#cómo-funciona-el-sistema)
- [Flujo de una OC](#flujo-de-una-oc)
- [Módulos del sistema](#módulos-del-sistema)
- [Preguntas frecuentes](#preguntas-frecuentes)

---

## Requisitos previos

| Herramienta | Versión | Para qué |
|---|---|---|
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.x | Compilar y ejecutar la app |
| SQL Server | 2019 o superior | Base de datos |
| SQL Server Express (alternativa gratuita) | cualquiera | Base de datos local sin costo |
| LocalDB (alternativa incluida con VS) | cualquiera | Base de datos solo para desarrollo |

> **Si tenés Visual Studio instalado, LocalDB ya viene incluido** y no necesitás instalar nada más de base de datos.

---

## Configuración de la base de datos

El archivo de configuración está en:

```
LogiticaOC/
└── src/
    └── LogiticaOC.Presentation/
        └── appsettings.json   ← ESTE ARCHIVO
```

### Opción A — LocalDB (solo desarrollo, viene con Visual Studio)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OC_Logistica;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
✅ No requiere instalar nada adicional. La base de datos `OC_Logistica` se crea automáticamente.

---

### Opción B — SQL Server Express (instalación propia, recomendado para producción)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=OC_Logistica;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

---

### Opción C — SQL Server en red o servidor dedicado

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=NOMBRE_SERVIDOR;Database=OC_Logistica;User Id=USUARIO;Password=CONTRASEÑA;MultipleActiveResultSets=true"
}
```

Reemplazá `NOMBRE_SERVIDOR`, `USUARIO` y `CONTRASEÑA` con los datos de tu instancia.

---

## Primera ejecución

### Método 1 — Automático (recomendado)

Al iniciar la aplicación, el sistema **aplica automáticamente las migraciones** y crea toda la estructura de tablas:

```bash
# Desde la carpeta raíz del proyecto (donde está LogiticaOC.sln)
dotnet run --project src/LogiticaOC.Presentation
```

Luego abrí el navegador en: `https://localhost:5001` (o el puerto que indique la consola)

---

### Método 2 — Script SQL manual (para entornos sin acceso a CLI)

Si el servidor no tiene el SDK de .NET o preferís ejecutar el schema desde SSMS o Azure Data Studio:

1. Abrí el archivo `scripts/OC_Logistica_schema.sql`
2. Conectate al servidor SQL que configuraste en `appsettings.json`
3. Ejecutá el script completo

> El script es **idempotente**: puede ejecutarse más de una vez sin romper nada.

---

## Cómo funciona el sistema

### Estructura de capas (solo para referencia técnica)

```
LogiticaOC.Domain         → Reglas del negocio (entidades, sin dependencias externas)
LogiticaOC.Application    → DTOs y contratos de operaciones
LogiticaOC.Infrastructure → Base de datos (EF Core), repositorios, exportación a PDF/Excel
LogiticaOC.Presentation   → Interfaz web (Controllers, Views, estilos)
```

No es necesario modificar capas internas para el uso normal. Todo lo configurable está en `appsettings.json`.

---

## Flujo de una OC

```
1. Llega OC por correo
       ↓
2. Registrar en el sistema (Nueva OC)
   - Número de OC
   - Entidad (se crea automáticamente si es nueva)
   - Responsable asignado
   - Productos con estado individual
   - Adjuntar PDF de la OC (opcional)
       ↓
3. Seguimiento
   - Cambiar estado de la OC o de cada producto
   - Agregar guías de despacho cuando se envía mercadería
   - El historial se registra automáticamente en cada cambio
       ↓
4. Alertas automáticas del Dashboard
   🟢 Verde   → más del 40% de días restantes
   🟡 Amarillo → entre 20% y 40% de días restantes
   🔴 Rojo    → menos del 20% o vencida
   ⚫ Gris    → OC Cerrada o Cancelada
       ↓
5. Generar Hoja de Ruta (Excel)
   - Agrupa automáticamente por responsable
   - Cambia el estado de todas las OC incluidas a "Listo para Despacho"
       ↓
6. Cerrar OC
   - Cambiar estado a "Cerrada" desde edición
   - O cancelar con motivo desde el detalle
```

---

## Módulos del sistema

### Dashboard
- Vista principal con **3 métricas en tiempo real**: OC Pendientes, Entregadas, Críticas
- Tabla de todas las OC con colores de alerta
- Filtros por N° OC, Responsable, Estado, Fecha

### Órdenes de Compra
| Acción | Dónde |
|---|---|
| Ver todas | Menú → Órdenes de Compra |
| Crear nueva | Menú → Nueva OC |
| Editar | Botón "Editar" en el detalle |
| Cancelar | Botón "Cancelar OC" en el detalle (pide motivo) |
| Exportar PDF individual | Botón "Exportar PDF" en el detalle |
| Descargar PDF adjunto | Botón "PDF adjunto" en el detalle (si fue adjuntado) |

### Hoja de Ruta
- Botón en el menú lateral: **"Generar Hoja de Ruta"**
- Genera un archivo Excel (.xlsx) agrupado por responsable
- Incluye solo las OC que NO están en estado Cerrada/Cancelada/Listo para Despacho
- Al generar, cambia automáticamente el estado de esas OC a **"Listo para Despacho"**

### Guías de Despacho
- Se agregan desde el **detalle de cada OC** (botón "Agregar guía")
- Cada guía tiene: número, fecha, y responsable de entrega
- Se guardan sin recargar la página (AJAX)

### Productos / Líneas de OC
- Cada OC puede tener múltiples productos
- Cada producto tiene su propio estado:

| Estado | Significado |
|---|---|
| En Proceso | Pedido en trámite normal |
| Urgente Compras | Requiere atención prioritaria |
| Importación | Producto de importación (mayor plazo) |
| Entrega Parcial | Se entregó parcialmente (pide motivo) |
| Cerrada | Producto entregado completamente |
| Cancelada | Producto cancelado |

### Entidades (proveedores/clientes)
- Se crean **automáticamente** al escribir el nombre en el campo "Entidad" de una OC
- El sistema busca si ya existe y la reutiliza
- No hay pantalla de gestión de entidades (diseño intencional para simplificar)

---

## Preguntas frecuentes

**¿Puedo correr esto en Windows sin instalar SQL Server?**  
Sí. Si tenés Visual Studio instalado, usá la opción A (LocalDB). Si no tenés Visual Studio, instalá [SQL Server Express](https://www.microsoft.com/es-ar/sql-server/sql-server-downloads) (gratuito) y usá la opción B.

**¿Qué pasa si cambio la connection string con la BD ya creada?**  
El sistema intenta conectarse a la nueva BD. Si no existe, la crea automáticamente con todas las tablas y datos iniciales.

**¿Cómo agrego un nuevo responsable?**  
Por ahora se agrega directamente en la BD (`Responsables` tabla) o mediante un INSERT SQL. En una versión futura se puede agregar un CRUD de responsables.

**¿Puedo tener el sistema en red (varios usuarios)?**  
Sí. Instalá la app en un servidor Windows con IIS y apuntá la connection string a un SQL Server en red. Todos los usuarios que accedan a la URL comparten la misma BD.

**¿Los PDF adjuntos ocupan mucho espacio en la BD?**  
Sí, se guardan en la columna `PDFAdjunto` como `varbinary(MAX)`. Si el volumen de OC es alto, conviene revisar el tamaño de la BD periódicamente o mover los PDF a una carpeta de archivos.
