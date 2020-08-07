using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamenCecytech.Data;
using ExamenCecytech.Extensions;
using ExamenCecytech.Models.PlantelesViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace ExamenCecytech.Controllers
{
    [Authorize(Roles = "Administrativo")]
    public class PlantelesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Aspirante> _userManager;

        public List<int> LcTextoExpositivo = new List<int> { 1, 2, 3, 4, 9, 28, 29, 30, 31, 32, 34 };
        public List<int> LcTextoArgumentativo = new List<int> { 5, 10, 19, 21, 25, 26, 27, 35, 36, 47, 49 };
        public List<int> LcTextoLiterario = new List<int> { 7, 11, 16, 17, 20, 22, 40, 42, 43, 50 };
        public List<int> LcManejoInformacion = new List<int> { 6, 8, 12, 13, 14, 15, 18, 23, 24, 33, 37, 38, 39, 41, 44, 45, 46, 48 };

        public List<int> MatSentidoPensamiento = new List<int> { 51, 52, 53, 54, 55, 56, 57, 58, 63, 76, 77, 78, 79, 80, 81, 82, 83, 84 };
        public List<int> MatCambioRelaciones = new List<int> { 59, 60, 64, 65, 66, 67, 68, 69, 70, 85, 86, 87, 88, 89, 93, 94, 96 };
        public List<int> MatFormaEspacioMedida = new List<int> { 71, 72, 95, 97, 98 };
        public List<int> MatManejoInfo = new List<int> { 61, 62, 73, 74, 75, 90, 91, 92, 99, 100 };


        [TempData]
        public string ErrorMsg { get; set; }
        [TempData]
        public string ExitoMsg { get; set; }
        [TempData]
        public string TempAspirante { get; set; }

        public PlantelesController(ApplicationDbContext context,
            UserManager<Aspirante> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userActive = await _userManager.GetUserAsync(User);
            List<string> adminMaestros = new List<string> {
                "a.garnica@cecytechihuahua.edu.mx"   ,
                "p.baeza@cecytechihuahua.edu.mx"     ,
                "f.hernandezc@cecytechihuahua.edu.mx",
                "k.franco@cecytechihuahua.edu.mx"    ,
                "e.hernandez@cecytechihuahua.edu.mx" ,
                "i.gonzalezp@cecytechihuahua.edu.mx" ,
                "b.caballero@cecytechihuahua.edu.mx" ,
                "a.vazquez@cecytechihuahua.edu.mx",
                "h.lujan@cecytechihuahua.edu.mx"
            };

            var plantelesAsignados = await _context.UsuariosPlantel
                .Where(p => p.Id == userActive.Id)
                .Select(p => p.ClavePlantel)
                .ToListAsync();

            var planteles = await _context.Planteles
                .AsNoTracking()
                .Where(p => plantelesAsignados.Contains(p.ClavePlantel))
                .Include(p => p.GruposPlantel)
                .Include(p => p.UsuariosPlantel)
                .ThenInclude(up => up.Aspirante)
                .Select(p => new PlantelViewModel
                {
                    ClavePlantel = p.ClavePlantel,
                    Plantel = p.Nombre,
                    Grupos = p.GruposPlantel.Count(),
                    AdministradoresPlantel = p.UsuariosPlantel
                            .Where(up => !adminMaestros.Contains(up.Aspirante.Email.ToLower()))
                            .Select(up => up.Aspirante.Email.ToLower())
                            .OrderBy(o => o)
                            .ToList()
                })
                .ToListAsync();

            foreach (var plantel in planteles)
            {
                plantel.EvaluacionesExpedidas = _context.Aspirante.AsNoTracking().Include(a => a.Grupo).Where(a => a.Grupo.ClavePlantel == plantel.ClavePlantel).Count();
                plantel.EvaluacionesAplicadas = _context.Aspirante.AsNoTracking().Include(a => a.Grupo).Include(a => a.RespuestasEvaluacion).Where(a => a.Grupo.ClavePlantel == plantel.ClavePlantel && a.RespuestasEvaluacion.Any()).Count();
            }

            return View(planteles);
        }

        [Route("Planteles/{clavePlantel}")]
        public async Task<IActionResult> DetallesPlantel(string clavePlantel)
        {
            if (await ExistePlantel(clavePlantel))
            {
                return View("NoExistePlantel", clavePlantel);
            }

            if (await UsuarioAutorizadoEnPlantel(clavePlantel))
            {
                return View("UsuarioNoAutorizadoEnPlantel", $"El usuario no esta autorizado en el Plantel {clavePlantel}");
            }

            var plantel = _context.Planteles
                .AsNoTracking()
                .Include(p => p.GruposPlantel)
                .ThenInclude(g => g.Aspirantes)
                .ThenInclude(a => a.RespuestasEvaluacion)
                .ThenInclude(r => r.RespuestaPregunta)
                .Include(p => p.UsuariosPlantel)
                .FirstAsync(p => p.ClavePlantel == clavePlantel);

            return View(await plantel);
        }
        private async Task<bool> ExistePlantel(string clavePlantel)
        {
            return !await _context.Planteles.AnyAsync(p => p.ClavePlantel == clavePlantel);
        }

        private async Task<bool> UsuarioAutorizadoEnPlantel(string clavePlantel)
        {
            var usuarioActual = await _userManager.GetUserAsync(User);

            return !_context.Planteles
                    .AsNoTracking()
                    .Include(p => p.UsuariosPlantel)
                    .Any(p => p.ClavePlantel == clavePlantel && p.UsuariosPlantel.Select(up => up.Id).Contains(usuarioActual.Id));
        }

        [Route("Planteles/{clavePlantel}/{grupo}")]
        public async Task<IActionResult> DetalleGrupo(string clavePlantel, string grupo)
        {
            if (string.IsNullOrEmpty(TempAspirante))
            {
                ViewData["nvoAspirante"] = new Aspirante();
            }
            else
            {
                ViewData["nvoAspirante"] = JsonConvert.DeserializeObject<Aspirante>(TempAspirante);
            }
            if (await ExistePlantel(clavePlantel))
            {
                return View("NoExistePlantel", clavePlantel);
            }


            if (await UsuarioAutorizadoEnPlantel(clavePlantel))
            {
                return View("UsuarioNoAutorizadoEnPlantel", $"El usuario no esta autorizado en el Plantel {clavePlantel}");
            }

            var aspirantes = _context.Aspirante
                            .AsNoTracking()
                            .Include(g => g.Grupo).ThenInclude(g => g.Plantel)
                            .Include(a => a.RespuestasEvaluacion)
                            .ThenInclude(r => r.RespuestaPregunta)
                            .Where(g => g.Grupo.ClavePlantel == clavePlantel && g.Grupo.Nombre == grupo)
                            .OrderBy(a => a.Paterno)
                            .ThenBy(a => a.Materno)
                            .ThenBy(a => a.Nombre);
            ViewBag.Grupos = new SelectList(
                await _context.Grupos
                                    .AsNoTracking()
                                    .Where(g => g.ClavePlantel == clavePlantel && g.Nombre != grupo)
                                    .Select(g => new { g.GrupoId, g.Nombre })
                                    .ToListAsync()
                , "GrupoId", "Nombre", null);
            return View(await aspirantes.ToListAsync());
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Planteles/{clavePlantel}/{grupo}/CrearAspirante")]
        public async Task<IActionResult> CrearAspirante(string clavePlantel, string grupo, Aspirante aspirante)
        {
            aspirante.Email = aspirante.Email?.ToLower().Trim();
            if (await ExistePlantel(clavePlantel))
            {
                return View("NoExistePlantel", clavePlantel);
            }


            if (await UsuarioAutorizadoEnPlantel(clavePlantel))
            {
                return View("UsuarioNoAutorizadoEnPlantel", $"El usuario no esta autorizado en el Plantel {clavePlantel}");
            }
            var gpo = await _context.Grupos.AsNoTracking().FirstOrDefaultAsync(g => g.ClavePlantel == clavePlantel && g.Nombre == grupo);
            if (gpo == null)
            {
                ErrorMsg = $"El grupo {grupo}, no existe en el plantel{clavePlantel}";
                return RedirectToAction(nameof(DetallesPlantel), new { ClavePlantel = clavePlantel });
            }

            try
            {
                int sigFolio = 0;
                var alumnosPortabilidad = await _context.Aspirante.Where(a => a.Ficha.Contains("PORTAB-")).Select(a => a.Ficha).ToListAsync();
                if (alumnosPortabilidad.Count() > 0)
                {
                    var ultimoFolio = alumnosPortabilidad.Select(x => x.Substring(11)).Max();
                    int.TryParse(ultimoFolio.Trim(), out sigFolio);
                }
                sigFolio++;
                string nvoFolio = $"PORTAB-{clavePlantel}-{sigFolio.ToString().PadLeft(3, '0') }";
                aspirante.Ficha = nvoFolio;
                aspirante.UserName = nvoFolio;
                aspirante.Email = $"{nvoFolio}@{clavePlantel}.cecytechihuahua.edu.mx";
                if (gpo.Nombre.ToUpper() == "DOCENTES")
                {
                    aspirante.UserName = aspirante.Email;
                    aspirante.Ficha = aspirante.Email.Split('@')[0];
                }
                aspirante.GrupoId = gpo.GrupoId;
                aspirante.Edad = 0;
                aspirante.DescripcionOtraSecundaria = "";
                aspirante.NombreSecundaria = "";
                aspirante.PromedioSecundaria = 0;
                aspirante.TipoSecundaria = "";
                aspirante.TipoSostenimientoSecundaria = "";
                aspirante.PlainPass = PasswordAleatorio();

                if (!aspirante.Email.EndsWith("cecytechihuahua.edu.mx"))
                {
                    ModelState.AddModelError("", "el correo no esta en el dominio cecytechihuahua.edu.mx");
                    ErrorMsg = "el correo no esta en el dominio cecytechihuahua.edu.mx";
                }
                if (ModelState.IsValid)
                {
                    var usr = await _userManager.FindByNameAsync(aspirante.UserName);
                    if (usr == null)
                    {
                        var resInsertaUsuario = await _userManager.CreateAsync(aspirante, aspirante.PlainPass);
                        if (resInsertaUsuario.Succeeded)
                        {
                            ExitoMsg = $"Se creo con exito el aspirante {aspirante.Paterno} {aspirante.Materno} {aspirante.Nombre} en el grupo {grupo}";
                        }
                        else
                        {
                            ErrorMsg = string.Join(';', resInsertaUsuario.Errors.Select(e => e.Description).ToArray());
                        }
                    }
                    else
                    {
                        if (usr.GrupoId == null)
                        {
                            usr.GrupoId = gpo.GrupoId;
                            await _userManager.UpdateAsync(usr);
                        }
                        ErrorMsg = $"El aspirante {aspirante.UserName} ya existe";
                    }
                    usr = await _userManager.FindByNameAsync(aspirante.UserName);
                    if (usr != null && !await _userManager.IsInRoleAsync(usr, "Aspirante"))
                    {
                        await _userManager.AddToRoleAsync(usr, "Aspirante");
                    }
                }
                else
                {
                    ErrorMsg = "No se cumplen las condiciones del modelo";
                }

                return RedirectToAction(nameof(DetalleGrupo), new { clavePlantel = clavePlantel, grupo = grupo });
            }
            catch (Exception e)
            {
                TempAspirante = JsonConvert.SerializeObject(aspirante);
                ErrorMsg = $"Error: {e.Message} ";
                if (e.InnerException != null)
                {
                    ErrorMsg += " => " + e.InnerException.Message;
                }
                return RedirectToAction(nameof(DetalleGrupo), new { clavePlantel = clavePlantel, grupo = grupo });
            }
        }

        private string PasswordAleatorio(int longitud = 8, string caracteres = "ABCDEFGHKMNPRSTUVWXYZ0123456789abcdefghkmnprstuvwxyz")
        {
            Random aleatorio = new Random((int)DateTime.Now.Ticks);
            string pass = "";
            for (int i = 0; i < longitud; i++)
            {
                pass += caracteres[aleatorio.Next(caracteres.Length - 1)];
            }
            return pass;
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Route("Planteles/CambiarEstatusUsuario/{clavePlantel}/{aspiranteId}")]
        public async Task<IActionResult> CambiarEstatusUsuario(string clavePlantel, int aspiranteId)
        {
            if (await ExistePlantel(clavePlantel))
            {
                return View("NoExistePlantel", clavePlantel);
            }

            if (await UsuarioAutorizadoEnPlantel(clavePlantel))
            {
                return View("UsuarioNoAutorizadoEnPlantel", $"El usuario no esta autorizado en el Plantel {clavePlantel}");
            }

            var aspirante = await _context.Aspirante.Include(a => a.Grupo).FirstOrDefaultAsync(a => a.Id == aspiranteId);
            string gpoRegreso = aspirante.Grupo.Nombre;
            if (aspirante != null)
            {
                try
                {
                    aspirante.LockoutEnabled = true;

                    if (aspirante.LockoutEnd == null)
                    {
                        aspirante.LockoutEnd = new DateTime(2020, 12, 31);
                    }
                    else
                    {
                        aspirante.LockoutEnd = null;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return RedirectToAction(nameof(DetalleGrupo), new { clavePlantel = clavePlantel, grupo = gpoRegreso });
                }
            }

            return RedirectToAction(nameof(DetalleGrupo), new { clavePlantel = clavePlantel, grupo = gpoRegreso });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Route("Planteles/CambiarAspiranteDeGrupo/{clavePlantel}/{aspiranteId}")]
        public async Task<IActionResult> CambiarAspiranteDeGrupo(string clavePlantel, int aspiranteId, [Bind] int grupoId)
        {
            if (await ExistePlantel(clavePlantel))
            {
                return View("NoExistePlantel", clavePlantel);
            }

            if (await UsuarioAutorizadoEnPlantel(clavePlantel))
            {
                return View("UsuarioNoAutorizadoEnPlantel", $"El usuario no esta autorizado en el Plantel {clavePlantel}");
            }

            var aspirante = await _context.Aspirante.Include(a => a.Grupo).FirstOrDefaultAsync(a => a.Id == aspiranteId);
            string grupoAnterior = aspirante.Grupo.Nombre;
            var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.ClavePlantel == clavePlantel && g.GrupoId == grupoId);
            if (aspirante != null && grupo != null)
            {
                try
                {
                    aspirante.Grupo = grupo;
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return RedirectToAction(nameof(DetalleGrupo), new { clavePlantel = clavePlantel, grupo = grupoAnterior });
                }
            }

            return RedirectToAction(nameof(DetalleGrupo), new { clavePlantel = clavePlantel, grupo = grupoAnterior });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Route("Planteles/CambiaGrupoHabilitado/{clavePlantel}/{grupoId}")]
        public async Task<IActionResult> CambiaGrupoHabilitado(string clavePlantel, int grupoId)
        {
            if (await ExistePlantel(clavePlantel))
            {
                return View("NoExistePlantel", clavePlantel);
            }

            if (await UsuarioAutorizadoEnPlantel(clavePlantel))
            {
                return View("UsuarioNoAutorizadoEnPlantel", $"El usuario no esta autorizado en el Plantel {clavePlantel}");
            }

            var grupo = await _context.Grupos.FirstAsync(g => g.GrupoId == grupoId);
            if (grupo == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    grupo.EvaluacionHabilitada = !grupo.EvaluacionHabilitada;
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return RedirectToAction(nameof(DetallesPlantel), "Planteles");
                }
            }

            return RedirectToAction(nameof(DetallesPlantel), "Planteles");
        }

        [Route("Planteles/{clavePlantel}/Analisis/{grupo}")]
        public async Task<IActionResult> AnalisisGrupo(string clavePlantel, string grupo)
        {

            if (string.IsNullOrEmpty(clavePlantel))
            {
                return NotFound();
            }
            clavePlantel = clavePlantel.ToUpper().Trim();

            var evaluacion = _context.Competencias
                                .AsNoTracking()
                                .Include(c => c.Preguntas)
                                .ThenInclude(a => a.RespuestasPregunta)
                                //.AsQueryable();
                                ;

            var y = _context.Grupos
                            .AsNoTracking()
                            .Where(g => (clavePlantel.ToUpper() == "TODOS" || g.ClavePlantel.ToUpper() == clavePlantel) & (grupo.ToUpper() == "TODOS" || g.Nombre == grupo) & (grupo.ToUpper() != "TODOS" || g.Nombre.ToUpper() != "DOCENTES"))
                                .Include(g => g.Aspirantes)
                                .ThenInclude(a => a.RespuestasEvaluacion)
                            .SelectMany(g => g.Aspirantes);

            int NumEvaluados = await y.CountAsync();

            var p = y.SelectMany(b => b.RespuestasEvaluacion);


            var res = evaluacion.Select(a => new EstadisticaExamen
            {
                Titulo = $"Analisis Examen Plantel {clavePlantel} - Grupo {grupo}",
                PrefijoGraficas = "pregAl",
                NombreCompetencia = a.Nombre,
                NumeroPreguntas = a.Preguntas.Count(),
                Preguntas = a.Preguntas.Select(b => new PreguntaVM
                {
                    NumeroPregunta = b.NumeroPregunta,
                    Texto = b.Texto,
                    SinContestar = NumEvaluados - p.Count(x => x.PreguntaId == b.PreguntaId),
                    Respuestas = b.RespuestasPregunta.Select(c => new RespuestaVM

                    {
                        Letra = c.ClaveCOSDAC,
                        Texto = c.Texto,
                        Correcta = c.Valor == 1,
                        Frecuencia = p.Count(x => x.RespuestaPreguntaId == c.RespuestaPreguntaId)
                    }).OrderBy(o => o.Letra).ToArray()
                }).OrderBy(o => o.NumeroPregunta).ToArray(),
            });


            return View(await res.ToArrayAsync());
        }

        [Route("Planteles/{clavePlantel}/DescargarExcel/{grupo}")]
        public async Task<IActionResult> DescargarExcel(string clavePlantel = "", string grupo = "")
        {
            clavePlantel = clavePlantel.ToUpper().Trim();

            if (clavePlantel != "TODOS" && await ExistePlantel(clavePlantel))
            {
                return View("NoExistePlantel", clavePlantel);
            }


            if (clavePlantel != "TODOS" && await UsuarioAutorizadoEnPlantel(clavePlantel))
            {
                return View("UsuarioNoAutorizadoEnPlantel", $"El usuario no esta autorizado en el Plantel {clavePlantel}");
            }

            var aspiranteEvaluados = _context.Users
                                        .AsNoTracking()
                                        .Include(u => u.Grupo)
                                        .ThenInclude(g => g.Plantel)
                                        .Include(u => u.RespuestasEvaluacion)
                                        .ThenInclude(re => re.RespuestaPregunta)
                                        .ThenInclude(rp => rp.Pregunta)
                                        .Where(u => ((clavePlantel.ToUpper() == "TODOS" || u.Grupo.ClavePlantel == clavePlantel)) & (grupo.ToUpper() == "TODOS" || u.Grupo.Nombre == grupo) & (grupo.ToUpper() != "TODOS" || u.Grupo.Nombre.ToUpper() != "DOCENTES"))
                                        .OrderBy(u => u.Grupo.Nombre)
                                        .ThenBy(u => u.Ficha);

            var res = aspiranteEvaluados
                .AsNoTracking()
                .Select(a => new
                {
                    a.UserName,
                    a.Paterno,
                    a.Materno,
                    a.Nombre,
                    a.Genero,
                    a.Edad,
                    a.PromedioSecundaria,
                    a.TipoSecundaria,
                    a.TipoSostenimientoSecundaria,
                    a.Grupo.ClavePlantel,
                    Grupo = a.Grupo.Nombre,
                    Correctas = a.RespuestasEvaluacion.Sum(x => x.RespuestaPregunta.Valor),
                    Incorrectas = a.RespuestasEvaluacion.Count(x => x.RespuestaPregunta.Valor == 0),
                    SinContestar = 100 - a.RespuestasEvaluacion.Count(),
                    Respuestas = string.Join("",
                    _context.Preguntas.AsNoTracking()
                    .Select(w => new {
                        w.NumeroPregunta,
                        clave = a.RespuestasEvaluacion.Any(x => x.RespuestaPregunta.PreguntaId == w.PreguntaId)
                                ? a.RespuestasEvaluacion.First(x => x.RespuestaPregunta.PreguntaId == w.PreguntaId).RespuestaPregunta.ClaveCOSDAC
                                : "X"
                    })
                                .OrderBy(h => h.NumeroPregunta)
                                .Select(h => h.clave)


                    )
                }).ToArray();


            ExcelPackage package = new ExcelPackage();
            package.Workbook.Properties.Title = "CECYTECH";
            package.Workbook.Properties.Author = "I.S.C. Alejandro Torres";
            package.Workbook.Properties.Subject = "Exportacion de datos para simulador CECYTECH PLANEA 2018";
            package.Workbook.Properties.Keywords = "PLANEA";



            OfficeOpenXmlExtensions.CrearHojaConLista(ref package, res, "Empleados");


            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"archivoCOSDAC_{clavePlantel}_{grupo}_{DateTime.Now.ToString("yyyyMMddhhmm")}.xlsx");
        }

        [Route("Planteles/{clavePlantel}/ArchivoPlantel/{descargaArchivo}")]
        public async Task<IActionResult> ArchivoPlantel(string clavePlantel, bool descargaArchivo = false)
        {
            if (await ExistePlantel(clavePlantel))
            {
                return View("NoExistePlantel", clavePlantel);
            }


            if (await UsuarioAutorizadoEnPlantel(clavePlantel))
            {
                return View("UsuarioNoAutorizadoEnPlantel", $"El usuario no esta autorizado en el Plantel {clavePlantel}");
            }

            var aspiranteEvaluados = _context.Users
                                        .AsNoTracking()
                                        .Include(u => u.Grupo)
                                        .ThenInclude(g => g.Plantel)
                                        .Include(u => u.RespuestasEvaluacion)
                                        .ThenInclude(re => re.RespuestaPregunta)
                                        .ThenInclude(rp => rp.Pregunta)
                                        .Where(u => u.Grupo.ClavePlantel == clavePlantel && u.RespuestasEvaluacion.Count() > 0)
                                        .OrderBy(u => u.Grupo.Nombre)
                                        .ThenBy(u => u.Ficha);

            var resArchivo = new List<FormatoCargaSistemaCOSDAC>();
            foreach (var aspirante in aspiranteEvaluados)
            {
                int folio = 0;
                if (int.TryParse(aspirante.Ficha.Substring(4), out folio))
                {
                    var item = new FormatoCargaSistemaCOSDAC
                    {
                        ColumnaA = folio.ToString("000000"),
                        ColumnaE = aspirante.Edad.ToString("00"),
                    };

                    if (aspirante.Paterno.Length > 12)
                    {
                        item.ColumnaB = aspirante.Paterno.ToUpper().Trim().Substring(0, 12);
                    }
                    else
                    {
                        item.ColumnaB = aspirante.Paterno.ToUpper().Trim();
                    }

                    if (aspirante.Materno.Length > 12)
                    {
                        item.ColumnaC = aspirante.Materno.ToUpper().Trim().Substring(0, 12);
                    }
                    else
                    {
                        item.ColumnaC = aspirante.Materno.ToUpper().Trim();
                    }

                    if (aspirante.Nombre.Length > 12)
                    {
                        item.ColumnaD = aspirante.Nombre.ToUpper().Trim().Substring(0, 12);
                    }
                    else
                    {
                        item.ColumnaD = aspirante.Nombre.ToUpper().Trim();
                    }

                    if (aspirante.NombreSecundaria.Length > 40)
                    {
                        item.ColumnaG = aspirante.NombreSecundaria.ToUpper().Trim().Substring(0, 40);
                    }
                    else
                    {
                        item.ColumnaG = aspirante.NombreSecundaria.ToUpper().Trim();
                    }




                    if (aspirante.Paterno.Length > 12)
                    {
                        aspirante.Paterno = aspirante.Paterno.Substring(0, 12);
                    }
                    if (aspirante.Materno.Length > 12)
                    {
                        aspirante.Materno = aspirante.Materno.Substring(0, 12);
                    }
                    if (aspirante.Nombre.Length > 12)
                    {
                        aspirante.Nombre = aspirante.Nombre.Substring(0, 12);
                    }
                    if (aspirante.NombreSecundaria.Length > 40)
                    {
                        aspirante.NombreSecundaria = aspirante.NombreSecundaria.Substring(0, 40);
                    }


                    string respuestasString = "";
                    for (int i = 1; i < 101; i++)
                    {
                        var resp = aspirante.RespuestasEvaluacion.FirstOrDefault(re => re.AspiranteId == aspirante.Id && re.RespuestaPregunta.Pregunta.NumeroPregunta == i);
                        if (resp == null)
                        {
                            respuestasString += "X";
                        }
                        else
                        {
                            respuestasString += resp.RespuestaPregunta.ClaveCOSDAC;
                        }
                    }
                    item.ColumnaH = respuestasString;

                    switch (aspirante.Genero)
                    {
                        case "M": item.ColumnaF = "1"; break;
                        // case "H": item.ColumnaF = "1"; break;
                        case "F": item.ColumnaF = "2"; break;
                        //case "M": item.ColumnaF = "2"; break;
                        default:
                            item.ColumnaF = "";
                            break;
                    }

                    switch (aspirante.TipoSecundaria)
                    {
                        case "SECUNDARIA GENERAL": item.ColumnaI = "1"; break;
                        case "SECUNDARIA TECNICA": item.ColumnaI = "2"; break;
                        case "SECUNDARIA PARA TRABAJADORES": item.ColumnaI = "3"; break;
                        case "SECUNDARIA COMUNITARIA": item.ColumnaI = "4"; break;
                        case "TELESECUNDARIA": item.ColumnaI = "5"; break;
                        case "OTRA": item.ColumnaI = "6"; break;
                        default:
                            item.ColumnaI = "";
                            break;
                    }

                    switch (aspirante.TipoSostenimientoSecundaria)
                    {
                        case "FEDERAL": item.ColumnaJ = "1"; break;
                        case "ESTATAL": item.ColumnaJ = "2"; break;
                        case "PARTICULAR (RVOE)": item.ColumnaJ = "3"; break;
                        default: item.ColumnaJ = ""; break;
                    }

                    if (aspirante.PromedioSecundaria >= 6m && aspirante.PromedioSecundaria <= 6.5m)
                    {
                        item.ColumnaK = "1";
                    }
                    else if (aspirante.PromedioSecundaria >= 6.6m && aspirante.PromedioSecundaria <= 7m)
                    {
                        item.ColumnaK = "2";
                    }
                    else if (aspirante.PromedioSecundaria >= 7.1m && aspirante.PromedioSecundaria <= 7.5m)
                    {
                        item.ColumnaK = "3";
                    }
                    else if (aspirante.PromedioSecundaria >= 7.6m && aspirante.PromedioSecundaria <= 8m)
                    {
                        item.ColumnaK = "4";
                    }
                    else if (aspirante.PromedioSecundaria >= 8.1m && aspirante.PromedioSecundaria <= 8.5m)
                    {
                        item.ColumnaK = "5";
                    }
                    else if (aspirante.PromedioSecundaria >= 8.6m && aspirante.PromedioSecundaria <= 9m)
                    {
                        item.ColumnaK = "6";
                    }
                    else if (aspirante.PromedioSecundaria >= 9.1m && aspirante.PromedioSecundaria <= 9.5m)
                    {
                        item.ColumnaK = "7";
                    }
                    else if (aspirante.PromedioSecundaria >= 9.6m && aspirante.PromedioSecundaria <= 10m)
                    {
                        item.ColumnaK = "8";
                    }

                    //if (aspirante.PromedioSecundaria >= 6m)
                    //{
                    //    if (aspirante.PromedioSecundaria > 6.5m)
                    //    {
                    //        if (aspirante.PromedioSecundaria > 7m)
                    //        {
                    //            if (aspirante.PromedioSecundaria > 7.5m )
                    //            {
                    //                if (aspirante.PromedioSecundaria > 8m )
                    //                {

                    //                    if (aspirante.PromedioSecundaria > 8.5m )

                    //                        if (aspirante.PromedioSecundaria > 9m)
                    //                        {
                    //                            if (aspirante.PromedioSecundaria > 9.5m && aspirante.PromedioSecundaria <= 10m)
                    //                            {
                    //                                if (aspirante.PromedioSecundaria <= 10m)
                    //                                {
                    //                                    item.ColumnaK = "8";
                    //                                }
                    //                                else
                    //                                {
                    //                                    // Eso quiere decir que la calificacion es mayor que 10, lo cual es un error para la clasificacion
                    //                                    item.ColumnaK = "";
                    //                                }

                    //                            }
                    //                            else
                    //                            {
                    //                                item.ColumnaK = "8";
                    //                            }
                    //                        }
                    //                        else
                    //                        {
                    //                            item.ColumnaK = "7";
                    //                        }


                    //                    else
                    //                    {
                    //                        item.ColumnaK = "6";
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    item.ColumnaK = "5";
                    //                }
                    //            }
                    //            else
                    //            {
                    //                item.ColumnaK = "4";
                    //            }
                    //        }
                    //        else
                    //        {
                    //            item.ColumnaK = "3";
                    //        }
                    //    }
                    //    else
                    //    {
                    //        item.ColumnaK = "2";
                    //    }

                    //}
                    //else
                    //{
                    //    item.ColumnaK = "1";
                    //}


                    resArchivo.Add(item);
                }
            }

            if (descargaArchivo)
            {
                ExcelPackage package = new ExcelPackage();
                package.Workbook.Properties.Title = "CECYTECH";
                package.Workbook.Properties.Author = "I.S.C. Alejandro Torres";
                package.Workbook.Properties.Subject = "Exportacion de datos para sistema COSDAC";
                package.Workbook.Properties.Keywords = "COSDAC";



                OfficeOpenXmlExtensions.CrearHojaConLista<FormatoCargaSistemaCOSDAC>(ref package, resArchivo, "Empleados");


                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"archivoCOSDAC_{clavePlantel}_{DateTime.Now.ToString("yyyyMMddhhmm")}.xlsx");
            }
            else
            {

                ViewBag.clavePlantel = clavePlantel;
                return View(resArchivo);
            }

        }
    }
}
