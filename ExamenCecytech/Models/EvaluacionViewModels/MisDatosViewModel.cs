using System.ComponentModel.DataAnnotations;

namespace ExamenCecytech.Models.EvaluacionViewModels
{
    public class MisDatosViewModel
    {
        public string Ficha { get; set; }
        public string Grupo { get; set; }
        public string Plantel { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Paterno { get; set; } = "";

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Materno { get; set; } = "";

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Genero { get; set; } = "";

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public decimal Edad { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public decimal PromedioSecundaria { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string NombreSecundaria { get; set; } = "";

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string TipoSecundaria { get; set; } = "";

        public string DescripcionOtraSecundaria { get; set; } = "";

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string TipoSostenimientoSecundaria { get; set; } = "";
    }
}
