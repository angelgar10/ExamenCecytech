using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ExamenCecytech.Data;
using ExamenCecytech.Models.EditarExamenViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExamenCecytech.Controllers
{
    [Authorize(Roles = "SysAdmin, Administrativo")]
    public class EditarExamenController : Controller
    {
        private readonly UserManager<Aspirante> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly SeedDbAdmin _seedDbAdmin;
        private readonly string NombreExamen;
        private readonly string TipoPlantel;

        public EditarExamenController(ApplicationDbContext context,
             SeedDbAdmin seedDbAdmin,
            IConfiguration configuration,
            UserManager<Aspirante> userManager)
        {
            _userManager = userManager;
            _context = context;
            _seedDbAdmin = seedDbAdmin;
            NombreExamen = configuration["DatosExamen:Nombre"];
            TipoPlantel = configuration["DatosExamen:TipoPlantel"];

        }

        public async Task<IActionResult> Index()
        {
            ViewData["NombreExamen"] = NombreExamen;
            ViewData["TipoPlantel"] = TipoPlantel;

            var competencias = _context.Competencias.AsNoTracking().OrderBy(c => c.Nombre);
            return View(await competencias.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> EditarCompetencia(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var competencia = await _context.Competencias
                .AsNoTracking()
                .Include(c => c.Preguntas)
                .ThenInclude(p => p.RespuestasPregunta)
                .FirstOrDefaultAsync(c => c.CompetenciaId == id);
            if (competencia == null)
            {
                return NotFound();
            }
            return View(competencia);
        }
        [HttpPost]
        public async Task<IActionResult> EditarCompetencia(int id, [Bind] Competencia competencia)
        {
            if (id != competencia.CompetenciaId)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(competencia.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre de la competencia es requerido");
            }
            else
            {
                if (competencia.Nombre?.Length > 50)
                {
                    ModelState.AddModelError("Nombre", "La longitud maxima del nombre es 50");
                }
                if (_context.Competencias.Any(c => c.Nombre == competencia.Nombre && competencia.CompetenciaId != c.CompetenciaId))
                {
                    ModelState.AddModelError("Nombre", "Esta competencia ya existe, no se pueden repetir nombres");
                }
            }
            competencia.LecturaPrevia = HttpUtility.HtmlDecode(competencia.LecturaPrevia);
            if (competencia.LecturaPrevia?.Length == 0)
            {
                competencia.LecturaPrevia = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Competencias.Update(competencia);
                    int res = await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(EditarCompetencia), new { id = competencia.CompetenciaId });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                    if (e.InnerException != null)
                    {
                        ModelState.AddModelError("", e.InnerException.Message);
                    }
                    return View(competencia);
                }
            }
            return View(competencia);
        }
        [HttpGet]
        public async Task<IActionResult> EditarPregunta(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var pregunta = await _context.Preguntas.AsNoTracking().Include(p => p.RespuestasPregunta).FirstOrDefaultAsync(p => p.PreguntaId == id);
            if (pregunta is null)
            {
                return NotFound();
            }
            ViewBag.CompetenciaId = await ListaCompetencias(id);
            return View(pregunta);
        }

        [HttpPost]
        public async Task<IActionResult> EditarPregunta(int id, [Bind] Pregunta pregunta)
        {
            if (id != pregunta.PreguntaId)
            {
                return NotFound();
            }
            if (pregunta.NumeroPregunta == 0)
            {
                ModelState.AddModelError("NumeroPregunta", "Debes especificar el numero de pregunta, no cero");
            }
            if (await _context.Preguntas.AnyAsync(p => p.NumeroPregunta == pregunta.NumeroPregunta && p.PreguntaId != pregunta.PreguntaId))
            {
                ModelState.AddModelError("NumeroPregunta", $"El numero de pregunta {pregunta.NumeroPregunta} ya existe en esta competencia");
            }
            if (string.IsNullOrEmpty(pregunta.Texto))
            {
                ModelState.AddModelError("Texto", $"El texto de la pregunta es requerido");
            }
            pregunta.LecturaPrevia = HttpUtility.HtmlDecode(pregunta.LecturaPrevia);
            if (string.IsNullOrEmpty(pregunta.LecturaPrevia))
            {
                pregunta.LecturaPrevia = null;
            }

            ViewBag.CompetenciaId = await ListaCompetencias(pregunta.CompetenciaId);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Preguntas.Update(pregunta);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(EditarPregunta), new { id = pregunta.PreguntaId });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                    if (e.InnerException != null)
                    {
                        ModelState.AddModelError("", e.InnerException.Message);
                    }
                    return View(pregunta);
                }
            }
            return View(pregunta);
        }
        private async Task<SelectList> ListaCompetencias(int? competenciaId = null)
        {
            return new SelectList(
                await _context.Competencias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(),
                "CompetenciaId",
                "Nombre",
                competenciaId
                );
        }
        [HttpGet]
        public async Task<IActionResult> CrearPregunta(int? id = null)
        {
            ViewBag.CompetenciaId = await ListaCompetencias(id);
            return View(new Pregunta { CompetenciaId = id.Value });
        }

        [HttpPost]
        public async Task<IActionResult> CrearPregunta(int id, [Bind] Pregunta pregunta)
        {
            if (id != pregunta.CompetenciaId)
            {
                return NotFound();
            }
            if (pregunta.NumeroPregunta == 0)
            {
                ModelState.AddModelError("NumeroPregunta", "Debes especificar el numero de pregunta, no cero");
            }
            if (await _context.Preguntas.AnyAsync(p => p.CompetenciaId == id && p.NumeroPregunta == pregunta.NumeroPregunta))
            {
                ModelState.AddModelError("NumeroPregunta", $"El numero de pregunta {pregunta.NumeroPregunta} ya existe en esta competencia");
            }
            if (string.IsNullOrEmpty(pregunta.Texto))
            {
                ModelState.AddModelError("Texto", $"El texto de la pregunta es requerido");
            }
            pregunta.LecturaPrevia = HttpUtility.HtmlDecode(pregunta.LecturaPrevia);
            if (string.IsNullOrEmpty(pregunta.LecturaPrevia))
            {
                pregunta.LecturaPrevia = null;
            }

            ViewBag.CompetenciaId = await ListaCompetencias(pregunta.CompetenciaId);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Preguntas.Add(pregunta);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(EditarCompetencia), new { id = pregunta.CompetenciaId });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                    if (e.InnerException != null)
                    {
                        ModelState.AddModelError("", e.InnerException.Message);
                    }
                    return View(pregunta);
                }
            }
            return View(pregunta);
        }
        [HttpGet]
        public async Task<IActionResult> CrearRespuesta(int preguntaId)
        {
            if (preguntaId == 0)
            {
                return NotFound();
            }
            var pregunta = await _context.Preguntas.AsNoTracking().FirstOrDefaultAsync(p => p.PreguntaId == preguntaId);
            if (pregunta == null)
            {
                return NotFound();
            }

            return View(new RespuestaPregunta { PreguntaId = preguntaId });
        }

        [HttpPost]
        public async Task<IActionResult> CrearRespuesta(int preguntaId, [Bind] RespuestaPregunta rp)
        {
            if (preguntaId != rp.PreguntaId)
            {
                return NotFound();
            }

            rp.Texto = HttpUtility.HtmlDecode(rp.Texto);
            if (string.IsNullOrEmpty(rp.Texto))
            {
                rp.Texto = null;
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Respuestas.AddAsync(rp);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(EditarPregunta), new { id = rp.PreguntaId });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                    if (e.InnerException != null)
                    {
                        ModelState.AddModelError("", e.InnerException.Message);
                    }
                    return View(rp);
                }
            }
            return View(rp);
        }
        [HttpGet]
        public async Task<IActionResult> EditarRespuesta(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var respuesta = await _context.Respuestas.AsNoTracking().FirstOrDefaultAsync(rp => rp.RespuestaPreguntaId == id);
            if (respuesta == null)
            {
                return NotFound();
            }
            return View(respuesta);
        }

        [HttpPost]
        public async Task<IActionResult> EditarRespuesta(int id, [Bind] RespuestaPregunta rp)
        {
            if (id != rp.RespuestaPreguntaId)
            {
                return NotFound();
            }
            var respuesta = await _context.Respuestas.AsNoTracking().FirstOrDefaultAsync(r => r.RespuestaPreguntaId == id);
            if (respuesta == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Respuestas.Update(rp);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(EditarPregunta), new { id = rp.PreguntaId });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                    if (e.InnerException != null)
                    {
                        ModelState.AddModelError("", e.InnerException.Message);
                    }
                    return View(rp);
                }
            }

            return View(rp);
        }
        [HttpGet]
        public async Task<IActionResult> BorrarCompetencia(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var competencia = await _context.Competencias.AsNoTracking().FirstOrDefaultAsync(c => c.CompetenciaId == id);
            if (competencia == null)
            {
                return NotFound();
            }
            return View(competencia);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCompetenciaConfirmado(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var competencia = await _context.Competencias.Include(c => c.Preguntas).FirstOrDefaultAsync(c => c.CompetenciaId == id);
            if (competencia == null)
            {
                return NotFound();
            }
            _context.Remove(competencia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> AgregarAlumno()
        {
            var userActive = await _userManager.GetUserAsync(User);
            var plantelesAsignados = await _context.UsuariosPlantel
                .Where(p => p.Id == userActive.Id)
                .ToListAsync();

            ViewBag.GrupoId = await Grupos(null);

            ViewBag.ListaAlumnos = await _context.Aspirante
                                            .AsNoTracking()
                                            .Where(a => plantelesAsignados.Select(p => p.ClavePlantel).Contains(a.Grupo.ClavePlantel))
                                            .Include(g => g.Grupo).ThenInclude(g => g.Plantel)
                                            .Where(g => g.GrupoId.HasValue)
                                            .OrderBy(g => g.Paterno)
                                            .ThenBy(g => g.Materno)
                                            .ThenBy(g => g.Nombre)
                                            .ToListAsync();

            return View(new AspiranteViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AgregarAlumno(AspiranteViewModel nvoAlumno)
        {

            var usuario = await _userManager.FindByNameAsync(nvoAlumno.Matricula);
            if (usuario != null)
            {
                ModelState.AddModelError(string.Empty, $"El alumno con matricula {nvoAlumno.Matricula} ya esta capturado");
            }
            if (ModelState.IsValid)
            {
                string pass = _seedDbAdmin.PasswordAleatorio();
                Plantel plantel = _context.Grupos.AsNoTracking().Include(g => g.Plantel).First(g => g.GrupoId == nvoAlumno.GrupoId).Plantel;
                var resUser = await _userManager.CreateAsync(new Aspirante()
                {
                    Ficha = nvoAlumno.Matricula,
                    UserName = nvoAlumno.Matricula,
                    Edad = nvoAlumno.Semestre,
                    Email = nvoAlumno.Matricula,
                    PlainPass = pass,
                    Genero = nvoAlumno.Genero,
                    GrupoId = nvoAlumno.GrupoId,
                    EspecialidadId = null,
                    Estatus = "A",
                    Paterno = nvoAlumno.Paterno,
                    Materno = nvoAlumno.Materno,
                    Nombre = nvoAlumno.Nombre,
                    NombreSecundaria = plantel.Nombre,
                    TipoSecundaria = plantel.ClavePlantel[0] == 'C' ? "CECyT" : "EMSAD",
                    TipoSostenimientoSecundaria = "MIXTO"
                }, pass);
                if (!resUser.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Ocurrio un error, no se pudo dar de alta al usuario");
                }
                else
                {
                    var usuarioCreado = await _userManager.FindByNameAsync(nvoAlumno.Matricula);
                    var resRol = await _userManager.AddToRolesAsync(usuarioCreado, new List<string>() { "Aspirante" });
                    return RedirectToAction(nameof(AgregarAlumno));
                }
            }

            ViewBag.GrupoId = await Grupos(nvoAlumno.GrupoId);
            return View(nvoAlumno);
        }
        private async Task<SelectList> Grupos(int? grupoId = null)
        {
            var userActive = await _userManager.GetUserAsync(User);
            var plantelesAsignados = await _context.UsuariosPlantel
                .Where(p => p.Id == userActive.Id)
                .ToListAsync();

            var lista = await _context.Grupos
                                .AsNoTracking()
                                .Where(g => plantelesAsignados.Select(p => p.ClavePlantel).Contains(g.Plantel.ClavePlantel))
                                .Include(g => g.Plantel)
                                .OrderBy(g => g.Plantel.ClavePlantel)
                                .Select(g => new { GrupoId = g.GrupoId, Grupo = $"{g.Plantel.ClavePlantel}-{g.Nombre}" })
                                .ToListAsync();
            return new SelectList(
                    lista,
                    "GrupoId",
                    "Grupo",
                    grupoId
                    );
        }
    }
}
