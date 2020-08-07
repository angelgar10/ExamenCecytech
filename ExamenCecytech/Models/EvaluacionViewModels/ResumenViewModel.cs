using ExamenCecytech.Data;
using System.Collections.Generic;

namespace ExamenCecytech.Models.EvaluacionViewModels
{
    public class ResumenViewModel
    {
        public Aspirante Aspirante { get; set; }
        public ICollection<RespuestaEvaluacion> RespuestasEvaluacion { get; set; } = new HashSet<RespuestaEvaluacion>();
    }
}
