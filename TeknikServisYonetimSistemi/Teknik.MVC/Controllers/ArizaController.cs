using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Teknik.BLL.Account;
using Teknik.BLL.Repository;
using Teknik.BLL.Settings;
using Teknik.Entity.Entities;
using Teknik.Entity.ViewModels;

namespace Teknik.MVC.Controllers
{
    public class ArizaController : Controller
    {
        // GET: Ariza
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public ActionResult ArizaEkle()
        {
            var userManager = MembershipTools.NewUserManager();
            var user = userManager.FindById(HttpContext.User.Identity.GetUserId());
            if (userManager.IsInRole(user.Id, "Passive") || userManager.IsInRole(user.Id, "Banned"))
            {
                ModelState.AddModelError(string.Empty, "Profiliniz Yeni ilan açmak için uygun değildir.");
                return RedirectToAction("Profile", "Account");
            }
            var model = new ArizaViewModel();
            var markalar = new List<SelectListItem>();
            new PcMarkaRepo().GetAll().OrderBy(x => x.MarkaAdi).ToList().ForEach(x =>
            markalar.Add(new SelectListItem()
            {
                Text = x.MarkaAdi,
                Value = x.ID.ToString()
            }));
            var modeller = new List<SelectListItem>();
            new PcModelRepo().GetAll().OrderBy(x => x.ModelAdi).ToList().ForEach(x =>
               modeller.Add(new SelectListItem()
               {
                   Text = x.ModelAdi,
                   Value = x.ID.ToString()
               }));
            ViewBag.modelleri = modeller;
            ViewBag.markalari = markalar;
            return View(model);
        }
        
        [HttpPost, ValidateInput(false)]
        [Authorize]
        public async Task<ActionResult> ArizaEkle(ArizaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Errör");
                return View();
            }
            Ariza yeniAriza = new Ariza()
            {
                Baslik = model.Baslik,
                Aciklama = model.Aciklama,
                Adres = model.Adres,
                Boylam = model.Boylam,
                Enlem = model.Enlem,
                ModelID = model.ModelID,
                MarkaID = model.MarkaID,
                KullaniciID = HttpContext.User.Identity.GetUserId()
            };
            new ArizaRepo().Insert(yeniAriza);
            if (model.Dosyalar.Count>0)
            {
                model.Dosyalar.ForEach(file =>
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        string extName = Path.GetExtension(file.FileName);
                        fileName = fileName.Replace(" ", "");
                        fileName += Guid.NewGuid().ToString().Replace("-", "");
                        fileName = SiteSettings.UrlFormatConverter(fileName);
                        var klasoryolu = Server.MapPath("~/Upload/" + yeniAriza.ID);
                        var dosyayolu = Server.MapPath("~/Upload/" + yeniAriza.ID + "/") + fileName + extName;
                        if (!Directory.Exists(klasoryolu))
                            Directory.CreateDirectory(klasoryolu);
                        file.SaveAs(dosyayolu);
                        WebImage img = new WebImage(dosyayolu);
                        img.Resize(870, 480, false);
                        img.AddTextWatermark("Pc-Service", "RoyalBlue", opacity: 75, fontSize: 25, fontFamily: "Verdana", horizontalAlign: "Left");
                        img.Save(dosyayolu);
                        new FotografRepo().Insert(new Fotograf()
                        {
                            ArizaID = yeniAriza.ID,
                            Yol = @"Upload/" + yeniAriza.ID + "/" + fileName + extName
                        });
                    }
                });
            }
            string SiteUrl = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host +
(Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            var roleManager = MembershipTools.NewRoleManager();
            var users = roleManager.FindByName("Admin").Users;

            var userManager = MembershipTools.NewUserManager();
            List<string> mailler = new List<string>();

            foreach (var item in users)
            {
                mailler.Add(userManager.FindById(item.UserId).Email);
            }

            foreach (var mail in mailler)
            {
                await SiteSettings.SendMail(new MailModel
                {
                    Subject = "Yeni Arıza Bildirimi",
                    Message = $"Sayın Operatör,<br/>Sitenize bir arıza eklendi, Lütfen gereken işlemleri gerçekleştirin.<br/><a href='{SiteUrl}/Admin/ArizaDetay/{yeniAriza.ID}'>Haydi Onayla</a><p>İyi Çalışmalar<br/>Sitenin Nöbetçisi</p>",
                    To = mail
                });
            }
            return RedirectToAction("Index", "Home");
        }

        #region JsonResults
        [HttpPost]
        public JsonResult modelDoldur(int? markaid)
        {
            var modeller = new List<SelectListItem>();
            new PcModelRepo().GetAll().Where(y => y.MarkaID == markaid).OrderBy(x => x.ModelAdi).ToList().ForEach(x =>
               modeller.Add(new SelectListItem()
               {
                   Text = x.ModelAdi,
                   Value = x.ID.ToString()
               }));
            ViewBag.modelleri = modeller;
            return Json(new
            {
                success = true,
                message = modeller
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}