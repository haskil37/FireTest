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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Groups()
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
        public PartialViewResult Groups(string Groups, int Statistics, int? DateRange)
        {
            if (DateRange == null || Statistics != 2)
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
                        return PartialView("GroupsBattles");
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
                        return PartialView("GroupsAnswers");
                    case 2:
                        return PartialView("SelectDateRangeGroups");
                }
            }
            else
            {
                var users = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Course + u.Group == Groups)
                    .Select(u => u.Id).ToList();
                int range = 0;

                List<bool> selected = new List<bool>() { false, false, false };

                switch (DateRange)
                {
                    case 1:
                        range = 1;
                        selected[0] = true;
                        ViewBag.Range = "за послений месяц";
                        break;
                    case 2:
                        range = 3;
                        selected[1] = true;
                        ViewBag.Range = "за последние 3 месяца";
                        break;
                    case 3:
                        range = 6;
                        selected[2] = true;
                        ViewBag.Range = "за последние 6 месяцев";
                        break;
                }
                var today = DateTime.Today;
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = beforeMonth; date <= today; date = date.AddDays(1))
                    allDates.Add(date);
                string osX = "";
                List<int> osYD = new List<int>();
                List<int> osYQ = new List<int>();
                foreach (var item in allDates)
                {
                    osX += item.Day + "." + item.Month + ",";
                    osYD.Add(0);
                    osYQ.Add(0);
                }
                osX = osX.Substring(0, osX.Length - 1);
                foreach (var item in users) //Квалификации
                {
                    var allSelfyQualifications = dbContext.SelfyTestQualifications.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart <= today).Select(u => u.TimeStart).ToList();
                    foreach (var date in allSelfyQualifications)
                    {
                        var tempDate = new DateTime(today.Year, date.Month, date.Day);
                        var index = allDates.IndexOf(tempDate);
                        osYD[index] += 1;
                    }
                    var allSelfyDisciplines = dbContext.SelfyTestDisciplines.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart <= today).Select(u => u.TimeStart).ToList();
                    foreach (var date in allSelfyDisciplines)
                    {
                        var tempDate = new DateTime(today.Year, date.Month, date.Day);
                        var index = allDates.IndexOf(tempDate);
                        osYQ[index] += 1;
                    }

                }
                ViewBag.osXD = osX;
                ViewBag.osYD = "";
                foreach (var item in osYD)
                    ViewBag.osYD += item + ",";

                ViewBag.osXQ = osX;
                ViewBag.osYQ = "";
                foreach (var item in osYQ)
                    ViewBag.osYQ += item + ",";

                ViewBag.DateRange = new[]{
                 new SelectListItem{ Text=" -- Выберите период -- ", Disabled=true},
                 new SelectListItem{ Value="1",Text="За послений месяц", Selected=selected[0]},
                 new SelectListItem{ Value="2",Text="За последние 3 месяца", Selected=selected[1]},
                 new SelectListItem{ Value="3",Text="За последние 6 месяцев", Selected=selected[2]},
                }.ToList();

                return PartialView("MonthDateRangeGroups");
            }
            return PartialView();
        }
        public ActionResult Courses()
        {
            var courses = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Course + u.Group.Substring(0, 1)).Distinct().ToList();
            if (courses == null)
                RedirectToAction("Index", "Home");
            var selectList = courses //Выпадающий список групп
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
            ViewBag.Courses = selectList;
            return View();
        }
        [HttpPost]
        public PartialViewResult Courses(string Courses, int Statistics, int? DateRange)
        {
            if (DateRange == null || Statistics != 2)
            {
                switch (Statistics)
                {
                    case 0:
                        var users = dbContext.Users
                            .Where(u => u.Course != 100)
                            .Where(u => u.Course + u.Group.Substring(0, 1) == Courses)
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

                        ViewBag.TitleChart = Courses;
                        ViewBag.BattlesLose = count - winCount;
                        ViewBag.BattlesWin = winCount;
                        return PartialView("CoursesBattles");
                    case 1:
                        var users2 = dbContext.Users
                            .Where(u => u.Course != 100)
                            .Where(u => u.Course + u.Group.Substring(0, 1) == Courses)
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
                        ViewBag.TitleChart = Courses;
                        ViewBag.Answers = answers - correctAnswers;
                        ViewBag.AnswersCorrect = correctAnswers;
                        return PartialView("CoursesAnswers");
                    case 2:
                        return PartialView("SelectDateRangeCourses");
                }
            }
            else
            {
                var users = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Course + u.Group.Substring(0, 1) == Courses)
                    .Select(u => u.Id).ToList();
                int range = 0;

                List<bool> selected = new List<bool>() { false, false, false };

                switch (DateRange)
                {
                    case 1:
                        range = 1;
                        selected[0] = true;
                        ViewBag.Range = "за послений месяц";
                        break;
                    case 2:
                        range = 3;
                        selected[1] = true;
                        ViewBag.Range = "за последние 3 месяца";
                        break;
                    case 3:
                        range = 6;
                        selected[2] = true;
                        ViewBag.Range = "за последние 6 месяцев";
                        break;
                }

                var today = DateTime.Today;
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = beforeMonth; date <= today; date = date.AddDays(1))
                    allDates.Add(date);
                string osX = "";
                List<int> osYD = new List<int>();
                List<int> osYQ = new List<int>();
                foreach (var item in allDates)
                {
                    osX += item.Day + "." + item.Month + ",";
                    osYD.Add(0);
                    osYQ.Add(0);
                }
                osX = osX.Substring(0, osX.Length - 1);
                foreach (var item in users) //Квалификации
                {
                    var allSelfyQualifications = dbContext.SelfyTestQualifications.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart <= today).Select(u => u.TimeStart).ToList();
                    foreach (var date in allSelfyQualifications)
                    {
                        var tempDate = new DateTime(today.Year, date.Month, date.Day);
                        var index = allDates.IndexOf(tempDate);
                        osYD[index] += 1;
                    }
                    var allSelfyDisciplines = dbContext.SelfyTestDisciplines.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart <= today).Select(u => u.TimeStart).ToList();
                    foreach (var date in allSelfyDisciplines)
                    {
                        var tempDate = new DateTime(today.Year, date.Month, date.Day);
                        var index = allDates.IndexOf(tempDate);
                        osYQ[index] += 1;
                    }

                }
                ViewBag.osXD = osX;
                ViewBag.osYD = "";
                foreach (var item in osYD)
                    ViewBag.osYD += item + ",";

                ViewBag.osXQ = osX;
                ViewBag.osYQ = "";
                foreach (var item in osYQ)
                    ViewBag.osYQ += item + ",";

                ViewBag.DateRange = new[]{
                 new SelectListItem{ Text=" -- Выберите период -- ", Disabled=true},
                 new SelectListItem{ Value="1",Text="За послений месяц", Selected=selected[0]},
                 new SelectListItem{ Value="2",Text="За последние 3 месяца", Selected=selected[1]},
                 new SelectListItem{ Value="3",Text="За последние 6 месяцев", Selected=selected[2]},
                }.ToList();

                return PartialView("MonthDateRangeCourses");
            }
            return PartialView();
        }
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
    }
}