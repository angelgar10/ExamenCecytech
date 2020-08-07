using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Models.PlantelesViewModels
{
    public class RespuestaVM
    {
        public string Letra { get; set; }
        public string Texto { get; set; }
        public bool Correcta { get; set; }
        public int Frecuencia { get; set; }
        public int NoContestaron { get; set; }
    }
}
