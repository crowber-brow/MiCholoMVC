using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.Models
{
    public class Lote
    {
        [Key]
        public int IdLote { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroLote { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }

        public DateTime FechaIngreso { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public bool Estado { get; set; } = true;

        public Producto? Producto { get; set; }
    }
}
