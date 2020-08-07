using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class RespuestaEvaluacion
    {
        public int RespuestaEvaluacionId { get; set; }
        public int AspiranteId { get; set; }
        public Aspirante Aspirante { get; set; }


        public int CompetenciaId { get; set; }
        public int PreguntaId { get; set; }
        public int RespuestaPreguntaId { get; set; }
        public RespuestaPregunta RespuestaPregunta { get; set; }

        public Pregunta Pregunta { get; set; }

        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string UsuarioModificacion { get; set; }
    }
}
