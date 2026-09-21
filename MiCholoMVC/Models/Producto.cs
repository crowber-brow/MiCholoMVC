using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiCholoMVC.Models
{
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [Required]
        public int IdCategoria { get; set; }

        public int? IdProveedor { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCompra { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        public int StockMinimo { get; set; } = 5;

        public bool Estado { get; set; } = true;

        public DateTime FechaRegistro { get; set; }

        public Categoria? Categoria { get; set; }

        public Proveedor? Proveedor { get; set; }

        public ICollection<Lote>? Lotes { get; set; }
    }
}