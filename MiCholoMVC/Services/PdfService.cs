using System.Text;
namespace MiCholoMVC.Services;
// PDF paginado sin procesos externos. Fuente base Courier con codificación WinAnsi.
public static class PdfService
{
    public static byte[] Crear(string titulo, IEnumerable<string> lineas)
    {
        var contenido = new List<string>();
        foreach (var linea in lineas)
        {
            var texto = linea.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
            while (texto.Length > 88) { var corte = texto.LastIndexOf(' ', 88); if (corte < 25) corte = 88; contenido.Add(texto[..corte]); texto = "  " + texto[corte..].TrimStart(); }
            contenido.Add(texto);
        }
        if (contenido.Count == 0) contenido.Add("No hay registros para los filtros seleccionados.");
        var paginas = new List<string[]>();
        while (contenido.Count > 0)
        {
            var cantidad = Math.Min(43, contenido.Count);
            if (contenido.Count > 43)
            {
                var separador = contenido.FindLastIndex(42, 43, string.IsNullOrWhiteSpace);
                if (separador >= 20) cantidad = separador + 1;
            }
            paginas.Add(contenido.Take(cantidad).ToArray()); contenido.RemoveRange(0, cantidad);
        }
        var objetos = new List<byte[]>();
        byte[] Bytes(string s) => Encoding.Latin1.GetBytes(s);
        string Esc(string s) => s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        objetos.Add(Bytes("<< /Type /Catalog /Pages 2 0 R >>"));
        objetos.Add(Bytes($"<< /Type /Pages /Count {paginas.Count} /Kids [ {string.Join(' ', Enumerable.Range(0,paginas.Count).Select(i => $"{4+i*2} 0 R"))} ] >>"));
        objetos.Add(Bytes("<< /Type /Font /Subtype /Type1 /BaseFont /Courier /Encoding /WinAnsiEncoding >>"));
        for(var i=0;i<paginas.Count;i++)
        {
            var paginaId = 4+i*2;
            objetos.Add(Bytes($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >> >> /Contents {paginaId+1} 0 R >>"));
            var s = new StringBuilder($"BT /F1 16 Tf 42 792 Td ({Esc(titulo.Length > 50 ? titulo[..50] : titulo)}) Tj /F1 9 Tf 0 -22 Td (Generado: {ReglasNegocio.Ahora:dd/MM/yyyy HH:mm}) Tj /F1 9.5 Tf 0 -30 Td 15 TL ");
            foreach(var linea in paginas[i]) s.Append($"({Esc(linea)}) Tj T* ");
            s.Append($"ET BT /F1 9 Tf 42 36 Td (Mi Cholo | Página {i+1} de {paginas.Count}) Tj ET");
            var stream = Bytes(s.ToString());
            objetos.Add(Bytes($"<< /Length {stream.Length} >>\nstream\n").Concat(stream).Concat(Bytes("\nendstream")).ToArray());
        }
        using var output = new MemoryStream();
        void Escribir(string s) => output.Write(Bytes(s));
        Escribir("%PDF-1.4\n%âãÏÓ\n");
        var offsets = new List<long>{0};
        for(var i=0;i<objetos.Count;i++) { offsets.Add(output.Position); Escribir($"{i+1} 0 obj\n"); output.Write(objetos[i]); Escribir("\nendobj\n"); }
        var xref = output.Position;
        Escribir($"xref\n0 {objetos.Count+1}\n0000000000 65535 f \n");
        foreach(var offset in offsets.Skip(1)) Escribir($"{offset:D10} 00000 n \n");
        Escribir($"trailer\n<< /Size {objetos.Count+1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return output.ToArray();
    }
}
