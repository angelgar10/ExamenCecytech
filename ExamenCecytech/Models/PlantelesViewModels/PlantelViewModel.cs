using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Models.PlantelesViewModels
{
    public class PlantelViewModel
    {
        public string Plantel { get; set; }
        public int Grupos { get; set; }
        public int EvaluacionesExpedidas { get; set; }
        public ICollection<string> AdministradoresPlantel { get; set; } = new HashSet<string>();
        public int EvaluacionesAplicadas { get; set; }
        public int EvaluacionesPendientes { get { return EvaluacionesExpedidas - EvaluacionesAplicadas; } }
        public decimal Avance
        {
            get
            {
                if (EvaluacionesExpedidas != 0)
                {
                    return ((decimal)EvaluacionesAplicadas / EvaluacionesExpedidas) * 100;
                }
                return 0;
            }
        }
        public string Color
        {
            get
            {

                switch (Avance)
                {
                    case decimal n when n > 75: return "success";
                    case decimal n when n > 50: return "info";
                    case decimal n when n > 25: return "warning";
                    default: return "danger";
                }
            }
        }
        public string ClavePlantel { get; internal set; }
    }
}
