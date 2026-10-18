CREATE TABLE [Cajas] (
    [IdCaja] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Estado] nvarchar(20) NOT NULL,
    [FechaApertura] datetime2 NULL,
    [MontoInicial] decimal(10,2) NOT NULL,
    [FechaCierre] datetime2 NULL,
    [MontoCierre] decimal(10,2) NULL,
    [EfectivoContado] decimal(10,2) NULL,
    [Diferencia] decimal(10,2) NULL,
    CONSTRAINT [PK_Cajas] PRIMARY KEY ([IdCaja])
);
GO


CREATE TABLE [Categorias] (
    [IdCategoria] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Descripcion] nvarchar(250) NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([IdCategoria])
);
GO


CREATE TABLE [Proveedores] (
    [IdProveedor] int NOT NULL IDENTITY,
    [RazonSocial] nvarchar(150) NOT NULL,
    [Ruc] nvarchar(11) NULL,
    [Telefono] nvarchar(20) NULL,
    [Direccion] nvarchar(200) NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_Proveedores] PRIMARY KEY ([IdProveedor])
);
GO


CREATE TABLE [MovimientosCaja] (
    [IdMovimiento] int NOT NULL IDENTITY,
    [IdCaja] int NOT NULL,
    [Tipo] nvarchar(20) NOT NULL,
    [Monto] decimal(10,2) NOT NULL,
    [Descripcion] nvarchar(250) NULL,
    [Fecha] datetime2 NOT NULL,
    CONSTRAINT [PK_MovimientosCaja] PRIMARY KEY ([IdMovimiento]),
    CONSTRAINT [FK_MovimientosCaja_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([IdCaja]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Ventas] (
    [IdVenta] int NOT NULL IDENTITY,
    [IdCaja] int NOT NULL,
    [Fecha] datetime2 NOT NULL,
    [Total] decimal(10,2) NOT NULL,
    [DineroRecibido] decimal(10,2) NOT NULL,
    [Vuelto] decimal(10,2) NOT NULL,
    [Estado] nvarchar(20) NOT NULL,
    [TokenOperacion] uniqueidentifier NOT NULL,
    [FechaAnulacion] datetime2 NULL,
    [MotivoAnulacion] nvarchar(500) NULL,
    [StockDevuelto] bit NOT NULL,
    [MetodoPago] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_Ventas] PRIMARY KEY ([IdVenta]),
    CONSTRAINT [FK_Ventas_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([IdCaja]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Productos] (
    [IdProducto] int NOT NULL IDENTITY,
    [Nombre] nvarchar(150) NOT NULL,
    [Descripcion] nvarchar(250) NULL,
    [IdCategoria] int NOT NULL,
    [IdProveedor] int NULL,
    [PrecioCompra] decimal(10,2) NOT NULL,
    [PrecioVenta] decimal(10,2) NOT NULL,
    [StockMinimo] int NOT NULL,
    [Estado] bit NOT NULL,
    [FechaRegistro] datetime2 NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_Productos_Categorias_IdCategoria] FOREIGN KEY ([IdCategoria]) REFERENCES [Categorias] ([IdCategoria]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Productos_Proveedores_IdProveedor] FOREIGN KEY ([IdProveedor]) REFERENCES [Proveedores] ([IdProveedor]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Lotes] (
    [IdLote] int NOT NULL IDENTITY,
    [IdProducto] int NOT NULL,
    [NumeroLote] nvarchar(50) NOT NULL,
    [Cantidad] int NOT NULL,
    [CostoUnitario] decimal(10,2) NOT NULL,
    [FechaIngreso] datetime2 NOT NULL,
    [FechaVencimiento] datetime2 NOT NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_Lotes] PRIMARY KEY ([IdLote]),
    CONSTRAINT [FK_Lotes_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION
);
GO


CREATE TABLE [DetalleVentas] (
    [IdDetalleVenta] int NOT NULL IDENTITY,
    [IdVenta] int NOT NULL,
    [IdLote] int NOT NULL,
    [Cantidad] int NOT NULL,
    [PrecioUnitario] decimal(10,2) NOT NULL,
    [Subtotal] decimal(10,2) NOT NULL,
    [CostoUnitario] decimal(10,2) NOT NULL,
    [ProductoNombre] nvarchar(150) NOT NULL,
    [CategoriaNombre] nvarchar(150) NOT NULL,
    [IdCategoriaHistorica] int NOT NULL,
    CONSTRAINT [PK_DetalleVentas] PRIMARY KEY ([IdDetalleVenta]),
    CONSTRAINT [FK_DetalleVentas_Lotes_IdLote] FOREIGN KEY ([IdLote]) REFERENCES [Lotes] ([IdLote]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DetalleVentas_Ventas_IdVenta] FOREIGN KEY ([IdVenta]) REFERENCES [Ventas] ([IdVenta]) ON DELETE CASCADE
);
GO


CREATE TABLE [MovimientosInventario] (
    [IdMovimiento] int NOT NULL IDENTITY,
    [IdProducto] int NOT NULL,
    [IdLote] int NULL,
    [Tipo] nvarchar(30) NOT NULL,
    [Cantidad] int NOT NULL,
    [SaldoLote] int NOT NULL,
    [SaldoProducto] int NOT NULL,
    [ValorUnitario] decimal(10,2) NOT NULL,
    [Descripcion] nvarchar(250) NULL,
    [Fecha] datetime2 NOT NULL,
    CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY ([IdMovimiento]),
    CONSTRAINT [FK_MovimientosInventario_Lotes_IdLote] FOREIGN KEY ([IdLote]) REFERENCES [Lotes] ([IdLote]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MovimientosInventario_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Perdidas] (
    [IdPerdida] int NOT NULL IDENTITY,
    [IdLote] int NOT NULL,
    [Cantidad] int NOT NULL,
    [ValorUnitario] decimal(10,2) NOT NULL,
    [Motivo] nvarchar(30) NOT NULL,
    [Justificacion] nvarchar(500) NOT NULL,
    [Fecha] datetime2 NOT NULL,
    CONSTRAINT [PK_Perdidas] PRIMARY KEY ([IdPerdida]),
    CONSTRAINT [FK_Perdidas_Lotes_IdLote] FOREIGN KEY ([IdLote]) REFERENCES [Lotes] ([IdLote]) ON DELETE NO ACTION
);
GO


CREATE UNIQUE INDEX [IX_Cajas_Estado] ON [Cajas] ([Estado]) WHERE [Estado] = N'ABIERTA';
GO


CREATE INDEX [IX_DetalleVentas_IdLote] ON [DetalleVentas] ([IdLote]);
GO


CREATE INDEX [IX_DetalleVentas_IdVenta] ON [DetalleVentas] ([IdVenta]);
GO


CREATE INDEX [IX_Lotes_IdProducto] ON [Lotes] ([IdProducto]);
GO


CREATE INDEX [IX_MovimientosCaja_IdCaja] ON [MovimientosCaja] ([IdCaja]);
GO


CREATE INDEX [IX_MovimientosInventario_IdLote] ON [MovimientosInventario] ([IdLote]);
GO


CREATE INDEX [IX_MovimientosInventario_IdProducto] ON [MovimientosInventario] ([IdProducto]);
GO


CREATE INDEX [IX_Perdidas_IdLote] ON [Perdidas] ([IdLote]);
GO


CREATE INDEX [IX_Productos_IdCategoria] ON [Productos] ([IdCategoria]);
GO


CREATE INDEX [IX_Productos_IdProveedor] ON [Productos] ([IdProveedor]);
GO


CREATE INDEX [IX_Ventas_IdCaja] ON [Ventas] ([IdCaja]);
GO


CREATE UNIQUE INDEX [IX_Ventas_TokenOperacion] ON [Ventas] ([TokenOperacion]);
GO


