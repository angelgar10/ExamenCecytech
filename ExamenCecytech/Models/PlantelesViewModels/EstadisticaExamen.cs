using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Models.PlantelesViewModels
{
    public class EstadisticaExamen
    {
        public string NombreCompetencia { get; set; }
        public int NumeroPreguntas { get; set; }
        public string PrefijoGraficas { get; set; }
        public string Titulo { get; set; }
        public PreguntaVM[] Preguntas { get; set; }
    }
}
