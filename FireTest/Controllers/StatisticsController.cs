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

        public PartialViewResult UserStatistics()
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser user = dbContext.Users.Find(userId);
            ViewBag.BattlesLose = user.BattleCount - user.BattleWinCount;
            ViewBag.BattlesWin = user.BattleWinCount;
            ViewBag.Answers = user.AnswersCount - user.CorrectAnswersCount;
            ViewBag.AnswersCorrect = user.CorrectAnswersCount;

            List<int> RightQ = new List<int>();
            for (int i = 1; i <= 5; i++) //От количества квалификаций
            {
                List<string> tempAllRightOrWrong = dbContext.SelfyTestQualifications
                    .Where(u => u.IdUser == userId)
                    .Where(u => u.End == true)
                    .Where(u => u.IdQualification == i)
                    .Select(u => u.RightOrWrong).ToList();
                List<string> tempRoW = new List<string>();
                foreach (string item in tempAllRightOrWrong)
                {
                    var temp = item.Split('|').ToList();
                    foreach (string item2 in temp)
                        tempRoW.Add(item2);
                }
                int right = tempRoW.Where(u => u != "0").Count();
                if (tempRoW.Count() != 0)
                    RightQ.Add(right * 100 / tempRoW.Count());
                else
                    RightQ.Add(0);
            }
            ViewBag.QualificationRight = RightQ;

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
        public PartialViewResult CountAnswers(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                userId = User.Identity.GetUserId();
            ApplicationUser user = dbContext.Users.Find(userId);

            ViewBag.TitleChart = "Ответы на вопросы";
            ViewBag.Answers = user.AnswersCount - user.CorrectAnswersCount;
            ViewBag.AnswersCorrect = user.CorrectAnswersCount;
            return PartialView();
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public PartialViewResult Index(int id, string userId) //Это для учителя
        {
            switch (id)
            {
                case 1:
                    CountBattles(userId);
                    return PartialView("CountBattles");
                default:
                    CountAnswers(userId);
                    return PartialView("CountAnswers");
            }
        }
    }
}