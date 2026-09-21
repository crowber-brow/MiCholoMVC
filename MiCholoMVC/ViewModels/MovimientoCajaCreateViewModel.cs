using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class MovimientoCajaCreateViewModel
    {
        [Required(ErrorMessage = "Seleccione el tipo de movimiento.")]
        public string Tipo { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue,
            ErrorMessage = "El monto debe ser mayor que 0.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "Ingrese una descripción.")]
        [StringLength(250,
            ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;
    }
}