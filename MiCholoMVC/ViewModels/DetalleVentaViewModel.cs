using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class DetalleVentaViewModel
    {
        [Range(1, int.MaxValue)]
        public int IdProducto { get; set; }
        public int IdLote { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string NumeroLote { get; set; } = string.Empty;

        public DateTime FechaVencimiento { get; set; }

        public int StockDisponible { get; set; }

        public decimal PrecioUnitario { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0.")]
        public int Cantidad { get; set; }

        public decimal Subtotal
        {
            get
            {
                return Cantidad * PrecioUnitario;
            }
        }
    }
}
