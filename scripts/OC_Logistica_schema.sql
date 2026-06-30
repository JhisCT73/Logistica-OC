IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE TABLE [Entidades] (
        [EntidadId] int NOT NULL IDENTITY,
        [RazonSocial] nvarchar(200) NOT NULL,
        [Activo] bit NOT NULL,
        [CreadoEn] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Entidades] PRIMARY KEY ([EntidadId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE TABLE [EstadosOC] (
        [EstadoOcId] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [ColorHex] nvarchar(7) NOT NULL,
        CONSTRAINT [PK_EstadosOC] PRIMARY KEY ([EstadoOcId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE TABLE [EstadosProducto] (
        [EstadoProductoId] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [ColorHex] nvarchar(7) NOT NULL,
        CONSTRAINT [PK_EstadosProducto] PRIMARY KEY ([EstadoProductoId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE TABLE [Responsables] (
        [ResponsableId] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Responsables] PRIMARY KEY ([ResponsableId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE TABLE [OrdenCompra] (
        [OcId] int NOT NULL IDENTITY,
        [NumeroOC] nvarchar(50) NOT NULL,
        [EntidadId] int NOT NULL,
        [ResponsableId] int NULL,
        [EstadoOcId] int NOT NULL,
        [FechaLlegadaCorreo] date NOT NULL,
        [FechaSolicitada] date NOT NULL,
        [FechaRegistro] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [FechaDespacho] date NULL,
        [FechaCierre] date NULL,
        [TotalDiasEstablecidos] int NULL,
        [LugarEntrega] nvarchar(200) NULL,
        [Departamento] nvarchar(100) NULL,
        [Movilidad] nvarchar(200) NULL,
        [Observaciones] nvarchar(max) NULL,
        [PDFAdjunto] varbinary(max) NULL,
        [PDFNombreArchivo] nvarchar(255) NULL,
        CONSTRAINT [PK_OrdenCompra] PRIMARY KEY ([OcId]),
        CONSTRAINT [FK_OrdenCompra_Entidades_EntidadId] FOREIGN KEY ([EntidadId]) REFERENCES [Entidades] ([EntidadId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrdenCompra_EstadosOC_EstadoOcId] FOREIGN KEY ([EstadoOcId]) REFERENCES [EstadosOC] ([EstadoOcId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrdenCompra_Responsables_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Responsables] ([ResponsableId]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE TABLE [Guias] (
        [GuiaId] int NOT NULL IDENTITY,
        [OcId] int NOT NULL,
        [NumeroGuia] nvarchar(100) NOT NULL,
        [FechaGuia] date NOT NULL,
        [Responsable] nvarchar(150) NULL,
        [CreadoEn] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Guias] PRIMARY KEY ([GuiaId]),
        CONSTRAINT [FK_Guias_OrdenCompra_OcId] FOREIGN KEY ([OcId]) REFERENCES [OrdenCompra] ([OcId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE TABLE [HistorialOC] (
        [HistorialId] int NOT NULL IDENTITY,
        [OcId] int NOT NULL,
        [FechaModificacion] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [EstadoAnterior] nvarchar(100) NULL,
        [EstadoNuevo] nvarchar(100) NULL,
        [ResponsableAnterior] nvarchar(100) NULL,
        [ResponsableNuevo] nvarchar(100) NULL,
        [Observacion] nvarchar(500) NULL,
        CONSTRAINT [PK_HistorialOC] PRIMARY KEY ([HistorialId]),
        CONSTRAINT [FK_HistorialOC_OrdenCompra_OcId] FOREIGN KEY ([OcId]) REFERENCES [OrdenCompra] ([OcId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE TABLE [OrdenCompraDetalle] (
        [DetalleId] int NOT NULL IDENTITY,
        [OcId] int NOT NULL,
        [CodigoProducto] nvarchar(50) NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [Cantidad] decimal(18,2) NOT NULL,
        [EstadoProductoId] int NOT NULL,
        [MotivoEntregaParcial] nvarchar(500) NULL,
        CONSTRAINT [PK_OrdenCompraDetalle] PRIMARY KEY ([DetalleId]),
        CONSTRAINT [FK_OrdenCompraDetalle_EstadosProducto_EstadoProductoId] FOREIGN KEY ([EstadoProductoId]) REFERENCES [EstadosProducto] ([EstadoProductoId]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrdenCompraDetalle_OrdenCompra_OcId] FOREIGN KEY ([OcId]) REFERENCES [OrdenCompra] ([OcId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EstadoOcId', N'ColorHex', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadosOC]'))
        SET IDENTITY_INSERT [EstadosOC] ON;
    EXEC(N'INSERT INTO [EstadosOC] ([EstadoOcId], [ColorHex], [Nombre])
    VALUES (1, N''#0d6efd'', N''Pendiente''),
    (2, N''#6f42c1'', N''Listo para Despacho''),
    (3, N''#6c757d'', N''Cerrada''),
    (4, N''#343a40'', N''Cancelada'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EstadoOcId', N'ColorHex', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadosOC]'))
        SET IDENTITY_INSERT [EstadosOC] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EstadoProductoId', N'ColorHex', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadosProducto]'))
        SET IDENTITY_INSERT [EstadosProducto] ON;
    EXEC(N'INSERT INTO [EstadosProducto] ([EstadoProductoId], [ColorHex], [Nombre])
    VALUES (1, N''#28a745'', N''En Proceso''),
    (2, N''#fd7e14'', N''Urgente Compras''),
    (3, N''#0dcaf0'', N''Importación''),
    (4, N''#ffc107'', N''Entrega Parcial''),
    (5, N''#6c757d'', N''Cerrada''),
    (6, N''#343a40'', N''Cancelada'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EstadoProductoId', N'ColorHex', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadosProducto]'))
        SET IDENTITY_INSERT [EstadosProducto] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Entidades_RazonSocial] ON [Entidades] ([RazonSocial]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Guias_OcId] ON [Guias] ([OcId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistorialOC_OcId] ON [HistorialOC] ([OcId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrdenCompra_EntidadId] ON [OrdenCompra] ([EntidadId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrdenCompra_EstadoOcId] ON [OrdenCompra] ([EstadoOcId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrdenCompra_FechaRegistro] ON [OrdenCompra] ([FechaRegistro]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrdenCompra_NumeroOC] ON [OrdenCompra] ([NumeroOC]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrdenCompra_ResponsableId] ON [OrdenCompra] ([ResponsableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrdenCompraDetalle_EstadoProductoId] ON [OrdenCompraDetalle] ([EstadoProductoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrdenCompraDetalle_OcId] ON [OrdenCompraDetalle] ([OcId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623050450_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260623050450_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623185017_SeedEstados'
)
BEGIN
    EXEC(N'UPDATE [EstadosOC] SET [ColorHex] = N''#4f8ef7''
    WHERE [EstadoOcId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623185017_SeedEstados'
)
BEGIN
    EXEC(N'UPDATE [EstadosOC] SET [ColorHex] = N''#f59e0b'', [Nombre] = N''Entrega Parcial''
    WHERE [EstadoOcId] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623185017_SeedEstados'
)
BEGIN
    EXEC(N'UPDATE [EstadosOC] SET [ColorHex] = N''#22c55e'', [Nombre] = N''Cerrada''
    WHERE [EstadoOcId] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623185017_SeedEstados'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EstadoOcId', N'ColorHex', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadosOC]'))
        SET IDENTITY_INSERT [EstadosOC] ON;
    EXEC(N'INSERT INTO [EstadosOC] ([EstadoOcId], [ColorHex], [Nombre])
    VALUES (5, N''#6c757d'', N''Cancelada'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'EstadoOcId', N'ColorHex', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadosOC]'))
        SET IDENTITY_INSERT [EstadosOC] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623185017_SeedEstados'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260623185017_SeedEstados', N'8.0.11');
END;
GO

COMMIT;
GO

