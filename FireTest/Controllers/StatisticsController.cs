using FireTest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    public class StatisticsController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public PartialViewResult Index(int id, string userId)
        {
            switch (id)
            {
                case 1:
                    break;
                default:
                    return PartialView("CountBattles", new { userId = userId });
            }

            return PartialView();
        }
        public PartialViewResult CountBattles(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                userId = User.Identity.GetUserId();
            ApplicationUser user = dbContext.Users.Find(userId);

            ViewBag.TitleChart = "Поединки";
            ViewBag.BattlesLose = user.BattleCount - user.BattleWinCount;
            ViewBag.BattlesWin = user.BattleWinCount;
            return PartialView();
        }
    }
}