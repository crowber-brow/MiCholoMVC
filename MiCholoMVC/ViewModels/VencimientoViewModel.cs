namespace MiCholoMVC.ViewModels

{
    public class VencimientoViewModel
    {
        public int IdLote { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string NumeroLote { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public int DiasRestantes
        {
            get
            {
                return (FechaVencimiento.Date - DateTime.Today).Days;
            }
        }

        public string EstadoVencimiento
        {
            get
            {
                if (DiasRestantes <= 0)
                    return "VENCIDO";

                if (DiasRestantes <= 7)
                    return "ROJO";

                if (DiasRestantes <= 30)
                    return "AMARILLO";

                return "VERDE";
            }
        }
    }
}