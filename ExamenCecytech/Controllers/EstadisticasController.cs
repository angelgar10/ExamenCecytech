using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamenCecytech.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamenCecytech.Controllers
{
    public class EstadisticasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<int, string> aspectosEvaluados = new Dictionary<int, string>()
        {
            {1,"Texto Expositivo"},
            {2,"Texto Expositivo"},
            {3,"Texto Expositivo"},
            {4,"Texto Expositivo"},
            {9,"Texto Expositivo"},
            {28,"Texto Expositivo"},
            {29,"Texto Expositivo"},
            {30,"Texto Expositivo"},
            {31,"Texto Expositivo"},
            {32,"Texto Expositivo"},
            {34,"Texto Expositivo"},
            {5,"Texto Argumentativo"},
            {10,"Texto Argumentativo"},
            {19,"Texto Argumentativo"},
            {21,"Texto Argumentativo"},
            {25,"Texto Argumentativo"},
            {26,"Texto Argumentativo"},
            {27,"Texto Argumentativo"},
            {35,"Texto Argumentativo"},
            {36,"Texto Argumentativo"},
            {47,"Texto Argumentativo"},
            {49,"Texto Argumentativo"},
            {7,"Texto Literario"},
            {11,"Texto Literario"},
            {16,"Texto Literario"},
            {17,"Texto Literario"},
            {20,"Texto Literario"},
            {22,"Texto Literario"},
            {40,"Texto Literario"},
            {42,"Texto Literario"},
            {43,"Texto Literario"},
            {50,"Texto Literario"},
            {6,"Manejo de la Información"},
            {8,"Manejo de la Información"},
            {12,"Manejo de la Información"},
            {13,"Manejo de la Información"},
            {14,"Manejo de la Información"},
            {15,"Manejo de la Información"},
            {18,"Manejo de la Información"},
            {23,"Manejo de la Información"},
            {24,"Manejo de la Información"},
            {33,"Manejo de la Información"},
            {37,"Manejo de la Información"},
            {38,"Manejo de la Información"},
            {39,"Manejo de la Información"},
            {41,"Manejo de la Información"},
            {44,"Manejo de la Información"},
            {45,"Manejo de la Información"},
            {46,"Manejo de la Información"},
            {48,"Manejo de la Información"},
            {51,"Sentido Numérico y pensamiento algebráico"},
            {52,"Sentido Numérico y pensamiento algebráico"},
            {53,"Sentido Numérico y pensamiento algebráico"},
            {54,"Sentido Numérico y pensamiento algebráico"},
            {55,"Sentido Numérico y pensamiento algebráico"},
            {56,"Sentido Numérico y pensamiento algebráico"},
            {57,"Sentido Numérico y pensamiento algebráico"},
            {58,"Sentido Numérico y pensamiento algebráico"},
            {63,"Sentido Numérico y pensamiento algebráico"},
            {76,"Sentido Numérico y pensamiento algebráico"},
            {77,"Sentido Numérico y pensamiento algebráico"},
            {78,"Sentido Numérico y pensamiento algebráico"},
            {79,"Sentido Numérico y pensamiento algebráico"},
            {80,"Sentido Numérico y pensamiento algebráico"},
            {81,"Sentido Numérico y pensamiento algebráico"},
            {82,"Sentido Numérico y pensamiento algebráico"},
            {83,"Sentido Numérico y pensamiento algebráico"},
            {84,"Sentido Numérico y pensamiento algebráico"},
            {59,"Cambios y Relaciones"},
            {60,"Cambios y Relaciones"},
            {64,"Cambios y Relaciones"},
            {65,"Cambios y Relaciones"},
            {66,"Cambios y Relaciones"},
            {67,"Cambios y Relaciones"},
            {68,"Cambios y Relaciones"},
            {69,"Cambios y Relaciones"},
            {70,"Cambios y Relaciones"},
            {85,"Cambios y Relaciones"},
            {86,"Cambios y Relaciones"},
            {87,"Cambios y Relaciones"},
            {88,"Cambios y Relaciones"},
            {89,"Cambios y Relaciones"},
            {93,"Cambios y Relaciones"},
            {94,"Cambios y Relaciones"},
            {96,"Cambios y Relaciones"},
            {71,"Forma, espacio y medida"},
            {72,"Forma, espacio y medida"},
            {95,"Forma, espacio y medida"},
            {97,"Forma, espacio y medida"},
            {98,"Forma, espacio y medida"},
            {61,"Manejo de la información"},
            {62,"Manejo de la información"},
            {73,"Manejo de la información"},
            {74,"Manejo de la información"},
            {75,"Manejo de la información"},
            {90,"Manejo de la información"},
            {91,"Manejo de la información"},
            {92,"Manejo de la información"},
            {99,"Manejo de la información"},
            {100,"Manejo de la información"},
    };

        [TempData]
        public string ErrorMsg { get; set; }
        [TempData]
        public string ExitoMsg { get; set; }

        public EstadisticasController(ApplicationDbContext context )
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var planteles = _context.Planteles.AsNoTracking().OrderBy(p => p.ClaveCentroTrabajo);
            return View(await planteles.ToListAsync());
        }

        public IActionResult Filtros()
        {
            return View();
        }
        public async Task<IActionResult> Resumen(
            [FromHeader] string tipo,
            [FromHeader] string semestre,
            [FromHeader] string turno,
            [FromHeader] string plantel,
            [FromHeader] string genero)

        {
            string[] tipoParam = null;
            string[] semestreParam = null;
            string[] turnoParam = null;
            string[] plantelParam = null;
            string[] generosParam = null;

            var lstTipos = new List<string> { "DOCENTES", "ALUMNOS" };
            var lstSemestres = await _context.Grupos.AsNoTracking().Select(p => p.Semestre).Distinct().ToListAsync();
            var lstTurnos = await _context.Grupos.AsNoTracking().Select(p => p.Turno).Distinct().ToListAsync();
            var lstPlanteles = await _context.Planteles.AsNoTracking().Select(p => p.ClavePlantel).ToListAsync();
            var lstGeneros = await _context.Aspirante.AsNoTracking().Select(a => a.Genero).Distinct().ToListAsync();

            if (!string.IsNullOrEmpty(tipo))
            {
                tipoParam = tipo.Split(',');
                foreach (var item in tipoParam)
                {
                    if (!lstTipos.Contains(item.ToUpper()))
                    {
                        return BadRequest(error: $"No existe el tipo {item}, los valores posibles son {string.Join(',', lstTipos)}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(semestre))
            {
                semestreParam = semestre.Split(',');
                foreach (var item in semestreParam)
                {
                    if (!lstSemestres.Contains(item))
                    {
                        return BadRequest(error: $"No existe el semestre {item}, los valores posibles son {string.Join(',', lstSemestres)}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(turno))
            {
                turnoParam = turno.Split(',');
                foreach (var item in turnoParam)
                {
                    if (!lstTurnos.Contains(item))
                    {
                        return BadRequest(error: $"No existe el turno {item}, los valores posibles son {string.Join(',', lstTurnos)}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(plantel))
            {
                plantelParam = plantel.Split(',');
                foreach (var item in plantelParam)
                {
                    if (!lstPlanteles.Contains(item))
                    {
                        return BadRequest(error: $"No existe el plantel {item}, los valores posibles son {string.Join(',', lstPlanteles)}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(genero))
            {
                generosParam = genero.Split(',');
                for (int i = 0; i < generosParam.Count(); i++)
                {
                    generosParam[i] = generosParam[i] ?? "";
                    if (!(lstGeneros.Contains(generosParam[i])))
                    {
                        return BadRequest(error: "El genero es un arreglo con valores " + string.Join(',', lstGeneros));
                    }
                }
            }


            var generosDefinido = generosParam != null;
            var plantelDefinido = plantelParam != null;
            var tipoDefinido = tipoParam != null;
            var semestreDefinido = semestreParam != null;
            var turnoDefinido = turnoParam != null;

            var evaluaciones = _context.RespuestasEvaluaciones
                                .Include(e => e.Aspirante).ThenInclude(a => a.Grupo).ThenInclude(g => g.Plantel)
                                .AsNoTracking()
                                .Where(e =>
                                    // Se filtra el plantel, si es diferente de null
                                    //(plantelParam == null || e.Aspirante.Grupo.Plantel.ClavePlantel == plantelParam)
                                    (!plantelDefinido
                                     ||

                                     //plantelParam.Contains(e.Aspirante.Grupo.Plantel.ClavePlantel)
                                     plantelParam.Contains(e.Aspirante.Grupo.Plantel.ClavePlantel)
                                    )
                                    &&
                                    // Se filtra el genero, si es diferente de null
                                    (!generosDefinido
                                    ||
                                     //(
                                     //(genero.Contains("SIN") && string.IsNullOrEmpty(e.Aspirante.Genero) )
                                     //|
                                     //(genero.Contains("M") && e.Aspirante.Genero == "M")
                                     //|
                                     //(genero.Contains("F") && e.Aspirante.Genero == "F")
                                     //)
                                     generosParam.Contains(e.Aspirante.Genero)
                                    )
                                    &&
                                    // Se filtra el tipo de evaluado, si es diferente de null
                                    (!tipoDefinido
                                     ||
                                            //(
                                            //    (tipo.ToUpper() == "ALUMNOS" && e.Aspirante.Grupo.Nombre != "Docentes")
                                            //    ||
                                            //    (tipo.ToUpper() == "DOCENTES" && e.Aspirante.Grupo.Nombre == "Docentes")
                                            //)
                                            (
                                            (tipoParam.Contains("DOCENTES") && e.Aspirante.Grupo.Nombre != "Docentes")
                                            ||
                                            (tipoParam.Contains("ALUMNOS") && e.Aspirante.Grupo.Nombre == "Docentes")
                                            )
                                    )
                                    &&
                                    // Se filtra el semestre, si es diferente de null
                                    (!semestreDefinido ||
                                            //(
                                            //    (semestre == "3" && e.Aspirante.Grupo.Semestre == "3")
                                            //    ||
                                            //    (semestre == "5" && e.Aspirante.Grupo.Semestre == "5")
                                            //    ||
                                            //    (semestre.ToUpper() == "SIN" && e.Aspirante.Grupo.Semestre == null)
                                            //)
                                            semestreParam.Contains(e.Aspirante.Grupo.Semestre)
                                    )
                                    &&
                                    // Se filtra el turno, si es diferente de null
                                    (!turnoDefinido ||
                                            //(
                                            //    (turno == "M" && e.Aspirante.Grupo.Turno == "M")
                                            //    ||
                                            //    (turno == "V" && e.Aspirante.Grupo.Turno == "V")
                                            //    ||
                                            //    (turno.ToUpper() == "SIN" && e.Aspirante.Grupo.Turno == null)
                                            //)
                                            turnoParam.Contains(e.Aspirante.Grupo.Turno)
                                    )
                                );


            // Numero de Evaluaciones realizadas
            decimal numeroEvaluaciones = await evaluaciones.Select(e => e.AspiranteId).Distinct().CountAsync();

            var preguntas = _context.Competencias
                                        .AsNoTracking()
                                        .OrderBy(c => c.Nombre)
                                        .SelectMany(c => c.Preguntas, (competencia, pregunta) => new
                                        {
                                            Competencia = competencia.Nombre,
                                            AspectoEvaluado = aspectosEvaluados[pregunta.NumeroPregunta],
                                            pregunta.NumeroPregunta,
                                            ClaveCorrecta = pregunta.RespuestasPregunta.First(rp => rp.Valor == 1).ClaveCOSDAC,
                                            NumCorrectas = evaluaciones.Count(e => e.RespuestaPregunta.Pregunta == pregunta && e.RespuestaPregunta.Valor == 1),
                                            NumErroneas = evaluaciones.Count(e => e.RespuestaPregunta.Pregunta == pregunta && e.RespuestaPregunta.Valor == 0),
                                            NumSinContestar = numeroEvaluaciones - evaluaciones.Count(e => e.RespuestaPregunta.Pregunta == pregunta),
                                            PorcCorrectas = evaluaciones.Count(e => e.RespuestaPregunta.Pregunta == pregunta && e.RespuestaPregunta.Valor == 1) / numeroEvaluaciones * 100,
                                            PorcErroneas = evaluaciones.Count(e => e.RespuestaPregunta.Pregunta == pregunta && e.RespuestaPregunta.Valor == 0) / numeroEvaluaciones * 100,
                                            PorcSinContestar = (numeroEvaluaciones - evaluaciones.Count(e => e.RespuestaPregunta.Pregunta == pregunta)) / numeroEvaluaciones * 100,
                                        })
                                        .OrderBy(r => r.NumeroPregunta);
            return Json(new
            {
                plantel = _context.Planteles
                    .AsNoTracking()
                    .Select(x => new
                    {
                        Plantel = x.ClavePlantel,
                        Cantidad = evaluaciones.Select(e => e.Aspirante)
                                        .Distinct()
                                        .Count(y => y.Grupo.ClavePlantel == x.ClavePlantel),
                        IncluidoEnFiltro = plantelParam == null ? false : plantelParam.Contains(x.ClavePlantel)
                    }),

                tipo = new object[]{
                    new {
                        Tipo = "DOCENTES",
                        Cantidad = await evaluaciones.Select(e => e.Aspirante).Distinct().Select(a => a.Grupo).CountAsync(g => g.Nombre == "Docentes"),
                        IncluidoEnFiltro = tipoParam ==null ? true : tipoParam.Contains("DOCENTES")
                    },
                    new {
                        Tipo = "ALUMNOS",
                        Cantidad = await evaluaciones.Select(e => e.Aspirante).Distinct().Select(a => a.Grupo).CountAsync(g => g.Nombre != "Docentes"),
                        IncluidoEnFiltro = tipoParam ==null ? true : tipoParam.Contains("ALUMNOS")
                    }
                },
                numeroEvaluaciones,

                genero = _context.Aspirante
                            .AsNoTracking()
                            .GroupBy(a => a.Genero)
                            .Select(x => new
                            {
                                Genero = x.Key,
                                Cantidad = evaluaciones.Select(e => e.Aspirante).Distinct().Count(y => y.Genero == x.Key),
                                IncluidoEnFiltro = generosParam == null ? true : generosParam.Contains(x.Key)
                            }),

                Turno = _context.Grupos
                        .AsNoTracking()
                        .GroupBy(g => g.Turno)
                        .Select(x => new
                        {
                            Turno = x.Key,
                            Cantidad = evaluaciones.Select(e => e.Aspirante).Distinct().Count(y => y.Grupo.Turno == x.Key),
                            IncluidoEnFiltro = turnoParam == null ? true : turnoParam.Contains(x.Key)
                        }),

                Semestre = _context.Grupos
                        .AsNoTracking()
                        .GroupBy(g => g.Semestre)
                        .Select(x => new
                        {
                            Semestre = x.Key,
                            Cantidad = evaluaciones.Select(e => e.Aspirante).Distinct().Count(y => y.Grupo.Semestre == x.Key),
                            IncluidoEnFiltro = semestreParam == null ? true : semestreParam.Contains(x.Key)
                        }),
                preguntas
            });
        }
    }
}
