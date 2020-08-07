using ExamenCecytech.Data;
using System.Collections.Generic;

namespace ExamenCecytech.Models.EvaluacionViewModels
{
    public class CompetenciaPreguntasRespuestasViewModel
    {
        public Competencia Competencia { get; set; }
        public ICollection<RespuestaPreguntaViewModel> RespuestasEvaluacion { get; set; }
    }
}
