using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.Models
{
    public class Perdida
    {
        [Key]
        public int IdPerdida { get; set; }

        public int IdLote { get; set; }

        public int Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public decimal ValorTotal => Cantidad * ValorUnitario;

        [Required]
        [StringLength(30)]
        public string Motivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Justificacion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public Lote? Lote { get; set; }
    }
}
