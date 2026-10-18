# Calidad y pruebas — Mi Cholo

Fecha de verificación: 1 de octubre de 2026.

## Resultado

- Compilación: 0 errores, 0 advertencias.
- Reglas de negocio e integración SQL: 62 comprobaciones correctas.
- Aplicación HTTP y formularios MVC: 45 comprobaciones correctas.
- Revisión de interfaz: búsqueda por nombre, carrito, cantidades, total, vuelto, eliminación y dashboard.
- PDF: estructura, acentos, registros, paginación y revisión visual del reporte de varias páginas.
- Base real: actualización transaccional con respaldo previo. Las pruebas de ventas se ejecutaron sobre bases separadas.

## Matriz de cobertura

| Área | Casos comprobados |
|---|---|
| Ventas | Stock suficiente, FEFO entre dos lotes, cantidades cero y negativas, stock insuficiente, producto inactivo, lote vencido, efectivo insuficiente, pago inválido, más de dos decimales, doble envío y ventas simultáneas |
| Stock y Kardex | Descuento por lote, saldo, registro de venta, entrada, salida, salida mayor al stock y lote de otro producto |
| Devoluciones | Reposición, reversión del efectivo, doble devolución rechazada, devolución no apta sin segundo descuento y valoración de pérdida |
| Pérdidas | Registro, descuento, motivo inválido, cantidad superior al stock, costo histórico y cambio posterior de precio |
| Caja | Apertura, segunda apertura rechazada, ingreso, egreso, egreso superior al saldo, cierre con faltante, caja cerrada y apertura posterior |
| Seguridad | Hash, contraseña incorrecta, usuario incorrecto, cinco intentos fallidos, cambio de contraseña, login web, cierre de sesión, rutas protegidas y CSRF |
| Reportes | Seis tipos en pantalla y PDF, filtro por categoría con subtotal y conservación de nombre/categoría históricos |
| Web | Pantallas principales, recursos estáticos, formulario real de venta, comprobante, stock actualizado, carrito conservado ante error, cierre y logout |

## Reproducir reglas de negocio

Desde la raíz del proyecto:

```powershell
dotnet build MiCholoMVC.slnx
dotnet run --project MiCholoMVC.Pruebas --no-build
```

La base se genera con un nombre aleatorio y se elimina al finalizar. Un nombre de producción no puede pasar la validación de limpieza. El ejecutor devuelve un código de error si falla alguna comprobación. Los resultados se guardan en `output/pruebas/resultados.txt`.

## Reproducir pruebas web

1. Preparar la base temporal:

```powershell
dotnet run --project MiCholoMVC.Pruebas --no-build -- --preparar-web
```

2. En otra terminal, iniciar la aplicación aislada:

```powershell
$fixture = Get-Content output/pruebas/web-fixture.json -Raw | ConvertFrom-Json
$env:ConnectionStrings__MiCholoConnection = $fixture.Conexion
dotnet run --project MiCholoMVC --no-build --no-launch-profile --urls http://localhost:5239
```

3. En la terminal original, introducir la contraseña administrativa actual para las pruebas. No guardarla en el archivo de pruebas:

```powershell
$secreto = Read-Host 'Contraseña administrativa para la prueba' -AsSecureString
$env:MI_CHOLO_TEST_PASSWORD = [System.Net.NetworkCredential]::new('', $secreto).Password
python MiCholoMVC.Pruebas/web_pruebas.py
Remove-Item Env:MI_CHOLO_TEST_PASSWORD
```

4. Detener la aplicación aislada y limpiar su base:

```powershell
dotnet run --project MiCholoMVC.Pruebas --no-build -- --limpiar-web
```

Cerrar la terminal que contiene `ConnectionStrings__MiCholoConnection` antes de iniciar la aplicación real. Los resultados se guardan en `output/pruebas/resultados-web.txt`.

El recorrido HTTP deja una venta y un cierre exclusivamente en la base temporal. Verifica respuestas y resultados; la compilación por sí sola no se considera prueba funcional.
