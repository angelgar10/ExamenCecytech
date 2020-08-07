using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class Plantel
    {
        public int PlantelId { get; set; }
        public string ClavePlantel { get; set; }
        public string Nombre { get; set; }
        public string ClaveCentroTrabajo { get; set; }
        public string ClaveSIIACE { get; set; }
        public virtual ICollection<UsuarioPlantel> UsuariosPlantel { get; set; } = new HashSet<UsuarioPlantel>();
        public virtual ICollection<Grupo> GruposPlantel { get; set; } = new HashSet<Grupo>();
    }
}
