
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MiCholoMVC.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        [JsonRequired]
        public int IdProducto { get; set; }

        public int? IdLote { get; set; }

        [Required]
        [StringLength(30)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [JsonRequired]
        public int Cantidad { get; set; }

        [Required]
        [JsonRequired]
        public int SaldoLote { get; set; }

        [Required]
        [JsonRequired]
        public int SaldoProducto { get; set; }

        [Required]
        [JsonRequired]
        public decimal ValorUnitario { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [Required]
        [JsonRequired]
        public DateTime Fecha { get; set; }

        public Producto? Producto { get; set; }

        public Lote? Lote { get; set; }
    }
}
