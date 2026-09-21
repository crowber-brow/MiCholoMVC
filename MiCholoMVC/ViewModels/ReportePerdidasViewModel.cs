using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class ReportePerdidasViewModel
    {
        [DataType(DataType.Date)]
        public DateTime? FechaDesde { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaHasta { get; set; }

        public string? Motivo { get; set; }

        public int CantidadTotal { get; set; }

        public decimal ValorEconomicoTotal { get; set; }

        public List<ReportePerdidaDetalleViewModel> Perdidas { get; set; }
            = new List<ReportePerdidaDetalleViewModel>();

        public List<ReportePerdidaResumenViewModel> ResumenPorMotivo { get; set; }
            = new List<ReportePerdidaResumenViewModel>();
    }

    public class ReportePerdidaDetalleViewModel
    {
        public int IdPerdida { get; set; }

        public DateTime Fecha { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string NumeroLote { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public string Justificacion { get; set; } = string.Empty;

        public decimal PrecioCompra { get; set; }

        public decimal ValorEconomico { get; set; }
    }

    public class ReportePerdidaResumenViewModel
    {
        public string Motivo { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal ValorEconomico { get; set; }
    }
}