
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MiCholoMVC.Models
{
    public class Producto
    {
        [Key]
        [Required]
        [JsonRequired]
        public int IdProducto { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [Required]
        [JsonRequired]
        public int IdCategoria { get; set; }

        public int? IdProveedor { get; set; }

        [Required]
        [JsonRequired]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCompra { get; set; }

        [Required]
        [JsonRequired]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Required]
        [JsonRequired]
        public int StockMinimo { get; set; } = 5;

        [Required]
        [JsonRequired]
        public bool Estado { get; set; } = true;

        [Required]
        [JsonRequired]
        public DateTime FechaRegistro { get; set; }

        public Categoria? Categoria { get; set; }

        public Proveedor? Proveedor { get; set; }

        public ICollection<Lote>? Lotes { get; set; }
    }
}
