using ExamenCecytech.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Data
{
    public class ApplicationDbContext : IdentityDbContext<Aspirante, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Competencia> Competencias { get; set; }

        //        public DbSet<Aspirante> Aspirantes { get; set; }

        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<RespuestaPregunta> Respuestas { get; set; }
        public DbSet<RespuestaEvaluacion> RespuestasEvaluaciones { get; set; }
        public DbSet<Plantel> Planteles { get; set; }
        public DbSet<UsuarioPlantel> UsuariosPlantel { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Aspirante> Aspirante { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Especialidades
            builder.Entity<Especialidad>().ToTable("Especialidades");
            builder.Entity<Especialidad>().Property(e => e.EspecialidadId).HasMaxLength(12);
            builder.Entity<Especialidad>().HasKey(e => e.EspecialidadId);
            builder.Entity<Especialidad>().Property(e => e.Nombre).HasMaxLength(60).IsRequired();

            #endregion

            #region Planteles
            builder.Entity<Plantel>().ToTable("Planteles");
            builder.Entity<Plantel>().HasKey(p => p.PlantelId);
            builder.Entity<Plantel>().Property(p => p.ClavePlantel).IsRequired().HasMaxLength(3);
            builder.Entity<Plantel>().Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            builder.Entity<Plantel>().HasAlternateKey(p => p.ClavePlantel);
            builder.Entity<Plantel>().Property(p => p.ClaveCentroTrabajo).HasMaxLength(10);
            builder.Entity<Plantel>().HasIndex(p => p.ClaveCentroTrabajo).IsUnique();
            builder.Entity<Plantel>().Property(p => p.ClaveSIIACE).HasMaxLength(2);
            builder.Entity<Plantel>().HasIndex(p => p.ClaveSIIACE).IsUnique();
            #endregion

            #region Usuarios Plantel
            builder.Entity<UsuarioPlantel>().ToTable("UsuariosPlantel");
            builder.Entity<UsuarioPlantel>().HasKey(up => new { up.Id, up.ClavePlantel });
            builder.Entity<UsuarioPlantel>().HasOne(up => up.Aspirante).WithMany().HasForeignKey(up => up.Id);
            builder.Entity<UsuarioPlantel>().HasOne(up => up.Plantel).WithMany(p => p.UsuariosPlantel).HasPrincipalKey(p => p.ClavePlantel);

            #endregion

            #region Aspirantes
            //builder.Entity<Aspirante>()
            //    .ToTable("Aspirantes");

            //builder.Entity<Aspirante>()
            //    .HasKey(a => a.UserId);

            builder.Entity<Aspirante>().Property(a => a.Paterno).IsRequired().HasMaxLength(40);
            builder.Entity<Aspirante>().Property(a => a.Materno).IsRequired().HasMaxLength(40);
            builder.Entity<Aspirante>().Property(a => a.Nombre).IsRequired().HasMaxLength(40);
            builder.Entity<Aspirante>().Property(a => a.Ficha).IsRequired().HasMaxLength(18);

            // Solo un registro por ficha
            builder.Entity<Aspirante>().HasAlternateKey(a => a.Ficha);
            builder.Entity<Aspirante>().Property(a => a.Estatus).HasMaxLength(2);
            builder.Entity<Aspirante>()
                .HasOne(a => a.Grupo)
                .WithMany(g => g.Aspirantes)
                .HasForeignKey(a => a.GrupoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Aspirante>().Property(a => a.Genero).IsRequired().HasMaxLength(1);
            builder.Entity<Aspirante>().Property(a => a.NombreSecundaria).IsRequired().HasMaxLength(100);
            builder.Entity<Aspirante>().Property(a => a.TipoSecundaria).IsRequired().HasMaxLength(50);
            builder.Entity<Aspirante>().Property(a => a.DescripcionOtraSecundaria).IsRequired().HasMaxLength(50);
            builder.Entity<Aspirante>().Property(a => a.TipoSostenimientoSecundaria).IsRequired().HasMaxLength(50);
            builder.Entity<Aspirante>().Property(a => a.PlainPass).IsRequired().HasMaxLength(50);
            builder.Entity<Aspirante>().Property(a => a.EspecialidadId).HasMaxLength(12);
            builder.Entity<Aspirante>().HasOne(a => a.Especialidad)
                .WithMany()
                .HasForeignKey(a => a.EspecialidadId)
                .IsRequired(false);
            #endregion

            #region Competencia
            builder.Entity<Competencia>().ToTable("Competencias");
            builder.Entity<Competencia>().HasKey(c => c.CompetenciaId);
            builder.Entity<Competencia>().HasIndex(c => c.Nombre).IsUnique();
            builder.Entity<Competencia>()
                .Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(50);
            builder.Entity<Competencia>()
                .Property(c => c.LecturaPrevia)
                .IsRequired(false);

            #endregion

            #region Pregunta
            builder.Entity<Pregunta>().ToTable("Preguntas");
            builder.Entity<Pregunta>().HasKey(p => p.PreguntaId);
            builder.Entity<Pregunta>().HasAlternateKey(p => new { p.CompetenciaId, p.PreguntaId });

            // Para que el numero de pregunta, sea unico por evaluacion
            builder.Entity<Pregunta>().HasAlternateKey(p => p.NumeroPregunta);
            builder.Entity<Pregunta>().Property(p => p.Texto).IsRequired();
            builder.Entity<Pregunta>().Property(p => p.LecturaPrevia).IsRequired(false);


            builder.Entity<Pregunta>()
                .HasOne(p => p.Competencia)
                .WithMany(c => c.Preguntas)
                .HasForeignKey(p => p.CompetenciaId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion


            #region Grupo
            builder.Entity<Grupo>().ToTable("Grupos");
            builder.Entity<Grupo>().HasKey(g => g.GrupoId);
            builder.Entity<Grupo>().Property(g => g.ClavePlantel).IsRequired().HasMaxLength(3);
            builder.Entity<Grupo>().Property(g => g.Nombre).IsRequired().HasMaxLength(40);
            builder.Entity<Grupo>().Property(g => g.ClaveSIIACE).HasMaxLength(5);
            builder.Entity<Grupo>().Property(g => g.Semestre).HasMaxLength(1);
            builder.Entity<Grupo>().Property(g => g.Turno).HasMaxLength(1);

            // Se establece la relacion uno a muchos apuntando a llave no primaria en la tabla planteles
            builder.Entity<Grupo>()
                .HasOne(g => g.Plantel)
                .WithMany(p => p.GruposPlantel)
                .HasPrincipalKey(p => p.ClavePlantel)
                .HasForeignKey(g => g.ClavePlantel)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Respuestas a las Preguntas
            builder.Entity<RespuestaPregunta>().ToTable("RespuestasPreguntas");
            builder.Entity<RespuestaPregunta>().HasKey(rp => rp.RespuestaPreguntaId);
            builder.Entity<RespuestaPregunta>().Property(rp => rp.ClaveCOSDAC).IsRequired().HasMaxLength(1);
            builder.Entity<RespuestaPregunta>().Property(rp => rp.Texto).IsRequired();


            // ClaveCosdac unica para cada respuesta
            builder.Entity<RespuestaPregunta>().HasAlternateKey(rp => new { rp.CompetenciaId, rp.PreguntaId, rp.ClaveCOSDAC });

            builder.Entity<RespuestaPregunta>()
                .HasOne(rp => rp.Pregunta)
                .WithMany(p => p.RespuestasPregunta)
                .HasForeignKey(rp => new { rp.CompetenciaId, rp.PreguntaId })
                .HasPrincipalKey(p => new { p.CompetenciaId, p.PreguntaId })
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Respuestas Evaluaciones
            builder.Entity<RespuestaEvaluacion>().ToTable("RespuestasEvaluaciones");
            builder.Entity<RespuestaEvaluacion>()
                .HasKey(re => re.RespuestaEvaluacionId);

            // Relacion con la pregunta
            builder.Entity<RespuestaEvaluacion>()
                .HasOne(re => re.Pregunta)
                .WithMany()
                .HasForeignKey(re => new { re.CompetenciaId, re.PreguntaId })
                .HasPrincipalKey(p => new { p.CompetenciaId, p.PreguntaId })
                .OnDelete(DeleteBehavior.Restrict);

            // Relacion con catalogo de respuestas
            builder.Entity<RespuestaEvaluacion>()
                .HasOne(re => re.RespuestaPregunta)
                .WithMany()
                .HasForeignKey(re => new { re.CompetenciaId, re.PreguntaId, re.RespuestaPreguntaId })
                .HasPrincipalKey(rp => new { rp.CompetenciaId, rp.PreguntaId, rp.RespuestaPreguntaId })
                .OnDelete(DeleteBehavior.Restrict);


            // Limita a que solo exista una respuesta por pregunta y aspirante
            builder.Entity<RespuestaEvaluacion>()
                .HasAlternateKey(re => new { re.AspiranteId, re.CompetenciaId, re.PreguntaId });

            builder.Entity<RespuestaEvaluacion>()
                .Property(re => re.FechaCreacion);

            builder.Entity<RespuestaEvaluacion>()
                .Property(re => re.FechaModificacion);

            // Registrar el usuario que creo el registro
            builder.Entity<RespuestaEvaluacion>()
                .Property(re => re.UsuarioCreacion)
                .IsRequired()
                .HasMaxLength(255);

            // Registrar el usuario que modifico el registro
            builder.Entity<RespuestaEvaluacion>()
                .Property(re => re.UsuarioModificacion)
                .IsRequired()
                .HasMaxLength(255);
            #endregion
        }
    }
}
