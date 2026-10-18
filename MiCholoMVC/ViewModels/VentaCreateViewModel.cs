using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class VentaCreateViewModel
    {
        [Required(ErrorMessage = "Seleccione el método de pago.")]
        public string MetodoPago { get; set; } = string.Empty;
        [Range(typeof(decimal), "0", "99999999")]
        public decimal DineroRecibido { get; set; }
        public Guid TokenOperacion { get; set; } = Guid.NewGuid();

        public List<DetalleVentaViewModel> Detalles { get; set; }
            = new List<DetalleVentaViewModel>();
    }
}
