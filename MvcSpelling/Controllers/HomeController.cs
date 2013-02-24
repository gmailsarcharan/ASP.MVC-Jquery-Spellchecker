using MvcSpelling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcSpelling.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Bio()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Bio(BiographyViewModel model)
        {
            return View();
        }

    }
}
