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
                    MarkaId = x.MarkaID,
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
        public ActionResult ArizaYonetimi()
        {
            List<ArizaViewModel> arizalar = new ArizaRepo().GetAll().OrderByDescending(y => y.EklemeTarihi).Select(x => new ArizaViewModel()
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

            var userManager = MembershipTools.NewUserManager();
            List<ApplicationUser> kullanicilar = userManager.Users.ToList();
            var BosTeknisyenIdleri = new ArizaRepo().GetAll();
            foreach (var item in BosTeknisyenIdleri)
            {
                if (!item.ArizaYapildiMi)
                {
                    kullanicilar.Remove(userManager.FindById(item.TeknikerID));
                }
            }
            var Kullanicilar = kullanicilar.Select(x => new KullaniciViewModel()
            {
                ID = x.Id,
                Email = x.Email,
                KullaniciAdi = x.UserName,
                Rol = MembershipTools.NewRoleManager().FindById(x.Roles.FirstOrDefault().RoleId).Name
            }).Where(x => x.Rol == "Teknisyen").ToList();
            var Teknisyenler = new List<SelectListItem>();
            Kullanicilar.ForEach(x => Teknisyenler.Add(new SelectListItem()
            {
                Text = x.KullaniciAdi,
                Value = x.ID.ToString()
            }));
            ViewBag.teknikerler = Teknisyenler;
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
            ariza.TeknikerID = model.TeknikerID;
            if (ariza.TeknikerID != null)
            {
                #region Kullanıcı Bilgilendirme

                var userManager = MembershipTools.NewUserManager();
                var Teknisyen = userManager.FindById(ariza.TeknikerID);


                string SiteUrl = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host +
(Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);

                if (ariza.OnaylandiMi == true && model.OnaylandiMi == false)
                {
                    await SiteSettings.SendMail(new MailModel()
                    {
                        Message = $"Merhaba {Teknisyen.Name}<br/><strong>'{ariza.ID}'</strong> nolu Arıza sistemden kaldırılmıştır. Yapacağınız işlemleri durdurmanız rica olunur.<br/>",
                        Subject = "Arıza Sistemden kaldırıldı",
                        To = Teknisyen.Email
                    });
                }
                else if (ariza.OnaylandiMi == false && model.OnaylandiMi == true)
                {
                    ariza.OnaylamaTarihi = DateTime.Now;
                    await SiteSettings.SendMail(new MailModel()
                    {
                        Message = $"Merhaba {Teknisyen.Name}<br/><strong>'{ariza.ID}'</strong> nolu arıza sisteme alınmıştır<br/><a href='{SiteUrl}/Teknisyen/ArizaDetay/{ariza.ID}'>Arızayı görmek için tıklayınız</a>",
                        Subject = "Arızanız sisteme alındı!",
                        To = Teknisyen.Email
                    });
                }
                #endregion

            }
            ariza.OnaylandiMi = model.OnaylandiMi;
            new ArizaRepo().Update();
            return RedirectToAction("ArizaDetay", new { id = ariza.ID });
        }
        public ActionResult AnketDetay(int? id)
        {
            if (id == null)
                return RedirectToAction("AnketYonetimi");
            var anket = new AnketRepo().GetByID(id.Value);
            if (anket == null)
                return RedirectToAction("AnketYonetimi");
            var userManager = MembershipTools.NewUserManager();
            var Kullanici = userManager.FindById(anket.KullaniciID);
            var Teknisyen = userManager.FindById(anket.TeknikerID);
            var model = new AnketViewModel()
            {
                ID = anket.ID,
                Aciklama = anket.Aciklama,
                ArizaID = anket.ArizaID,
                KullaniciID = Kullanici.Name,
                Puan = anket.Puan,
                TeknikerID = Teknisyen.Name
            };
            return View(model);
        }
        public ActionResult AnketYonetimi()
        {
            List<AnketViewModel> Anketler = new AnketRepo().GetAll().OrderByDescending(y => y.ID).Select(x => new AnketViewModel()
            {
                ID = x.ID,
                TeknikerID = x.TeknikerID,
                Aciklama = x.Aciklama,
                ArizaID = x.ArizaID,
                KullaniciID = x.KullaniciID,
                Puan = x.Puan

            }).ToList();
            return View(Anketler);
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
        public JsonResult modelDoldurSetting(int? markaid)
        {
            ViewBag.SecilenMarkaID = markaid;
            return Json(new
            {
                success = true,
                message = markaid
            }, JsonRequestBehavior.AllowGet);
        }
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
        #region PartialViews
        public PartialViewResult AdminMenu()
        {
            return PartialView("_adminmenuPartial");
        }
        #endregion
    }
}

