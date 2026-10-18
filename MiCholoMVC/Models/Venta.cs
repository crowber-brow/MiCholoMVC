using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiCholoMVC.Models
{
    public class Venta
    {
        [Key]
        public int IdVenta { get; set; }

        public int IdCaja { get; set; }

        public DateTime Fecha { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }
        public decimal DineroRecibido { get; set; }
        public decimal Vuelto { get; set; }
        [StringLength(20)]
        public string Estado { get; set; } = "VIGENTE";
        public Guid TokenOperacion { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        [StringLength(500)]
        public string? MotivoAnulacion { get; set; }
        public bool StockDevuelto { get; set; }

        [Required]
        [StringLength(30)]
        public string MetodoPago { get; set; } = string.Empty;

        public Caja? Caja { get; set; }

        public ICollection<DetalleVenta>? Detalles { get; set; }
    }
}
