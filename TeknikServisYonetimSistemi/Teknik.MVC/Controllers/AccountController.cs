using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Teknik.BLL.Account;
using Teknik.BLL.Settings;
using Teknik.Entity.IdentityModels;
using Teknik.Entity.ViewModels;

namespace Teknik.MVC.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(LoginAndRegisterViewModel Model)
        {
            if (!ModelState.IsValid)
                return View(Model);
            var userManager = MembershipTools.NewUserManager();
            var checkUser = userManager.FindByName(Model.Register.Name);
            if (checkUser != null)
            {
                ModelState.AddModelError(string.Empty, "Bu kullanıcı zaten kayıtlı !");
                return View(Model);
            }
            var aktivasyonKodu = Guid.NewGuid().ToString().Replace("-", "");
            var user = new ApplicationUser()
            {
                UserName = Model.Register.Username,
                Name = Model.Register.Name,
                Surname = Model.Register.Surname,
                PhoneNumber = Model.Register.PhoneNumber,
                Email = Model.Register.Email,
                ActivationCode = aktivasyonKodu
            };
            var sonuc = userManager.Create(user, Model.Register.Password);
            if (sonuc.Succeeded)
            {
                if (userManager.Users.ToList().Count == 1)
                {
                    userManager.AddToRole(user.Id, "Admin");
                    await SiteSettings.SendMail(new MailModel()
                    {
                        To = user.Email,
                        Subject = "Hoşgeldiniz",
                        Message = $"Merhaba {user.UserName}, </br> Sisteme Admin rolünde kayıt oldunuz. <br/><a href='http://localhost:14320/Account/Profile'>Profil Sayfanız</a>"
                    });
                }
                else
                {
                    userManager.AddToRole(user.Id, "Passive");
                    await SiteSettings.SendMail(new MailModel()
                    {
                        To = user.Email,
                        Subject = "Hoşgeldiniz",
                        Message = $"Merhaba {user.UserName}, </br> Sisteme başarı ile kayıt oldunuz. <br/> Hesabınızı aktifleştirmek için <a href='http://localhost:14320/Account/Activation?code={aktivasyonKodu}'>Aktivasyon Kodu</a>"
                    });
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı kayıt işleminde hata oluştu!");
                return View(Model);
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginAndRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var userManager = MembershipTools.NewUserManager();
            var user = await userManager.FindAsync(model.Login.KullaniciAdi, model.Login.Sifre);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Böyle bir kullanıcı bulunamadı.");
                return View(model);
            }
            var authManager = HttpContext.GetOwinContext().Authentication;
            var userIdentity = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            authManager.SignIn(new AuthenticationProperties()
            {
                IsPersistent = model.Login.HatirlansinMi
            }, userIdentity);
            return RedirectToAction("Index", "Home");
        }
        [Authorize]
        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut();
            return RedirectToAction("Index", "Home");
        }
        [Authorize]
        public ActionResult Profile()
        {
            var userManager = MembershipTools.NewUserManager();
            var id = HttpContext.User.Identity.GetUserId();
            var user = userManager.FindById(id);

            ProfileViewModel model = new ProfileViewModel()
            {
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                Username = user.UserName,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }
    }
}