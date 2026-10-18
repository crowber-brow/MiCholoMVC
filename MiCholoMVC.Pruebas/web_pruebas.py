"""Pruebas HTTP de Mi Cholo contra una base temporal creada por el ejecutor C#."""
import os, json, re, html, urllib.request, urllib.parse, urllib.error, http.cookiejar
from html.parser import HTMLParser
from pathlib import Path
base=os.environ.get('MI_CHOLO_TEST_URL','http://localhost:5239')
password=os.environ['MI_CHOLO_TEST_PASSWORD']
fixture=json.loads(Path('output/pruebas/web-fixture.json').read_text(encoding='utf-8-sig'))
cookie=http.cookiejar.CookieJar()
client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie))
resultados=[]
def check(ok,nombre):
    if not ok: raise AssertionError(nombre)
    resultados.append('OK: '+nombre); print(resultados[-1])
def get(url):
    with client.open(base+url) as r: return r.read().decode('utf-8'),r.geturl()
def post(url,datos):
    with client.open(base+url,urllib.parse.urlencode(datos).encode()) as r: return r.read().decode('utf-8'),r.geturl()
class Inputs(HTMLParser):
    def __init__(self,text): super().__init__();self.values={};self.feed(text)
    def handle_starttag(self,tag,attrs):
        a=dict(attrs)
        if tag=='input' and a.get('name'): self.values[a['name']]=a.get('value','')
def token(url): return Inputs(get(url)[0]).values['__RequestVerificationToken']
texto,url=get('/Dashboard/Index');check('/Account/Login' in url,'Rutas protegidas requieren login')
try: post('/Account/Login',{'Usuario':'micholo@gmail.com','Password':password});raise AssertionError('CSRF')
except urllib.error.HTTPError as e:check(e.code==400,'POST sin protección CSRF rechazado')
texto,url=post('/Account/Login',{'Usuario':'micholo@gmail.com','Password':'fallida','__RequestVerificationToken':token('/Account/Login')});check('/Account/Login' in url and 'Acceso no' in texto,'Login incorrecto permanece en login')
texto,url=post('/Account/Login',{'Usuario':'micholo@gmail.com','Password':password,'__RequestVerificationToken':token('/Account/Login')});check('/Home/Index' in url,'Login correcto crea sesión')
for ruta in ['/Dashboard/Index','/Stock/Index','/Vencimientos/Index','/Inventario/Index','/Perdidas/Index','/Perdidas/Create','/Lotes/Create','/Lotes/Edit?id='+fixture['Lote'],'/Productos/Index','/Productos/Create','/Categorias/Index','/Proveedores/Index','/Cajas/Index','/Cajas/Movimientos?id='+fixture['Caja'],'/Cajas/Cerrar?id='+fixture['Caja'],'/Account/Password']:
    texto,url=get(ruta);check('/Home/Error' not in url and '<!DOCTYPE html>' in texto,'Pantalla '+ruta)
for ruta in ['/css/site.css','/lib/bootstrap/dist/css/bootstrap.min.css','/js/pos.js']:
    texto,url=get(ruta);check(len(texto)>200 and '<!DOCTYPE html>' not in texto,'Recurso estático '+ruta)
texto,url=get('/Ventas/Crear');initial=Inputs(texto).values
modelo={'__RequestVerificationToken':initial['__RequestVerificationToken'],'TokenOperacion':initial['TokenOperacion'],'MetodoPago':'EFECTIVO','DineroRecibido':'50','Detalles[0].IdProducto':fixture['Producto'],'Detalles[0].Cantidad':'3'}
texto,url=post('/Ventas/Crear',modelo);check('/Ventas/Detalle/' in url or '/Ventas/Detalle?id=' in url,'Venta completa mediante formulario MVC');check('30.00' in texto and '20.00' in texto,'Comprobante muestra total y vuelto')
venta_url=url
texto,url=post('/Ventas/Crear',modelo);check(url==venta_url,'Doble envío HTTP conserva la misma venta')
catalogo=json.loads(get('/Ventas/ObtenerProductos')[0]);check(catalogo[0]['stock']==3,'Stock disponible actualizado después de vender')
texto,url=get('/Ventas/Crear');initial=Inputs(texto).values;invalido=dict(modelo,TokenOperacion=initial['TokenOperacion'],__RequestVerificationToken=initial['__RequestVerificationToken']);invalido['Detalles[0].Cantidad']='999'
texto,url=post('/Ventas/Crear',invalido);check('No hay stock suficiente' in texto and 'carritoInicial' in texto,'Stock insuficiente conserva el carrito y muestra error')
invalido['Detalles[0].Cantidad']='0';texto,url=post('/Ventas/Crear',invalido);check('/Ventas/Crear' in url and 'mayor que 0' in html.unescape(texto),'Cantidad inválida rechazada por validación MVC')
for tipo in ['ventas','perdidas','inventario','stock','vencimientos','caja']:
    texto,url=get('/Reportes/Informes?tipo='+tipo);check('Exportar PDF' in texto,'Reporte web '+tipo)
    with client.open(base+'/Reportes/Informes?tipo='+tipo+'&pdf=true') as r: data=r.read();check(r.headers.get_content_type()=='application/pdf' and data.startswith(b'%PDF-1.4'),'Descarga PDF '+tipo)
texto,url=post('/Cajas/CerrarPost',{'id':fixture['Caja'],'efectivoContado':'129','__RequestVerificationToken':token('/Cajas/Cerrar?id='+fixture['Caja'])});check('Diferencia' in html.unescape(texto) and '-1.00' in texto,'Cierre de caja mediante formulario')
texto,url=get('/Ventas/Crear');check('/Cajas' in url,'Venta requiere caja abierta en la interfaz')
texto,url=post('/Account/Logout',{'__RequestVerificationToken':token('/Home/Index')});check('Password' in texto and 'INGRESAR' in texto,'Logout vuelve al login')
texto,url=get('/Dashboard/Index');check('/Account/Login' in url,'Sesión cerrada bloquea rutas protegidas')
Path('output/pruebas/resultados-web.txt').write_text('\n'.join(resultados),encoding='utf-8')
print('COMPLETADO:',len(resultados),'comprobaciones web correctas.')
