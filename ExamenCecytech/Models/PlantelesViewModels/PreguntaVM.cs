using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Models.PlantelesViewModels
{
    public class PreguntaVM
    {
        public int NumeroPregunta { get; set; }
        public string Texto { get; set; }
        public RespuestaVM[] Respuestas { get; set; }
        public int SinContestar { get; set; }
    }
}
