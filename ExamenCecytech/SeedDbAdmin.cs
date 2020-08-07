using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Hosting;
using ExamenCecytech.Data;
using ExamenCecytech.Models;

namespace ExamenCecytech
{
    public class SeedDbAdmin
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly UserManager<Aspirante> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly List<Competencia> _examenPlanea;

        public SeedDbAdmin(RoleManager<ApplicationRole> roleManager,
            UserManager<Aspirante> userManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment
            )
        {
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _roleManager = roleManager;
            //_examenPlanea = JsonConvert.DeserializeObject<List<Competencia>>(System.IO.File.ReadAllText(hostingEnvironment.ContentRootPath + "\\Planea2017.json")
            //    , new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
        }

        public async Task SeedUsuarioAdmin()
        {
            await ChecarCrearRol("SysAdmin");
            await ChecarCrearRol("Administrativo");
            await ChecarCrearRol("Aspirante");

            await ChecarCrearUsuarioAdmin("Garnica", "Alvarez", "Angel Omar", "a.garnica@cecytechihuahua.edu.mx", "123456");
            await ChecarCrearUsuarioAdmin("Baeza", "Saenz", "Pamela", "p.baeza@cecytechihuahua.edu.mx", PasswordAleatorio());
            await ChecarCrearUsuarioAdmin("Hernandez", "C", "Fernando", "f.hernandezc@cecytechihuahua.edu.mx", PasswordAleatorio());
            await ChecarCrearUsuarioAdmin("Franco", "", "Kevin", "k.franco@cecytechihuahua.edu.mx", PasswordAleatorio());
            await ChecarCrearUsuarioAdmin("Hernandez", "Aguilar", "Erick", "e.hernandez@cecytechihuahua.edu.mx", PasswordAleatorio());
            await ChecarCrearUsuarioAdmin("Gonzalez", "P", "Irving", "i.gonzalezp@cecytechihuahua.edu.mx", PasswordAleatorio());
            await ChecarCrearUsuarioAdmin("Caballero", "", "Bibian", "b.caballero@cecytechihuahua.edu.mx", PasswordAleatorio());

            var todosLosPlanteles = await _context.Planteles.AsNoTracking().Select(p => p.ClavePlantel).ToListAsync();
            await ChecarUsuarioEnPlantel("a.garnica@cecytechihuahua.edu.mx", todosLosPlanteles);
            await ChecarUsuarioEnPlantel("p.baeza@cecytechihuahua.edu.mx", todosLosPlanteles);
            await ChecarUsuarioEnPlantel("f.hernandezc@cecytechihuahua.edu.mx", todosLosPlanteles);
            await ChecarUsuarioEnPlantel("k.franco@cecytechihuahua.edu.mx", todosLosPlanteles);
            await ChecarUsuarioEnPlantel("e.hernandez@cecytechihuahua.edu.mx", todosLosPlanteles);
            await ChecarUsuarioEnPlantel("i.gonzalezp@cecytechihuahua.edu.mx", todosLosPlanteles);
            await ChecarUsuarioEnPlantel("b.caballero@cecytechihuahua.edu.mx", todosLosPlanteles);
        }

        private async Task ChecarCrearUsuarioAdmin(string paterno, string materno, string nombre, string correo, string pass = null)
        {
            await CrearUsuarioConRol(
            new Aspirante
            {
                Paterno = paterno,
                Materno = materno,
                Nombre = nombre,
                Ficha = correo.Split("@")[0],
                UserName = correo,
                Email = correo,
                Edad = 0M,
                Genero = "M",
                GrupoId = null,
                NombreSecundaria = "",
                PromedioSecundaria = 0,
                TipoSecundaria = "",
                TipoSostenimientoSecundaria = "",
                DescripcionOtraSecundaria = "",
                PlainPass = pass
            },
            new List<string> { "SysAdmin", "Administrativo" });
        }

