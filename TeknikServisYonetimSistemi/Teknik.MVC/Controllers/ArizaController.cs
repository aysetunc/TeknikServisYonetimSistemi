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
            if (model.Dosyalar.Count > 0)
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
        public ActionResult ArizaYonetimi()
        {
            List<ArizaViewModel> arizalar = new ArizaRepo().GetAll().Where(z => z.KullaniciID == HttpContext.User.Identity.GetUserId()).OrderByDescending(y => y.EklemeTarihi).Select(x => new ArizaViewModel()
            {
                KullaniciID = x.KullaniciID,
                Aciklama = x.Aciklama,
                Adres = x.Adres,
                Baslik = x.Baslik,
                Boylam = x.Boylam,
                Enlem = x.Enlem,
                MarkaID = x.MarkaID,
                ModelID = x.ModelID,
                TeknikerID = x.TeknikerID,
                FotografYollari = (x.Fotograflari.Count > 0 ? x.Fotograflari.Select(y => y.Yol).ToList() : new List<string>()),
                ID = x.ID,
                OnaylamaTarihi = x.OnaylamaTarihi,
                ArizaYapildiMi = x.ArizaYapildiMi,
                OnaylandiMi = x.OnaylandiMi,
                EklemeTarihi = x.EklemeTarihi
            }).ToList();
            return View(arizalar);
        }
        public ActionResult ArizaDetay(int? id)
        {
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
            if (id == null)
                return RedirectToAction("ArizaYonetimi");
            var ariza = new ArizaRepo().GetByID(id.Value);
            if (ariza == null)
                return RedirectToAction("ArizaYonetimi");
            var model = new ArizaViewModel()
            {
                Baslik = ariza.Baslik,
                Aciklama = ariza.Aciklama,
                Adres = ariza.Adres,
                Boylam = ariza.Boylam,
                Enlem = ariza.Enlem,
                ID = ariza.ID,
                MarkaID = ariza.MarkaID,
                ModelID = ariza.ModelID,
                OnaylamaTarihi = ariza.OnaylamaTarihi,
                FotografYollari = ariza.Fotograflari.Select(x => x.Yol).ToList(),
                Bilgilendirmeleri = new ArizaBilgilendirmeRepo().GetAll().Where(z => z.ArizaID == ariza.ID).Select(y => new ArizaBilgilendirmeViewModel()
                {
                    ID = y.ID,
                    Aciklama = y.Aciklama,
                    AciklamaZamani = y.AciklamaZamani,
                    ArizaID = y.ArizaID,
                    YoneticiID = y.YoneticiID,
                    OlumluMu = y.OlumluMu
                }).ToList(),
                KullaniciID = ariza.KullaniciID,
                TeknikerID = ariza.TeknikerID,
                ArizaYapildiMi = ariza.ArizaYapildiMi,
                OnaylandiMi = ariza.OnaylandiMi
            };
            return View(model);
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> ArizaDuzenle(ArizaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("ArizaYonetimi");
            }
            var ariza = new ArizaRepo().GetByID(model.ID);

            ariza.Aciklama = model.Aciklama;
            ariza.Adres = model.Adres;
            ariza.Baslik = model.Baslik;
            ariza.Boylam = model.Boylam;
            ariza.Enlem = model.Enlem;
            ariza.MarkaID = model.MarkaID;
            ariza.ModelID = model.ModelID;
            ariza.OnaylandiMi = false;
            #region Operator Bilgilendirme
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
                    Message = $"Sayın Operatör,<br/>Sitenize bir arıza güncellendi, Lütfen gereken işlemleri gerçekleştirin.<br/><a href='{SiteUrl}/Admin/ArizaDetay/{ariza.ID}'>Haydi Onayla</a><p>İyi Çalışmalar<br/>Sitenin Nöbetçisi</p>",
                    To = mail
                });
            }
            #endregion
            new ArizaRepo().Update();
            return RedirectToAction("ArizaDetay", new { id = ariza.ID });
        }
        public ActionResult ArizaAnket()
        {
            var model = new AnketViewModel();
            return View(model);
        }
        [HttpPost, ValidateInput(false)]
        [Authorize]
        public async Task<ActionResult> ArizaAnket(int? id,AnketViewModel model)

        {
            if (id == null)
                return RedirectToAction("Index", "Home");
            var ariza = new ArizaRepo().GetByID(id.Value);
            if (ariza == null)
                return RedirectToAction("Index", "Home");
            Anket yeniAnket = new Anket()
            {
                Aciklama = model.Aciklama,
                ArizaID = ariza.ID,
                KullaniciID = ariza.KullaniciID,
                Puan = model.Puan,
                TeknikerID = ariza.TeknikerID
            };
            new AnketRepo().Insert(yeniAnket);
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
                    Subject = "Yeni Anket Bildirimi",
                    Message = $"Sayın Operatör,<br/>Sitenize bir anket eklendi, Lütfen gereken işlemleri gerçekleştirin.<br/><a href='{SiteUrl}/Admin/AnketDetay/{yeniAnket.ID}'>Şimdi Bak</a><p>İyi Çalışmalar<br/>Sitenin Nöbetçisi</p>",
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
        [HttpPost]
        public JsonResult Resimsil(List<string> values)
        {
            try
            {
                values.ForEach(path =>
                {
                    var yol = path.Substring(1);
                    var foto = new FotografRepo().GetAll().Where(x => x.Yol == yol).FirstOrDefault();
                    new FotografRepo().Delete(foto);
                    System.IO.File.Delete(Server.MapPath(path));
                });
                return Json(new
                {
                    success = true,
                    message = "Seçili Resimler Silindi"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Resim Silme İşleminde Hata Var => {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}