using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class UsuarioPlantel
    {
        public int Id { get; set; }
        public Aspirante Aspirante { get; set; }
        public string ClavePlantel { get; set; }
        public Plantel Plantel { get; set; }
    }
}
