namespace MiCholoMVC.ViewModels
{
    public class StockProductoViewModel
    {
        public int IdProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        public bool StockBajo =>
            StockActual <= StockMinimo;
    }
}