using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiCholoMVC.Models
{
    public class DetalleVenta
    {
        [Key]
        public int IdDetalleVenta { get; set; }

        public int IdVenta { get; set; }

        public int IdLote { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
        public decimal CostoUnitario { get; set; }
        [StringLength(150)]
        public string ProductoNombre { get; set; } = string.Empty;
        [StringLength(150)]
        public string CategoriaNombre { get; set; } = string.Empty;
        public int IdCategoriaHistorica { get; set; }

        public Venta? Venta { get; set; }

        public Lote? Lote { get; set; }
    }
}
