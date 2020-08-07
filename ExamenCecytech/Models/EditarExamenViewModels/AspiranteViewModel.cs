namespace ExamenCecytech.Models.EditarExamenViewModels
{
    public class AspiranteViewModel
    {
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Nombre { get; set; }
        public string Estatus { get; set; }
        public string Matricula { get; set; }
        public int GrupoId { get; set; }
        public string Genero { get; set; }
        public decimal Edad { get; set; }
        public decimal PromedioSecundaria { get; set; }
        public string NombreSecundaria { get; set; }
        public string TipoSecundaria { get; set; }
        public string DescripcionOtraSecundaria { get; set; } = "";
        public string TipoSostenimientoSecundaria { get; set; }
        public string PlainPass { get; set; }
        public int Semestre { get; set; }
    }
}
