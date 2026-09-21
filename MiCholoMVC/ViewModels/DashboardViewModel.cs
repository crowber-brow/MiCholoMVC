namespace MiCholoMVC.ViewModels
{
    public class DashboardViewModel
    {
        public decimal VentasHoy { get; set; }

        public decimal VentasTotales { get; set; }

        public int ProductosStockBajo { get; set; }

        public int LotesPorVencer { get; set; }

        public int LotesVencidos { get; set; }

        public decimal TotalPerdidas { get; set; }

        public int CantidadPerdidas { get; set; }

        public int CantidadVentas { get; set; }

        public List<VentaMetodoPagoViewModel> VentasPorMetodoPago { get; set; }
            = new List<VentaMetodoPagoViewModel>();

        public List<VentaDiaViewModel> VentasUltimos7Dias { get; set; }
            = new List<VentaDiaViewModel>();
    }

    public class VentaMetodoPagoViewModel
    {
        public string MetodoPago { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public int Cantidad { get; set; }
    }

    public class VentaDiaViewModel
    {
        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public int Cantidad { get; set; }
    }
}