using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int IdMovimiento { get; set; }

        public int IdProducto { get; set; }

        public int? IdLote { get; set; }

        [Required]
        [StringLength(30)]
        public string Tipo { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public int SaldoLote { get; set; }
        public int SaldoProducto { get; set; }
        public decimal ValorUnitario { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }

        public DateTime Fecha { get; set; }

        public Producto? Producto { get; set; }

        public Lote? Lote { get; set; }
    }
}
