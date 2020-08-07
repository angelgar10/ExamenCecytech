using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ExamenCecytech.Data;
using ExamenCecytech.Models.AccountViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExamenCecytech.Controllers
{
    public class AccountController : BaseController
    {
        [TempData]
        public string ErrorMessage { get; set; }
        private readonly SignInManager<Aspirante> _signInManager;
        private readonly UserManager<Aspirante> _userManager;
        private readonly ILogger _logger;

        public AccountController(SignInManager<Aspirante> signInManager,
            UserManager<Aspirante> userManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        //[AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);

                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email.Split("@")[1] != "cecytechihuahua.edu.mx")
                {
                    return View("SoloUsuariosCecyte");
                }
                //return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
                // Si llegamos hasta aqui, es porque el usuario pertenece a un dominio valido y se ha autenticado con google y no esta bloqueado localmente
                // Por lo que procedemos a crear el usuario local con los datos de gmail

                // Checamos si el usuario ya existe, pero no tiene habilitado el external login
                var usuarioExistente = await _userManager.FindByNameAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
                if (usuarioExistente == null)
                {
                    //          DESABILITAR ESTA PARTE DE CODIGO PARA EL PRE REGISTRO
                    //var arrApellidos = info.Principal.FindFirstValue(ClaimTypes.Surname).Split(" ");
                    //var emailU = info.Principal.FindFirstValue(ClaimTypes.Email);
                    //var user = new Aspirante
                    //{
                    //    UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                    //    Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                    //    Nombre = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                    //    Paterno = arrApellidos[0],
                    //    Materno = arrApellidos[1],
                    //    Ficha = emailU.Substring(0, emailU.IndexOf("@cecytechihuahua.edu.mx")),
                    //    Genero = "M",
                    //    NombreSecundaria = "",
                    //    PromedioSecundaria = 7,
                    //    TipoSecundaria = "OTRA",
                    //    TipoSostenimientoSecundaria = "ESTATAL",
                    //    PlainPass = "123456"
                    //};
                    //var resultCrearUsuario = await _userManager.CreateAsync(user);
                    //if (resultCrearUsuario.Succeeded)
                    //{
                    //    var resultAnadirInfoExternalLogin = await _userManager.AddLoginAsync(user, info);

                    //    if (resultAnadirInfoExternalLogin.Succeeded)
                    //    {
                    //        _logger.LogInformation("Se habilito el logueo con proveedor externo del usuario {Name}.", info.LoginProvider);

                    //        await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                    //        return RedirectToLocal(returnUrl);

                    //    }
                    //    AddErrors(resultAnadirInfoExternalLogin);

                    //}
                    //AddErrors(resultCrearUsuario);

                    return View("SoloUsuariosPreRegistrados");
                }
                else
                {   // El usuario ya existe pero no tiene habilitado el inicio por google
                    var resultAnadirInfoExternalLogin = await _userManager.AddLoginAsync(usuarioExistente, info);

                    if (resultAnadirInfoExternalLogin.Succeeded)
                    {
                        _logger.LogInformation("Se habilito el logueo con proveedor externo del usuario {Name}.", info.LoginProvider);

                        await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                        return RedirectToLocal(returnUrl);
                    }
                    AddErrors(resultAnadirInfoExternalLogin);

                }

                return RedirectToAction(nameof(Login));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
