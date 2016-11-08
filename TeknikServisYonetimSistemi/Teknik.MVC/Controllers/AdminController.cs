using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Teknik.BLL.Account;
using Teknik.BLL.Repository;
using Teknik.Entity.IdentityModels;
using Teknik.Entity.ViewModels;

namespace Teknik.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Settings()
        {
            var userManager = MembershipTools.NewUserManager();
            List<ApplicationUser> kullanicilar = userManager.Users.ToList();

            var model = new SettingsViewModel()
            {
                PcMarkalari = new PcMarkaRepo().GetAll().Select(x => new PcMarkaViewModel()
                {
                    ID = x.ID,
                    MarkaAdi = x.MarkaAdi
                }).ToList(),
                PcModelleri = new PcModelRepo().GetAll().Select(x => new PcModelViewModel()
                {
                    ID = x.ID,
                    ModelAdi = x.ModelAdi
                }).ToList(),
                Kullanicilar = kullanicilar.Select(x => new KullaniciViewModel()
                {
                    ID = x.Id,
                    Email = x.Email,
                    KullaniciAdi = x.UserName,
                    Rol = MembershipTools.NewRoleManager().FindById(x.Roles.FirstOrDefault().RoleId).Name
                }).ToList(),
                 Yetkiler=MembershipTools.NewRoleManager().Roles.ToList().Select(x=> new YönetimYetkiViewModel()
                 {
                      Yetki=x.Name
                 }).ToList()
            };
            return View(model);
        }

        #region PartialViews
        public PartialViewResult AdminMenu()
        {
            return PartialView("_adminmenuPartial");
        }
        #endregion
    }

}