using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.Models
{
    public class Categoria
    {
        [Key]
        public int IdCategoria { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        public bool Estado { get; set; } = true;

        public ICollection<Producto>? Productos { get; set; }
    }
}