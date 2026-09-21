using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.Models
{
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        [Required]
        [StringLength(150)]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(11)]
        public string? Ruc { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public bool Estado { get; set; } = true;

        public ICollection<Producto>? Productos { get; set; }
    }
}