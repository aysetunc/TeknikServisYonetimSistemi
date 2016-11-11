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
                return RedirectToAction("Login", "Account");
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

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var userStore = MembershipTools.NewUserStore();
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindById(HttpContext.User.Identity.GetUserId());
            if (user.Email != model.Email)
            {
                userManager.AddToRole(user.Id, "Passive");
                user.Email = model.Email;
                user.EmailConfirmed = false;
                if (HttpContext.User.IsInRole("User"))
                {
                    userManager.RemoveFromRole(user.Id, "User");
                    user.PreRole = "User";
                }
                else if (HttpContext.User.IsInRole("Teknisyen"))
                {
                    userManager.RemoveFromRole(user.Id, "Teknisyen");
                    user.PreRole = "Teknisyen";
                }
                else if (HttpContext.User.IsInRole("Admin"))
                {
                    userManager.RemoveFromRole(user.Id, "Admin");
                    user.PreRole = "Admin";
                }
                var aktivasyonKodu = Guid.NewGuid().ToString().Replace("-", "");
                user.ActivationCode = aktivasyonKodu;
                string SiteUrl = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host +
(Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                await SiteSettings.SendMail(new MailModel()
                {
                    To = user.Email,
                    Subject = "Hesabınızı Aktifleştirmeniz Gerekiyor",
                    Message = $"Merhaba {user.UserName}, </br> Sistemi Kullanabilmek için Hesabınız tekrar aktifleştirmeniz gerekiyor. <br/> Hesabınızı aktifleştirmek için <a href='{SiteUrl}/Account/Activation?code={aktivasyonKodu}'>Aktivasyon Kodu</a>"
                });
                var authManager = HttpContext.GetOwinContext().Authentication;
                var userIdentity = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                authManager.SignIn(new AuthenticationProperties()
                {
                    IsPersistent = true
                }, userIdentity);
            }
            user.Name = model.Name;
            user.Surname = model.Surname;
            user.PhoneNumber = model.PhoneNumber;

            await userStore.UpdateAsync(user);
            await userStore.Context.SaveChangesAsync();

            return RedirectToAction("Profile");
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Profile");
            var userStore = MembershipTools.NewUserStore();
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindById(HttpContext.User.Identity.GetUserId());

            var checkuser = userManager.Find(user.UserName, model.OldPassword);
            if (checkuser == null)
            {
                ModelState.AddModelError(string.Empty, "Mevcut şifreniz yanlış");
                return RedirectToAction("Profile");
            }
            await userStore.SetPasswordHashAsync(user, userManager.PasswordHasher.HashPassword(model.ConfirmPassword));
            await userStore.UpdateAsync(user);
            await userStore.Context.SaveChangesAsync();
            await SiteSettings.SendMail(new MailModel()
            {
                Message = $"Merhaba {user.UserName}, </br> Şifreniz Panelden değiştirilmiştir.",
                Subject = "Şifreniz Değişti!",
                To = user.Email
            });
            return RedirectToAction("Logout");
        }
        public ActionResult RecoverPassword()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> RecoverPassword(string recover)
        {
            var userStore = MembershipTools.NewUserStore();
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindByEmail(recover);
            var user2 = userManager.FindByName(recover);
            string newpass = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            if (user == null && user2 == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı Bulunamadı");
                return View();
            }
            else if (user != null)
            {
                await userStore.SetPasswordHashAsync(user, userManager.PasswordHasher.HashPassword(newpass));
                await userStore.UpdateAsync(user);
                await userStore.Context.SaveChangesAsync();
                await SiteSettings.SendMail(new MailModel()
                {
                    To = user.Email,
                    Subject = "Yeni Parolanız",
                    Message = $"Merhaba {user.UserName} </br>Yeni Parolanız: <strong>{newpass}</strong></br>"
                });
                return RedirectToAction("Login", "Account");
            }
            else if (user2 != null)
            {
                await userStore.SetPasswordHashAsync(user2, userManager.PasswordHasher.HashPassword(newpass));
                await userStore.UpdateAsync(user2);
                await userStore.Context.SaveChangesAsync();
                await SiteSettings.SendMail(new MailModel()
                {
                    To = user2.Email,
                    Subject = "Yeni Parolanız",
                    Message = $"Merhaba {user2.UserName} </br>Yeni Parolanız: <strong>{newpass}</strong></br>"
                });
                return RedirectToAction("Login", "Account");
            }
            else
                return View();
        }
        public async Task<ActionResult> Activation(string code)
        {
            var userStore = MembershipTools.NewUserStore();
            var userManager = new UserManager<ApplicationUser>(userStore);
            var sonuc = userStore.Context.Set<ApplicationUser>().Where(x => x.ActivationCode == code).FirstOrDefault();
            if (sonuc == null)
            {
                ViewBag.sonuc = "Kod Aktivasyon Başarısız";
                return View();
            }
            sonuc.EmailConfirmed = true;
            await userStore.UpdateAsync(sonuc);
            await userStore.Context.SaveChangesAsync();

            userManager.RemoveFromRole(sonuc.Id, "Passive");
            if (string.IsNullOrEmpty(sonuc.PreRole))
                userManager.AddToRole(sonuc.Id, "User");
            else
                userManager.AddToRole(sonuc.Id, sonuc.PreRole);
            ViewBag.sonuc = $"Merhaba, {sonuc.UserName} Aktivasyon Başarılı.";
            await SiteSettings.SendMail(new MailModel()
            {
                Message = $"Merhaba, {sonuc.UserName} Aktivasyon Başarılı.",
                Subject = "Aktivasyon",
                To = sonuc.Email
            });
            HttpContext.GetOwinContext().Authentication.SignOut();
            return View();
        }
        [Authorize]
        public async Task<ActionResult> ResendActivation()
        {
            var userStore = MembershipTools.NewUserStore();
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindById(HttpContext.User.Identity.GetUserId());
            string SiteUrl = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host +
(Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            await SiteSettings.SendMail(new MailModel()
            {
                Message = $"Merhaba {user.UserName}, </br> Hesabınızı aktifleştirmek için <a href='{SiteUrl}/Account/Activation?code={user.ActivationCode}'>Aktivasyon Kodu</a>",
                Subject = "Aktivasyon Kodu",
                To = user.Email
            });
            return RedirectToAction("Profile");
        }
    }
}