(() => {
    const $ = id => document.getElementById(id);
    const money = n => 'S/ ' + Number(n).toFixed(2);
    let productos = [], carrito = JSON.parse($('carritoInicial').textContent), enviando = false, consultando = false;
    function error(texto) { $('posError').textContent = texto; $('posError').classList.toggle('d-none', !texto); }
    function totales() {
        let total = 0;
        $('carrito').querySelectorAll('tr').forEach((fila, i) => {
            const p = productos.find(p => p.idProducto === carrito[i].idProducto);
            const subtotal = (p?.precio || 0) * carrito[i].cantidad;
            fila.querySelector('.subtotal').textContent = money(subtotal); total += subtotal;
        });
        $('total').textContent = money(total);
        const efectivo = $('MetodoPago').value === 'EFECTIVO';
        $('recibidoGrupo').hidden = !efectivo;
        $('DineroRecibido').required = efectivo;
        $('DineroRecibido').min = efectivo ? total.toFixed(2) : '0';
        $('vuelto').textContent = !carrito.length ? '' : efectivo ? 'Vuelto: ' + money(Math.max(0, Number($('DineroRecibido').value) - total)) : $('MetodoPago').value ? 'Pago digital: ' + money(total) : 'Seleccione un método de pago.';
        $('confirmar').disabled = enviando || consultando || !carrito.length || !productos.length;
        return total;
    }
    function dibujar() {
        $('carrito').replaceChildren();
        carrito.forEach((item, i) => {
            const p = productos.find(p => p.idProducto === item.idProducto);
            const fila = document.createElement('tr');
            const nombre = document.createElement('td'); nombre.textContent = p?.nombre || 'Producto no disponible';
            const id = document.createElement('input'); id.type = 'hidden'; id.name = `Detalles[${i}].IdProducto`; id.value = item.idProducto; nombre.append(id);
            const precio = document.createElement('td'); precio.textContent = money(p?.precio || 0);
            const cant = document.createElement('td'), input = document.createElement('input'); input.type = 'number'; input.min = '1'; input.max = p?.stock || 0; input.step = '1'; input.value = item.cantidad; input.name = `Detalles[${i}].Cantidad`; input.className = 'form-control'; input.required = true; input.style.width = '85px';
            input.setAttribute('aria-label', 'Cantidad de ' + (p?.nombre || 'producto'));
            input.addEventListener('input', () => { item.cantidad = Number(input.value); totales(); });
            cant.append(input); const stock = document.createElement('small'); stock.textContent = 'Disponible: ' + (p?.stock || 0); cant.append(stock);
            const sub = document.createElement('td'); sub.className = 'subtotal';
            const accion = document.createElement('td'), boton = document.createElement('button'); boton.type = 'button'; boton.className = 'btn btn-outline-danger btn-sm'; boton.textContent = 'Quitar'; boton.addEventListener('click', () => { carrito.splice(i, 1); dibujar(); }); accion.append(boton);
            fila.append(nombre, precio, cant, sub, accion); $('carrito').append(fila);
        });
        $('vacio').hidden = carrito.length > 0; dibujarBusqueda(); totales();
    }
    function dibujarBusqueda() {
        const q = $('buscar').value.trim().toLocaleLowerCase(); $('resultados').replaceChildren();
        productos.filter(p => p.nombre.toLocaleLowerCase().includes(q)).slice(0, 40).forEach(p => {
            const b = document.createElement('button'); b.type = 'button'; b.className = 'list-group-item list-group-item-action'; b.textContent = `${p.nombre} · ${money(p.precio)} · Stock: ${p.stock}`; b.disabled = p.stock < 1;
            b.addEventListener('click', async () => {
                try {
                    await actualizar();
                    const vigente = productos.find(x => x.idProducto === p.idProducto);
                    const item = carrito.find(x => x.idProducto === p.idProducto);
                    if (!vigente || (item?.cantidad || 0) >= vigente.stock) { error('No hay más stock disponible para este producto.'); return; }
                    if (item) item.cantidad++; else carrito.push({ idProducto: p.idProducto, cantidad: 1 });
                    error(''); dibujar();
                } catch { error('No se pudo consultar el stock. Intente nuevamente.'); }
            }); $('resultados').append(b);
        });
        if (!$('resultados').children.length) { const p = document.createElement('p'); p.textContent = 'No se encontraron productos.'; $('resultados').append(p); }
    }
    async function actualizar() {
        const response = await fetch('/Ventas/ObtenerProductos', { headers: { Accept: 'application/json' } });
        if (!response.ok || !response.headers.get('content-type')?.includes('application/json')) throw new Error('Sesión o conexión no disponible');
        productos = await response.json(); $('estadoStock').textContent = 'Stock actualizado: ' + new Date().toLocaleTimeString();
    }
    $('buscar').addEventListener('input', dibujarBusqueda);
    $('MetodoPago').addEventListener('change', totales); $('DineroRecibido').addEventListener('input', totales);
    $('ventaForm').addEventListener('submit', async e => {
        e.preventDefault(); if (enviando || consultando) return;
        consultando = true; totales();
        try {
            await actualizar(); dibujar();
            if (carrito.some(i => !Number.isInteger(i.cantidad) || i.cantidad < 1 || i.cantidad > (productos.find(p => p.idProducto === i.idProducto)?.stock || 0))) throw new Error('Revise las cantidades: el stock cambió o un producto no está disponible.');
            if (!$('ventaForm').reportValidity()) return;
            if (!confirm('¿Confirmar la venta por ' + money(totales()) + '?')) return;
            enviando = true; $('confirmar').textContent = 'Registrando...'; $('ventaForm').submit();
        } catch (ex) { error(ex.message); }
        finally { consultando = false; totales(); }
    });
    actualizar().then(dibujar).catch(() => { error('No se pudo cargar el catálogo. Recargue la pantalla.'); totales(); });
    setInterval(async () => { if (enviando || consultando || document.hidden) return; try { await actualizar(); dibujarBusqueda(); $('carrito').querySelectorAll('tr').forEach((f,i) => { const p=productos.find(p=>p.idProducto===carrito[i].idProducto); f.querySelector('input[type=number]').max=p?.stock||0; f.querySelector('small').textContent='Disponible: '+(p?.stock||0); }); totales(); } catch { error('No se pudo actualizar el stock. Se comprobará al confirmar.'); } }, 15000);
})();
