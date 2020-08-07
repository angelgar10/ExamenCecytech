using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class Aspirante : IdentityUser<int>
    {
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Nombre { get; set; }
        public string Estatus { get; set; }
        public string Ficha { get; set; }

        public int? GrupoId { get; set; }
        public Grupo Grupo { get; set; }
        public string Genero { get; set; }
        public decimal Edad { get; set; }
        public decimal PromedioSecundaria { get; set; }
        public string NombreSecundaria { get; set; }
        public string TipoSecundaria { get; set; }
        public string DescripcionOtraSecundaria { get; set; } = "";
        public string TipoSostenimientoSecundaria { get; set; }
        [Required()]
        public string PlainPass { get; set; }
        public virtual ICollection<RespuestaEvaluacion> RespuestasEvaluacion { get; set; } = new HashSet<RespuestaEvaluacion>();

        public string EspecialidadId { get; set; }
        public Especialidad Especialidad { get; set; }
    }
}
