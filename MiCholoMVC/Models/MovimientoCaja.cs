using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiCholoMVC.Models
{
    public class MovimientoCaja
    {
        [Key]
        public int IdMovimiento { get; set; }

        public int IdCaja { get; set; }

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }

        public DateTime Fecha { get; set; }

        public Caja? Caja { get; set; }
    }
}