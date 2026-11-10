
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MiCholoMVC.Models
{
    public class Categoria
    {
        [Key]
        [Required]
        [JsonRequired]
        public int IdCategoria { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [Required]
        [JsonRequired]
        public bool Estado { get; set; } = true;

        public ICollection<Producto>? Productos { get; set; }
    }
}
