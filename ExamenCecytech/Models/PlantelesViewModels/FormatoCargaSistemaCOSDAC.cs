using System.ComponentModel.DataAnnotations;

namespace ExamenCecytech.Models.PlantelesViewModels
{
    public class FormatoCargaSistemaCOSDAC
    {
        /// <summary>
        /// Columna A
        /// Descripcion: Folio
        /// Caracteristicas Maximo 6 caracteres numericos
        /// </summary>
        public string ColumnaA { get; set; }
        /// <summary>
        /// Columna B
        /// Descripcion: Apellido Paterno del alumno
        /// Caracteristicas Maximo 12 caracteres 
        /// </summary>
        [MaxLength(12)]
        public string ColumnaB { get; set; }
        /// <summary>
        /// Columna C
        /// Descripcion: Apellido Materno del alumno
        /// Caracteristicas Maximo 12 caracteres 
        /// </summary>
        [MaxLength(12)]
        public string ColumnaC { get; set; }
        /// <summary>
        /// Columna D
        /// Descripcion: nombre del alumno
        /// Caracteristicas Maximo 12 caracteres 
        /// </summary>
        [MaxLength(12)]
        public string ColumnaD { get; set; }
        /// <summary>
        /// Columna E
        /// Descripcion: Edad del Aspirante
        /// Caracteristicas 2 caracteres numericos
        /// </summary>
        public string ColumnaE { get; set; }
        /// <summary>
        /// Columna F
        /// Descripcion: Genero del aspirante
        /// Caracteristicas Maximo 1 caracter, M si es mujer y H si es hombre
        /// </summary>
        [MaxLength(1)]
        public string ColumnaF { get; set; }
        /// <summary>
        /// Columna G
        /// Descripcion: Nombre de la escuela de procedencia
        /// Caracteristicas Maximo 40 caracteres 
        /// </summary>
        [MaxLength(40)]
        public string ColumnaG { get; set; }
        /// <summary>
        /// Columna H
        /// Descripcion: Respuestas de la aplicacion
        /// Caracteristicas Deben de ser 96 caracteres exactos
        /// </summary>
        [MaxLength(96)]
        public string ColumnaH { get; set; }
        /// <summary>
        /// Columna I
        /// Descripcion: Tipo de secundaria de procedencia
        /// 1 caracter numerico de las siguientes opciones del 1 al 5:
        ///• 1 Secundaria general
        ///• 2 Secundaria técnica
        ///• 3 Secundaria para trabajadores
        ///• 4 Secundaria comunitaria
        ///• 5 Telesecundaria
        ///• 6 Otra
        /// </summary>
        [MaxLength(1)]
        public string ColumnaI { get; set; }
        /// <summary>
        /// Columna J
        /// Descripcion: Tipo de sostenimiento de la secundaria de procedencia
        /// Caracteristicas: 1 caracter numerico de las siguientes opciones del 1 al 3:
        ///• 1 Federal
        ///• 2 Estatal
        ///• 3 Particular
        /// </summary>
        [MaxLength(1)]
        public string ColumnaJ { get; set; }
        /// <summary>
        /// Columna K
        /// Descripcion: Promedio de secundaria del aspirante
        /// 1 caracter numerico de las siguientes opciones:
        /// • 1 De 6.0 a 6.5
        /// • 2 De 6.6 a 7.0
        /// • 3 De 7.1 a 7.5
        /// • 4 De 7.6 a 8.0
        /// • 5 De 8.1 a 8.5
        /// • 6 De 8.6 a 9.0
        /// • 7 De 9.1 a 9.5
        /// • 8 De 9.6 a 10.0
        /// </summary>

        public string ColumnaK { get; set; }


    }
}
