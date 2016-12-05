using FireTest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize]
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
            if (user.BattleCount == 0)
            {
                ViewBag.BattlesWin = 100;
                ViewBag.BattlesLose = 100;
            }
            if (user.AnswersCount == 0)
            {
                ViewBag.Answers = 100;
                ViewBag.AnswersCorrect = 100;
            }
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
            if (user.BattleCount == 0)
            {
                ViewBag.BattlesWin = 100;
                ViewBag.BattlesLose = 100;
            }
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
            if (user.AnswersCount == 0)
            {
                ViewBag.Answers = 100;
                ViewBag.AnswersCorrect = 100;
            }
            return PartialView();
        }
        public ActionResult Index()
        {
            var groups = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Course + u.Group).Distinct().ToList();
            if (groups == null)
                RedirectToAction("Index", "Home");
            var selectList = groups //Выпадающий список групп
                .Select(u => new SelectListItem()
                    {
                        Value = u,
                        Text = u,
                    }).ToList();
            ViewBag.Groups = selectList;
            return View();
        }
        [HttpPost]
        public PartialViewResult Index(string Groups, int Statistics, int? DateRange)
        {
            if (DateRange == null)
            {
                switch (Statistics)
                {
                    case 0:
                        var users = dbContext.Users
                            .Where(u => u.Course != 100)
                            .Where(u => u.Course + u.Group == Groups)
                            .Select(u => new
                            {
                                BattleCount = u.BattleCount,
                                BattleWinCount = u.BattleWinCount
                            }).ToList();
                        int count = 0;
                        int winCount = 0;
                        foreach (var item in users)
                        {
                            count += item.BattleCount;
                            winCount += item.BattleWinCount;
                        }

                        ViewBag.TitleChart = Groups;
                        ViewBag.BattlesLose = count - winCount;
                        ViewBag.BattlesWin = winCount;
                        return PartialView("GroupBattles");
                    case 1:
                        var users2 = dbContext.Users
                            .Where(u => u.Course != 100)
                            .Where(u => u.Course + u.Group == Groups)
                            .Select(u => new
                            {
                                AnswersCount = u.AnswersCount,
                                CorrectAnswersCount = u.CorrectAnswersCount
                            }).ToList();
                        int answers = 0;
                        int correctAnswers = 0;
                        foreach (var item in users2)
                        {
                            answers += item.AnswersCount;
                            correctAnswers += item.CorrectAnswersCount;
                        }
                        ViewBag.TitleChart = Groups;
                        ViewBag.Answers = answers - correctAnswers;
                        ViewBag.AnswersCorrect = correctAnswers;
                        return PartialView("GroupAnswers");
                    case 2:
                        return PartialView("SelectDateRange");
                }
            }
            else
            {
                var users = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Course + u.Group == Groups)
                    .Select(u => u.Id).ToList();
                //За месяц
                string osX = "";
                int[] osY = new int[30];
                for (int i = 1; i <= 30; i++)
                {
                    osX += i + ",";
                    osY[i - 1] = 0;
                }
                osX = osX.Substring(0, osX.Length - 1);
                foreach (var item in users) //Квалификации
                {
                    var allSelfyQualifications = dbContext.SelfyTestQualifications.Where(u => u.IdUser == item).Select(u => u.TimeStart).ToList();
                    foreach (var date in allSelfyQualifications)
                        if (date.Month == 11)
                            osY[date.Day] += 1;
                }
                ViewBag.osX = osX;
                ViewBag.osY = "";
                foreach (var item in osY)
                    ViewBag.osY += item + ",";
                return PartialView("MonthDateRange");
            }
            return PartialView();
        }
    }
}