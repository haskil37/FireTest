using FireTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize]
    public class ExaminationController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        public ActionResult Access()
        {
            return View();
        }
    }
}