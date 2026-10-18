using System.ComponentModel.DataAnnotations;

namespace MiCholoMVC.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese su correo.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(254)]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su contraseña.")]
        [DataType(DataType.Password)]
        [StringLength(128)]
        public string Password { get; set; } = string.Empty;

        public bool Recordarme { get; set; }
    }
}
