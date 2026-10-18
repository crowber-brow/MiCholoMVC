SET XACT_ABORT ON;
IF COL_LENGTH('Ventas','DineroRecibido') IS NULL ALTER TABLE Ventas ADD DineroRecibido decimal(10,2) NOT NULL CONSTRAINT DF_Ventas_Recibido DEFAULT 0;
IF COL_LENGTH('Ventas','Vuelto') IS NULL ALTER TABLE Ventas ADD Vuelto decimal(10,2) NOT NULL CONSTRAINT DF_Ventas_Vuelto DEFAULT 0;
IF COL_LENGTH('Ventas','Estado') IS NULL ALTER TABLE Ventas ADD Estado nvarchar(20) NOT NULL CONSTRAINT DF_Ventas_Estado DEFAULT 'VIGENTE';
IF COL_LENGTH('Ventas','TokenOperacion') IS NULL ALTER TABLE Ventas ADD TokenOperacion uniqueidentifier NOT NULL CONSTRAINT DF_Ventas_Token DEFAULT NEWID();
IF COL_LENGTH('Ventas','FechaAnulacion') IS NULL ALTER TABLE Ventas ADD FechaAnulacion datetime2 NULL, MotivoAnulacion nvarchar(500) NULL, StockDevuelto bit NOT NULL DEFAULT 0;
IF COL_LENGTH('Lotes','CostoUnitario') IS NULL ALTER TABLE Lotes ADD CostoUnitario decimal(10,2) NOT NULL DEFAULT 0;
IF COL_LENGTH('DetalleVentas','CostoUnitario') IS NULL ALTER TABLE DetalleVentas ADD CostoUnitario decimal(10,2) NOT NULL DEFAULT 0, ProductoNombre nvarchar(150) NOT NULL DEFAULT '', CategoriaNombre nvarchar(150) NOT NULL DEFAULT '', IdCategoriaHistorica int NOT NULL DEFAULT 0;
IF COL_LENGTH('Perdidas','ValorUnitario') IS NULL ALTER TABLE Perdidas ADD ValorUnitario decimal(10,2) NOT NULL DEFAULT 0;
IF COL_LENGTH('Cajas','EfectivoContado') IS NULL ALTER TABLE Cajas ADD EfectivoContado decimal(10,2) NULL, Diferencia decimal(10,2) NULL;
IF COL_LENGTH('MovimientosInventario','SaldoLote') IS NULL ALTER TABLE MovimientosInventario ADD SaldoLote int NOT NULL DEFAULT 0, SaldoProducto int NOT NULL DEFAULT 0, ValorUnitario decimal(10,2) NOT NULL DEFAULT 0;
GO
UPDATE l SET CostoUnitario=p.PrecioCompra FROM Lotes l JOIN Productos p ON p.IdProducto=l.IdProducto WHERE l.CostoUnitario=0;
UPDATE d SET CostoUnitario=l.CostoUnitario,ProductoNombre=p.Nombre,CategoriaNombre=c.Nombre,IdCategoriaHistorica=p.IdCategoria FROM DetalleVentas d JOIN Lotes l ON l.IdLote=d.IdLote JOIN Productos p ON p.IdProducto=l.IdProducto JOIN Categorias c ON c.IdCategoria=p.IdCategoria WHERE d.ProductoNombre='';
UPDATE x SET ValorUnitario=l.CostoUnitario FROM Perdidas x JOIN Lotes l ON l.IdLote=x.IdLote WHERE x.ValorUnitario=0;
UPDATE Ventas SET DineroRecibido=Total WHERE DineroRecibido=0;
-- Un punto de partida explícito: no se inventa un historial anterior completo.
DECLARE @fecha datetime=GETDATE();
INSERT INTO MovimientosInventario(IdProducto,IdLote,Tipo,Cantidad,Descripcion,Fecha,SaldoLote,SaldoProducto,ValorUnitario)
SELECT l.IdProducto,l.IdLote,'APERTURA_KARDEX',l.Cantidad,'Saldo inicial al activar Kardex',@fecha,l.Cantidad,(SELECT SUM(l2.Cantidad) FROM Lotes l2 WHERE l2.IdProducto=l.IdProducto),l.CostoUnitario
FROM Lotes l WHERE NOT EXISTS(SELECT 1 FROM MovimientosInventario m WHERE m.IdLote=l.IdLote AND m.Tipo='APERTURA_KARDEX');
-- Los cobros digitales anteriores se conservan; una contrapartida los retira del efectivo.
INSERT INTO MovimientosCaja(IdCaja,Tipo,Monto,Descripcion,Fecha)
SELECT v.IdCaja,'EGRESO',v.Total,CONCAT('Separación pago digital venta #',v.IdVenta),@fecha FROM Ventas v
WHERE v.MetodoPago<>'EFECTIVO' AND EXISTS(SELECT 1 FROM MovimientosCaja m WHERE m.IdCaja=v.IdCaja AND m.Descripcion=CONCAT('Venta #',v.IdVenta) AND m.Tipo='INGRESO')
AND NOT EXISTS(SELECT 1 FROM MovimientosCaja m WHERE m.Descripcion=CONCAT('Separación pago digital venta #',v.IdVenta));
GO
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Ventas_TokenOperacion') CREATE UNIQUE INDEX IX_Ventas_TokenOperacion ON Ventas(TokenOperacion);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Cajas_UnaAbierta') CREATE UNIQUE INDEX IX_Cajas_UnaAbierta ON Cajas(Estado) WHERE Estado='ABIERTA';
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Lotes_FEFO') CREATE INDEX IX_Lotes_FEFO ON Lotes(IdProducto,Estado,FechaVencimiento) INCLUDE(Cantidad);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Kardex_ProductoFecha') CREATE INDEX IX_Kardex_ProductoFecha ON MovimientosInventario(IdProducto,Fecha);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Lotes_Stock') ALTER TABLE Lotes ADD CONSTRAINT CK_Lotes_Stock CHECK(Cantidad>=0);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Detalle_Cantidad') ALTER TABLE DetalleVentas ADD CONSTRAINT CK_Detalle_Cantidad CHECK(Cantidad>0);
GO
