using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiCholoMVC.Models
{
    public class Caja
    {
        [Key]
        public int IdCaja { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "CERRADA";

        public DateTime? FechaApertura { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoInicial { get; set; }

        public DateTime? FechaCierre { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MontoCierre { get; set; }

        public ICollection<Venta>? Ventas { get; set; }

        public ICollection<MovimientoCaja>? MovimientosCaja { get; set; }
    }
}