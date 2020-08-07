using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Models.EvaluacionViewModels
{
    public class RespuestaPreguntaViewModel
    {
        public int AspiranteId { get; set; }
        public int CompetenciaId { get; set; }
        public string NombreCompetencia { get; set; }
        public int PreguntaId { get; set; }
        public int NumeroPregunta { get; set; }
        public string LecturaPrevia { get; set; }
        public string Texto { get; set; }
        public List<RespuestaViewModel> Respuestas { get; set; }
        public int? RespuestaIdSeleccionada { get; set; }
        public string BackUrl { get; set; }
    }
}
