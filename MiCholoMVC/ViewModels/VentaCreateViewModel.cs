using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class VentaCreateViewModel
    {
        [Required(ErrorMessage = "Seleccione el método de pago.")]
        public string MetodoPago { get; set; } = string.Empty;

        public List<DetalleVentaViewModel> Detalles { get; set; }
            = new List<DetalleVentaViewModel>();
    }
}