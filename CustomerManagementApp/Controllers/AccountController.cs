using CustomerManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Controllers
{
    /*
     * Wenn eine Seite passwortgeschützt ist wird der Anwender zum Account-Controller
     * umgeleitet damit er sich authorisieren kann. Bei erfolgreicher überprüfung erhält
     * er zugang zur entsprechenden Seite.
     */
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;

        public AccountController(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr)
        {
            userManager = userMgr;
            signInManager = signinMgr;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel details, string returnUrl)
        {
            if(ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(details.Email);
                if(user != null)
                {
                    // SignOutAsync beendet eine eventuell laufende Anwendersession
                    await signInManager.SignOutAsync();
                    // PasswordSignInAsync führt die Anwenderauthentifizierung durch
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, details.Password, false, false);
                    if(result.Succeeded)
                    {
                        /* 
                         * Im Erfolgsfall wird der Anwender weitergeleitet. Identity fügt automatisch
                         * ein Cookie hinzu das die neue Anwendersession identifiziert.
                        */
                        return Redirect(returnUrl ?? "/");
                    }
                }
                ModelState.AddModelError(nameof(LoginModel.Email), "Unzulässiger Anwendername oder Passwort");
            }
            return View(details);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Name");
        }
    }
}
