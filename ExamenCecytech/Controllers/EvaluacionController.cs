using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExamenCecytech.Data;
using ExamenCecytech.Models.EvaluacionViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExamenCecytech.Controllers
{
    [Authorize(Roles = "Aspirante")]
    public class EvaluacionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Aspirante> _userManager;
        private readonly string NombreExamen;
        private readonly string TipoPlantel;

        public EvaluacionController(ApplicationDbContext context,
            UserManager<Aspirante> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            NombreExamen = configuration["DatosExamen:Nombre"];
            TipoPlantel = configuration["DatosExamen:TipoPlantel"];
        }
        public async Task<IActionResult> Index()
        {
            if (!await MiGrupoEstaHabilitado())
            {
                return View("GrupoInhabilitado");
            }
            ViewBag.Accion = nameof(Index);
            return View();
        }

        private async Task<bool> MiGrupoEstaHabilitado()
        {
            var usuario = await AspiranteActual();
            if (usuario == null)
            {
                return false;
            }
            var aspirante = await _context.Aspirante
                .AsNoTracking()
                .Include(a => a.Grupo)
                .FirstOrDefaultAsync(a => a.Id == usuario.Id);
            if (aspirante == null)
            {
                return false;
            }
            return aspirante.Grupo.EvaluacionHabilitada;
        }
        private async Task<Aspirante> AspiranteActual()
        {
            return await _userManager.GetUserAsync(User);
        }

        [HttpGet]
        public async Task<IActionResult> MisDatos()
        {
            if (!await MiGrupoEstaHabilitado())
            {
                return View("GrupoInhabilitado");
            }
            ViewBag.Accion = nameof(MisDatos);

            var usuario = await _userManager.GetUserAsync(User);
            var aspirante = _context.Aspirante
                .AsNoTracking()
                .Include(a => a.Grupo)
                .ThenInclude(g => g.Plantel)
                .Where(a => a.Id == usuario.Id)
                .Select(a => new MisDatosViewModel
                {
                    Ficha = a.Ficha,
                    Grupo = a.Grupo.Nombre,
                    Plantel = a.Grupo.Plantel.Nombre,
                    Paterno = a.Paterno,
                    Materno = a.Materno,
                    Nombre = a.Nombre,
                    Edad = a.Edad,
                    Genero = a.Genero,
                    PromedioSecundaria = a.PromedioSecundaria,
                    NombreSecundaria = a.NombreSecundaria,
                    TipoSecundaria = a.TipoSecundaria,
                    DescripcionOtraSecundaria = a.DescripcionOtraSecundaria,
                    TipoSostenimientoSecundaria = a.TipoSostenimientoSecundaria
                }).FirstOrDefaultAsync();

            return View(await aspirante);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> MisDatos([Bind]MisDatosViewModel MisDatos)
        {
            if (!await MiGrupoEstaHabilitado())
            {
                return View("GrupoInhabilitado");
            }
            var usuarioActual = await AspiranteActual();
            var aspiranteModificar = await _context.Aspirante.FirstOrDefaultAsync(a => a.Id == usuarioActual.Id);
            if (aspiranteModificar == null)
            {
                ModelState.AddModelError(string.Empty, "El aspirante no existe");
            }
            if (aspiranteModificar.Ficha != MisDatos.Ficha)
            {
                ModelState.AddModelError(string.Empty, "Los datos de ficha no coinciden");
            }
            if (!string.IsNullOrEmpty(MisDatos.TipoSecundaria) && MisDatos.TipoSecundaria.ToUpper().Trim() == "OTRA" && string.IsNullOrEmpty(MisDatos.DescripcionOtraSecundaria))
            {
                ModelState.AddModelError("DescripcionOtraSecundaria", "Seleccionaste otro tipo de secundaria, debes especificar que otro tipo");
            }
            if (ModelState.IsValid)
            {
                if (MisDatos.Paterno == null) { MisDatos.Paterno = ""; }
                if (MisDatos.Materno == null) { MisDatos.Materno = ""; }
                if (MisDatos.Nombre == null) { MisDatos.Nombre = ""; }
                if (MisDatos.Genero == null) { MisDatos.Genero = ""; }
                if (MisDatos.NombreSecundaria == null) { MisDatos.NombreSecundaria = ""; }
                if (MisDatos.TipoSecundaria == null) { MisDatos.TipoSecundaria = ""; }
                if (MisDatos.DescripcionOtraSecundaria == null) { MisDatos.DescripcionOtraSecundaria = ""; }
                if (MisDatos.TipoSostenimientoSecundaria == null) { MisDatos.TipoSostenimientoSecundaria = ""; }

                try
                {
                    aspiranteModificar.Paterno = MisDatos.Paterno.ToUpper().Trim();
                    aspiranteModificar.Materno = MisDatos.Materno.ToUpper().Trim();
                    aspiranteModificar.Nombre = MisDatos.Nombre.ToUpper().Trim();
                    aspiranteModificar.Edad = MisDatos.Edad;
                    aspiranteModificar.Genero = MisDatos.Genero.ToUpper().Trim();
                    aspiranteModificar.PromedioSecundaria = MisDatos.PromedioSecundaria;
                    aspiranteModificar.NombreSecundaria = MisDatos.NombreSecundaria.ToUpper().Trim();
                    aspiranteModificar.TipoSecundaria = MisDatos.TipoSecundaria.ToUpper().Trim();
                    aspiranteModificar.DescripcionOtraSecundaria = MisDatos.DescripcionOtraSecundaria.ToUpper().Trim();
                    aspiranteModificar.TipoSostenimientoSecundaria = MisDatos.TipoSostenimientoSecundaria.ToUpper().Trim();
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(MisDatos), "Evaluacion");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, "Ocurrio una excepcion: " + e.Message);
                    return View(MisDatos);
                }
            }

            return View(MisDatos);
        }

        public async Task<IActionResult> Competencia(string id)
        {
            if (!await MiGrupoEstaHabilitado())
            {
                return View("GrupoInhabilitado");
            }

            var aspirante = await AspiranteActual();
            int orden = aspirante.Id % 2;
            var competencia = (
                await
                    _context.Competencias
                        .AsNoTracking()
                        .Include(c => c.Preguntas)
                            .ThenInclude(p => p.RespuestasPregunta)
                        .Where(c => c.Nombre.ToUpper().Trim() == id.ToUpper().Trim())
                .FirstAsync()
                );
            ViewBag.Accion = nameof(competencia.Nombre);
            var respuestas = _context.Preguntas
                    .OrderBy(p => orden == 0 ? p.Orden1 : p.Orden2)
                    .Include(p => p.RespuestasPregunta)
                    .Where(p => p.CompetenciaId == competencia.CompetenciaId)
                    .Select(p => new RespuestaPreguntaViewModel()
                    {
                        AspiranteId = aspirante.Id,
                        CompetenciaId = competencia.CompetenciaId,
                        PreguntaId = p.PreguntaId,
                        NumeroPregunta = p.NumeroPregunta,
                        Texto = p.Texto,
                        LecturaPrevia = p.LecturaPrevia,
                        Respuestas = p.RespuestasPregunta
                        .OrderBy(rp => orden == 0 ? rp.Orden1 : rp.Orden2)
                        .Select(rp => new RespuestaViewModel()
                        {
                            RespuestaId = rp.RespuestaPreguntaId,
                            Texto = rp.Texto
                        }
                        )
                        .ToList(),
                        RespuestaIdSeleccionada = _context.RespuestasEvaluaciones
                                                    .Where(re =>
                                                        re.PreguntaId == p.PreguntaId
                                                        &&
                                                        re.AspiranteId == aspirante.Id
                                                        ).FirstOrDefault().RespuestaPreguntaId
                    }
                );

            return View("Competencia", new CompetenciaPreguntasRespuestasViewModel()
            {
                Competencia = competencia,
                RespuestasEvaluacion = await respuestas.ToListAsync()
            });

        }

        [HttpPost]
        public async Task<IActionResult> Respuesta([Bind] ContestarPreguntaViewModel contestacion, string BackUrl)
        {
            if (!await MiGrupoEstaHabilitado())
            {
                return View("GrupoInhabilitado");
            }
            StringBuilder errores = new StringBuilder();

            if (contestacion.AspiranteId == 0 || contestacion.CompetenciaId == 0 || contestacion.PreguntaId == 0 || contestacion.RespuestaId == 0)
            {
                errores.Append("Falta uno de los parametros para poder recibir una Respuesta a la pregunta");
                //if (contestacion.AspiranteId == 0) errores.AppendLine("Falta Aspirante");
                if (contestacion.CompetenciaId == 0) errores.AppendLine("Falta Competencia");
                if (contestacion.PreguntaId == 0) errores.AppendLine("Falta Pregunta");
                if (contestacion.RespuestaId == 0) errores.AppendLine("Falta Respuesta");

            }
            else
            {

                //var aspirante = await _context.Users
                //                    .FirstOrDefaultAsync(u =>
                //                        u.Id == contestacion.AspiranteId
                //                    );
                var aspirante = await AspiranteActual();

                var pregunta = await _context.Preguntas
                                    .FirstOrDefaultAsync(p =>
                                        p.CompetenciaId == contestacion.CompetenciaId
                                        &&
                                        p.PreguntaId == contestacion.PreguntaId
                                    );

                var respuesta = await _context.Respuestas
                                    .FirstOrDefaultAsync(r =>
                                        r.CompetenciaId == contestacion.CompetenciaId
                                        &&
                                        r.PreguntaId == contestacion.PreguntaId
                                        &&
                                        r.RespuestaPreguntaId == contestacion.RespuestaId
                                    );

                if (aspirante == null) { errores.AppendLine("El aspirante no existe"); }
                if (pregunta == null) { errores.AppendLine("La pregunta no existe"); }
                if (respuesta == null) { errores.AppendLine("La respuesta no existe"); }

                var respuestaEvaluacion = await _context.RespuestasEvaluaciones.FirstOrDefaultAsync(re => re.Aspirante == aspirante && re.Pregunta == pregunta);
                if (respuestaEvaluacion == null)
                {
                    var newRespuesta = new RespuestaEvaluacion
                    {
                        Aspirante = aspirante,
                        Pregunta = pregunta,
                        RespuestaPregunta = respuesta,
                        FechaCreacion = DateTime.Now,
                        FechaModificacion = DateTime.Now,
                        UsuarioCreacion = aspirante.NormalizedUserName,
                        UsuarioModificacion = aspirante.NormalizedUserName
                    };
                    await _context.RespuestasEvaluaciones.AddAsync(newRespuesta);
                }
                else
                {
                    respuestaEvaluacion.FechaModificacion = DateTime.Now;
                    respuestaEvaluacion.UsuarioModificacion = aspirante.NormalizedUserName;
                    respuestaEvaluacion.RespuestaPregunta = respuesta;
                }
                try
                {

                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    errores.AppendLine(e.Message);
                    if (!string.IsNullOrEmpty(errores.ToString()))
                    {
                        TempData["MensajeErrorPregunta" + contestacion.PreguntaId.ToString().PadLeft(3, '0')] = errores.ToString();
                    }
                    return Redirect(Url.RouteUrl(new { controller = "Evaluacion", Action = "Competencia", id = contestacion.NombreCompetencia }) + $"#pregunta{contestacion.PreguntaId.ToString().PadLeft(3, '0')}");
                    //return RedirectToLocal(BackUrl);
                }
            }

            if (!string.IsNullOrEmpty(errores.ToString()))
            {
                TempData["MensajeErrorPregunta" + contestacion.PreguntaId.ToString().PadLeft(3, '0')] = errores.ToString();
            }
            return Redirect(Url.RouteUrl(new { controller = "Evaluacion", Action = "Competencia", id = contestacion.NombreCompetencia }) + $"#pregunta{contestacion.PreguntaId.ToString().PadLeft(3, '0')}");
            //return RedirectToLocal(BackUrl);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarRespuesta([Bind] int RespuestaId, string backUrl, int PreguntaId, string NombreCompetencia)
        {
            if (!await MiGrupoEstaHabilitado())
            {
                return View("GrupoInhabilitado");
            }

            if (RespuestaId == 0)
            {
                TempData["MensajeErrorPregunta" + PreguntaId.ToString().PadLeft(3, '0')] = "No se especifico una respuesta a borrar";
                return Redirect(Url.RouteUrl(new { controller = "Evaluacion", Action = "Competencia", id = NombreCompetencia }) + $"#pregunta{PreguntaId.ToString().PadLeft(3, '0')}");
                //return RedirectToLocal(backUrl);
            }

            var aspiranteActual = await AspiranteActual();

            var respuestaBorrar = await _context.RespuestasEvaluaciones.FirstOrDefaultAsync(re => re.Aspirante == aspiranteActual && re.RespuestaPreguntaId == RespuestaId);
            if (respuestaBorrar != null)
            {
                _context.RespuestasEvaluaciones.Remove(respuestaBorrar);
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(Url.RouteUrl(new { controller = "Evaluacion", Action = "Competencia", id = NombreCompetencia }) + $"#pregunta{PreguntaId.ToString().PadLeft(3, '0')}");
                    //return RedirectToLocal(backUrl);
                }
                catch (Exception e)
                {

                    TempData["MensajeErrorPregunta" + PreguntaId.ToString().PadLeft(3, '0')] = $"No se pudo borrar: {e.Message}";
                    return Redirect(Url.RouteUrl(new { controller = "Evaluacion", Action = "Competencia", id = NombreCompetencia }) + $"#pregunta{PreguntaId.ToString().PadLeft(3, '0')}");
                    //return RedirectToLocal(backUrl);
                }
            }
            TempData["MensajeErrorPregunta" + PreguntaId.ToString().PadLeft(3, '0')] = "No se pudo borrar esta pregunta, ya que no esta registrada";
            return Redirect(Url.RouteUrl(new { controller = "Evaluacion", Action = "Competencia", id = NombreCompetencia }) + $"#pregunta{PreguntaId.ToString().PadLeft(3, '0')}");
            //return RedirectToLocal(backUrl);
        }

        public async Task<IActionResult> Resumen()
        {
            if (!await MiGrupoEstaHabilitado())
            {
                return View("GrupoInhabilitado");
            }
            ViewBag.Accion = nameof(Resumen);

            var aspirante = await AspiranteActual();
            var resumen = new ResumenViewModel()
            {
                Aspirante = await _context.Aspirante.FirstAsync(a => a.Id == aspirante.Id),
                RespuestasEvaluacion = await _context.RespuestasEvaluaciones
                            .AsNoTracking()
                            .Where(re => re.AspiranteId == aspirante.Id)
                            .Include(re => re.RespuestaPregunta)
                            .Include(re => re.Pregunta)
                            .OrderBy(re => re.Pregunta.NumeroPregunta)
                            .ToListAsync()
            };



            return View(resumen);
        }
    }
}
