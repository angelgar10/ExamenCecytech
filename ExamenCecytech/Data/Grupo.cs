using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class Grupo
    {
        public int GrupoId { get; set; }
        public string ClavePlantel { get; set; }
        public Plantel Plantel { get; set; }
        public string Nombre { get; set; }
        public string ClaveSIIACE { get; set; }
        public string Turno { get; set; }
        public string Semestre { get; set; }
        public DateTime FechaExamen { get; set; }
        public bool EvaluacionHabilitada { get; set; } = false;
        public virtual ICollection<Aspirante> Aspirantes { get; set; } = new HashSet<Aspirante>();
    }
}
