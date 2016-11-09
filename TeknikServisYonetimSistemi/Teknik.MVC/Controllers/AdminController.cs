using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Teknik.BLL.Account;
using Teknik.BLL.Repository;
using Teknik.Entity.Entities;
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
                Yetkiler = MembershipTools.NewRoleManager().Roles.ToList().Select(x => new YönetimYetkiViewModel()
                {
                    Yetki = x.Name
                }).ToList()
            };
            var roller = new List<SelectListItem>();
            MembershipTools.NewRoleManager().Roles.ToList().ForEach(r => roller.Add(new SelectListItem()
            {
                Text = r.Name,
                Value = r.Id
            }));
            ViewBag.roller = roller;
            var markalar = new List<SelectListItem>();
            new PcMarkaRepo().GetAll().ForEach(m =>

            markalar.Add(new SelectListItem()
            {
                Text = m.MarkaAdi,
                Value = m.ID.ToString()
            }));
            ViewBag.markalar = markalar;
            return View(model);
        }

        #region JsonResults
        [HttpPost]
        public JsonResult YeniMarkaAdi(string markaadi)
        {
            if (string.IsNullOrEmpty(markaadi.Trim()))
                return Json(new
                {
                    success = false,
                    message = "Boş geçme"
                }, JsonRequestBehavior.AllowGet);
            var marka = new PcMarkaRepo().GetAll().Where(x => x.MarkaAdi == markaadi).FirstOrDefault();
            if (marka != null)
                return Json(new
                {
                    success = false,
                    message = $"Zaten {marka.MarkaAdi} adında bir kayıt var"
                }, JsonRequestBehavior.AllowGet);
            try
            {
                new PcMarkaRepo().Insert(new PcMarka()
                {
                    MarkaAdi = markaadi
                });
                return Json(new
                {
                    success = true,
                    message = $"{markaadi} Kaydı Eklenmiştir"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"{markaadi} Kaydı Eklenemedi=> {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult markaGetir(int? id)
        {
            if (id == null)
                return Json(new
                {
                    success = false,
                    message = "Boş geçme"
                }, JsonRequestBehavior.AllowGet);
            var marka = new PcMarkaRepo().GetByID(id.Value);
            return Json(new
            {
                success = true,
                message = marka.MarkaAdi
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult markaguncelle(int? markaid, string markaadi)
        {
            if (markaid == null || string.IsNullOrEmpty(markaadi.Trim()))
                return Json(new
                {
                    success = false,
                    message = "Boş Geçme"
                }, JsonRequestBehavior.AllowGet);
            try
            {
                var marka = new PcMarkaRepo().GetByID(markaid.Value);
                marka.MarkaAdi = markaadi;
                new PcMarkaRepo().Update();
                return Json(new
                {
                    success = true,
                    message = "Güncelleme Başarılı"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Güncelleme Başarısız:> {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<JsonResult> RolGuncelle(string id, string rolename, string oldrole)
        {
            var userStore = MembershipTools.NewUserStore();
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindById(id);
            try
            {
                userManager.RemoveFromRole(user.Id, oldrole);
                userManager.AddToRole(user.Id, rolename);

                user.PreRole = oldrole;
                await userStore.UpdateAsync(user);
                await userStore.Context.SaveChangesAsync();
                return Json(new
                {
                    success = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Hata oluştu"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult YeniModelAdi(int markaid, string markaadi, string yenimodeladi)
        {
            if (string.IsNullOrEmpty(markaadi.Trim()) && string.IsNullOrEmpty(yenimodeladi.Trim()))
                return Json(new
                {
                    success = false,
                    message = "Boş geçme"
                }, JsonRequestBehavior.AllowGet);
            var model = new PcModelRepo().GetAll().Where(x => x.ModelAdi == yenimodeladi).FirstOrDefault();
            if (model != null)
                return Json(new
                {
                    success = false,
                    message = $"Zaten {model.ModelAdi} adında bir kayıt var"
                }, JsonRequestBehavior.AllowGet);
            var marka = new PcMarkaRepo().GetByID(markaid);
            try
            {
                new PcModelRepo().Insert(new PcModel()
                {
                    MarkaID = marka.ID,
                    ModelAdi = yenimodeladi
                });
                return Json(new
                {
                    success = true,
                    message = $"{yenimodeladi} Kaydı Eklenmiştir"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"{yenimodeladi} Kaydı Eklenemedi=> {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult modelGetir(int? id)
        {
            if (id == null)
                return Json(new
                {
                    success = false,
                    message = "Boş geçme"
                }, JsonRequestBehavior.AllowGet);
            var model = new PcModelRepo().GetByID(id.Value);
            return Json(new
            {
                success = true,
                message = model.ModelAdi
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult modelGuncelle(int? modelid, string modeladi, int markaid)
        {
            if (modelid == null || string.IsNullOrEmpty(modeladi.Trim()))
                return Json(new
                {
                    success = false,
                    message = "Boş Geçme"
                }, JsonRequestBehavior.AllowGet);
            try
            {
                var model = new PcModelRepo().GetByID(modelid.Value);
                model.MarkaID = markaid;
                model.ModelAdi = modeladi;
                new PcModelRepo().Update();
                return Json(new
                {
                    success = true,
                    message = "Güncelleme Başarılı"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Güncelleme Başarısız:> {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region PartialViews
        public PartialViewResult AdminMenu()
        {
            return PartialView("_adminmenuPartial");
        }
        #endregion
    }
}

