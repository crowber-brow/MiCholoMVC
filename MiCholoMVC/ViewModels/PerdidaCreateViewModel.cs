using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class PerdidaCreateViewModel
    {
        [Required(ErrorMessage = "Seleccione un producto.")]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "Seleccione un lote.")]
        public int? IdLote { get; set; }

        [Required(ErrorMessage = "Ingrese la cantidad.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Seleccione un motivo.")]
        public string Motivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La justificación es obligatoria.")]
        [StringLength(500, ErrorMessage = "La justificación no puede superar los 500 caracteres.")]
        public string Justificacion { get; set; } = string.Empty;
    }
}