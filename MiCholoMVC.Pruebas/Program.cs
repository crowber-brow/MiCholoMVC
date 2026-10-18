using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using MiCholoMVC.Controllers;
using MiCholoMVC.Data;
using MiCholoMVC.Models;
using MiCholoMVC.Services;
using MiCholoMVC.ViewModels;

if(args.Contains("--esquema"))
{
    using var schema=new MiCholoDbContext(new DbContextOptionsBuilder<MiCholoDbContext>().UseSqlServer("Server=localhost;Database=SoloEsquema;Trusted_Connection=True").Options);
    File.WriteAllText("MiCholoMVC/Database/Crear.sql",schema.Database.GenerateCreateScript());Console.WriteLine("Esquema SQL generado.");return;
}
if(args.Contains("--pdf"))
{
    Directory.CreateDirectory("output/pdf");
    File.WriteAllBytes("output/pdf/verificacion-reportes.pdf",PdfService.Crear("Mi Cholo - Reporte de pérdidas",new[]{"Desde: 01/10/2026 | Hasta: 31/10/2026","Registros: 60 | Unidades: 120 | Valor perdido: S/ 240.00","Producto: Café, azúcar y lácteos"}.Concat(Enumerable.Range(1,60).SelectMany(i=>new[]{ $"Registro {i}","Producto: Leche descremada presentación familiar de larga duración","Lote: L-2026-001 | Motivo: VENCIMIENTO","Cantidad: 2 | Costo unitario: S/ 2.00 | Valor perdido: S/ 4.00"," "}))));
    Console.WriteLine("PDF de verificación generado.");return;
}
if(args.Contains("--limpiar-web"))
{
    var datos=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText("output/pruebas/web-fixture.json"))!;
    var builder=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(datos["Conexion"]);
    if(!builder.InitialCatalog.StartsWith("MiCholoPruebas_")||builder.InitialCatalog.Length!=47)throw new Exception("La base indicada no es una base temporal de pruebas.");
    await using var ctx=new MiCholoDbContext(new DbContextOptionsBuilder<MiCholoDbContext>().UseSqlServer(builder.ConnectionString).Options);
    await ctx.Database.EnsureDeletedAsync();Console.WriteLine("Base temporal web eliminada.");return;
}
var prepararWeb=args.Contains("--preparar-web");
var nombre="MiCholoPruebas_"+Guid.NewGuid().ToString("N");
var connection=$"Server=localhost\\SQLEXPRESS;Database={nombre};Trusted_Connection=True;TrustServerCertificate=True";
MiCholoDbContext Db()=>new(new DbContextOptionsBuilder<MiCholoDbContext>().UseSqlServer(connection).Options);
var resultados=new List<string>();
void Check(bool ok,string nombreCaso){if(!ok)throw new Exception("FALLÓ: "+nombreCaso);resultados.Add("OK: "+nombreCaso);Console.WriteLine(resultados[^1]);}
async Task Rechaza(Func<Task> accion,string caso){try{await accion();}catch(InvalidOperationException){Check(true,caso);return;}throw new Exception("No se rechazó: "+caso);}
T Preparar<T>(T controller)where T:Controller{var http=new DefaultHttpContext();controller.ControllerContext=new ControllerContext{HttpContext=http};controller.TempData=new TempDataDictionary(http,new MemTemp());return controller;}
VentaCreateViewModel Carrito(int producto,int cantidad,string metodo="EFECTIVO",decimal recibido=100)=>new(){MetodoPago=metodo,DineroRecibido=recibido,Detalles=[new(){IdProducto=producto,Cantidad=cantidad}]};
try
{
    await using(var seed=Db())
    {
        await seed.Database.EnsureCreatedAsync();
        var categoria=new Categoria{Nombre="Pruebas"};seed.Categorias.Add(categoria);await seed.SaveChangesAsync();
        var producto=new Producto{Nombre="Leche de prueba",IdCategoria=categoria.IdCategoria,PrecioCompra=2,PrecioVenta=10,FechaRegistro=ReglasNegocio.Ahora};seed.Productos.Add(producto);await seed.SaveChangesAsync();
        seed.Lotes.AddRange(new Lote{IdProducto=producto.IdProducto,NumeroLote="Primero",Cantidad=2,CostoUnitario=2,FechaIngreso=ReglasNegocio.Hoy,FechaVencimiento=ReglasNegocio.Hoy.AddDays(2)},new Lote{IdProducto=producto.IdProducto,NumeroLote="Segundo",Cantidad=4,CostoUnitario=3,FechaIngreso=ReglasNegocio.Hoy,FechaVencimiento=ReglasNegocio.Hoy.AddDays(20)},new Lote{IdProducto=producto.IdProducto,NumeroLote="Vencido",Cantidad=5,CostoUnitario=2,FechaIngreso=ReglasNegocio.Hoy.AddMonths(-1),FechaVencimiento=ReglasNegocio.Hoy.AddDays(-1)});
        seed.Cajas.Add(new Caja{Nombre="Caja de prueba",Estado="ABIERTA",MontoInicial=100,FechaApertura=ReglasNegocio.Ahora});await seed.SaveChangesAsync();
    }
    int idProducto,idCaja,primero,segundo;
    await using(var db=Db()){idProducto=(await db.Productos.SingleAsync()).IdProducto;idCaja=(await db.Cajas.SingleAsync()).IdCaja;primero=(await db.Lotes.SingleAsync(l=>l.NumeroLote=="Primero")).IdLote;segundo=(await db.Lotes.SingleAsync(l=>l.NumeroLote=="Segundo")).IdLote;}
    if(prepararWeb){Directory.CreateDirectory("output/pruebas");File.WriteAllText("output/pruebas/web-fixture.json",System.Text.Json.JsonSerializer.Serialize(new Dictionary<string,string>{{"Conexion",connection},{"Producto",idProducto.ToString()},{"Caja",idCaja.ToString()},{"Lote",primero.ToString()}}));Console.WriteLine("Base temporal preparada para pruebas de pantallas.");return;}
    var modelo=Carrito(idProducto,3,recibido:50);int ventaId;
    await using(var db=Db()){ventaId=await new OperacionService(db).VenderAsync(modelo);}
    await using(var db=Db()){
        var v=await db.Ventas.Include(v=>v.Detalles).SingleAsync();Check(v.Total==30&&v.Vuelto==20,"Venta suficiente y vuelto");Check(v.Detalles!.Count==2,"FEFO distribuye entre lotes");Check((await db.Lotes.FindAsync(primero))!.Cantidad==0&&(await db.Lotes.FindAsync(segundo))!.Cantidad==3,"Descuento correcto de stock");Check(await new OperacionService(db).SaldoCajaAsync((await db.Cajas.FindAsync(idCaja))!)==130,"Venta en efectivo aumenta caja por el total");Check(await db.MovimientosInventario.CountAsync(m=>m.Tipo=="VENTA")==2,"Venta crea Kardex");}
    await using(var db=Db()){Check(await new OperacionService(db).VenderAsync(modelo)==ventaId,"Doble envío es idempotente");Check(await db.Ventas.CountAsync()==1,"Doble envío no duplica venta");}
    foreach(var cantidad in new[]{0,-1,99})await Rechaza(async()=>{await using var db=Db();await new OperacionService(db).VenderAsync(Carrito(idProducto,cantidad));},$"Cantidad rechazada: {cantidad}");
    await Rechaza(async()=>{await using var db=Db();await new OperacionService(db).VenderAsync(Carrito(idProducto,1,recibido:1));},"Efectivo insuficiente");
    await Rechaza(async()=>{await using var db=Db();await new OperacionService(db).VenderAsync(Carrito(idProducto,1,"INVALIDO"));},"Método de pago inválido");
    await using(var db=Db()){var p=await db.Productos.FindAsync(idProducto);p!.Estado=false;await db.SaveChangesAsync();}
    await Rechaza(async()=>{await using var db=Db();await new OperacionService(db).VenderAsync(Carrito(idProducto,1));},"Producto inactivo");
    await using(var db=Db()){var p=await db.Productos.FindAsync(idProducto);p!.Estado=true;await db.SaveChangesAsync();}
    async Task<int?> Competir(){try{await using var db=Db();return await new OperacionService(db).VenderAsync(Carrito(idProducto,2,"YAPE"));}catch(InvalidOperationException){return null;}}
    var concurrentes=await Task.WhenAll(Competir(),Competir());Check(concurrentes.Count(x=>x!=null)==1,"Dos ventas simultáneas: solo una consume el stock disponible");
    var digital=concurrentes.First(x=>x!=null)!.Value;
    await using(var db=Db()){Check((await db.Lotes.FindAsync(segundo))!.Cantidad==1,"Concurrencia conserva stock correcto");Check(await new OperacionService(db).SaldoCajaAsync((await db.Cajas.FindAsync(idCaja))!)==130,"Pago digital no aumenta efectivo");}
    await using(var db=Db()){await new OperacionService(db).AnularAsync(digital,"Devolución de prueba",true);}
    await Rechaza(async()=>{await using var db=Db();await new OperacionService(db).AnularAsync(digital,"Repetida",true);},"Impide doble devolución");
    await using(var db=Db()){await new OperacionService(db).AnularAsync(ventaId,"Error de prueba",true);Check(await new OperacionService(db).SaldoCajaAsync((await db.Cajas.FindAsync(idCaja))!)==100,"Devolución revierte efectivo");Check((await db.Lotes.FindAsync(primero))!.Cantidad==2,"Devolución repone lotes");}
    await using(var db=Db()){
        var c=Preparar(new PerdidasController(db,new(db),NullLogger<PerdidasController>.Instance));var result=await c.Create(new(){IdProducto=idProducto,IdLote=primero,Cantidad=1,Motivo="ROTURA",Justificacion="Prueba controlada"});Check(result is RedirectToActionResult,"Registrar pérdida");Check((await db.Lotes.FindAsync(primero))!.Cantidad==1,"Pérdida descuenta stock");var p=await db.Perdidas.SingleAsync();Check(p.ValorUnitario==2&&p.ValorTotal==2,"Pérdida guarda costo histórico");}
    await using(var db=Db()){var p=await db.Productos.FindAsync(idProducto);p!.PrecioCompra=99;await db.SaveChangesAsync();Check((await db.Perdidas.SingleAsync()).ValorTotal==2,"Cambio de precio no altera pérdidas anteriores");}
    await using(var db=Db()){var c=Preparar(new PerdidasController(db,new(db),NullLogger<PerdidasController>.Instance));Check(await c.Create(new(){IdProducto=idProducto,IdLote=primero,Cantidad=99,Motivo="ROTURA",Justificacion="Prueba"})is ViewResult,"Pérdida superior al stock rechazada");}
    await using(var db=Db()){var c=Preparar(new PerdidasController(db,new(db),NullLogger<PerdidasController>.Instance));Check(await c.Create(new(){IdProducto=idProducto,IdLote=primero,Cantidad=1,Motivo="INVALIDO",Justificacion="Prueba"})is ViewResult,"Motivo de pérdida inválido rechazado");}
    async Task<bool> Movimiento(int cantidad,string tipo,int producto){await using var db=Db();var c=Preparar(new InventarioController(db,new(db),NullLogger<InventarioController>.Instance));return await c.Create(new(){IdProducto=producto,IdLote=primero,Tipo=tipo,Cantidad=cantidad,Descripcion="Prueba"})is RedirectToActionResult;}
    Check(await Movimiento(2,"ENTRADA",idProducto),"Entrada de inventario");Check(await Movimiento(1,"SALIDA",idProducto),"Salida de inventario");Check(!await Movimiento(999,"SALIDA",idProducto),"Salida superior al stock rechazada");Check(!await Movimiento(1,"SALIDA",idProducto+999),"Lote de otro producto rechazado");
    await using(var db=Db()){
        var p=new Producto{Nombre="Solo vencido",IdCategoria=(await db.Categorias.SingleAsync()).IdCategoria,PrecioCompra=1,PrecioVenta=5};db.Productos.Add(p);await db.SaveChangesAsync();db.Lotes.Add(new(){IdProducto=p.IdProducto,NumeroLote="EXP",Cantidad=10,CostoUnitario=1,FechaVencimiento=ReglasNegocio.Hoy.AddDays(-1),FechaIngreso=ReglasNegocio.Hoy.AddMonths(-1)});await db.SaveChangesAsync();idProducto=p.IdProducto;}
    await Rechaza(async()=>{await using var db=Db();await new OperacionService(db).VenderAsync(Carrito(idProducto,1));},"Impide venta de producto vencido");
    await using(var db=Db()){
        var c=Preparar(new CajasController(db,new(db),NullLogger<CajasController>.Instance));Check(await c.Abrir(new(){Nombre="Duplicada",MontoInicial=0})is ViewResult,"Impide segunda caja abierta");}
    await using(var db=Db()){
        var c=Preparar(new CajasController(db,new(db),NullLogger<CajasController>.Instance));Check(await c.Movimiento(idCaja,new(){Tipo="INGRESO",Monto=10,Descripcion="Prueba"})is RedirectToActionResult,"Ingreso de caja");}
    await using(var db=Db()){
        var c=Preparar(new CajasController(db,new(db),NullLogger<CajasController>.Instance));Check(await c.Movimiento(idCaja,new(){Tipo="EGRESO",Monto=5,Descripcion="Prueba"})is RedirectToActionResult,"Egreso de caja");}
    await using(var db=Db()){
        var c=Preparar(new CajasController(db,new(db),NullLogger<CajasController>.Instance));Check(await c.Movimiento(idCaja,new(){Tipo="EGRESO",Monto=999,Descripcion="Prueba"})is ViewResult,"Impide egreso superior al saldo");}
    await using(var db=Db()){
        var c=Preparar(new CajasController(db,new(db),NullLogger<CajasController>.Instance));await c.CerrarPost(idCaja,103);var caja=await db.Cajas.FindAsync(idCaja);Check(caja!.Estado=="CERRADA"&&caja.Diferencia==-2,"Cierre calcula faltante");}
    await using(var db=Db()){
        var c=Preparar(new CajasController(db,new(db),NullLogger<CajasController>.Instance));Check(await c.Movimiento(idCaja,new(){Tipo="INGRESO",Monto=1,Descripcion="Prueba"})is ViewResult,"Impide movimiento en caja cerrada");}
    await Rechaza(async()=>{await using var db=Db();await new OperacionService(db).VenderAsync(Carrito(idProducto,1));},"Impide venta sin caja abierta");
    await using(var db=Db()){
        var c=Preparar(new CajasController(db,new(db),NullLogger<CajasController>.Instance));Check(await c.Abrir(new(){Nombre="Nueva",MontoInicial=0})is RedirectToActionResult,"Apertura después de cierre");}
    await using(var db=Db())
    {
        var leche=await db.Productos.SingleAsync(p=>p.Nombre=="Leche de prueba");var original=leche.IdCategoria;
        var cat=new Categoria{Nombre="Segunda categoría"};db.Categorias.Add(cat);await db.SaveChangesAsync();
        var pan=new Producto{Nombre="Pan de prueba",IdCategoria=cat.IdCategoria,PrecioCompra=4,PrecioVenta=20};db.Productos.Add(pan);await db.SaveChangesAsync();
        db.Lotes.Add(new(){IdProducto=pan.IdProducto,NumeroLote="PAN",Cantidad=2,CostoUnitario=4,FechaIngreso=ReglasNegocio.Hoy,FechaVencimiento=ReglasNegocio.Hoy.AddDays(10)});await db.SaveChangesAsync();
        var venta=Carrito(leche.IdProducto,1);venta.Detalles.Add(new(){IdProducto=pan.IdProducto,Cantidad=1});
        var id=await new OperacionService(db).VenderAsync(venta);
        var result=(ViewResult)await new ReportesController(db).Informes(idCategoria:original);
        var informe=(InformeViewModel)result.Model!;
        Check(informe.Filas.Count==1&&informe.Filas[0][6]=="S/ 10.00","Reporte por categoría suma solo su subtotal");
        leche.Nombre="Nombre actualizado";leche.IdCategoria=cat.IdCategoria;await db.SaveChangesAsync();
        var detalles=await db.DetalleVentas.Where(d=>d.IdVenta==id).ToListAsync();Check(detalles.Any(d=>d.ProductoNombre=="Leche de prueba"&&d.IdCategoriaHistorica==original),"Venta conserva nombre y categoría históricos");
        var stockAntes=await db.Lotes.SumAsync(l=>l.Cantidad);await new OperacionService(db).AnularAsync(id,"Productos no aptos",false);
        Check(await db.Lotes.SumAsync(l=>l.Cantidad)==stockAntes,"Devolución no apta no descuenta stock otra vez");
        Check(await db.Perdidas.CountAsync(p=>p.Motivo=="DEVOLUCION_NO_APTA")==2,"Devolución no apta registra valor de pérdida");
    }
    try{ReglasNegocio.Vuelto("EFECTIVO",10,10.001m);throw new Exception("Se permitieron tres decimales.");}catch(InvalidOperationException){Check(true,"Moneda rechaza más de dos decimales");}
    var hash=AccesoService.Hash("PruebaSegura123");Check(AccesoService.Verificar("PruebaSegura123",hash),"Contraseña correcta verifica hash");Check(!AccesoService.Verificar("incorrecta",hash),"Contraseña incorrecta rechazada");
    var accesoRoot=Path.Combine(Path.GetTempPath(),"MiCholoAcceso_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path.Combine(accesoRoot,"App_Data"));
    try{
        File.WriteAllText(Path.Combine(accesoRoot,"App_Data","acceso.json"),System.Text.Json.JsonSerializer.Serialize(new AccesoService.Configuracion("admin@prueba.local",hash)));
        var acceso=new AccesoService(new TestEnv{ContentRootPath=accesoRoot});Check(acceso.Autenticar("admin@prueba.local","PruebaSegura123","test"),"Login correcto");Check(!acceso.Autenticar("otro","PruebaSegura123","test"),"Usuario incorrecto rechazado");
        for(var i=0;i<5;i++)acceso.Autenticar("admin@prueba.local","incorrecta","atacante");Check(!acceso.Autenticar("admin@prueba.local","PruebaSegura123","atacante"),"Bloqueo por intentos fallidos");
        acceso.Cambiar("PruebaSegura123","OtraSegura456");Check(AccesoService.Verificar("OtraSegura456",System.Text.Json.JsonSerializer.Deserialize<AccesoService.Configuracion>(File.ReadAllText(Path.Combine(accesoRoot,"App_Data","acceso.json")))!.Hash),"Cambio de contraseña protegido");
    }finally{Directory.Delete(accesoRoot,true);}
    Check(ReglasNegocio.Vendible(ReglasNegocio.Hoy)&&!ReglasNegocio.Vendible(ReglasNegocio.Hoy.AddDays(-1)),"Regla de vencimiento consistente");
    await using(var db=Db())foreach(var tipo in new[]{"ventas","perdidas","inventario","stock","vencimientos","caja"}){
        var c=Preparar(new ReportesController(db));var pantalla=(ViewResult)await c.Informes(tipo);Check(pantalla.Model is InformeViewModel,"Reporte "+tipo);var pdf=(FileContentResult)await c.Informes(tipo,pdf:true);Check(System.Text.Encoding.ASCII.GetString(pdf.FileContents.Take(8).ToArray()).StartsWith("%PDF-1.4"),"PDF "+tipo);}
    Directory.CreateDirectory("output/pruebas");await File.WriteAllLinesAsync("output/pruebas/resultados.txt",resultados);
    Console.WriteLine($"COMPLETADO: {resultados.Count} comprobaciones correctas.");
}
finally
{
    if(!nombre.StartsWith("MiCholoPruebas_")||nombre.Length!=47)throw new Exception("Nombre de base de prueba inesperado.");
    if(!prepararWeb){await using var limpieza=Db();await limpieza.Database.EnsureDeletedAsync();}
}
class MemTemp:ITempDataProvider{public IDictionary<string,object> LoadTempData(HttpContext context)=>new Dictionary<string,object>();public void SaveTempData(HttpContext context,IDictionary<string,object> values){}}
class TestEnv:IWebHostEnvironment{public string ApplicationName{get;set;}="Pruebas";public string EnvironmentName{get;set;}="Development";public string ContentRootPath{get;set;}="";public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider();public string WebRootPath{get;set;}="";public IFileProvider WebRootFileProvider{get;set;}=new NullFileProvider();}
