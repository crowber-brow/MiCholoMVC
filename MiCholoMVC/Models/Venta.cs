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

        [Required]
        [StringLength(30)]
        public string MetodoPago { get; set; } = string.Empty;

        public Caja? Caja { get; set; }

        public ICollection<DetalleVenta>? Detalles { get; set; }
    }
}