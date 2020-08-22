using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamenCecytech.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExamenCecytech.Controllers
{
    [Authorize(Roles = "SysAdmin, Administrativo")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Aspirante> _userManager;
        [TempData]
        public string ErrorMsg { get; set; }
        [TempData]
        public string ExitoMsg { get; set; }

        public UsuariosController(ApplicationDbContext context, UserManager<Aspirante> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userActive = await _userManager.GetUserAsync(User);
            var rolAdministrator = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADMINISTRATIVO");
            var plantelesAsignados = await _context.UsuariosPlantel
                .Where(p => p.Id == userActive.Id)
                .Select(p => p.ClavePlantel)
                .ToListAsync();

            ViewBag.Planteles = await _context.Planteles.AsNoTracking()
                .Where( p => plantelesAsignados.Contains(p.ClavePlantel))
                .Select(p => p.ClavePlantel).ToArrayAsync();

            ViewBag.PlantelesSelectList = await PlantelesSelectList(null);

            ViewBag.UsuariosPlantel = await _context.UsuariosPlantel.AsNoTracking().ToListAsync();

            var usuariosAdmvo = _context.Aspirante.AsNoTracking().Where(a =>
                    _context.UserRoles
                        .AsNoTracking()
                        .Where(u => u.RoleId == rolAdministrator.Id)
                        .Select(u => u.UserId).Contains(a.Id) &&
                    _context.UsuariosPlantel
                        .AsNoTracking()
                        .Where(u => plantelesAsignados.Contains(u.ClavePlantel))
                        .Select(u => u.Id).Contains(a.Id));

            return View(await usuariosAdmvo.ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> AgregarAdministrativo([Bind] Aspirante aspirante)
        {
            aspirante.PlainPass = PasswordAleatorio();
            aspirante.Paterno = aspirante.Paterno ?? "";
            aspirante.Materno = aspirante.Materno ?? "";
            aspirante.Nombre = aspirante.Nombre ?? "";
            aspirante.Edad = 0;
            aspirante.DescripcionOtraSecundaria = "";
            aspirante.EspecialidadId = null;
            aspirante.Estatus = "";
            aspirante.UserName = aspirante.Email.ToLower().Trim();
            aspirante.Ficha = aspirante.Email.Split("@")[0];
            aspirante.NombreSecundaria = "";
            aspirante.PromedioSecundaria = 0;
            aspirante.TipoSecundaria = "";
            aspirante.TipoSostenimientoSecundaria = "";
            aspirante.Genero = "";
            if (ModelState.IsValid)
            {
                var usr = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == aspirante.UserName.ToUpper());
                if (usr == null)
                {

                    var resAddUsr = await _userManager.CreateAsync(aspirante, aspirante.PlainPass);
                    if (resAddUsr.Succeeded)
                    {
                        usr = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == aspirante.UserName);

                        var plantel = await _context.Planteles.FirstOrDefaultAsync(p => p.PlantelId == Convert.ToInt32(aspirante.GrupoId));

                        ExitoMsg += $"{Environment.NewLine}El usuario {aspirante.UserName} se anadio con exito";

                        var existeEnPlantel = await _context.UsuariosPlantel
                            .Where(up => up.Id == usr.Id && up.ClavePlantel == plantel.ClavePlantel).FirstOrDefaultAsync();

                        if (existeEnPlantel == null)
                        {
                            await _context.UsuariosPlantel
                                .AddAsync(new UsuarioPlantel { Id = usr.Id, ClavePlantel = plantel.ClavePlantel });
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        ErrorMsg += $"{Environment.NewLine}El usuario {aspirante.UserName} no se pudo crear, {string.Join(';', resAddUsr.Errors.Select(e => e.Description).ToArray())}";
                        return RedirectToAction(nameof(Index));
                    }

                }
                if (usr != null && !await _userManager.IsInRoleAsync(usr, "Administrativo"))
                {
                    var resRol = await _userManager.AddToRoleAsync(usr, "Administrativo");
                    if (resRol.Succeeded)
                    {
                        ExitoMsg += $"{Environment.NewLine}El usuario {usr.UserName} se anadio con exito al rol Administrativo";
                    }
                    else
                    {
                        ErrorMsg += $"{Environment.NewLine}Ocurrio un error al tratar de anadir al usuario {usr.UserName} al rol Administrativo, {string.Join(';', resRol.Errors.Select(e => e.Description).ToArray())}";
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AutorizaPlantel(int id, string clavePlantel, [FromForm] bool autorizar)
        {
            if (id == 0 || string.IsNullOrEmpty(clavePlantel))
            {
                return NotFound();
            }
            var usuario = await _context.Aspirante.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null || !await _context.Planteles.AsNoTracking().AnyAsync(p => p.ClavePlantel == clavePlantel))
            {
                return NotFound();
            }

            var existeEnPlantel = await _context.UsuariosPlantel.Where(up => up.Id == id && up.ClavePlantel == clavePlantel).FirstOrDefaultAsync();
            if (existeEnPlantel == null && autorizar)
            {
                await _context.UsuariosPlantel.AddAsync(new UsuarioPlantel { Id = id, ClavePlantel = clavePlantel });
                await _context.SaveChangesAsync();
            }
            if (existeEnPlantel != null && !autorizar)
            {
                _context.UsuariosPlantel.Remove(existeEnPlantel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
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
        private async Task<SelectList> PlantelesSelectList(int? plantelId = null)
        {
            var userActive = await _userManager.GetUserAsync(User);

            var plantelesAsignados = await _context.UsuariosPlantel
                .Where(p => p.Id == userActive.Id)
                .ToListAsync();

            var lista = await _context.Planteles
                                .AsNoTracking()
                                .OrderBy(p => p.ClavePlantel)
                                .Select(p => new { p.PlantelId, Grupo = $"{ p.Nombre }" })
                                .ToListAsync();
            return new SelectList(
                    lista,
                    "PlantelId",
                    "Grupo",
                    plantelId
                    );
        }
    }
}
