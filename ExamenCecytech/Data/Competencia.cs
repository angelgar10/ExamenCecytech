using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class Competencia
    {
        public int CompetenciaId { get; set; }
        public string Nombre { get; set; }
        public int TiempoParaResolver { get; set; } = 0;
        public string LecturaPrevia { get; set; }
        public virtual ICollection<Pregunta> Preguntas { get; set; } = new HashSet<Pregunta>();
    }
}
