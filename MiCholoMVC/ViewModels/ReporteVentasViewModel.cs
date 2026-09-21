using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class ReporteVentasViewModel
    {
        [DataType(DataType.Date)]
        public DateTime? FechaDesde { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaHasta { get; set; }

        public int? IdCaja { get; set; }

        public int? IdCategoria { get; set; }

        public decimal TotalVentas { get; set; }

        public int CantidadVentas { get; set; }

        public int CantidadProductosVendidos { get; set; }

        public List<ReporteVentaDetalleViewModel> Ventas { get; set; }
            = new List<ReporteVentaDetalleViewModel>();

        public List<ReporteFiltroCajaViewModel> Cajas { get; set; }
            = new List<ReporteFiltroCajaViewModel>();

        public List<ReporteFiltroCategoriaViewModel> Categorias { get; set; }
            = new List<ReporteFiltroCategoriaViewModel>();
    }

    public class ReporteVentaDetalleViewModel
    {
        public int IdVenta { get; set; }

        public DateTime Fecha { get; set; }

        public string Caja { get; set; } = string.Empty;

        public string MetodoPago { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public int CantidadProductos { get; set; }

        public string Categorias { get; set; } = string.Empty;
    }

    public class ReporteFiltroCajaViewModel
    {
        public int IdCaja { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }

    public class ReporteFiltroCategoriaViewModel
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}