        private async Task ChecarCrearRol(string v)
        {
            if (!await _roleManager.RoleExistsAsync(v))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = v });
            }
        }

        private async Task<Aspirante> CrearUsuarioConRol(Aspirante aspirante, List<string> roles)
        {
            aspirante.Ficha = aspirante.UserName.Split("@")[0];
            var usuario = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == aspirante.UserName);
            if (usuario == null)
            {
                var resCrearAdmin = await _userManager.CreateAsync(aspirante, aspirante.PlainPass);

                usuario = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == aspirante.NormalizedUserName);
            }

            foreach (var rol in roles)
            {
                if (!await _userManager.IsInRoleAsync(usuario, rol))
                {

                    await _userManager.AddToRolesAsync(usuario, new List<string> { rol });
                }
            }

            return usuario;
        }

        //public async Task SeedProfesores()
        //{
        //    using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("superNomina")))
        //    {
        //        SqlCommand cmd = new SqlCommand("select e.NTRAB,e.nsexo,e.NNOM,e.nccto,(select substring(c.ICDNOM,1,1)+substring(c.icdnom,CHARINDEX(' ',icdnom,1)+1,2)  from [Catalogo Centros de Costos] c where  len(c.CCTO_ID) < 3 and c.CCTO_ID like replicate('0',2-len(e.NCCTO))+cast(e.nccto as nvarchar)	+'%') as plantel,ltrim(rtrim(e.NEMAIL)) as nemail from [Catalogo Empleados] e where e.NVIGENTE = 1 and e.NPUESTO_NO > 100 and nemail not like '' and nemail is not null ", con);
        //        await con.OpenAsync();
        //        SqlDataReader rdr = await cmd.ExecuteReaderAsync();
        //        if (rdr.HasRows)
        //        {
        //            while (rdr.Read())
        //            {
        //                string usrProfe = rdr.GetFieldValue<string>("nemail");
        //                var usuarioProfe = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == usrProfe);
        //                if (usuarioProfe == null)
        //                {
        //                    var gpo = _context.Grupos.FirstOrDefault(g => g.Nombre == "Docentes" && g.ClavePlantel == rdr.GetFieldValue<string>("Plantel"));
        //                    if (gpo == null)
        //                    {
        //                        _context.Grupos.Add(new Grupo
        //                        {
        //                            ClavePlantel = rdr.GetFieldValue<string>("Plantel"),
        //                            ClaveSIIACE = "",
        //                            FechaExamen = new DateTime(2018, 11, 22),
        //                            Nombre = "Docentes"
        //                        });
        //                        _context.SaveChanges();
        //                    }
        //                    gpo = _context.Grupos.FirstOrDefault(g => g.Nombre == "Docentes" && g.ClavePlantel == rdr.GetFieldValue<string>("Plantel"));
        //                    var resCrearAdmin = await _userManager.CreateAsync(new Aspirante
        //                    {
        //                        Paterno = rdr.GetFieldValue<string>("nnom").Split('/')[0],
        //                        Materno = rdr.GetFieldValue<string>("nnom").Split('/')[1],
        //                        Nombre = rdr.GetFieldValue<string>("nnom").Split('/')[2],
        //                        Ficha = usrProfe.Split('@')[0],
        //                        UserName = usrProfe,
        //                        Email = usrProfe,
        //                        Edad = 0,
        //                        Genero = rdr.GetFieldValue<string>("nsexo"),
        //                        GrupoId = gpo.GrupoId,
        //                        NombreSecundaria = "",
        //                        PromedioSecundaria = 0,
        //                        TipoSecundaria = "",
        //                        TipoSostenimientoSecundaria = "",
        //                        DescripcionOtraSecundaria = "",
        //                        PlainPass = ""
        //                    }, "Kazezepima.8713");

        //                    usuarioProfe = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == usrProfe);

        //                }

        //                if (usuarioProfe != null && !await _userManager.IsInRoleAsync(usuarioProfe, "Aspirante"))
        //                {
        //                    await _userManager.AddToRolesAsync(usuarioProfe, new List<string> { "Aspirante" });
        //                    if (usuarioProfe.GrupoId == null)
        //                    {
        //                        var usuarioGrupo = await _context.Aspirante.FirstOrDefaultAsync(u => u.UserName == usuarioProfe.UserName);
        //                        if (usuarioGrupo != null)
        //                        {
        //                            usuarioGrupo.GrupoId = _context.Grupos.FirstOrDefault(g => g.Nombre == "Docentes" && g.ClavePlantel == rdr.GetFieldValue<string>("Plantel")).GrupoId;
        //                            _context.SaveChanges();
        //                        }
        //                    }
        //                }

        //            }
        //        }
        //    }
        //}

        //public async Task SeedPlanteles()
        //{
        //    foreach (var plantel in _context.Planteles)
        //    {
        //        foreach (var grupo in plantel.GruposPlantel)
        //        {
        //            foreach (var aspirante in grupo.Aspirantes)
        //            {
        //                _context.Remove(aspirante);
        //            }
        //            _context.SaveChanges();
        //            _context.Remove(grupo);
        //        }
        //        _context.SaveChanges();
        //        _context.Remove(plantel);
        //    }
        //    _context.SaveChanges();

        //    var usuarioAdmin = _context.Aspirante.First(a => a.NormalizedUserName == "A.GARNICA");
        //    #region sembrar planteles, grupos y aspirantes
        //    if (!_context.Planteles.Any())
        //    {
        //        var hasher = new PasswordHasher<Aspirante>();
        //        using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("inscripciones2018")))
        //        {
        //            SqlCommand cmd = new SqlCommand("SELECT PLANTELID,CLAVEPLANTEL,NOMBRE FROM PLANTELES ORDER BY CLAVEPLANTEL ", con);
        //            try
        //            {
        //                con.Open();
        //                SqlDataReader rdr = cmd.ExecuteReader();

        //                List<string> plantelesAImportar = new List<string>
        //                {
        //                    "C07",
        //                    "C13",
        //                    "C14",
        //                    "C20",
        //                    "C24",
        //                    "E04",
        //                    "E12",
        //                    "E13",
        //                    "E26",
        //                    "E33",
        //                };

        //                while (rdr.Read())
        //                {
        //                    if (plantelesAImportar.Contains(rdr.GetFieldValue<string>("ClavePlantel")))
        //                    {
        //                        var newPlantel = new Plantel
        //                        {
        //                            ClavePlantel = rdr.GetFieldValue<string>("claveplantel"),
        //                            Nombre = rdr.GetFieldValue<string>("nombre").Replace("COLEGIO DE ESTUDIOS CIENTIFICOS Y TECNOLOGICOS", "CECYTECH")
        //                        };
        //                        string nomUsuarioPlantel = rdr.GetFieldValue<string>("clavePlantel").StartsWith('C') ? "CECYT" + rdr.GetFieldValue<string>("clavePlantel").Substring(1) : "EMSAD" + rdr.GetFieldValue<string>("clavePlantel").Substring(1);
        //                        var usuarioPlantel = new Aspirante
        //                        {
        //                            Paterno = "",
        //                            Materno = "",
        //                            Nombre = nomUsuarioPlantel,
        //                            Ficha = nomUsuarioPlantel,
        //                            Genero = "M",
        //                            Edad = 0,
        //                            PromedioSecundaria = 0,
        //                            NombreSecundaria = "",
        //                            DescripcionOtraSecundaria = "",
        //                            TipoSecundaria = "",
        //                            TipoSostenimientoSecundaria = "",
        //                            PlainPass = ""

        //                        };
        //                        usuarioPlantel.Email = usuarioPlantel.Ficha.ToUpper().Trim();
        //                        usuarioPlantel.NormalizedEmail = usuarioPlantel.Ficha.ToUpper().Trim();
        //                        usuarioPlantel.UserName = usuarioPlantel.Ficha.ToUpper().Trim();
        //                        usuarioPlantel.NormalizedUserName = usuarioPlantel.Ficha.ToUpper().Trim();
        //                        usuarioPlantel.SecurityStamp = Guid.NewGuid().ToString();
        //                        usuarioPlantel.PasswordHash = hasher.HashPassword(usuarioPlantel, "Kazepima." + rdr.GetFieldValue<string>("clavePlantel"));


        //                        newPlantel.UsuariosPlantel.Add(new UsuarioPlantel
        //                        {
        //                            Aspirante = usuarioPlantel,
        //                            ClavePlantel = newPlantel.ClavePlantel
        //                        });
        //                        // Agregar el usuario admin a cada plantel
        //                        newPlantel.UsuariosPlantel.Add(new UsuarioPlantel { Aspirante = usuarioAdmin, ClavePlantel = newPlantel.ClavePlantel });

        //                        SqlCommand cmdGpos = new SqlCommand("SELECT NOMBRE,FECHAEXAMEN FROM AULAS WHERE PLANTELID = @PlantelId", con);
        //                        cmdGpos.Parameters.AddWithValue("@PlantelId", rdr.GetFieldValue<int>("PlantelId"));
        //                        SqlDataReader rdrGpos = cmdGpos.ExecuteReader();
        //                        while (rdrGpos.Read())
        //                        {
        //                            var newGrupo = new Grupo
        //                            {
        //                                ClavePlantel = rdr.GetFieldValue<string>("claveplantel"),
        //                                Nombre = rdrGpos.GetFieldValue<string>("nombre"),
        //                                FechaExamen = rdrGpos.GetFieldValue<DateTime>("FECHAEXAMEN")
        //                            };

        //                            SqlCommand cmdAspirantes = new SqlCommand(
        //                                @"
        //                            SELECT 
      
	       //                                Paterno
	       //                               ,materno
	       //                               ,nombre
	       //                               ,(select fo.folioInterno from folios fo where fo.FolioId = fi.FolioId) as ficha
	       //                               ,(select g.nombre from aulas g where g.AulaId = (select fo2.AulaId from folios fo2 where fo2.FolioId = fi.FolioId)) as grupoNombre
        //                                  ,case when sexo =1 then 'F' else 'M' end as genero
	       //                               ,DATEDIFF(year,fechaNacimiento,CURRENT_TIMESTAMP) edad
	       //                               ,PromedioFinal as promedioSecundaria
	       //                               ,(select s.ClaveCentroTrabajo + ' - '+ s.nombre from Secundarias s where s.SecundariaId = fi.SecundariaId) as nombresecundaria
        //                                  ,(select s.Servicio from Secundarias s where s.SecundariaId = fi.SecundariaId) as tiposecundaria
	       //                               ,(select s.Sostenimiento from Secundarias s where s.SecundariaId = fi.SecundariaId) as sostenimiento
        //                              FROM [Inscripciones2018].[dbo].[Fichas] fi
        //                              where folioId in 
        //                              (select fo3.folioid from folios fo3 where fo3.AulaId = (select a.AulaId 
        //                              from Aulas a 
        //                              where a.Nombre = @grupoNombre 
        //                              and a.PlantelId = (select p.plantelId from planteles p where p.ClavePlantel = @clavePlantel )
        //                              )) ORDER BY 4
        //                            ", con);
        //                            cmdAspirantes.Parameters.AddWithValue("@grupoNombre", newGrupo.Nombre);
        //                            cmdAspirantes.Parameters.AddWithValue("@clavePlantel", newGrupo.ClavePlantel);
        //                            SqlDataReader rdrAspirantes = cmdAspirantes.ExecuteReader();

        //                            while (rdrAspirantes.Read())
        //                            {
        //                                var newAspirante = new Aspirante
        //                                {
        //                                    Paterno = rdrAspirantes.GetFieldValue<string>("Paterno"),
        //                                    Materno = rdrAspirantes.GetFieldValue<string>("Materno"),
        //                                    Nombre = rdrAspirantes.GetFieldValue<string>("Nombre"),
        //                                    Ficha = rdrAspirantes.GetFieldValue<string>("Ficha"),
        //                                    Genero = rdrAspirantes.GetFieldValue<string>("Genero"),
        //                                    Edad = rdrAspirantes.GetFieldValue<decimal>("Edad"),
        //                                    PromedioSecundaria = rdrAspirantes.GetFieldValue<decimal>("promedioSecundaria"),
        //                                    NombreSecundaria = rdrAspirantes.GetFieldValue<string>("nombreSecundaria"),
        //                                    DescripcionOtraSecundaria = "",
        //                                    LockoutEnabled = true,
        //                                    PlainPass = CodeShare.Library.Passwords.PasswordGenerator.GeneratePassword(true, true, true, false, false, 10)
        //                                };

        //                                switch (rdrAspirantes.GetFieldValue<string>("tipoSecundaria"))
        //                                {
        //                                    case "": newAspirante.TipoSecundaria = ""; break;
        //                                    case "23 PRIMARIA PARA ADULTOS": newAspirante.TipoSecundaria = ""; break;
        //                                    case "41 SECUNDARIA GENERAL": newAspirante.TipoSecundaria = "SECUNDARIA GENERAL"; break;
        //                                    case "42 SECUNDARIA PARA TRABAJADORES": newAspirante.TipoSecundaria = "SECUNDARIA PARA TRABAJADORES"; break;
        //                                    case "43 SECUNDARIA TECNICA INDUSTRIAL": newAspirante.TipoSecundaria = "SECUNDARIA TECNICA"; break;
        //                                    case "44 SECUNDARIA TECNICA AGROPECUARIA": newAspirante.TipoSecundaria = "SECUNDARIA TECNICA"; break;
        //                                    case "47 SECUNDARIA ABIERTA": newAspirante.TipoSecundaria = "OTRA"; newAspirante.DescripcionOtraSecundaria = "SECUNDARIA ABIERTA"; break;
        //                                    case "48 TELESECUNDARIA": newAspirante.TipoSecundaria = "TELESECUNDARIA"; break;
        //                                    case "EDUCACION": newAspirante.TipoSecundaria = ""; newAspirante.DescripcionOtraSecundaria = ""; break;
        //                                    case "EDUCACIÓN": newAspirante.TipoSecundaria = ""; newAspirante.DescripcionOtraSecundaria = ""; break;
        //                                    case "EDUCATIVO": newAspirante.TipoSecundaria = ""; newAspirante.DescripcionOtraSecundaria = ""; break;
        //                                    case "EDUCATIVOS": newAspirante.TipoSecundaria = ""; newAspirante.DescripcionOtraSecundaria = ""; break;
        //                                    case "ESTATAL": newAspirante.TipoSecundaria = "SECUNDARIA GENERAL"; break;
        //                                    case "FEDERAL": newAspirante.TipoSecundaria = "SECUNDARIA GENERAL"; break;
        //                                    case "PUBLICO": newAspirante.TipoSecundaria = "SECUNDARIA GENERAL"; break;
        //                                    case "SECUNDARIA": newAspirante.TipoSecundaria = "SECUNDARIA GENERAL"; break;
        //                                    case "SECUNDARIA GENERAL": newAspirante.TipoSecundaria = "SECUNDARIA GENERAL"; break;
        //                                    default: newAspirante.TipoSecundaria = ""; break;
        //                                }

        //                                switch (rdrAspirantes.GetFieldValue<string>("Sostenimiento"))
        //                                {
        //                                    case "": newAspirante.TipoSostenimientoSecundaria = ""; break;
        //                                    case "21 ESTATAL": newAspirante.TipoSostenimientoSecundaria = "ESTATAL"; break;
        //                                    case "24 FEDERAL TRANSFERIDO": newAspirante.TipoSostenimientoSecundaria = "FEDERAL"; break;
        //                                    case "42 SUBSIDIO SECRETARIA DE EDUCACION DEL GOBIERNO DEL ESTADO -ASOCIACION CIVIL": newAspirante.TipoSostenimientoSecundaria = "ESTATAL"; break;
        //                                    case "61 PARTICULAR": newAspirante.TipoSostenimientoSecundaria = "PARTICULAR (RVOE)"; break;
        //                                    case "ESTATAL": newAspirante.TipoSostenimientoSecundaria = "ESTATAL"; break;
        //                                    case "FEDERAL": newAspirante.TipoSostenimientoSecundaria = "FEDERAL"; break;
        //                                    case "PUBLICO": newAspirante.TipoSostenimientoSecundaria = ""; break;
        //                                    default:
        //                                        newAspirante.TipoSostenimientoSecundaria = ""; break;
        //                                }


        //                                newAspirante.Email = newAspirante.Ficha.ToUpper().Trim();
        //                                newAspirante.NormalizedEmail = newAspirante.Ficha.ToUpper().Trim();
        //                                newAspirante.UserName = newAspirante.Ficha.ToUpper().Trim();
        //                                newAspirante.NormalizedUserName = newAspirante.Ficha.ToUpper().Trim();
        //                                newAspirante.SecurityStamp = Guid.NewGuid().ToString();
        //                                newAspirante.PasswordHash = hasher.HashPassword(newAspirante, newAspirante.PlainPass);

        //                                if (newAspirante.NombreSecundaria == null)
        //                                {
        //                                    newAspirante.NombreSecundaria = "";
        //                                }
        //                                newGrupo.Aspirantes.Add(newAspirante);
        //                            }

        //                            newPlantel.GruposPlantel.Add(newGrupo);
        //                        }

        //                        _context.Planteles.Add(newPlantel);
        //                        _context.SaveChanges();

        //                    }
        //                }

        //                var lstUsuariosPlantel = _context.UsuariosPlantel.Where(up => up.Id != usuarioAdmin.Id).Select(up => up.Id).ToList();
        //                var idRolAdministrativo = _context.Roles.First(r => r.Name == "Administrativo").Id;
        //                var lstUsuariosEnRol = new List<IdentityUserRole<int>>();
        //                foreach (var item in lstUsuariosPlantel)
        //                {
        //                    lstUsuariosEnRol.Add(new IdentityUserRole<int>
        //                    {
        //                        RoleId = idRolAdministrativo,
        //                        UserId = item
        //                    });
        //                }
        //                _context.UserRoles.AddRange(lstUsuariosEnRol);
        //                _context.SaveChanges();

        //            }
        //            catch (Exception e)
        //            {

        //                throw new Exception(e.Message);
        //            }

        //        }


        //    }
        //    #endregion

        //}

        public async Task SeedExamenPlanea()
        {
            _context.RespuestasEvaluaciones.RemoveRange(_context.RespuestasEvaluaciones);
            await _context.SaveChangesAsync();
            _context.Respuestas.RemoveRange(_context.Respuestas);
            await _context.SaveChangesAsync();
            _context.Preguntas.RemoveRange(_context.Preguntas);
            await _context.SaveChangesAsync();
            _context.Competencias.RemoveRange(_context.Competencias);
            await _context.SaveChangesAsync();

            foreach (var competencia in _examenPlanea)
            {
                Competencia newComp = new Competencia
                {
                    Nombre = competencia.Nombre,
                    TiempoParaResolver = competencia.TiempoParaResolver,
                    LecturaPrevia = competencia.LecturaPrevia
                };
                foreach (var pregunta in competencia.Preguntas)
                {
                    var newPregunta = new Pregunta
                    {
                        NumeroPregunta = pregunta.NumeroPregunta,
                        Texto = pregunta.Texto,
                        Orden1 = pregunta.Orden1,
                        Orden2 = pregunta.Orden2,
                        LecturaPrevia = pregunta.LecturaPrevia
                    };
                    foreach (var resp in pregunta.RespuestasPregunta)
                    {
                        newPregunta.RespuestasPregunta.Add(new RespuestaPregunta
                        {
                            Texto = resp.Texto,
                            ClaveCOSDAC = resp.ClaveCOSDAC,
                            Orden1 = resp.Orden1,
                            Orden2 = resp.Orden2,
                            Valor = resp.Valor
                        });
                    }
                    newComp.Preguntas.Add(newPregunta);
                }
                _context.Competencias.Add(newComp);
            }
            await _context.SaveChangesAsync();
        }



        //public async Task SeedDatosPlanea()
        //{

        //    // Checamos que las especialidades Existan
        //    using (SqlConnection conSIIACE = new SqlConnection(_configuration.GetConnectionString("SIIACE")))
        //    {
        //        SqlCommand cmdGrupos = new SqlCommand("SELECT esp_id,esp_nombre from especialidad order by esp_nombre", conSIIACE);
        //        try
        //        {
        //            await conSIIACE.OpenAsync();
        //            SqlDataReader rdr = await cmdGrupos.ExecuteReaderAsync();
        //            if (rdr.HasRows)
        //            {
        //                while (rdr.Read())
        //                {

        //                    if (!_context.Especialidades.Any(e => e.EspecialidadId == rdr.GetFieldValue<string>("esp_id")))
        //                    {
        //                        _context.Especialidades.Add(new Especialidad { EspecialidadId = rdr.GetFieldValue<string>("esp_id"), Nombre = rdr.GetFieldValue<string>("esp_nombre") });
        //                        _context.SaveChanges();
        //                    }

        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {

        //            throw;
        //        }
        //    }

        //    CrearPlanteles();


        //    // Checamos que los grupos por plantel existan
        //    var listaPlanteles = _context.Planteles.AsNoTracking().OrderBy(p => p.ClavePlantel).ToArray();
        //    for (int i = 0; i < listaPlanteles.Count(); i++)
        //    {
        //        var gposPlantel = (await ChecarGrupos(listaPlanteles[i].ClavePlantel, listaPlanteles[i].ClaveSIIACE)).Where(g => g.Semestre == "3" || g.Semestre == "5").ToArray();
        //        var gposContext = _context.Grupos;
        //        for (int j = 0; j < gposPlantel.Count(); j++)
        //        {
        //            if (!gposContext.Any(g => g.ClaveSIIACE == gposPlantel[j].ClaveSIIACE))
        //            {
        //                //_context.Grupos.Add(gposPlantel[j]);
        //                _context.Grupos.Add(new Grupo
        //                {
        //                    ClavePlantel = gposPlantel[j].ClavePlantel,
        //                    ClaveSIIACE = gposPlantel[j].ClaveSIIACE,
        //                    EvaluacionHabilitada = gposPlantel[j].EvaluacionHabilitada,
        //                    FechaExamen = gposPlantel[j].FechaExamen,
        //                    Nombre = gposPlantel[j].Nombre,
        //                    Semestre = gposPlantel[j].Semestre,
        //                    Turno = gposPlantel[j].Turno
        //                });
        //            }
        //        }
        //    }
        //    _context.SaveChanges();

        //    // Checamos que los alumnos de los grupos Existan
        //    var gpos = _context.Grupos.OrderBy(g => g.ClavePlantel).ThenBy(g => g.GrupoId).ToArray();
        //    for (int i = 0; i < gpos.Count(); i++)
        //    {
        //        Aspirante[] alumnosGpo = await AlumnosGrupo(gpos[i].ClaveSIIACE);
        //        for (int j = 0; j < alumnosGpo.Count(); j++)
        //        {
        //            if (!await _userManager.Users.AnyAsync(u => u.UserName == alumnosGpo[j].UserName))
        //            {
        //                alumnosGpo[j].GrupoId = gpos[i].GrupoId;
        //                //var res = await _userManager.CreateAsync(alumnosGpo[j], alumnosGpo[j].PlainPass);

        //                var res = await _userManager.CreateAsync(
        //                    new Aspirante
        //                    {
        //                        GrupoId = gpos[i].GrupoId,
        //                        Ficha = alumnosGpo[j].Ficha,
        //                        UserName = alumnosGpo[j].UserName,
        //                        Email = alumnosGpo[j].Email,
        //                        EmailConfirmed = true,
        //                        Edad = alumnosGpo[j].Edad,
        //                        Estatus = alumnosGpo[j].Estatus,
        //                        EspecialidadId = alumnosGpo[j].EspecialidadId,
        //                        DescripcionOtraSecundaria = alumnosGpo[j].DescripcionOtraSecundaria,
        //                        Genero = alumnosGpo[j].Genero,
        //                        Paterno = alumnosGpo[j].Paterno,
        //                        Materno = alumnosGpo[j].Materno,
        //                        Nombre = alumnosGpo[j].Nombre,
        //                        NombreSecundaria = alumnosGpo[j].NombreSecundaria,
        //                        PromedioSecundaria = alumnosGpo[j].PromedioSecundaria,
        //                        PlainPass = alumnosGpo[j].PlainPass,
        //                        TipoSecundaria = alumnosGpo[j].TipoSecundaria,
        //                        TipoSostenimientoSecundaria = alumnosGpo[j].TipoSostenimientoSecundaria
        //                    }

        //                    , alumnosGpo[j].PlainPass);
        //            }
        //            var aspirante = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == alumnosGpo[j].Ficha);
        //            if (aspirante != null)
        //            {
        //                var resAddRol = await _userManager.AddToRoleAsync(aspirante, "Aspirante");
        //            }

        //        }
        //    }

        //}

        private void CrearPlanteles()
        {
            var planteles = new List<Plantel>()
            {
                new Plantel {ClavePlantel="C02",Nombre="CECYTECH NO. 02 LA JUNTA",ClaveCentroTrabajo="08ETC0001Y",ClaveSIIACE="01"},
                new Plantel {ClavePlantel="C03",Nombre="CECYTECH NO. 03 VALLE DE ALLENDE",ClaveCentroTrabajo="08ETC0002X",ClaveSIIACE="02"},
                new Plantel {ClavePlantel="C04",Nombre="CECYTECH NO. 04 GUADALUPE Y CALVO",ClaveCentroTrabajo="08ETC0003W",ClaveSIIACE="03"},
                new Plantel {ClavePlantel="C05",Nombre="CECYTECH NO. 05 SAN JUANITO",ClaveCentroTrabajo="08ETC0004V",ClaveSIIACE="04"},
                new Plantel {ClavePlantel="C06",Nombre="CECYTECH NO. 06 CHIHUAHUA",ClaveCentroTrabajo="08ETC0011E",ClaveSIIACE="05"},
                new Plantel {ClavePlantel="C07",Nombre="CECYTECH NO. 07 SAN ISIDRO",ClaveCentroTrabajo="08ETC0005U",ClaveSIIACE="06"},
                new Plantel {ClavePlantel="C08",Nombre="CECYTECH NO. 08 CUAUHTÉMOC",ClaveCentroTrabajo="08ETC0006T",ClaveSIIACE="07"},
                new Plantel {ClavePlantel="C09",Nombre="CECYTECH NO. 09 LOMAS DE POLEO",ClaveCentroTrabajo="08ETC0008R",ClaveSIIACE="08"},
                new Plantel {ClavePlantel="C10",Nombre="CECYTECH NO. 10 GÓMEZ FARÍAS",ClaveCentroTrabajo="08ETC0007S",ClaveSIIACE="09"},
                new Plantel {ClavePlantel="C11",Nombre="CECYTECH NO. 11 CIUDAD DEL CONOCIMIENTO",ClaveCentroTrabajo="08ETC0009Q",ClaveSIIACE="10"},
                new Plantel {ClavePlantel="C12",Nombre="CECYTECH NO. 12 FLORES MAGÓN",ClaveCentroTrabajo="08ETC0012D",ClaveSIIACE="11"},
                new Plantel {ClavePlantel="C13",Nombre="CECYTECH NO. 13 SAN GUILLERMO",ClaveCentroTrabajo="08ETC0013C",ClaveSIIACE="12"},
                new Plantel {ClavePlantel="C14",Nombre="CECYTECH NO. 14 VILLA ESPERANZA",ClaveCentroTrabajo="08ETC0014B",ClaveSIIACE="13"},
                new Plantel {ClavePlantel="C15",Nombre="CECYTECH NO. 15 ASCENSIÓN",ClaveCentroTrabajo="08ETC0015A",ClaveSIIACE="14"},
                new Plantel {ClavePlantel="C16",Nombre="CECYTECH NO. 16 LEBARÓN",ClaveCentroTrabajo="08ETC0016Z",ClaveSIIACE="15"},
                new Plantel {ClavePlantel="C17",Nombre="CECYTECH NO. 17 BABORIGAME",ClaveCentroTrabajo="08ETC0017Z",ClaveSIIACE="16"},
                new Plantel {ClavePlantel="C18",Nombre="CECYTECH NO. 18 VILLA ALDAMA",ClaveCentroTrabajo="08ETC0018Y",ClaveSIIACE="40"},
                new Plantel {ClavePlantel="C19",Nombre="CECYTECH NO. 19 INDUSTRIAS",ClaveCentroTrabajo="08ETC0019X",ClaveSIIACE="41"},
                new Plantel {ClavePlantel="C20",Nombre="CECYTECH NO. 20 ORIENTE",ClaveCentroTrabajo="08ETC0020M",ClaveSIIACE="42"},
                new Plantel {ClavePlantel="C21",Nombre="CECYTECH NO. 21 RIBERAS CHIHUAHUA",ClaveCentroTrabajo="08ETC0021L",ClaveSIIACE="43"},
                new Plantel {ClavePlantel="C22",Nombre="CECYTECH NO. 22 AYUNTAMIENTO",ClaveCentroTrabajo="08ETC0022K",ClaveSIIACE="44"},
                new Plantel {ClavePlantel="C23",Nombre="CECYTECH NO. 23 RIBERAS JUÁREZ",ClaveCentroTrabajo="08ETC0023J",ClaveSIIACE="45"},
                new Plantel {ClavePlantel="C24",Nombre="CECYTECH NO. 24 TORIBIO ORTEGA",ClaveCentroTrabajo="08ETC0024I",ClaveSIIACE="52"},
                new Plantel {ClavePlantel="E01",Nombre="CECYTECH EMSAD NO. 01 TURUACHI",ClaveCentroTrabajo="08EMS0001P",ClaveSIIACE="17"},
                new Plantel {ClavePlantel="E02",Nombre="CECYTECH EMSAD NO. 02 EL VERGEL",ClaveCentroTrabajo="08EMS0002O",ClaveSIIACE="18"},
                new Plantel {ClavePlantel="E03",Nombre="CECYTECH EMSAD NO. 03 ROCHEACHI",ClaveCentroTrabajo="08EMS0003N",ClaveSIIACE="19"},
                new Plantel {ClavePlantel="E04",Nombre="CECYTECH EMSAD NO. 04 CARICHÍ",ClaveCentroTrabajo="08EMS0004M",ClaveSIIACE="20"},
                new Plantel {ClavePlantel="E05",Nombre="CECYTECH EMSAD NO. 05 TOMÓCHI",ClaveCentroTrabajo="08EMS0005L",ClaveSIIACE="21"},
                new Plantel {ClavePlantel="E06",Nombre="CECYTECH EMSAD NO. 06 BENITO JUÁREZ",ClaveCentroTrabajo="08EMS0006K",ClaveSIIACE="22"},
                new Plantel {ClavePlantel="E09",Nombre="CECYTECH EMSAD NO. 09 ATASCADEROS",ClaveCentroTrabajo="08EMS0011W",ClaveSIIACE="23"},
                new Plantel {ClavePlantel="E10",Nombre="CECYTECH EMSAD NO. 10 SAMACHIQUE",ClaveCentroTrabajo="08EMS0012V",ClaveSIIACE="24"},
                new Plantel {ClavePlantel="E11",Nombre="CECYTECH EMSAD NO. 11 BAHUICHIVO",ClaveCentroTrabajo="08EMS0010X",ClaveSIIACE="25"},
                new Plantel {ClavePlantel="E12",Nombre="CECYTECH EMSAD NO. 12 SATEVÓ",ClaveCentroTrabajo="08EMS0013U",ClaveSIIACE="26"},
                new Plantel {ClavePlantel="E13",Nombre="CECYTECH EMSAD NO. 13 NAICA",ClaveCentroTrabajo="08EMS0009H",ClaveSIIACE="27"},
                new Plantel {ClavePlantel="E15",Nombre="CECYTECH EMSAD NO. 15 MESA DE SAN RAFAEL",ClaveCentroTrabajo="08EMS0019O",ClaveSIIACE="28"},
                new Plantel {ClavePlantel="E17",Nombre="CECYTECH EMSAD NO. 17 MORELOS",ClaveCentroTrabajo="08EMS0015S",ClaveSIIACE="29"},
                new Plantel {ClavePlantel="E18",Nombre="CECYTECH EMSAD NO. 18 VALLE DEL ROSARIO",ClaveCentroTrabajo="08EMS0016R",ClaveSIIACE="30"},
                new Plantel {ClavePlantel="E19",Nombre="CECYTECH EMSAD NO. 19 MORIS",ClaveCentroTrabajo="08EMS0017Q",ClaveSIIACE="31"},
                new Plantel {ClavePlantel="E20",Nombre="CECYTECH EMSAD NO. 20 SANTA ISABEL",ClaveCentroTrabajo="08EMS0020D",ClaveSIIACE="32"},
                new Plantel {ClavePlantel="E22",Nombre="CECYTECH EMSAD NO. 22 NONOAVA",ClaveCentroTrabajo="08EMS0022B",ClaveSIIACE="33"},
                new Plantel {ClavePlantel="E23",Nombre="CECYTECH EMSAD NO. 23 SAN FRANCISCO DE BORJA",ClaveCentroTrabajo="08EMS0023A",ClaveSIIACE="34"},
                new Plantel {ClavePlantel="E24",Nombre="CECYTECH EMSAD NO. 24 URIQUE",ClaveCentroTrabajo="08EMS0024Z",ClaveSIIACE="35"},
                new Plantel {ClavePlantel="E25",Nombre="CECYTECH EMSAD NO. 25 CONGREGACIÓN ORTIZ",ClaveCentroTrabajo="08EMS0025Z",ClaveSIIACE="36"},
                new Plantel {ClavePlantel="E26",Nombre="CECYTECH EMSAD NO. 26 BACHÍNIVA",ClaveCentroTrabajo="08EMS0026Y",ClaveSIIACE="37"},
                //new Plantel {ClavePlantel="E27",Nombre="EMSAD 27 TORIBIO ORTEGA",ClaveCentroTrabajo="08EMS0027X",ClaveSIIACE="38"},
                new Plantel {ClavePlantel="E28",Nombre="CECYTECH EMSAD NO. 28 IGNACIO ZARAGOZA",ClaveCentroTrabajo="08EMS0028W",ClaveSIIACE="39"},
                new Plantel {ClavePlantel="E29",Nombre="CECYTECH EMSAD NO. 29 OJO DE AGUA",ClaveCentroTrabajo="08EMS0029V",ClaveSIIACE="46"},
                new Plantel {ClavePlantel="E30",Nombre="CECYTECH EMSAD NO. 30 RIVA PALACIO",ClaveCentroTrabajo="08EMS0030K",ClaveSIIACE="47"},
                new Plantel {ClavePlantel="E31",Nombre="CECYTECH EMSAD NO. 31 TEMORIS",ClaveCentroTrabajo="08EMS0031J",ClaveSIIACE="48"},
                new Plantel {ClavePlantel="E32",Nombre="CECYTECH EMSAD NO. 32 JANOS",ClaveCentroTrabajo="08EMS0032I",ClaveSIIACE="49"},
                new Plantel {ClavePlantel="E33",Nombre="CECYTECH EMSAD NO. 33 MATACHI",ClaveCentroTrabajo="08EMS0033H",ClaveSIIACE="50"},
                new Plantel {ClavePlantel="E34",Nombre="CECYTECH EMSAD NO. 34 SAN RAFAEL",ClaveCentroTrabajo="08EMS0034G",ClaveSIIACE="51"},
                new Plantel {ClavePlantel="E35",Nombre="EMSAD 35 URUACHI",ClaveCentroTrabajo="08EMS0035F",ClaveSIIACE="53"},
            };

            // Checamos que los planteles existan
            for (int i = 0; i < planteles.Count(); i++)
            {
                if (!_context.Planteles.Any(p => p.ClaveCentroTrabajo == planteles[i].ClaveCentroTrabajo))
                {
                    _context.Planteles.Add(planteles[i]);
                    _context.SaveChanges();
                }

                #region comentado
                //Checamos que exista un usuario administrativo por plantel y que pertenezca al rol Administrativo
                //string nomUsuarioPlantel = $"cecytech{planteles[i].ClavePlantel}";
                //var usuarioPlantel = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == nomUsuarioPlantel);
                //if (usuarioPlantel == null)
                //{
                //    string pass = PasswordAleatorio();
                //    var newUserRes = await _userManager.CreateAsync(new Aspirante
                //    {
                //        Paterno = nomUsuarioPlantel,
                //        Materno = "",
                //        Nombre = "",
                //        Email = nomUsuarioPlantel,
                //        Edad = 0,
                //        EspecialidadId = null,
                //        Estatus = "",
                //        Ficha = nomUsuarioPlantel,
                //        Genero = "",
                //        GrupoId = null,
                //        DescripcionOtraSecundaria = "",
                //        NombreSecundaria = "",
                //        PlainPass = pass,
                //        PromedioSecundaria = 0,
                //        TipoSecundaria = "",
                //        TipoSostenimientoSecundaria = "",
                //        UserName = nomUsuarioPlantel
                //    }, pass);
                //}

                //usuarioPlantel = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == nomUsuarioPlantel);
                //if (usuarioPlantel != null)
                //{
                //    var resAddRol = await _userManager.AddToRoleAsync(usuarioPlantel, "Administrativo");

                //    //Checamos que el usuario tenga permisos en el plantel

                //    if (!await _context.UsuariosPlantel.AsNoTracking().Include(u => u.Aspirante).AnyAsync(u => u.Aspirante.UserName == nomUsuarioPlantel))
                //    {
                //        _context.UsuariosPlantel.Add(new UsuarioPlantel
                //        {
                //            ClavePlantel = planteles[i].ClavePlantel,
                //            Id = usuarioPlantel.Id
                //        });
                //        _context.SaveChanges();
                //    }
                //}

                #endregion
            }

        }

//        private async Task<Aspirante[]> AlumnosGrupo(string claveSIIACE)
//        {
//            var listaPlanteles = await _context.Planteles.ToListAsync();
//            List<Aspirante> lista = new List<Aspirante>();
//            using (SqlConnection conSIIACE = new SqlConnection(_configuration.GetConnectionString("SIIACE")))
//            {
//                await conSIIACE.OpenAsync();
//                SqlCommand cmdAlumnos = new SqlCommand(
//@"SELECT 
//alumno_no_control,
//alumno_apell_pat, 
//alumno_apell_mat,
//alumno_nombre,
//alumno_sexo,
//alumno_estatus,
//esp_id,
//gpo_id,
//gpo_sem,
//plantel_cve
//from alumno 
//where
//gpo_id = @gpo_id
//and alumno_estatus not in ('T','D')
//order by alumno_no_control"
//                    , conSIIACE);
//                cmdAlumnos.Parameters.AddWithValue("gpo_id", claveSIIACE);
//                try
//                {
//                    SqlDataReader rdr = await cmdAlumnos.ExecuteReaderAsync();
//                    if (rdr.HasRows)
//                    {
//                        while (rdr.Read())
//                        {
//                            lista.Add(
//                                new Aspirante
//                                {
//                                    Ficha = rdr.GetFieldValue<string>("alumno_no_control"),
//                                    UserName = rdr.GetFieldValue<string>("alumno_no_control"),
//                                    NormalizedUserName = rdr.GetFieldValue<string>("alumno_no_control"),
//                                    Email = $"{rdr.GetFieldValue<string>("alumno_no_control")}@{listaPlanteles.FirstOrDefault(p => p.ClaveSIIACE == rdr.GetFieldValue<string>("plantel_cve")).ClavePlantel}.cecytechihuahua.edu.mx",
//                                    Edad = 0,
//                                    Estatus = rdr.GetFieldValue<string>("alumno_estatus"),
//                                    EspecialidadId = rdr.GetFieldValue<string>("esp_id"),
//                                    DescripcionOtraSecundaria = "",
//                                    Genero = rdr.GetFieldValue<string>("alumno_sexo"),
//                                    Paterno = rdr.GetFieldValue<string>("alumno_apell_pat"),
//                                    Materno = rdr.GetFieldValue<string>("alumno_apell_mat"),
//                                    Nombre = rdr.GetFieldValue<string>("alumno_nombre"),
//                                    NombreSecundaria = "",
//                                    PromedioSecundaria = 0,
//                                    PlainPass = PasswordAleatorio(),
//                                    TipoSecundaria = "",
//                                    TipoSostenimientoSecundaria = ""
//                                }
//                                );
//                        }
//                    }
//                }
//                catch (Exception e)
//                {

//                    throw;
//                }
//            }
//            return lista.ToArray();
//        }

        public string PasswordAleatorio(int longitud = 8, string caracteres = "ABCDEFGHKMNPRSTUVWXYZ0123456789abcdefghkmnprstuvwxyz")
        {
            Random aleatorio = new Random((int)DateTime.Now.Ticks);
            string pass = "";
            for (int i = 0; i < longitud; i++)
            {
                pass += caracteres[aleatorio.Next(caracteres.Length - 1)];
            }
            return pass;
        }

        //private async Task<IEnumerable<Grupo>> ChecarGrupos(string clavePlantel, string claveSIIACE, string gpo_ciclo = "2018-1", int gpo_ins = 1, string gpo_estatus = "A")
        //{
        //    var gpos = new List<Grupo>();
        //    using (SqlConnection conSIIACE = new SqlConnection(_configuration.GetConnectionString("SIIACE")))
        //    {
        //        SqlCommand cmdGrupos = new SqlCommand("SELECT gpo_id,gpo_nom,plant_cve,gpo_sem,gpo_ciclo,gpo_ins,gpo_estatus,coalesce(gpo_turno,'') as gpo_turno from grupo where gpo_ciclo=@gpo_ciclo and gpo_ins = @gpo_ins and gpo_estatus = @gpo_estatus and plant_cve = @plant_cve", conSIIACE);
        //        cmdGrupos.Parameters.AddWithValue("plant_cve", claveSIIACE);
        //        cmdGrupos.Parameters.AddWithValue("gpo_ciclo", gpo_ciclo);
        //        cmdGrupos.Parameters.AddWithValue("gpo_ins", gpo_ins);
        //        cmdGrupos.Parameters.AddWithValue("gpo_estatus", gpo_estatus);
        //        await conSIIACE.OpenAsync();
        //        SqlDataReader rdr = await cmdGrupos.ExecuteReaderAsync();
        //        if (rdr.HasRows)
        //        {
        //            while (await rdr.ReadAsync())
        //            {
        //                gpos.Add(new Grupo
        //                {
        //                    ClavePlantel = clavePlantel,
        //                    ClaveSIIACE = rdr.GetFieldValue<string>("gpo_id"),
        //                    Nombre = $"{rdr.GetFieldValue<string>("gpo_turno")}-{rdr.GetFieldValue<string>("gpo_sem")}{rdr.GetFieldValue<string>("gpo_nom")}",
        //                    Turno = rdr.GetFieldValue<string>("gpo_turno"),
        //                    Semestre = rdr.GetFieldValue<string>("gpo_sem"),
        //                    FechaExamen = new DateTime(2018, 11, 20),
        //                    EvaluacionHabilitada = false
        //                });
        //            }
        //        }
        //    }
        //    return gpos;
        //}


        public async Task SeedExamen()
        {

            await BorrarExamen();
            #region COMPETENCIA MATEMATICA
            Competencia matematica = new Competencia
            {
                Nombre = "MATEMATICA",
                LecturaPrevia = null,
                TiempoParaResolver = 60,
                Preguntas = new List<Pregunta>
                {
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 1,
                        Texto = @"
                        <p>Luis cuenta con $100.00, gasta $25.00 en pasaje para llegar a su escuela, y cobra una deuda de $45.00, de la cual sólo recibe una tercera parte. Si divide el dinero restante en dos partes iguales, una parte para sus pasajes y otra para sus alimentos, y si se considera que gastó completamente la cantidad destinada para alimentos y el pasaje de regreso a su casa, ¿cuánto dinero le quedó al final del día?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="$5.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="$20.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="$35.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="$65.00"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 2,
                        Texto = @"<p>José Luis realiza su servicio social en el zoológico de su entidad, entre sus actividades está alimentar a un mamífero en peligro de extinción. La indicación es de darle 5.5kg diarios de carne. En un día le ha dado dos raciones, una de 134 kg y la otra de 212 kg. ¿Cuál debe ser la cantidad de la tercera ración, para que el mamífero cubra sus requerimientos alimenticios del día?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="1 <sup>1</sup>&frasl;<sub>4</sub> kg"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="1 <sup>4</sup>&frasl;<sub>6</sub> kg"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="1 <sup>3</sup>&frasl;<sub>4</sub> kg"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="1 <sup>5</sup>&frasl;<sub>6</sub>"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 3,
                        Texto = "<p>La escuela Benito Juárez participará en el desfile del 20 de noviembre en conmemoración de la Revolución Mexicana, en el cual sólo participarán 34 partes de los estudiantes. Si en el plantel hay 1000 alumnos, ¿cuántos participarán en el desfile?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="250"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="300"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="400"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="750"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 4,
                        Texto = "<p>En un paseo dominical, la familia Sánchez gasta $13.00 en transporte de cada uno de sus cinco miembros, $35.00 de cada entrada al cine y $140.00 en golosinas; si disponían de $500.00, ¿cuánto les sobró para ahorrar y utilizarlo en su próximo paseo?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                                ClaveCOSDAC ="A",
                                Texto="$120.00"},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="B",
                                Texto="$172.00"},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="C",
                                Texto="$260.00"},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="D",
                                Texto="$380.00"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 5,
                        Texto = "<p>Juan y Pedro son jugadores del equipo de futbol “Real Aldama”, que participa en la liga “Costeros del golfo”, cada uno anotó 3&frasl;12 y 2&frasl;4 respectivamente del total de los goles del equipo. Si al término de la temporada el equipo anotó 40 goles, ¿cuántos goles anotaron los demás jugadores?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="10"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="16"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="20"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="30"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 6,
                        Texto = "En la primera hora de clase, Adriana se entera de que el hijo del director de la escuela va a contraer matrimonio, en la siguiente hora se lo comunica a tres compañeros de la escuela y éstos, a su vez, lo comunican cada uno a otros tres compañeros por hora y así sucesivamente. El número de personas que se enteran del rumor se puede expresar como la siguiente sucesión: 1, 3, 9,... Si el horario de clases es de 5 horas, ¿cuántas personas, en total, conocen el rumor?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="119"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="120"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="121"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="122"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 7,
                        Texto =
                        @"
                        <p>Adrián desea cercar un terreno para la crianza de ganado para ello, contrata a Pedro, quien le cobrará $20.00 por cada poste que utilice, el cual le muestra la siguiente serie de diseños:</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta07.png"" />
                        <p>Si Adrián elige el diseño 6, ¿cuántos postes se necesitarán y cuánto cobrará Pedro?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="20 y $240.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="20 y $480.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="24 y $240.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="24 y $480.00"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 8,
                        Texto = "<p>Johnny deposita diariamente la misma cantidad de dinero en una alcancía, porque quiere comprar un videojuego que cuesta $1,170.00. Si en el cuarto día ha ahorrado $180.00 ¿a los cuantos días de ahorro podrá comprar el videojuego?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="9"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="14"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="20"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="26"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 9,
                        Texto = "<p>Don Enrique decidió donar 400 m<sup>2</sup> de terreno para la construcción de una escuela, con la condición de que el espacio sea de forma rectangular, con un largo de 4 metros mayor que el ancho; y así poder tener un acceso privado de 4 metros de ancho a su propiedad. ¿Cuál de las siguientes ecuaciones permite expresar las dimensiones del terreno que donará?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="x(x+4)= 400"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="2x(2x)= 400"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="x+4= 400"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="(x-4)(x+4)= 40"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 10,
                        Texto = "<p>Para comprar un auto decides gastar tres cuartas partes de tu sueldo mensual y el resto ahorrar. ¿Cuál de las siguientes expresiones representa tu ahorro de cuatro meses?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="S-<sup>1</sup>&frasl;<sub>4</sub>"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="(S-<sup>1</sup>&frasl;<sub>4</sub>S)"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="4S-<sup>3</sup>&frasl;<sub>4</sub>"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="4(S-<sup>3</sup>&frasl;<sub>4</sub>S)"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 11,
                        Texto = "<p>Juan es chofer de autobuses de pasajeros, y debe realizar la corrida de Xalapa, Veracruz a Guadalajara, Jalisco. Hace una escala en la ciudad de Puebla, que se encuentra a una distancia de 150 km.</p><p>Al llegar observa que el autobús consumió 15 litros de diesel. ¿Cuánto diesel consumirá, si la distancia de Puebla a Guadalajara es de 600 km?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="40 litros"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="50 litros"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="60 litros"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="75 litros"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 12,
                        Texto = "<p>Una granja avícola tiene 41,600 gallinas destinadas a la producción de huevo, su dueño destina 7.48 kg de alimento para cubrir las necesidades de las aves por 9 días. Si decide comprar 12,500 gallinas más, ¿para cuántos días le alcanzará el alimento que tenía destinado para 9 días?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="2.7 d&iacute;as"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="6.9 d&iacute;as"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="11.7 d&iacute;as"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="29.9 d&iacute;as"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 13,
                        Texto = "<p>El papá de Javier compró diez motos para iniciar su negocio de pizzas. El primer mes pagó $37,200.00 por tres motos; los dos meses siguientes compró la misma cantidad y, en la compra de la décima moto le hicieron el 25% de descuento, ¿cuánto pagó por las diez motos?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="$46,500.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="$93,000.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="$120,900.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="$124,125.00"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 14,
                        Texto = "<p>Mariana guarda dinero en la alcancía, para pagar su deuda de $3,000.00. Si esta semana va a guardar $250.00, que es la octava parte del dinero que contiene, ¿cuánto le falta para completar el pago de la deuda?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="$750.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="$1,250.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="$2,000.00"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="$2,250.00"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 15,
                        Texto = "<p>Para reproducir un material que contiene algunas hojas de color, la maestra de inglés de los niños de la primaria “Patria y libertad” fue a un centro de fotocopiado, en donde por 16 copias en blanco y negro y 7 de color le cobraron 22 pesos; si las copias en blanco y negro cuestan 50 centavos, ¿cuál es el costo de una copia de color?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="2 pesos"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="3 pesos"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="4 pesos"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="5 pesos"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 16,
                        Texto = "<p>Delia quiere recortar imágenes de paisajes para pegarlas en una de las caras de una caja de cerillos. Las caras de la caja son rectangulares, y la superficie de la cara donde será pegada la imagen es de 24 cm2. El lado más largo es 2 cm mayor que su ancho. ¿Cuánto mide el lado más largo de la imagen recortada?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="4 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="6 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="8 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="10 cm"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 17,
                        Texto = "<p>En la clase de biología, al profesor le gusta que sus alumnos hagan cálculos mentales, por lo que les dice lo siguiente: “En el microscopio veo dos clases de microorganismos; si los sumo, son 37, pero si los multiplico son 300. ¿Cuántos microorganismos hay de cada clase?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="20 y 17"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="25 y 12"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="24 y 13"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="30 y 7"},
                        }
                    },
                new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 18,
                        Texto = @"
                        <p>El tanque que se utiliza para almacenar agua en tu comunidad mide 12 m de altura.</p>
                        <img src=""/evaluacion/imagenprivada/pregunta18.png""
                        <p>¿Cuál será la capacidad de almacenamiento en litros?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="108,000 litros"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="113,040 litros"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="226,080 litros"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="339,120 litros"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 19,
                        Texto = @"
                        <p>En la escuela se quiere colocar tabique para cubrir un camino que tiene forma rectangular, con medidas de 45 m de largo por 2.6 m de ancho. Si cada tabique mide 13 cm de ancho por 30 cm de largo.</p>
                        <img src=""/evaluacion/imagenprivada/pregunta19.png"">
                        <p>¿Cuántos tabiques se necesitan para cubrir el camino?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="3,000"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="3,010"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="3,020"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="3,150"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 20,
                        Texto = "<p>Jorge tiene un terreno que decide convertir en corral para la crianza de gallinas. El corral es de forma rectangular y se dispone de 94 m de malla.</p><p>Si desea que el ancho del corral sea de 12 m. ¿Cuál es el área del terreno que se utilizará para el corral de las gallinas?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="35 m<sup>2</sup>"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="70 m<sup>2</sup>"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="420 m<sup>2</sup>"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="840 m<sup>2</sup>"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 21,
                        Texto = "<p>Don Juan quiere elaborar un cilindro de cartón que tenga una capacidad exacta de un litro y 10 cm de diámetro para disolver colorante de pintura. ¿Cuál debe ser la altura del recipiente? (Recuerda que 1000 cm<sup>3</sup> es igual a 1 litro).</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="63.694 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="31.847 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="12.738 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="3.184 cm"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 22,
                        Texto = @"
                        <p>Lorena elabora abanicos para venderlos; para ello, recorta círculos de 10 cm de radio en cuatro partes iguales, y los decora colocando un listón en todo su borde, como se indica en la figura.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta22.png"" />
                        <p>¿Qué cantidad de listón ocupará para decorar cada abanico?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="15.70 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="20.17 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="27.85 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="35.70 cm"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 23,
                        Texto = @"
                        <p>En los juegos olímpicos de Moscú de 1980 se elaboró una escultura de los aros, la cual fue iluminada por focos incandescentes de 50 watts, colocados a una separación de 20 cm.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta23.png"" />
                        <p>Si el diámetro es de 10 m. ¿Cuántos focos se requirieron para la iluminación total de los cinco aros?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="250"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="393"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="785"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="1963"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 24,
                        Texto = "<p>Pedro elabora lámparas artesanales con tubos de PVC y una tapa de acrílico en la parte superior. Si le hacen un pedido de 7 lámparas con tubos de 32 cm de diámetro exterior, ¿qué cantidad de acrílico necesita para realizar el trabajo solicitado?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="351.68 cm<sup>2</sup>"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="1,792.00 cm<sup>2</sup>"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="5,626.88 cm<sup>2</sup>"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="22,507.52 cm<sup>2</sup>"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 25,
                        Texto = "<p>Emiliano debe ejercitar a un caballo en el corral, sujeto a una reata de 8 m de longitud, haciéndolo correr en círculos. ¿Cuántas vueltas tiene que dar el caballo para completar 2 km en un día?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="25"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="40"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="50"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="80"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 26,
                        Texto = @"
                        <p>En la comunidad “La Laguna” se coloca una antena para recibir señales de comunicación, debido a los fuertes vientos que se presentaron en la región, uno de los cables tensores se rompió, por lo que se deberá cambiar, como se muestra a continuación:</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta26.png"" />
                        <p>¿De qué tamaño debe ser el cable para reparar la antena?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="24 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="40 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="45 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="120 m"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 27,
                        Texto = "<p>En el proceso de reemplacamiento en el Estado de México, se diseñaron placas rectangulares para autos y motos. Las dimensiones de las placas para autos miden de 15 cm × 27 cm y de las motocicletas se propuso un largo de 15 cm, ¿cuál es el ancho de esta placa?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="6.6 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="8.33 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="10.0 cm"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="11.25 cm"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 28,
                        Texto = @"
                        <p>Juan trabaja en una carpintería. Le llevaron a reparar una mesa cuadrada que tiene una abertura justo en su diagonal. Para repararla decide colocarle un soporte de madera.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta28.png"" />
                        <p>¿Cuánto debe medir el soporte si el área de la mesa es de 9 m<sup>2</sup>?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="2.44 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="4.24 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="4.50 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="12.73 m"},

                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 29,
                        Texto = @"
                        <p>En el terremoto de septiembre de 2017, muchas de las paredes de las escuelas quedaron fracturadas, por lo cual deben reforzarse para prevenir accidentes, con una estructura en diagonal y marco de acero, como se muestra en la siguiente figura:</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta29.png"" />
                        <p>¿Cuántos metros de viga de acero se utilizarán para cada pared?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="6.10 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="12.20 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="24.00 m"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="24.20 m"},
                        }
                    },
                    new Pregunta
                    {
                        LecturaPrevia = null,
                        NumeroPregunta = 30,
                        Texto = @"
                        <p>En un hotel se necesita construir una rampa para el acceso a personas con capacidades diferentes. Los arquitectos determinan que lo más conveniente es que forme un ángulo de 8° y tenga una altura de 90 cm.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta30.png"" />
                        <p>¿A qué distancia de la entrada del edificio se debe iniciar la construcción?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                        ClaveCOSDAC ="A",
                        Texto="6.40 m"},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="B",
                        Texto="6.47 m"},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="C",
                        Texto="9.08 m"},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="D",
                        Texto="12.64 m"},

                        }
                    },

                }
            };
            _context.Competencias.Add(matematica);
            _context.SaveChanges();
            #endregion

            #region COMPETENCIA LECTORA
            Competencia lectora = new Competencia
            {
                Nombre = "LECTORA",
                LecturaPrevia = @"
<h3>Lee con atención el texto y responde las siguientes preguntas:</h3>
<h3><strong>RACISMO DISCRIMINACIÓN QUE PERSISTE</strong></h3>
<ul style=""list-style-type: none;"">
<li>
(1) Múltiples evidencias sociales, históricas, genéticas y antropológicas demuestran que es erróneo clasificar y jerarquizar a los seres humanos en términos de razas. ¿Por qué sigue tan arraigada esta tendencia?
</li>
<li>
(2) Aunque México es una nación pluricultural surgida del mestizaje y la mayoría de  su población —65%— considera que posee un tono de piel oscuro, en la práctica persisten abierta  o veladamente actitudes de rechazo y discriminación basadas en  prejuicios racistas.
</li>
<li>
(3) Un  estudio  de  la  empresa  estadounidense  de  medios  de  comunicación  BuzzFeed difundido a finales de 2016 refleja parte de esta tendencia: en algunas de las principales revistas que se producen y circulan en el país la presencia de personas de tez blanca resulta abrumadora, mientras que las de piel morena rara vez aparecen en sus páginas.
</li>
<li>
(4) Tras analizar el contenido editorial y los anuncios de 15 publicaciones seleccionadas, BuzzFeed encontró que en el mejor de los casos los individuos con  piel oscura ahí representados no rebasan el 20%. En ningún caso éstos figuraron en una foto de portada y cuando aparecieron en los espacios interiores fue en alusión a temas de filantropía o viajes.
</li>
<li>
(5) Aunque en los discursos se niegue o condene, el monstruo del racismo sigue mostrando sus múltiples caras y a menudo es un factor para jerarquizar a los individuos sobre el supuesto de que las diferencias anatómicas y de color son determinantes de la naturaleza humana.
</li>
</ul>
<h3>
<strong>Condenar lo diferente</strong>
</h3>
<ul style=""list-style-type: none;"">
<li>
(6) No hay certeza absoluta sobre los orígenes temporales y territoriales del racismo, aunque hay cierto consenso entre los expertos sobre la necesidad de distinguir las  prácticas sociales  de  rechazo  a  lo  diferente  de  las  ideologías  y  teorías  que  han  intentado sustentarlo. El antropólogo físico Víctor Acuña Alonzo señala que la  discriminación a aquellas personas que tienen un aspecto diferente al propio tiene  raíces milenarias, mientras el racismo como ideología para justificar la dominación sobre otros grupos está muy ligado al nacimiento del Estado-nación moderno a partir del siglo XVI. En este último caso, dice el profesor e investigador de la Escuela Nacional de Antropología e Historia (ENAH), algunos Estados-nación construyeron un discurso sobre la inferioridad de otros grupos humanos con la intención de darle a ésta un carácter “natural” y así justificarla. (…)
</li>
</ul>
<p>
Cárdenas, G. (2017, Junio). Racismo discriminación que persiste. ¿Cómo ves? Revista de divulgación de la ciencia de la UNAM,
<br/>
No. 223. Recuperado de: http://www.comoves.unam.mx/numeros/articulo/223/racismo-discriminacion-que-persiste
</p>                ",
                TiempoParaResolver = 80,
                Preguntas = new List<Pregunta>
                {
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 31,
                        Texto = "<p>El texto “Racismo discriminación que persiste”, podemos considerarlo como un texto:</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Periodístico"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Informativo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Expositivo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Científico"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 32,
                        Texto = "<p>¿Cuál es la hipótesis que plantea el autor en el texto “Racismo discriminación que persiste”?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="El racismo está ligado al nacimiento del Estado-nación moderno, a partir del siglo XVI."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Las diferencias anatómicas y de color de piel de los seres humanos son factores discriminatorios."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="En México persisten actitudes de rechazo y discriminación basadas en prejuicios racistas."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Las prácticas sociales de rechazo se basan en las diferencias ideológicas sustentadas en los prejuicios."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 33,
                        Texto = "<p>Selecciona la opción que incluya la idea principal de los párrafos 1 y 2.</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="La tendencia a clasificar y jerarquizar por razas, persiste en México por ser una nación pluricultural surgida del mestizaje."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Está demostrado que no se puede clasificar a las personas por su raza, pero en México sigue arraigada esta tendencia porque persisten prejuicios racistas."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Evidencias sociales, históricas, genéticas y antropológicas demuestran que no se puede clasificar a los seres humanos por su raza."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Los seres humanos se clasifican en términos genéticos y antropológicos, y se producen prejuicios racistas en la mayoría de la población."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 34,
                        Texto = "<p>¿Por qué la información incluida en el tercer párrafo es una idea secundaria?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Es un ejemplo que permite comprender el planteamiento del autor."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Es la idea básica que permite comprender el planteamiento del autor."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Es un resumen de las ideas expresadas por el autor."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Es una reiteración de las ideas expresadas por autor."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 35,
                        Texto = "<p>¿Cuál es la idea principal del párrafo 5?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="El racismo sigue existiendo, aunque en el discurso se niegue o condene."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="El racismo es un monstruo que determina la naturaleza humana."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="El racismo tiene múltiples caras determinantes para jerarquizar a individuos."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="El racismo persiste y es un factor para jerarquizar a los seres humanos."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 36,
                        Texto = "<p>¿Por qué el párrafo 6 se cataloga como idea secundaria?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                        ClaveCOSDAC ="A",
                        Texto="Es la idea que expresa lo que el autor quiere trasmitir."},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="B",
                        Texto="Es una afirmación que sustenta la idea principal."},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="C",
                        Texto="Es información básica del contenido de la idea principal."},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="D",
                        Texto="Sin ella, el significado de la idea principal no es claro y preciso."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 37,
                        Texto = "<p>¿Qué tipo de estructura textual presenta la lectura “Racismo discriminación que persiste”?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Narrativa"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Informativa"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Argumentativa"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Descriptiva"},
                        }
                    },
                    new Pregunta {
                        LecturaPrevia = @"
<h3>Lee con detenimiento el siguiente texto y responde la pregunta</h3>
<h3><strong>SISTEMA ÓSEO, OSTEOLOGÍA</strong></h3>
<p>
La osteología es la rama de la anatomía que estudia los huesos, que son los órganos blanquecinos y duros que forman el esqueleto.
</p>
<p>
Los huesos están constituidos por materia inorgánica y materia orgánica. La materia inorgánica (aproximadamente 67%) está compuesta básicamente por fosfato, carbonato y fluoruro de calcio, fosfato de magnesio y cloruro de sodio.
</p>
<p>
La materia orgánica (aproximadamente 33%) está compuesta por células, vasos sanguíneos y una sustancia intercelular, principalmente colágena que, a diferencia de la del cartílago, puede impregnarse por completo de sales de calcio sin que las células mueran al endurecerse dicha sustancia.
</p>
<p>
El hueso es de dos tipos: compacto o esponjoso; está cubierto por una membrana, el periostio (excepto en los extremos, que están cubiertos por cartílago), y algunos (los huesos largos) tienen otra membrana llamada endostio; contienen muchos vasos sanguíneos y también vasos linfáticos y nervios (…).
</p>
<p class=""text-right"">
Higashida, B. (2008). Ciencias de la salud. Ciudad de México: McGraw-Hill.
</p>
                        ",
                        NumeroPregunta = 38,
                        Texto = "<p>¿Qué modo de expresión predomina en la lectura “Sistema óseo, osteología”?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Descripción"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Argumentación"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Ejemplificación"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Narración"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 39,
                        Texto = "<p>Un texto es coherente cuando el tema central y todas las ideas principales y secundarias tienen relación. ¿Cuál de las siguientes ideas cumple con esta propiedad?</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Con frecuencia Sevilla huele a azahares. Se puede comprobar con su famosa catedral. Es la más grande del mundo. En la catedral, presumiblemente, descansa el descubridor de América. España conserva la mezquita del Patio de los Naranjos y la Giralda. La Giralda con 93 metros de altura, hace las veces de mirador."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Soy muy feliz por tener un amigo tan especial como tú. Los seres humanos somos por naturaleza sociables. Tú me aceptas como soy, me animas cuando lo necesito, compartes mis tristezas y haces que las alegrías duren mucho más. ¿Acaso puedo pedir algo más a la vida?"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="La contaminación, uno de los problemas más importantes que afectan a nuestro mundo, surge cuando se produce un desequilibrio como resultado de la adición de cualquier sustancia al medio ambiente, en cantidad tal, que cause efectos adversos a los seres vivos."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Investigaciones muestran una relación entre un estado mental optimista y signos físicos de buena salud. También afecta a su salud física. La salud emocional es una parte importante de la vida, permite desarrollar todo su potencial, y puede trabajar de forma productiva y hacer frente a las tensiones de la vida cotidiana."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 40,
                        Texto = "Selecciona el texto que cumple con la propiedad de coherencia.",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="La sangre es un tejido líquido que está compuesto por agua y algunas sustancias como las sales minerales, mismas que forman el plasma sanguíneo. Circula por todo nuestro cuerpo a través del sistema circulatorio, que está formado por el corazón y los vasos sanguíneos."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="La flotante melena negra de los leones de montaña, forma parte de todo un sistema bélico. El león pertenece a la familia de los félidos, dentro del orden de animales carnívoros. Su periodo de gestación es de unos 100 días. Existen casos en que los machos que llegan a una manada mantienen a cachorros que no son suyos."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Los glóbulos blancos contribuyen a proteger al organismo de las infecciones. Los glóbulos rojos transportan el oxígeno desde los pulmones hasta las células del cuerpo. Los glóbulos blancos combaten y destruyen los microbios del organismo."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Los cálculos biliares se forman cuando hay sustancias en la bilis que se endurecen. La vesícula biliar es un órgano con forma de pera, que se ubicada bajo el hígado. En forma menos común, se puede desarrollar cáncer en la vesícula. Afortunadamente, la vesícula biliar no es un órgano imprescindible para la vida. La bilis tiene otras vías para llegar al intestino delgado."},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 41,
                        Texto = @"
                        <p>¿Qué propiedades de los textos cumple el siguiente párrafo?</p>
                        <p>
                        En nuestra vida privada, aprobamos o desaprobamos conductas propias y ajenas, en conversaciones familiares, con la pareja, amigos o colegas. Lo mismo sucede en el terreno de lo público; por ejemplo, en periódicos, televisión, radio y otros medios de comunicación masiva. Hasta en nuestra soledad reflexionamos sobre cosas como: si ofendimos a alguien, cómo lo reparamos, qué decir o, por el contrario, si alguien nos ofendió, qué debemos hacer.
                        </p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Corrección y cohesión"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Cohesión y coherencia"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Coherencia y corrección"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Adecuación y corrección"},

                        }
                    },
                    new Pregunta { LecturaPrevia =
                        @"
                        <h3>Lee con atención el texto y responde las siguientes preguntas:</h3>
                        <h3><strong>RACISMO DISCRIMINACIÓN QUE PERSISTE</strong></h3>
                        <ul style=""list-style-type: none;"">
                        <li>
                        (1) Múltiples evidencias sociales, históricas, genéticas y antropológicas demuestran que es erróneo clasificar y jerarquizar a los seres humanos en términos de razas. ¿Por qué sigue tan arraigada esta tendencia?
                        </li>
                        <li>
                        (2) Aunque México es una nación pluricultural surgida del mestizaje y la mayoría de  su población —65%— considera que posee un tono de piel oscuro, en la práctica persisten abierta  o veladamente actitudes de rechazo y discriminación basadas en  prejuicios racistas.
                        </li>
                        <li>
                        (3) Un  estudio  de  la  empresa  estadounidense  de  medios  de  comunicación  BuzzFeed difundido a finales de 2016 refleja parte de esta tendencia: en algunas de las principales revistas que se producen y circulan en el país la presencia de personas de tez blanca resulta abrumadora, mientras que las de piel morena rara vez aparecen en sus páginas.
                        </li>
                        <li>
                        (4) Tras analizar el contenido editorial y los anuncios de 15 publicaciones seleccionadas, BuzzFeed encontró que en el mejor de los casos los individuos con  piel oscura ahí representados no rebasan el 20%. En ningún caso éstos figuraron en una foto de portada y cuando aparecieron en los espacios interiores fue en alusión a temas de filantropía o viajes.
                        </li>
                        <li>
                        (5) Aunque en los discursos se niegue o condene, el monstruo del racismo sigue mostrando sus múltiples caras y a menudo es un factor para jerarquizar a los individuos sobre el supuesto de que las diferencias anatómicas y de color son determinantes de la naturaleza humana.
                        </li>
                        </ul>
                        <h3>
                        <strong>Condenar lo diferente</strong>
                        </h3>
                        <ul style=""list-style-type: none;"">
                        <li>
                        (6) No hay certeza absoluta sobre los orígenes temporales y territoriales del racismo, aunque hay cierto consenso entre los expertos sobre la necesidad de distinguir las  prácticas sociales  de  rechazo  a  lo  diferente  de  las  ideologías  y  teorías  que  han  intentado sustentarlo. El antropólogo físico Víctor Acuña Alonzo señala que la  discriminación a aquellas personas que tienen un aspecto diferente al propio tiene  raíces milenarias, mientras el racismo como ideología para justificar la dominación sobre otros grupos está muy ligado al nacimiento del Estado-nación moderno a partir del siglo XVI. En este último caso, dice el profesor e investigador de la Escuela Nacional de Antropología e Historia (ENAH), algunos Estados-nación construyeron un discurso sobre la inferioridad de otros grupos humanos con la intención de darle a ésta un carácter “natural” y así justificarla. (…)
                        </li>
                        </ul>
                        <p>
                        Cárdenas, G. (2017, Junio). Racismo discriminación que persiste. ¿Cómo ves? Revista de divulgación de la ciencia de la UNAM,
                        No. 223. Recuperado de: http://www.comoves.unam.mx/numeros/articulo/223/racismo-discriminacion-que-persiste
                        </p>
                        "
                    , NumeroPregunta = 42,
                        Texto = "¿Qué tipo de texto es?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Argumentativo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Literario"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Informativo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Expositivo"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null,
                        NumeroPregunta = 43,
                        Texto = "¿Por qué las personas que desarrollan SIDA padecen múltiples infecciones?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Porque no reciben el tratamiento adecuado."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Porque desarrollan tipos de cáncer poco común."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Porque el sistema inmunitario se debilita."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Porque es un mal causado por microorganismos."},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 44,
                        Texto = "De acuerdo con el contenido del texto, ¿cuáles actividades podrían provocar contraer el virus de inmunodeficiencia humana?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Trasplantes y consumo de drogas."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Uso compartido de jeringas y contacto corporal."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Consumo de drogas y durante el parto."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Fluidos corporales y uso compartido de jeringas."},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 45,
                        Texto = "¿Por qué se le denomina síndrome a la enfermedad llamada SIDA?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Porque es sinónimo de enfermedad."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Porque es una alteración del funcionamiento de los órganos."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Porque es un conjunto de síntomas que caracteriza a una enfermedad."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Porque es un defecto adquirido que presenta causas perfectamente definidas."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 46,
                        Texto = "En el párrafo 3, el autor coloca entre guiones la palabra pandemia; tomando en cuenta el contexto, ¿cuál nos aproxima a su significado?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Enfermedad con mortalidad significativa"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Plaga originada en animales"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Calamidad que afecta a un país"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Propagación mundial de una nueva enfermedad"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 47,
                        Texto = "En el párrafo 4, el autor expone algunas dificultades para combatir el SIDA. ¿Qué tipo de modo discursivo utiliza para plantear dicha idea?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Comparación – contraste"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Concepto -ejemplo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Comparación – ejemplo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Problema - solución"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 48,
                        Texto = "En el párrafo 6 el autor presenta las características del SIDA. ¿Cuál es el modo discursivo que utiliza para exponer dicha información?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                                ClaveCOSDAC ="A",
                                Texto="Concepto -ejemplo"},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="B",
                                Texto="Comparación – contraste"},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="C",
                                Texto="Problema – solución"},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="D",
                                Texto="Causa-contraste"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 49,
                        Texto = "En el párrafo 9, ¿qué modo discursivo o recurso argumentativo utiliza el autor para presentar dicha información?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Comparación- contraste"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Problema - solución"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Causa - efecto"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Concepto - ejemplo"},
                        }
                    },
                    new Pregunta { LecturaPrevia =
                        @"
                        <h3>
                        Lee con detenimiento el siguiente texto y responde los cuestionamientos que se plantean.
                        </h3>
                        <h3>
                        <strong>
                        LA ESCOBA NEUMÁTICA
                        </strong>
                        </h3>
                        <ul style=""list-style-type: none;"">
                        <li>
                        (1) Muchos jóvenes sanos y conscientes esperan con ansia que terminen sus labores o sus clases para irse al gimnasio, ya sea de la escuela o de su comunidad. Ir al gimnasio obedece al deseo de conservar la salud, la talla y la forma. Al mismo tiempo, les permite dar cauce al único vicio que puede imputárseles: la búsqueda del placer endorfínico.
                        </li>
                        <li>
                        (2) De manera concomitante, hay una población un poco mayor de edad que ha hecho de la falta de vicios su vicio. Sus integrantes conciben la vida sana como un conjunto esotérico de jugos verde y naranja fosforescente, pan artesanal sin gluten, platillos vegetarianos donde predominan las ensaladas verdes y las semillas que antes eran pasto de pericos y otras aves. Los evidencia la ingesta diaria de tres litros de agua, que siempre traen a la mano, y la asistencia a gimnasios especializados donde corren kilómetros y levantan toneladas utilizando aparatos mecánicos. Ir más temprano seguramente reporta beneficios adicionales porque mientras sudan ven las noticias en la televisión y envían un mensaje casi yógico: no tengo prisa.
                        </li>
                        <li>
                        (3) Lo anterior es más obvio en gimnasios privados de comunidades pudientes, donde clasifico uno que paso diariamente en mi camino al Metro y que es parte de un club exclusivo que abarca casi una cuadra completa. La enorme sala de máquinas puede verse a través de impecables ventanales de piso a techo. Unas pantallas gigantes transmiten los noticiarios de rigor. Los asistentes sudan la gota gorda enfundados en extraños trajes injerto de ciclista de montaña y buzo de profundidades que ostentan marcas famosas por todos lados. Los tenis tienen suela antiderrapante, interiores acolchados y exteriores aerodinámicos. El mensaje es, adicionalmente: “no tengo prisa porque no me urge llegar a trabajar... soy mi propio jefe”. Si lo emite una mujer joven, puede tener valor social añadido: “no tengo que trabajar”.
                        22
                        </li>
                        <li>
                        (4) Pero el beneficio más evidente de la tempraneada es que pueden estacionar sus coches, bastante nuevos por cierto, en la misma acera que rodea al gimnasio. Esto implica que su contacto con el aire contaminado de la ciudad es mínimo, lo cual realza la medida de su atención a la salud. Si ya están ocupados todos los lugares, el usuario debe estacionarse en el sótano de las instalaciones, atendidas por personal uniformado y con guoquitoqui.
                        </li>
                        <li>
                        (5) Y entonces llama la atención que como parte del cuadro saludable incluyan el ecologismo, esa mezcla moderna de búsqueda de alimentos llamados orgánicos, es decir, nada de antibióticos y hormonas (animales certificadamente felices); cero sustancias químicas sospechosas de ser artificiales (ropa estrictamente de algodón, odio a los plásticos exceptuando los que conforman sus tenis y la botella de agua); reciclaje de la basura (mientras más diversidad de colores en los botes, más conciencia); y preocupación visceral por las especies en peligro (búsqueda de crema para arrugas y champú que se hayan probado solo en humanos).
                        </li>
                        <li>
                        (6) Paso diariamente por el gimnasio y los observo unos minutos: sus caras tienen el halo del cumplimiento del deber, del sacrificio agradable, de la conciencia tranquila. Se quedan, me imagino, a desayunar en el propio club: el menú hace énfasis no sólo en la organicidad de los alimentos sino también en las calorías que se van a ingerir.
                        </li>
                        <li>
                        (7) Uno de los asistentes a las sesiones tempranas, administrador del gimnasio, ha dado un paso más allá: al saber que la quincha utilizada para fabricar escobas está en peligro de extinción, tomó una medida draconiana*. Compró tres escobas neumáticas para barrer las tres aceras que rodean el gimnasio: compresoras de aire con motor de gasolina que empujan las hojas y la basura hacia lugares donde, una vez acumuladas, se recogen. El ruido es insoportable, el humo irrespirable y el aspecto repulsivo.
                        </li>
                        <li>
                        (8) Ya protesté: el pobre empleado alzó los hombros, como diciendo “qué quiere, son órdenes”. Espero un día tener el valor de decirle al administrador que la quincha no es una planta sino un sistema de entramado tradicional a base de ramas.
                        </li>
                        </ul>
                        <p class=""text-right"">
                        Sánchez, A. (2017, Agosto). La escoba neumática. ¿Cómo ves? Revista de divulgación de la ciencia de la UNAM, No. 225. Recuperado de: http://www.comoves.unam.mx/numeros/deletras/225 *Su origen se debe a Draco o Dracón, un legislador ateniense del siglo VIII a.C. famoso por su crueldad, que fue encargado de redactar el código penal. La legislación draconiana castigaba casi todos los delitos, hasta los más leves, con la pena de muerte. Popularmente se decía que sus leyes no estaban escritas con tinta, sino con sangre.
                        </p>
                        "
                        , NumeroPregunta = 50,
                        Texto = "A partir de la lectura del párrafo 6, en la línea 1, indica a qué se refiere la palabra “halo”.",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="destello"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="sorpresa"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="disgusto"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="reflejo"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 51,
                        Texto = "De acuerdo con el texto, ¿qué intención tiene la información que se incluye en los paréntesis del párrafo 5?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Explicar qué es el ecologismo."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Exponer las partes del cuadro saludable."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Describir los procesos ecologistas."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Criticar irónicamente los ejemplos del ecologismo."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 52,
                        Texto = "¿Por qué el autor considera una “medida draconiana” que el administrador del gimnasio haya comprado “tres escobas neumáticas para barrer las tres aceras que rodean el gimnasio”?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Por los accidentes que puede provocar."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Porque es una acción extremista."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Porque es un capricho del administrador."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Porque facilita la recolección de basura."},
                        }
                    },
                    new Pregunta { LecturaPrevia =
                        @"<h3>
                        Lee el siguiente texto titulado “El colesterol: lo bueno y lo malo”, y responde las preguntas:
                        </h3>
                        <h3>
                        <strong>
                        EL COLESTEROL: LO BUENO Y LO MALO
                        </strong>
                        </h3>
                        <ul style=""list-style-type: none;"">
                        <li>
                        (1) En la colección del Fondo de Cultura Económica, La ciencia desde México, con el número 140 se ha publicado el libro El colesterol: lo bueno y lo malo, dedicado a divulgar los beneficios y las desventajas de este compuesto químico. Casi todos sabemos que el colesterol es nocivo para la salud, pero no entendemos el por qué. El objetivo que se propone la autora, Victoria Tudela, bióloga y genetista de profesión, es explicar que el colesterol desempeña diversas funciones en los organismos vivos y que no todas son nocivas.
                        </li>
                        <li>
                        (2) Así, la autora define el colesterol como un compuesto químico, un alcohol del grupo de los esteroides, a los que se les clasifica como lípidos que no pueden disolverse en agua o en líquidos como el de la sangre, sino en disolventes orgánicos; por ejemplo, el alcohol o el éter, entre otros. Esta característica explica lo nocivo que puede ser encontrar altas concentraciones de él en las arterias, pues es capaz de producir accidentes vasculares al obstruir el paso de la sangre por esos vasos.
                        </li>
                        <li>
                        (3) No obstante, el colesterol tiene varias funciones benéficas, ya que es un componente fundamental de las células y, por tanto, de la membranas. Cuando una de éstas se rompe, el colesterol es necesario para restaurarla. Las partes de los animales vertebrados y de los seres humanos donde se encuentra principalmente este compuesto son los tejidos del cerebro, el hígado, la piel y las glándulas adrenales. Esto significa que el colesterol es indispensable para el buen funcionamiento del organismo, aunque en cantidades anormales resulta nocivo para la salud. Seguramente esta información proporcionada por Victoria Tudela será una revelación para la mayoría de las personas.
                        </li>
                        <li>
                        (4) Otra función importante del colesterol, a decir de Tudela, es la fabricación de las sales biliares, de numerosas hormonas de la vitamina D3, sustancias fundamentales para la salud. Un organismo normal produce el colesterol necesario para vivir, por eso puede prescindir de él en la alimentación.
                        </li>
                        <li>
                        (5) Nos enteramos, gracias a la lectura de esta obra que existen dos clases de colesterol: el que produce el propio organismo, fundamentalmente el hígado, llamado endógeno, y el que procede de la dieta que se consume, denominado exógeno, y que viene de los alimentos. El colesterol a su vez produce vitamina D, cuya función consiste en desarrollar y luego mantener los huesos en buen estado; también genera sales biliares que forman la bilis, sustancia indispensable para la digestión y absorción porque rompe las grasas; asimismo, las hormonas esteroides también son viables por la acción del colesterol y tiene efectos en los tejidos de los testículos, ovarios y corteza adrenal. Sin estas hormonas no es posible la vida. Lo aquí señalado por la autora me lleva a pensar que personas con un mayor requerimiento de esteroides para contrarrestar enfermedades como el asma o el lupus, deberían ingerir más colesterol exógeno. (…)
                        </li>
                        </ul>
                        <p class=""text-right"">
                        Alegría, M. (2005). Portal académico. Recuperado de: http://portalacademico.cch.unam.mx/materiales/al/cont/tall/tlriid/tlriid3/argumentar_demostrar/docs/ejemplo_de_resena.pdf
                        </p>
                        "
                    , NumeroPregunta = 53,
                        Texto = "De acuerdo con las características del texto, éste se clasifica como",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Noticia"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Ensayo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Reseña crítica"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Texto publicitario"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 54,
                        Texto = "¿En qué párrafo incluye su opinión la autora del texto?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Tres y cuatro"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Dos y cuatro"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Tres y cinco"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Dos y cinco"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 55,
                        Texto = "¿Cuáles son las aportaciones que hace Victoria Tudela, para ampliar tus conocimientos sobre el colesterol?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="La definición, las funciones y las clases del colesterol"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="La definición, la clasificación del colesterol y los beneficios."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="La definición, las funciones y la importancia en la vida."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="La definición, las características y los beneficios."},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 56,
                        Texto = "¿Cuál es tu opinión que responde a la tesis del texto?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="El colesterol necesario para vivir es producido por un organismo normal, por eso puede prescindirse de él en la alimentación."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="El colesterol tiene varias funciones benéficas, ya que es un componente fundamental de las células y, por tanto, de la membranas."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Las características del colesterol explican que encontrar altas concentraciones de él en las arterias, puede producir accidentes vasculares."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="El colesterol es indispensable para el buen funcionamiento del organismo, aunque en cantidades anormales resulta nocivo para la salud."},
                        }
                    },
                    new Pregunta { LecturaPrevia =
                        @"
                        <h3>
                        Lee detenidamente los siguientes textos y responde los cuestionamientos.
                        </h3>
                        <h3>
                        <strong>
                        LA NORMALIDAD RESQUEBRAJADA
                        </strong>
                        </h3>
                        <p>
                        Realmente, los terremotos mueven el piso. No sólo el suelo, la tierra, las calles, casas y edificios, convirtiéndose en tragedia para muchos. También el piso de la normalidad cotidiana, que vista desde la perspectiva de perder en un minuto la seguridad de la vida, la frágil certeza de que las y los cercanos estén bien, de perder la casa donde estamos cada día; la normalidad se derrumba junto a los edificios, puentes y carreteras mal construidos o mal mantenidos gracias al lucro y la corrupción.
                        </p>
                        <p>
                        En la Ciudad de México, este 19 de septiembre, la mayoría de la gente, en lugar de quedar pasmada en el lugar, pese al desconcierto y al golpe emocional, salió muy pronto a ver en qué podía ayudar, dónde apoyar, qué llevar, quién faltaba. Las brigadas solidarias se formaron en tiempo récord, como si los 32 años del terremoto anterior en otro 19 de septiembre, fueran apenas un momento. Existe una memoria colectiva de organización y espíritu que se movilizó inmediatamente y se expresa también en las generaciones que aunque no estaban, se comportan ahora como si la experiencia de esa generación también fuera la suya.
                        </p>
                        <p>
                        La solidaridad que se desplegó por todas partes –y lo sigue haciendo– es asombrosa, colectiva, generosa, no protagónica. Aquí está la gente de a pie, las y los de abajo, todas y todos trabajando en común, cada cual con lo que puede. Es un movimiento ejemplar que repercute en todo el mundo, pese a que autoridades y la tele-basura tratan de aplanarlo y ocultarlo, buscando, ellos sí, un protagonismo carroñero, que roba desde acopios a imágenes de rescate. Creen los poderosos que en el desastre lograrán ganar puntos para las elecciones, lavar su imagen, que olvidemos sus crímenes y engaños, que olvidemos que el Ejército que ahora viene a ayudar es la institución más denunciada ante la Comisión de Derechos Humanos por abusos de todo tipo, que olvidemos que son los que mataron maestros y comuneros en Nochixtlán, que por ellos nos faltan 43 y 30 mil más, que olvidemos que los feminicidios son parte de su normalidad, nunca de la nuestra.
                        </p>
                        <p>
                        Sucede lo contrario. Como dice Gloria Muñoz En las calles de México se gesta, junto a la tragedia, una fuerza civil cuyos frutos no son sólo inmediatos, en la atención del rescate de vidas y el apoyo a damnificados, sino de mediano y largo aliento. Sí, como en 1985, pero ahora con celular y redes sociales. La organización es espontánea y eficaz y visibiliza a una sociedad indignada que desde hacer mucho tiempo no confía en sus autoridades.
                        </p>
                        <p>
                        Por eso tratan de callarla, de que no se conozca y sobre todo de que no se comunique entre sí, ni con todas las otras solidaridades, organizaciones y resistencias desde abajo que existen por todo el país, que siguen creciendo y aumentan, con o sin prensa que lo difunda.
                        </p>
                        <p>
                        Es un proceso profundo, que como tal, no empieza ni termina ahora. Este momento dramático nos comunica con lo más radical –o sea, con las raíces– de las sociedades comunitarias, que es la ayuda mutua, la auto-organización, la solidaridad no cómo dar algo a otra persona sino entendiendo que somos parte del mismo cuerpo social y que apoyarnos es parte de la vida y la subsistencia. Ser y sentires comunitarios que se expresan social, cultural y económicamente por abajo de la gran ciudad, que de hecho la sostienen, como afirma Mike Davis.
                        </p>
                        <p>
                        Solidaridades y estructuras comunitarias (barriales, de pueblos, de organizaciones) sin las cuales, la ciudad entera se desplomaría física y socialmente, no por un terremoto, sino bajo el peso de la especulación inmobiliaria, de la sobreexplotación de agua, de la contaminación, de la basura, de los negocios privados, de traficantes y de funcionarios públicos que venden y privatizan parques, calles y mucho más. Todos negocios que han producido un colapso ambiental urbano y periurbano que hacen enormemente vulnerable la mega ciudad ante los desastres naturales.
                        </p>
                        <p>
                        Porque están ahí, ante el terremoto emergen desde la raíz esas manifestaciones, que pueden estar más o menos organizadas, pero siguen conectadas y en conjunto manteniendo las redes de cuidado de los comunes, personas y espacios.
                        </p>
                        <p>
                        Es un momento extraordinario, porque nos muestra lo que quieren tapar los poderosos con su absurda normalidad. Muestra tanto los desastres construidos que ahí estaban latentes, como la fuerza que hay abajo. Muchas plantas, cuando perciben una fuerte amenaza externa, florecen apresuradamente, para continuar la vida. Hemos visto y seguimos viendo flores de todas las formas y colores entre los escombros y en las veredas que los comunican. Son manifestaciones del sistema radical de abajo, del rizoma que sigue creciendo horizontalmente y de muchas maneras va agrietando esa normalidad que no queremos.
                        </p>
                        <p class=""text-right"">
                        Ribeiro, S. (30 de septiembre de 2017). La normalidad resquebrajada. La Jornada.
                        <br/> 
                        Recuperado de: http://www.jornada.unam.mx/2017/09/30/economia/031a1eco?partner=rss
                        </p>

                        "
                    , NumeroPregunta = 57,
                        Texto = "¿Qué tipo de texto es “la normalidad resquebrajada”?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Literario"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Científico"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Argumentativo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Informativo"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 58,
                        Texto = "Con base en el contenido del texto ¿Qué es lo que pretende resaltar la autora?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Las brigadas solidarias no requieren de una convocatoria anticipada, surgen de manera espontánea ante un desastre."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Se exige a las autoridades su intervención inmediata cuando los ciudadanos pierden la seguridad de la vida."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="La participación de las nuevas generaciones como un movimiento ejemplar que sorprendió a los mexicanos y al mundo."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="El uso indebido de los recursos naturales y la especulación inmobiliaria, hacen vulnerable a la ciudad ante un fenómeno natural."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 59,
                        Texto = "¿Cuál es la intención del autor en el último párrafo?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="A pesar de la adversidad crece la unión entre los más necesitados, rompiendo con las ideas y acciones impuestas a lo largo de los años."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="A pesar de la adversidad surge el movimiento y la solidaridad sin distinción social, rompiendo con las ideas y acciones impuestas a lo largo de los años."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="A pesar de las amenazas externas surgen oportunidades donde prevalece la unión entre la población y las autoridades."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="A pesar de la adversidad surge la solidaridad entre la población más necesitada, para continuar su vida con normalidad."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 60,
                        Texto = "¿Qué modo discursivo se utiliza en el texto?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Comparación-contraste"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Concepto-ejemplo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Problema-solución"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Causa-efecto"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 61,
                        Texto = "¿Por qué crees que la articulista Silvia Ribeiro habrá titulado su texto “La normalidad resquebrajada”?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Porque permite ver la carencia de recursos destinados a los damnificados, para atender sus necesidades y les permita continuar su vida con normalidad."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Porque deja ver cómo los gobernantes respondieron tardíamente para atender a los afectados."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Porque se rompió con la cotidianidad de la población y se vio la falta atención de sus gobernantes."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Porque deja ver las anormalidades estructurales que se han venido ocultando durante mucho tiempo gracias al lucro y la corrupción."},
                        }
                    },
                    new Pregunta { LecturaPrevia =
                        @"
                        <h3>LA GENERACIÓN DEL #SISMO</h3>
                        <h3>
                        <strong>
                        Los jóvenes demostraron al mundo que a 'tuitazos' sí se puede salir de una crisis
                        </strong>
                        </h3>
                        <p>
                        Hubo un tiempo que los sociólogos medían las generaciones en años, entre 40 y 70, después decidieron que era más exacto agruparlas por acontecimientos y más recientemente por hábitos de consumo, adaptación o comunicación. Para mí, por lo menos en la zona sísmica del país, las generaciones duran 32 años.
                        </p>
                        <p>
                        Lo que en 1985 hicieron algunos cientos de radioaficionados, ahora lo replicaron exponencialmente varios millones de usuarios de medios sociales. 4,583,284 hasta las 19:00 horas del lunes 25 de septiembre para ser exactos, participando a través de conversaciones en las etiquetas, #SismoMX, #FuerzaMexico, #Sismo o #PrayForMexico.
                        </p>
                        <p>
                        Hasta el momento, la cifra de víctimas mortales asciende a 324.
                        </p>
                        <p>
                        El día con mayor actividad es el miércoles 20 de septiembre, donde podemos clasificar los mensajes, no provenientes de medios de información, principalmente en: solicitudes y ofrecimientos de ayuda con el 68% y fotografías, videos e historias con el 23%. El porcentaje restante se reparte en quejas, advertencias y desinformación.
                        </p>
                        <p>
                        Las plataformas que más utilizaron los usuarios fueron Facebook con el 54.7%, y twitter con el 41.1%, una mezcla muy diferente a la que podemos observar regularmente donde la plataforma fundada por Marc Zuckerberg regularmente supera hasta en 4 a 1 a la del microblog. Seguramente por la inmediatez e impacto es que los usuarios en esta ocasión se decantaron por twitter. En términos de género, la conversación se comportó también de manera diversa a lo habitual, siendo acaparada en un 57% por mujeres.
                        </p>
                        <p>
                        El 43% de los mensajes fue generado por usuarios en el rango de edad 25-34, los famosos millennials de los que algunos se dicen sorprendidos por su participación.
                        </p>
                        <p>
                        Éste es el fenómeno probablemente más sencillo de explicar, los jóvenes no son “apáticos”, simplemente tienen formas diversas de participar en procesos, como el democrático, que hemos construido quienes vamos generaciones más adelante.
                        </p>
                        <p>
                        Es decir, no es que los millennials estén menos interesados en política, es que simplemente tienen una forma diversa de hacerlo, han decidido que el voto no es su herramienta de participación social. Pero cuando la acción no admite equívoco, es decir, el comportamiento para incidir en la crisis es una acción física, como levantar escombros o voluntarias en un centro de acopio, sí o sí, hacen su aparición sin dudarlo.
                        </p>
                        <p>
                        Y aún así, esta generación nos regaló innovación frente a la adversidad, como las plataformas programadas de manera colaborativa para buscar y ofrecer ayuda o verificar la información. No importa que ya existieran y que estuvieran disponibles en Facebook o a través de Google, si se podían desarrollar mejores y más cercanas a la realidad nacional y así trascender o participar del control de la emergencia era una oportunidad que no debía dejarse escapar.
                        </p>
                        <p>
                        Pero no dejamos de ser fuertemente impactados por la comunicación y la emotividad, en 7 días hemos visto nacer héroes, modelos, historias, fábulas y villanos, se han derramado igual cantidad de lágrimas frente a la pantalla grande que a la que se porta en la mano, se crearon símbolos, se terminaron épocas, se envilecieron aún más algunos políticos y también nos reconciliamos con algunas instituciones, todo en ciclos de comunicación acelerados, ya no de esperar las 8 columnas o los impresos del día siguiente, sino de cobertura permanente, digital y personalizada.
                        </p>
                        <p>
                        Nadie ignora ya que hay una heroína de cuatro patas que se llama Frida, pero tal vez desconocían que antes de estos tristes días ya llevaba más de 50 vidas salvadas en siete años de vida. Todo mundo conoce, o tienen entre sus contactos digitales, a alguien que ha participado personalmente en un centro de acopio o como voluntario en alguna zona de desastre, y todos, sin excepción, hemos compartido, tal vez sin intención, alguna información equivocada.
                        </p>
                        <p>
                        Y más recientemente casi todos tenemos un amigo japonés que nos ha dicho que no entiende por qué, con esta sociedad, México no es el mejor país del mundo, y casi todos hemos respondido “yo tampoco”.
                        </p>
                        <p>
                        Si hay que definir a México por alguna generación, ojalá que sea por la del Sismo del 17, ésa que le mostró al mundo, que a tuitazos sí se puede salir de una crisis y que sí se puede cambiar el rumbo de un país, que desde un celular sí se puede motivar a las personas de cualquier género, edad y condición a salir a la calle, que un HT no sólo sirve para burlarse o agrupar ideas, y que es más bien capaz de generar sentimientos y obligar a la clase política a donar sus participaciones.
                        </p>
                        <p>
                        Y ojalá que esta generación del 17 después de acabada la emergencia recuerde todo lo que consiguió y dejó encendido, para que entonces sí, sea capaz no sólo de reconstruir una o varias ciudades, sino de escribir y componer una realidad ya no virtual, una realidad de país.
                        </p>
                        <p class=""text-right"">
                        Cedeño, A. (26 de septiembre de 2017). “La Generación del #Sismo”. Debate. Recuperado de: 
                        <br />
                        https://www.debate.com.mx/mexico/La-Generacion-del-Sismo-20170926-0013.html
                        </p>

                        "

                    , NumeroPregunta = 62,
                        Texto = "Cuando el articulista de “La Generación del #Sismo” escribe: “Y más recientemente, casi todos tenemos un amigo japonés que nos ha dicho que no entiende por qué, con esta sociedad, México no es el mejor país del mundo, y casi todos hemos respondido “Yo tampoco”, la reflexión que nos provoca como lectores es:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                                ClaveCOSDAC ="A",
                                Texto="Que los mexicanos tenemos potencial para destacar en todos los ámbitos."},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="B",
                                Texto="Que somos diferentes a los japoneses porque pensamos y actuamos distinto."},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="C",
                                Texto="Que los mexicanos sólo utilizamos diversos recursos ante la adversidad."},
                                new RespuestaPregunta{
                                ClaveCOSDAC ="D",
                                Texto="Que los mexicanos nos quejamos de lo que no tenemos y nos conformamos."},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 63,
                        Texto = "Según el artículo “La Generación del #Sismo”, ¿por qué los millennials tienen otras formas de participar políticamente en nuestra sociedad?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Porque no quieren dejar escapar una oportunidad de trascender socialmente."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Porque utilizan las herramientas tecnológicas para participar en los procesos democráticos de nuestra sociedad."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Porque innovan las herramientas tecnológicas con sus conocimientos."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Porque son apáticos al voto como herramienta de participación social."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 64,
                        Texto = "¿Cuál es la temática principal en la que el texto “La normalidad resquebrajada” y “La generación del #sismo” coinciden?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Análisis del uso de los recursos públicos después de los sismos del 19 de septiembre de 2017."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Manifestaciones de solidaridad durante los sismos del 19 de septiembre de 2017."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Crítica a la cultura de prevención y apoyo durante el sismo del 19 de septiembre de 2017."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Uso de las nuevas tecnologías y redes sociales durante el sismo del 19 de septiembre de 2017."},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 65,
                        Texto = "Ambos textos invitan a que la población:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Participe en la transformación de nuestra realidad social."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Use las herramientas digitales para comunicarse."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Se manifieste ante la indiferencia de las autoridades."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Reflexione sobre su situación actual de vida."},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 66,
                        Texto = "Una de las conclusiones a la que se puede llegar después de leer ambos textos es:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="La movilidad social no es permanente, ya que sólo se presenta cuando no recibe apoyo, ante un desastre natural."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="La innovación tecnológica frente a la diversidad y la organización espontánea y eficaz visibilizaron a una sociedad indignada que no confía en sus autoridades."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Los mexicanos respondemos socialmente, sólo cuando se presenta un desastre, por medio de una acción física, económica, moral y de innovación."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="La desconfianza que tiene la población por el abuso de las autoridades, ha creado un individualismo que sólo responde ante los desastres."},

                        }
                    },


                }

            };
            _context.Competencias.Add(lectora);
            _context.SaveChanges();

            #endregion

            #region COMPETENCIA CIENCIAS EXPERIMENTALES
            Competencia ciencias = new Competencia
            {
                Nombre = "CIENCIAS EXPERIMENTALES",
                LecturaPrevia = null,
                TiempoParaResolver = 60,
                Preguntas = new List<Pregunta>
                {
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 67,
                        Texto = "La mamá de Pablo le prepara una gelatina de limón para su cumpleaños, con los siguientes ingredientes: 1 litro de agua, 30 gr de grenetina y 5 ml de saborizante. ¿A qué tipo de propiedad de la materia corresponde el sabor de la gelatina?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Cuantitativa"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Cualitativa"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Intensiva"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Extensiva"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 68,
                        Texto = @"
                        <p>En el laboratorio de química se miden las propiedades físicas de los materiales, con diferentes instrumentos.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta68.png"" />
                        <p>¿Cuál de ellos permite medir una propiedad intensiva?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Termómetro"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Balanza"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Cinta métrica"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Vaso de precipitado"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 69,
                        Texto = @"
                        <p>Genaro puso a hervir café en una olla de 20 litros y en una taza de 750 ml, ambos recipientes alcanzaron el mismo grado de ebullición (100ºC) independientemente de la cantidad de líquido en cada recipiente.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta69.png"" />
                        <p>¿A qué tipo de propiedad se refiere este proceso?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Intensiva"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Extensiva"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Cuantitativa"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Cualitativa"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 70,
                        Texto = "Víctor fue al doctor porque se sentía mal, el diagnóstico fue que tiene un exceso de sodio en el cuerpo; le recomendó sustituir la sal de mesa (NaCl) por cloruro de potasio (KCl); ambos se clasifican como:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Mezcla"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Elemento"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Compuesto"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Coloide"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 71,
                        Texto = "En la cafetería escolar se prepara una deliciosa sopa de fideo, ¿qué tipo de mezcla constituyen el agua y la sal?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Homogénea"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Heterogénea"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Coloide"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Suspensión"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 72,
                        Texto = "Para animar la fiesta de Paty se repartieron varitas fluorescentes que emiten luz en la oscuridad; este fenómeno se debe al brinco de los electrones por los orbitales. ¿Cuál de los siguientes modelos atómicos lo explica?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Thomson"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Rutherford"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Dalton"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Bohr"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 73,
                        Texto = "El oxígeno es el tercer elemento más abundante en el planeta y el componente mayoritario en la masa de los seres vivos y en todas las formas complejas de vida. ¿Qué partícula del modelo atómico de Bohr hace altamente reactivo al oxígeno?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Protón"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Electrón"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Catión"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Neutrón"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 74,
                        Texto = @"
                        <p>Pedro fue a acampar con su familia y en el lugar encontró un material brillante y sólido; a su regreso lo llevó al laboratorio de su escuela. Luego de analizarlo determinó que conduce electricidad, su temperatura de ebullición es elevada, es dúctil y maleable.</p>
                        <p>De acuerdo con la tabla periódica, ¿cómo clasificas el material que Pedro encontró?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Metal alcalino"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Metaloide"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="No metal"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Metal"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 75,
                        Texto = "Manuel tiene la piel muy reseca, sufre estreñimiento y constantes infecciones urinarias porque acostumbra beber solo 200 ml de agua al día. ¿Qué elementos químicos del agua son insuficientes en Manuel?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Oxígeno y fósforo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Hidrógeno y oxígeno"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Carbono e hidrógeno"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Hidrógeno y azufre"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 76,
                        Texto = "En la visita a un museo un grupo de alumnos aprendió que el carbono, el nitrógeno, el oxígeno, el hidrógeno, el fósforo y el azufre son los elementos químicos más abundantes en el cuerpo humano; para vincular lo aprendido, la maestra preguntó ¿cuáles de ellos se encuentran en la familia VA?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Oxígeno y azufre"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Hidrógeno y carbono"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Fósforo y oxígeno"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Nitrógeno y fósforo"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 77,
                        Texto = "Carlos prepara agua de limón para la comida con 800 ml de agua azucarada y 30 ml de jugo de limón. Por sus clases de química sabe que el pH del agua azucarada es de 7 y el del jugo de limón es de 2.3, ¿qué pasa con el pH al mezclar ambos líquidos?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Neutraliza"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Aumenta"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Disminuye"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Constante"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 78,
                        Texto = "En el restaurante “El Huarache Veloz” quitan el cochambre de las estufas con un limpiador cuyo ingrediente activo es NaOH. La dueña está contenta porque se ha facilitado el proceso de limpieza. ¿Cuál es la propiedad de este compuesto que facilita el trabajo?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Es neutralizante"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Su pH es de 3"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Disuelve compuestos orgánicos"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Aumenta la densidad de iones"},

                        }
                    },
                    new Pregunta { LecturaPrevia =
                    @"
                        <p>En el siguiente cartel se observan diferentes tipos de microorganismos, como las células procariontes y las eucariontes.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta79.png"" />
                        ",
                        NumeroPregunta = 79,
                        Texto = "La estructura presente en todos los microorganismos con células eucariontes y que no está definido en las células procariontes, es:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Membrana celular"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Ribosomas"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Citoplasma"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Núcleo celular"},

                        }
                    },
                    new Pregunta { LecturaPrevia =
@"
<h3>
El secreto del único animal que hace la fotosíntesis
</h3>
<p>
Hay una babosa marina (un tipo de caracol sin concha) que prefiere vivir de la luz del sol a tener que andar por el fondo del mar en busca de comida. Se llama Elysia clorótica, tiene un aspecto que la asemeja a una hoja de lechuga y habita en las costas de Nueva Escocia hasta el sur de Florida. ¿Cómo puede vivir del sol?
</p>
<p>
“Elysia” se alimenta de un alga llamada Vaucheria litorea, que sí es autótrofa, y eso quiere ser Elysia. El alga, igual que las plantas, puede ser autótrofa gracias a sus cloroplastos. Así que Elysia ha decidido robárselos.
</p>
<p>
Empeñada en facilitarse su comida diaria, la babosa ha aprendido a digerir al alga sin dañar los preciados cloroplastos, capaces de transformar la luz del sol en comida. Así que los trata con sumo cuidado y las integra en sus células digestivas. Gracias a esta estrategia sus aspiraciones se han visto cumplidas y se las arregla para vivir durante meses sin probar bocado, simplemente alimentándose de la luz del sol. Esta rebuscada estrategia ha hecho famosa a Elysia, como el primer animal capaz de realizar la fotosíntesis <sup>1</sup>.
</p>
<img src=""/Evaluacion/ImagenPrivada/Pregunta80.png"" />
<p class=""text-right"">
1 Quijada, Pilar. El secreto del único animal que hace la fotosíntesis.
<br/>
http://www.abc.es/ciencia/20150204/abci-babosa-secreto-terapia-genica-201502041746.html
</p>
"
                    , NumeroPregunta = 80,
                        Texto = "Si los humanos fuéramos seres fotosintéticos, que podríamos hacer:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Sobrevivir a niveles muy bajos o nulos de oxígeno en el aire."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Fabricar dentro de nuestras células los alimentos necesarios para sobrevivir."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Transferir nuestra información genética sin necesidad de una reproducción sexual."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Crear dentro del cuerpo los aminoácidos necesarios para tener una vida saludable."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 81,
                        Texto = "La mayoría de las babosas de mar no puede crear su propio alimento, pero un alga sí, ¿cómo se le llama al tipo de nutrición de babosas y algas?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Las babosas son heterótrofas y las algas son autótrofas."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Las babosas son autótrofas y las algas son heterótrofas."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Los dos seres son heterótrofos."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Los dos seres son autótrofos."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 82,
                        Texto = "Gracias a los cloroplastos existentes en el alga Vaucheria y “robados” por Elysia, ambos seres vivos pueden hacer el proceso biológico llamado:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Respiración"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Excreción"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Reproducción"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Fotosíntesis"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 83,
                        Texto = "Te inscribes en el equipo de tu escuela que se prepara para participar en competencias de atletismo, necesitas tener el máximo rendimiento físico posible para ganar la competencia. Para lograr un alto rendimiento es necesario que llegue el oxígeno necesario a tus músculos durante la carrera, por lo que tu aparato respiratorio debe de trabajar en conjunto con el aparato:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Circulatorio"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Excretor"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Digestivo"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Reproductor"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 84,
                        Texto = @"
<p>
Una familia integrada por ocho miembros, tiene problemas económicos; por lo tanto, el mayor de los hijos tuvo que dejar la escuela para buscar trabajo y ayudar a sus padres a mejorar los ingresos familiares. Ante tal situación, los padres están considerando utilizar un método anticonceptivo permanente.
</p>
<p>
¿Cuál de los siguientes métodos anticonceptivos recomendarías?
</p>",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Condón femenino"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Vasectomía"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Dispositivo intrauterino (DIU)"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Píldora anticonceptiva"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 85,
                        Texto = "Se presenta un frente frío en el lugar donde vives, que ya ha durado cinco días, y la temperatura del aire baja considerablemente, por los que tus compañeros de escuela se ausentan a causa de nariz tapada, dolor de garganta, dolor de cabeza, dolor muscular fiebre con escalofríos y fatiga. Estos son síntomas de una enfermedad respiratoria, que puede ser mortal en los adolescentes, si no es vigilada. ¿Cuál es esta enfermedad?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Gripe"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Rabia"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Tos"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Influenza"},
                        }
                    },
                    new Pregunta { LecturaPrevia =
@"
<p>
Lee detenidamente la siguiente información y señala la respuesta correcta a la pregunta posterior:
</p>
<ul style=""list-style-type: none;"">
    <li>Para el adecuado funcionamiento del cuerpo humano, deben consumirse diariamente diversos nutrientes presentes en los alimentos.</li>
    <li>Las proteínas son necesarias, entre otras cosas, para la reproducción y reemplazo celular en la piel, pelo, uñas y músculos.</li>
    <li>Los carbohidratos son fuente de energía inmediata para actividades físicas intensas.</li>
    <li>Si se acaban los carbohidratos presentes en el cuerpo, se empieza a consumir lípidos, como las grasas, a fin de obtener energía para el cuerpo.</li>
    <li>Las vitaminas y los minerales, aunque se necesitan en pequeñas cantidades diarias, son indispensables para el buen funcionamiento corporal.</li>
    <li>Finalmente, la fibra vegetal acelera el paso de los alimentos por los intestinos y favorece la expulsión de los componentes no digeridos.</li>
</ul>

"

                    , NumeroPregunta = 86,
                        Texto = "Si participas en un partido de futbol de 90 minutos, obtendrás energía para tus músculos de estos dos componentes nutricionales:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Grasas y proteínas"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Vitaminas y minerales"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Lípidos y carbohidratos"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Carbohidratos y fibra vegetal"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 87,
                        Texto = "Millones de personas mueren al año por diversas enfermedades causadas por el tabaquismo, como enfisema pulmonar, cáncer de garganta, bronquitis y asma. ¿Qué medidas preventivas tomarías para no estar dentro de estas cifras y evitar esas enfermedades?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Mejorar las habilidades sociales, buen manejo de problemas y elegir a mis amigos."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Respetar las normas de convivencia, mejorar las habilidades socioemocionales y tener autocontrol."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Mejorar las habilidades sociales, tener autocontrol y respetar las normas de convivencia."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Buena toma de decisiones, tener información confiable, cuidar la salud y valorar la vida."},

                        }
                    },
                    new Pregunta { LecturaPrevia =
@"
<p>
Lee detenidamente el texto y señala la respuesta correcta a la pregunta.
</p>
<img src=""/Evaluacion/ImagenPrivada/Pregunta88.png"" />
"
                    , NumeroPregunta = 88,
                        Texto = "De acuerdo con la lectura, ¿cuál es el principal componente del cigarrillo causante de nacimientos prematuros y abortos espontáneos?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Cadmio"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Monóxido de carbono"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Nicotina"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Gases oxidantes"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 89,
                        Texto = @"
<p>
En un sismo se presenta un movimiento ondulatorio; en algunos casos, la tierra fangosa se comprime y se dilata, empuja y atrae grandes áreas, y se genera así el movimiento que sentimos.
</p>
<p>
En estos casos se producen
</p>
",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Ondas longitudinales"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Ondas transversales"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Ondas electromagnéticas"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Ondas gravitacionales"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 90,
                        Texto = @"
<p>
El maestro de ecología está resfriado y se escucha su voz muy baja, por lo que decide utilizar un micrófono para dar su clase.
</p>
<p>
Dentro de las características que tiene el sonido, ¿cuál de ellas está presente al utilizar el micrófono?
</p>
",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Timbre"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Tono"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Intensidad"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Vibración"},

                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 91,
                        Texto = "Juan estaba limando una pieza de metal, después de unos minutos había juntado una cantidad visible de limadura de fierro en la mesa de trabajo. En ese momento conectó un taladro y el cable pasó por la limadura que había en la mesa; al activar el taladro ésta se movió, y se alineó en otra dirección, lo cual se parece al experimento realizado por",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                        ClaveCOSDAC ="A",
                        Texto="Faraday"},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="B",
                        Texto="Maxwell"},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="C",
                        Texto="Ampere"},
                        new RespuestaPregunta{
                        ClaveCOSDAC ="D",
                        Texto="Oersted"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 92,
                        Texto = @"
                        <p>Juan estaba limando una pieza de metal, después de unos minutos había juntado una cantidad visible de limadura de fierro en la mesa de trabajo. En ese momento conectó un taladro y el cable pasó por la limadura que había en la mesa; al activar el taladro ésta se movió, y se alineó en otra dirección, lo cual se parece al experimento realizado por</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta92.png"" />
                        <p>Para que la “c”, que es la velocidad de la luz, permanezca constante, ¿cómo debe ser “λ”, respecto de “f”?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Directamente proporcional"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Relación de igualdad"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Inversamente proporcional"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Relación constante"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 93,
                        Texto = @"
                        <p>Al entrar a la bodega, Gladys observó que del techo provenía un rayo de sol y pegaba justo en un recipiente de vidrio con agua; notó que el rayo no seguía la línea recta, sino que se desviaba en otra dirección.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta93.png"" />
                        <p>A este fenómeno se le conoce como:</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Refracción"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Continuidad"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Reflexión"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Expansión"},

                        }
                    },
                    new Pregunta { LecturaPrevia =
                    @"
                        <p>Observa la imagen siguiente y responde la pregunta.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta94.png"" />
                        
                        ",
                     NumeroPregunta = 94,
                        Texto = "¿Cuál es el punto de referencia para observar la distancia recorrida por el motociclista?",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="La pista"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="El centro de la pista"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Distancia cero"},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Distancia 10"},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 95,
                        Texto = @"
                        <p>Al auto de mi papá se le dañó la batería, y para orillarlo fue necesario empujarlo.</p>
                        <img src=""/Evaluacion/ImagenPrivada/Pregunta95.png"" />
                        <p>¿Qué ley de Newton se cumple al momento de mover el carro, si al aplicar la fuerza, se percibe otra en sentido opuesto, originada por el peso del automóvil?</p>
                        ",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Ley de la inercia."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Ley de acción-reacción."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Ley de acción-reacción."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Ley de relación de fuerza, masa y aceleración."},
                        }
                    },
                    new Pregunta { LecturaPrevia = null, NumeroPregunta = 96,
                        Texto = "Al equipo de Lucía le han encomendado realizar una presentación de las leyes del movimiento de los cuerpos. Dentro de éste se encuentra el concepto de “fuerza”. La fuerza se puede representar por una flecha y tiene tres elementos, que son:",
                        RespuestasPregunta = new List<RespuestaPregunta>
                        {
                            new RespuestaPregunta{
                            ClaveCOSDAC ="A",
                            Texto="Dirección, punto de aplicación y amplitud."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="B",
                            Texto="Sentido, dirección y magnitud."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="C",
                            Texto="Sentido, dirección y amplitud."},
                            new RespuestaPregunta{
                            ClaveCOSDAC ="D",
                            Texto="Magnitud, velocidad e intensidad."},
                        }
                    },

                }

            };
            _context.Competencias.Add(ciencias);
            _context.SaveChanges();

            #endregion
        }

        private async Task BorrarExamen()
        {
            foreach (var competencia in _context.Competencias)
            {
                foreach (var pregunta in competencia.Preguntas)
                {
                    foreach (var respuesta in pregunta.RespuestasPregunta)
                    {
                        _context.Remove(respuesta);
                    }
                    _context.SaveChanges();
                    _context.Remove(pregunta);
                }
                _context.SaveChanges();
                _context.Remove(competencia);
            }
            _context.SaveChanges();

        }


        public async Task ResetearPasswords()
        {
            var planteles = await _context.Planteles.ToListAsync();
            foreach (var plantel in planteles)
            {
                foreach (var grupo in plantel.GruposPlantel)
                {
                    foreach (var aspirante in grupo.Aspirantes)
                    {
                        var usuario = _userManager.Users.FirstOrDefault(u => u.UserName == aspirante.UserName);
                        await _userManager.AddPasswordAsync(usuario, "Kazepima.8713");
                    }
                }
            }

        }

        public async Task SeedExamenModelo(int numCompetencias = 1, int numPreguntas = 50, int numRespuestas = 4)
        {
            await BorrarExamen();
            string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int preguntasCreadas = 0;

            List<Competencia> competencias = new List<Competencia>();
            for (int i = 1; i <= numCompetencias; i++)
            {
                competencias.Add(new Competencia
                {
                    Nombre = $"Competencia {i}",
                    LecturaPrevia = $"Lectura previa de la competencia {i}",
                    TiempoParaResolver = 0
                });
            }

            foreach (var comp in competencias)
            {
                // Anadir Preguntas
                for (int j = 1; j <= numPreguntas; j++)
                {
                    comp.Preguntas.Add(new Pregunta
                    {
                        LecturaPrevia = $"Lectura Previa Pregunta {j}",
                        NumeroPregunta = ++preguntasCreadas,
                        Orden1 = j,
                        Orden2 = j,
                        Texto = $"Pregunta {j}"
                    });
                }

                foreach (var preg in comp.Preguntas)
                {
                    for (int k = 1; k <= numRespuestas; k++)
                    {
                        preg.RespuestasPregunta.Add(new RespuestaPregunta
                        {
                            ClaveCOSDAC = letras.Substring(k - 1, 1),
                            Orden1 = k,
                            Orden2 = k,
                            Texto = $"Pregunta {preg.NumeroPregunta} - Respuesta numero {k}",
                            Valor = 0
                        });
                    }
                }
            }

            _context.Competencias.AddRange(competencias);
            _context.SaveChanges();
        }

        public async Task seedDatosExamenRegional()
        {
            List<Aspirante> directores = new List<Aspirante>
            {
new Aspirante{ Paterno= "Rojas", Materno="Mendez", Nombre="Nancy Sofia",Ficha="n.rojas@cecytechihuahua.edu.mx",UserName="n.rojas@cecytechihuahua.edu.mx",Email="n.rojas@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E01",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Cruz", Materno="Ruiz", Nombre="Lorenzo",Ficha="l.cruz@cecytechihuahua.edu.mx",UserName="l.cruz@cecytechihuahua.edu.mx",Email="l.cruz@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E02",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Armendáriz", Materno="Ortiz", Nombre="Rafael Edén",Ficha="e.armendariz@cecytechihuahua.edu.mx",UserName="e.armendariz@cecytechihuahua.edu.mx",Email="e.armendariz@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E03",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Jacquez", Materno="Villalobos", Nombre="Miriam Lizzeth",Ficha="m.jacquez@cecytechihuahua.edu.mx",UserName="m.jacquez@cecytechihuahua.edu.mx",Email="m.jacquez@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E04",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Avitia", Materno="Ponce de Leon", Nombre="Gladys",Ficha="g.avitia@cecytechihuahua.edu.mx",UserName="g.avitia@cecytechihuahua.edu.mx",Email="g.avitia@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E05",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Ornelas", Materno="Miranda", Nombre="Carlos Leonel",Ficha="c.ornelas@cecytechihuahua.edu.mx",UserName="c.ornelas@cecytechihuahua.edu.mx",Email="c.ornelas@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E06",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "De La Rocha", Materno="Prieto", Nombre="Yadira Itcel",Ficha="y.delarocha@cecytechihuahua.edu.mx",UserName="y.delarocha@cecytechihuahua.edu.mx",Email="y.delarocha@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E09",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Egüis", Materno="Gonzalez", Nombre="Manuel Armando",Ficha="a.eguis@cecytechihuahua.edu.mx",UserName="a.eguis@cecytechihuahua.edu.mx",Email="a.eguis@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E10",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Loya", Materno="Mannings", Nombre="Bernardo",Ficha="b.loya@cecytechihuahua.edu.mx",UserName="b.loya@cecytechihuahua.edu.mx",Email="b.loya@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E11",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Pacheco", Materno="Rojas", Nombre="Ivan Alejandro",Ficha="a.pacheco@cecytechihuahua.edu.mx",UserName="a.pacheco@cecytechihuahua.edu.mx",Email="a.pacheco@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E12",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Loera", Materno="Puentes", Nombre="Silvia Maria",Ficha="s.loera@cecytechihuahua.edu.mx",UserName="s.loera@cecytechihuahua.edu.mx",Email="s.loera@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E13",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Baca", Materno="Terrazas", Nombre="Aaron",Ficha="a.bacat@cecytechihuahua.edu.mx",UserName="a.bacat@cecytechihuahua.edu.mx",Email="a.bacat@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E15",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Acosta", Materno="Garcia", Nombre="Nora Dilia",Ficha="n.acosta@cecytechihuahua.edu.mx",UserName="n.acosta@cecytechihuahua.edu.mx",Email="n.acosta@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E17",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Chávez", Materno="Avalos", Nombre="Norma Angélica",Ficha="n.chaveza@cecytechihuahua.edu.mx",UserName="n.chaveza@cecytechihuahua.edu.mx",Email="n.chaveza@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E18",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Meraz", Materno="", Nombre="Guadalupe Cabrera",Ficha="g.cabrera@cecytechihuahua.edu.mx",UserName="g.cabrera@cecytechihuahua.edu.mx",Email="g.cabrera@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E19",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Rodriguez", Materno="Servin", Nombre="Brisa Gail",Ficha="b.rodriguez@cecytechihuahua.edu.mx",UserName="b.rodriguez@cecytechihuahua.edu.mx",Email="b.rodriguez@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E20",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Carmona", Materno="Garcia", Nombre="Catalina",Ficha="c.carmona@cecytechihuahua.edu.mx",UserName="c.carmona@cecytechihuahua.edu.mx",Email="c.carmona@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E22",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Martinez", Materno="Cano", Nombre="Alberto",Ficha="a.martinez@cecytechihuahua.edu.mx",UserName="a.martinez@cecytechihuahua.edu.mx",Email="a.martinez@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E23",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Carreon", Materno="Ochoa", Nombre="Jorge Arturo",Ficha="j.carreon@cecytechihuahua.edu.mx",UserName="j.carreon@cecytechihuahua.edu.mx",Email="j.carreon@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E24",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Rivera", Materno="Sanchez", Nombre="Ramon Luis",Ficha="r.rivera@cecytechihuahua.edu.mx",UserName="r.rivera@cecytechihuahua.edu.mx",Email="r.rivera@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E25",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Marquez", Materno="Aranda", Nombre="Beatriz",Ficha="b.marquez@cecytechihuahua.edu.mx",UserName="b.marquez@cecytechihuahua.edu.mx",Email="b.marquez@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E26",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Quintana", Materno="Ruiz", Nombre="JulioErnesto",Ficha="j.quintanar@cecytechihuahua.edu.mx",UserName="j.quintanar@cecytechihuahua.edu.mx",Email="j.quintanar@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E28",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Fong", Materno="Gutierrez", Nombre="Laura Maria",Ficha="l.fong@cecytechihuahua.edu.mx",UserName="l.fong@cecytechihuahua.edu.mx",Email="l.fong@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E29",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Valenzuela", Materno="Acosta", Nombre="Jorge Alberto",Ficha="j.valenzuela@cecytechihuahua.edu.mx",UserName="j.valenzuela@cecytechihuahua.edu.mx",Email="j.valenzuela@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E30",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Navarrete", Materno="Rivero", Nombre="Olivia",Ficha="o.navarrete@cecytechihuahua.edu.mx",UserName="o.navarrete@cecytechihuahua.edu.mx",Email="o.navarrete@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E31",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Estrada", Materno="Mariscal", Nombre="Moises Adan",Ficha="m.estradam@cecytechihuahua.edu.mx",UserName="m.estradam@cecytechihuahua.edu.mx",Email="m.estradam@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E32",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Muñoz", Materno="Duarte", Nombre="Francisco Octavio",Ficha="f.munozd@cecytechihuahua.edu.mx",UserName="f.munozd@cecytechihuahua.edu.mx",Email="f.munozd@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E33",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Gonzalez", Materno="Dominguez", Nombre="Nallely",Ficha="l.gonzalezd@cecytechihuahua.edu.mx",UserName="l.gonzalezd@cecytechihuahua.edu.mx",Email="l.gonzalezd@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E34",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Diaz", Materno="Burciaga", Nombre="Elienai",Ficha="e.diazb@cecytechihuahua.edu.mx",UserName="e.diazb@cecytechihuahua.edu.mx",Email="e.diazb@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C10",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Arzate", Materno="Lem", Nombre="Jesus",Ficha="j.arzate@cecytechihuahua.edu.mx",UserName="j.arzate@cecytechihuahua.edu.mx",Email="j.arzate@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C11",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Ordoñez", Materno="Perez", Nombre="Cesar Daniel",Ficha="c.ordonezp@cecytechihuahua.edu.mx",UserName="c.ordonezp@cecytechihuahua.edu.mx",Email="c.ordonezp@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C12",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Pereda", Materno="Saenz", Nombre="Rocio",Ficha="r.pereda@cecytechihuahua.edu.mx",UserName="r.pereda@cecytechihuahua.edu.mx",Email="r.pereda@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C13",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Hernandez", Materno="Gonzalez", Nombre="Elsa",Ficha="e.hernandezg@cecytechihuahua.edu.mx",UserName="e.hernandezg@cecytechihuahua.edu.mx",Email="e.hernandezg@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C14",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Acosta", Materno="Castro", Nombre="Jose Feliciano",Ficha="j.acosta@cecytechihuahua.edu.mx",UserName="j.acosta@cecytechihuahua.edu.mx",Email="j.acosta@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C15",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Polanco", Materno="Flores", Nombre="Sergio Ivan",Ficha="s.polanco@cecytechihuahua.edu.mx",UserName="s.polanco@cecytechihuahua.edu.mx",Email="s.polanco@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C16",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Rubio", Materno="Urtuzuastegui", Nombre="Nora Liliana",Ficha="n.rubio@cecytechihuahua.edu.mx",UserName="n.rubio@cecytechihuahua.edu.mx",Email="n.rubio@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C17",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Torres", Materno="Dominguez", Nombre="Victor Manuel",Ficha="v.torres@cecytechihuahua.edu.mx",UserName="v.torres@cecytechihuahua.edu.mx",Email="v.torres@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C18",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Rios", Materno="Enriquez", Nombre="Ana Melissa",Ficha="a.rios@cecytechihuahua.edu.mx",UserName="a.rios@cecytechihuahua.edu.mx",Email="a.rios@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C19",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Plata", Materno="Romero", Nombre="Sonia",Ficha="s.plata@cecytechihuahua.edu.mx",UserName="s.plata@cecytechihuahua.edu.mx",Email="s.plata@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C02",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Luevano", Materno="Prieto", Nombre="Hector",Ficha="h.luevano@cecytechihuahua.edu.mx",UserName="h.luevano@cecytechihuahua.edu.mx",Email="h.luevano@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C20",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Quezada", Materno="Lugo", Nombre="Rogelio",Ficha="r.quezada@cecytechihuahua.edu.mx",UserName="r.quezada@cecytechihuahua.edu.mx",Email="r.quezada@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C21",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Daniel", Materno="Avitia", Nombre="Orlando",Ficha="o.daniel@cecytechihuahua.edu.mx",UserName="o.daniel@cecytechihuahua.edu.mx",Email="o.daniel@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C22",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "De Luna", Materno="Vasquez", Nombre="Nancy Gabriela",Ficha="n.deluna@cecytechihuahua.edu.mx",UserName="n.deluna@cecytechihuahua.edu.mx",Email="n.deluna@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C23",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Andrew", Materno="Amaya", Nombre="Jose",Ficha="j.andrew@cecytechihuahua.edu.mx",UserName="j.andrew@cecytechihuahua.edu.mx",Email="j.andrew@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C06",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Reyes", Materno="Gonzalez", Nombre="Sergio Alejandro",Ficha="s.reyesg@cecytechihuahua.edu.mx",UserName="s.reyesg@cecytechihuahua.edu.mx",Email="s.reyesg@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C07",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Zamarron", Materno="Holguin", Nombre="Luis Fernando",Ficha="f.zamarron@cecytechihuahua.edu.mx",UserName="f.zamarron@cecytechihuahua.edu.mx",Email="f.zamarron@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C08",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Perez", Materno="Orozco", Nombre="Irma Cristina",Ficha="c.perez@cecytechihuahua.edu.mx",UserName="c.perez@cecytechihuahua.edu.mx",Email="c.perez@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C09",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Rios", Materno="Aguirre", Nombre="Jose Alfredo",Ficha="j.rios@cecytechihuahua.edu.mx",UserName="j.rios@cecytechihuahua.edu.mx",Email="j.rios@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C04",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Perez", Materno="Rodriguez", Nombre="Cesar Armando",Ficha="c.perezr@cecytechihuahua.edu.mx",UserName="c.perezr@cecytechihuahua.edu.mx",Email="c.perezr@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C05",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Aguirre", Materno="Villegas", Nombre="Martin Eduardo",Ficha="m.aguirre@cecytechihuahua.edu.mx",UserName="m.aguirre@cecytechihuahua.edu.mx",Email="m.aguirre@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C03",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Acosta", Materno="", Nombre="Dione Frias",Ficha="d.frias@cecytechihuahua.edu.mx",UserName="d.frias@cecytechihuahua.edu.mx",Email="d.frias@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="C24",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
new Aspirante{ Paterno= "Saenz", Materno="", Nombre="Noel Molina",Ficha="n.molina@cecytechihuahua.edu.mx",UserName="n.molina@cecytechihuahua.edu.mx",Email="n.molina@cecytechihuahua.edu.mx",Edad=30M,Genero="M",GrupoId=null,NombreSecundaria="E35",PromedioSecundaria=0,TipoSecundaria="",TipoSostenimientoSecundaria="",DescripcionOtraSecundaria="",PlainPass = ""},
        };


            #region Borrar todo
            var planteles = _context.Planteles
                                    .Include(p => p.UsuariosPlantel)
                                    .Include(p => p.GruposPlantel).ThenInclude(g => g.Aspirantes).ThenInclude(a => a.RespuestasEvaluacion);
            foreach (var plantel in planteles)
            {
                foreach (var gpo in plantel.GruposPlantel)
                {
                    foreach (var aspirante in gpo.Aspirantes)
                    {
                        foreach (var respuesta in aspirante.RespuestasEvaluacion)
                        {
                            _context.Remove(respuesta);
                        }
                        _context.Remove(aspirante);
                    }
                    _context.Remove(gpo);
                }
                _context.Remove(plantel);
            }
            _context.SaveChanges();

            CrearPlanteles();
            await SeedUsuarioAdmin();

            foreach (var director in directores)
            {
                director.PlainPass = PasswordAleatorio();
                var nvoUsuario = await CrearUsuarioConRol(director, new List<string> { "Administrativo" });
                await ChecarUsuarioEnPlantel(nvoUsuario.UserName, new List<string> { nvoUsuario.NombreSecundaria });
            }

            #endregion
            var lstPlanteles = _context.Planteles;
            foreach (var plantel in lstPlanteles)
            {
                plantel.GruposPlantel.Add(new Grupo
                {
                    ClaveSIIACE = "9999",
                    Nombre = "Concurso",
                    EvaluacionHabilitada = false,
                    FechaExamen = new DateTime(2019, 03, 29),
                    Semestre = "",
                    Turno = ""
                    //,Aspirantes = new List<Aspirante> {
                    //    _context.Aspirante.First(a => a.UserName=="a.torresc@cecytechihuahua.edu.mx"),
                    //    _context.Aspirante.First(a => a.UserName=="p.baeza@cecytechihuahua.edu.mx"),
                    //    _context.Aspirante.First(a => a.UserName=="k.franco@cecytechihuahua.edu.mx"),
                    //    _context.Aspirante.First(a => a.UserName=="b.caballero@cecytechihuahua.edu.mx"),
                    //    _context.Aspirante.First(a => a.UserName=="i.gonzalezp@cecytechihuahua.edu.mx"),
                    //    _context.Aspirante.First(a => a.UserName=="b.caballero@cecytechihuahua.edu.mx"),
                    //    _context.Aspirante.First(a => a.UserName=="f.hernandezc@cecytechihuahua.edu.mx")
                    //}
                });
            }
            _context.SaveChanges();

            /// Todo: Agregar directores del plantel

        }

        private async Task ChecarUsuarioEnPlantel(string userName, List<string> planteles)
        {
            var usuario = await _userManager.FindByNameAsync(userName);

            if (usuario != null)
            {
                foreach (var plantel in planteles)
                {
                    if (!_context.UsuariosPlantel.AsNoTracking().Where(up => up.Id == usuario.Id).Any(up => up.ClavePlantel == plantel))
                    {
                        _context.UsuariosPlantel.Add(new UsuarioPlantel { ClavePlantel = plantel, Id = usuario.Id });
                        _context.SaveChanges();
                    }

                }

            }
        }
    }
}
