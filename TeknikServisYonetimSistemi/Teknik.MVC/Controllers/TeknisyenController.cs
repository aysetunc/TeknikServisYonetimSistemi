using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.BLL.Account;
using Teknik.BLL.Repository;
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

        #region PartialViews
        public PartialViewResult TeknisyenMenu()
        {
            return PartialView("_teknisyenmenuPartial");
        }
        #endregion
    }
}