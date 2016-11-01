using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Teknik.MVC.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        #region PartialViewResults
        public PartialViewResult menuPartial()
        {
            return PartialView("_menuPartial");
        }
        public PartialViewResult footerPartial()
        {
            return PartialView("_footerPartial");
        }
        #endregion
    }
}