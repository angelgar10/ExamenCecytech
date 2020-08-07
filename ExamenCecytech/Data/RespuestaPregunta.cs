using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class RespuestaPregunta
    {
        public RespuestaPregunta()
        {
            var numRand = new Random(DateTime.Now.Millisecond);
            Orden1 = numRand.Next(1, 100);
            Orden2 = numRand.Next(1, 100);
        }

        public int RespuestaPreguntaId { get; set; }

        public int CompetenciaId { get; set; }
        public int PreguntaId { get; set; }
        public Pregunta Pregunta { get; set; }

        public int Orden1 { get; set; }
        public int Orden2 { get; set; }
        public string Texto { get; set; }
        public string ClaveCOSDAC { get; set; }
        public int Valor { get; set; } = 0;
    }
}
