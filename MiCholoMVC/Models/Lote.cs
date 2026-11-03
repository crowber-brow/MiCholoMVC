
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MiCholoMVC.Models
{
    public class Lote
    {
        [Key]
        public int IdLote { get; set; }

        [Required]
        [JsonRequired]
        public int IdProducto { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroLote { get; set; } = string.Empty;

        [Required]
        [JsonRequired]
        public int Cantidad { get; set; }

        [Required]
        [JsonRequired]
        public decimal CostoUnitario { get; set; }

        [Required]
        [JsonRequired]
        public DateTime FechaIngreso { get; set; }

        [Required]
        [JsonRequired]
        public DateTime FechaVencimiento { get; set; }

        public bool Estado { get; set; } = true;

        public Producto? Producto { get; set; }
    }
}
