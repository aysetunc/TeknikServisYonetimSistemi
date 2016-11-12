using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Teknik.BLL.Account;
using Teknik.BLL.Repository;
using Teknik.BLL.Settings;
using Teknik.Entity.Entities;
using Teknik.Entity.IdentityModels;
using Teknik.Entity.ViewModels;

namespace Teknik.MVC.Controllers
{
    public class TeknisyenController : Controller
    {
        // GET: Teknisyen
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ArizaYonetimi()
        {
            var userManager = MembershipTools.NewUserManager();
            var id = HttpContext.User.Identity.GetUserId();
            var user = userManager.FindById(id);
            List<ArizaViewModel> arizalar = new ArizaRepo().GetAll().Where(z => z.TeknikerID == user.Id).OrderByDescending(y => y.EklemeTarihi).Select(x => new ArizaViewModel()
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
                OnaylandiMi = x.OnaylandiMi,
                ArizaYapildiMi = x.ArizaYapildiMi,
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
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> ArizaDuzenle(ArizaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("ArizaYonetimi");
            }
            var ariza = new ArizaRepo().GetByID(model.ID);
            ArizaBilgilendirme bilgilendirme = new ArizaBilgilendirme()
            {
                Aciklama = model.BilgilendirmeAciklamasi,
                ArizaID = model.ID,
                YoneticiID = HttpContext.User.Identity.GetUserId()
            };
            bool aciklamaVarMi = false;
            if (!string.IsNullOrEmpty(model.BilgilendirmeAciklamasi))
            {
                ariza.Bilgilendirmeleri.Add(bilgilendirme);
                aciklamaVarMi = true;
            }
            ariza.ArizaYapildiMi = model.ArizaYapildiMi;
            #region Kullanıcı Bilgilendirme

            string SiteUrl = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host +
(Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            if (aciklamaVarMi)
            {
                await SiteSettings.SendMail(new MailModel()
                {
                    Message = $"Merhaba {ariza.Gondereni.Name}<br/><strong>'{ariza.ID}'</strong> nolu Arızanız için bir bildirim var<br/><p>Bildirim: <em>{model.BilgilendirmeAciklamasi}</em></p><a href='{SiteUrl}/Ariza/ArizaDetay/{ariza.ID}'>Arızayı görmek ve düzenlemek için tıklayınız</a>",
                    Subject = "Arızanız için yeni bir bildirim var",
                    To = ariza.Gondereni.Email
                });
            }
            else if (model.ArizaYapildiMi)
            {
                await SiteSettings.SendMail(new MailModel()
                {
                    Message = $"Merhaba {ariza.Gondereni.Name}<br/><strong>'{ariza.ID}'</strong> nolu Arızanız sistemimize göre onarıldı . Lütfen anketimize katılın .<br/><a href='{SiteUrl}/Ariza/ArizaAnket/{ariza.ID}'>Anketi doldurmak için tıklayınız</a>",
                    Subject = "Arızanız için yeni bir bildirim var",
                    To = ariza.Gondereni.Email
                });
            }
            #endregion
            
            new ArizaRepo().Update();
            return RedirectToAction("ArizaDetay", new { id = ariza.ID });
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

        #region PartialViews
        public PartialViewResult TeknisyenMenu()
        {
            return PartialView("_teknisyenmenuPartial");
        }
        #endregion
    }
}