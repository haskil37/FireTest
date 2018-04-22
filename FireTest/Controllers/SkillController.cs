using FireTest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
//using static FireTest.Models.Worker;

namespace FireTest.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class SkillController : Controller
    {
        private IFireTestService service;

        public SkillController()
        {
            this.service = new FireTestService(System.Web.HttpContext.Current.User.Identity.GetUserId());
        }

        public ActionResult Index(int skill = 1)
        {
            var result = service.New(FireTestType.Skill);
            if (!result)
                return RedirectToAction("Continue");

            return View(service.SkillIndexViewModel(skill));
        }
    }
}