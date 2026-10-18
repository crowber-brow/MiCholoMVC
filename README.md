# Mi Cholo — sistema de gestión del minimarket

Aplicación ASP.NET Core MVC (.NET 10), Entity Framework Core y SQL Server Express. El alcance incluye ventas, caja, inventario por lotes, prevención de pérdidas, vencimientos, dashboard y reportes PDF. No incluye código de barras, Excel, módulo independiente de usuarios ni módulo de auditoría.

## Funciones implementadas

- Punto de venta: búsqueda por nombre, carrito con varios productos, cantidades, eliminación, subtotal, total, confirmación, pago, recibido y vuelto.
- FEFO: la venta utiliza primero los lotes vigentes que vencen antes y reparte cantidades entre lotes cuando es necesario.
- Stock: consulta al agregar y confirmar; actualización de disponibilidad cada 15 segundos; validación definitiva en el servidor.
- Consistencia: venta, stock, Kardex y efectivo se guardan conjuntamente. Un bloqueo transaccional de SQL Server serializa operaciones de inventario y caja. Un token único impide duplicar una venta por reenvío.
- Comprobante interno imprimible y descargable en PDF.
- Devolución completa: mantiene la venta original, evita repetir la devolución, devuelve efectivo si corresponde y repone únicamente lotes vigentes. Sin reposición, registra una pérdida por devolución no apta sin descontar unidades otra vez. El reintegro de Yape, Plin o tarjeta se efectúa por el medio original, fuera de esta aplicación.
- Caja: una sola apertura, ingresos, egresos, efectivo esperado, contado y diferencia. Los cobros digitales se muestran por separado.
- Kardex por producto con fecha, lote, tipo, cantidad, costo, saldo por lote y saldo físico del producto.
- Pérdidas con costo unitario histórico, valor total y filtros por producto, motivo y período.
- Stock físico, disponible y vencido diferenciados. Las unidades de lotes inactivos son físicas, pero no se venden.
- Vencimientos: vencidos, hoy, 1–7, 8–15, 16–30 y más de 30 días. Solo se alertan lotes activos con unidades. La fecha indicada es el último día de vigencia.
- Dashboard: ventas de hoy y mes, stock bajo, vencimientos, pérdidas, caja actual, utilidad bruta y gráficos por día, mes, producto, motivo y vencimiento.
- Reportes PDF: ventas, pérdidas, inventario, stock mínimo, vencimientos y movimientos de caja. Los filtros y totales son los mismos en pantalla y PDF; el filtro de categoría suma únicamente sus artículos.
- Acceso administrativo único: contraseña protegida con PBKDF2-SHA256, cinco intentos fallidos bloquean cinco minutos, cambio de contraseña y revocación de sesiones anteriores al cambio. Formularios protegidos contra CSRF y campos permitidos explícitos.

## Iniciar en esta laptop

Desde la carpeta del proyecto:

```powershell
dotnet build MiCholoMVC.slnx
dotnet run --project MiCholoMVC --launch-profile http
```

Abrir http://localhost:5228. SQL Server Express debe estar iniciado. La conexión se configura en `MiCholoMVC/appsettings.json` y se puede reemplazar mediante `ConnectionStrings__MiCholoConnection`.

El acceso existente de la laptop se conservó y se almacenó como hash en `MiCholoMVC/App_Data/acceso.json`. Se puede cambiar desde el botón de la llave en la barra superior. Ese archivo es privado: no se incluye en Git ni se sirve como contenido web.

Para una nueva instalación:

```powershell
dotnet restore MiCholoMVC.slnx
pwsh -File MiCholoMVC/Database/Crear.ps1
pwsh -File MiCholoMVC/Database/ConfigurarAcceso.ps1
```

Estos scripts requieren SQL Server y permisos de creación de base. `Crear.ps1` rechaza una base existente. En una base anterior se utiliza `Actualizar.ps1`, que primero realiza un respaldo y luego actualiza el esquema dentro de una transacción. La aplicación no elimina ni recrea una base al iniciar.

## Recorrido recomendado

1. Registrar categorías, proveedores y productos con precios y stock mínimo.
2. Crear lotes con cantidad, fecha de ingreso, vencimiento y costo unitario. Un costo cero utiliza el precio de compra del producto al crear el lote.
3. Abrir caja e ingresar el fondo inicial.
4. Buscar productos en Ventas, agregarlos al carrito, seleccionar el pago y confirmar. En efectivo, ingresar el dinero recibido.
5. Descargar o imprimir el comprobante. Consultar Kardex para ver el descuento por lotes.
6. Registrar pérdidas con producto, lote, cantidad, motivo y justificación.
7. Revisar stock disponible, alertas de vencimiento y dashboard.
8. Al terminar, contar el efectivo físico y cerrar caja. La diferencia queda conservada.
9. Consultar los reportes por período y exportarlos en PDF.

Los cambios de cantidad se realizan desde movimientos de inventario; editar un lote conserva su producto, costo y cantidad. La utilidad mostrada es bruta, antes de gastos operativos.

## Datos anteriores a la actualización

Se conservaron productos, lotes, ventas, pérdidas y movimientos anteriores. El Kardex comienza con un saldo inicial identificado al activarlo, porque las operaciones anteriores no registraban todos los movimientos. Los movimientos antiguos permanecen en la base.

Los costos históricos que no existían se inicializaron con los costos de compra disponibles al actualizar. Son una aproximación para los registros anteriores; las operaciones nuevas guardan su costo exacto. Los ingresos de ventas digitales anteriores se compensaron con una contrapartida identificada para separarlos del efectivo. Los cierres históricos sin conteo mantienen su registro previo; el conteo y la diferencia se registran en los nuevos cierres.

## Respaldo y recuperación

```powershell
pwsh -File MiCholoMVC/Database/Respaldar.ps1
```

El respaldo se guarda en la carpeta de respaldos de SQL Server y se verifica con `RESTORE VERIFYONLY`. Copiar el archivo `.bak` y la configuración privada de acceso a un lugar seguro es parte del respaldo completo. Respaldar antes de actualizaciones y al terminar jornadas con cambios importantes.

Para recuperar, usar SQL Server Management Studio: Bases de datos → Restaurar base de datos → Dispositivo → seleccionar el `.bak`. Restaurar primero con un nombre diferente, verificar los registros y configurar la conexión hacia esa base. Evitar sobrescribir la base actual antes de comprobar la copia recuperada.

## Pruebas

```powershell
dotnet run --project MiCholoMVC.Pruebas
```

El ejecutor crea una base temporal con un nombre `MiCholoPruebas_` y un identificador aleatorio, ejecuta 62 comprobaciones y elimina solo esa base. No apunta a MiCholoDB. Requiere permisos de creación y eliminación de bases temporales en SQL Express.

Las 45 comprobaciones HTTP se ejecutan contra una instancia aislada con una base temporal. Instrucciones y matriz en `docs/PRUEBAS.md`. Los resultados de la ejecución se escriben en `output/pruebas/`.

Verificación final: compilación sin errores ni advertencias, 62 comprobaciones de negocio y 45 comprobaciones web correctas. Los PDFs paginados se renderizaron y revisaron visualmente.
