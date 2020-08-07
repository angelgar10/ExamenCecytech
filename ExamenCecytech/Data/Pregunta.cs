using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class Pregunta
    {
        private static Random numRand;

        public Pregunta()
        {
            numRand = new Random(DateTime.Now.Millisecond);
            Orden1 = numRand.Next(1, 100);
            Orden2 = numRand.Next(1, 100);
        }

        public int PreguntaId { get; set; }

        public int CompetenciaId { get; set; }
        public Competencia Competencia { get; set; }
        public int NumeroPregunta { get; set; }
        public int Orden1 { get; set; }
        public int Orden2 { get; set; }
        public string Texto { get; set; }
        public string LecturaPrevia { get; set; }
        public virtual ICollection<RespuestaPregunta> RespuestasPregunta { get; set; } = new HashSet<RespuestaPregunta>();
    }
}
