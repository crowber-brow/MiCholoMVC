using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class CajaAbrirViewModel
    {
        [Required(ErrorMessage = "Ingrese el nombre de la caja.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, double.MaxValue,
            ErrorMessage = "El monto inicial no puede ser negativo.")]
        public decimal MontoInicial { get; set; }
    }
}