using FireTest.Models;
using Microsoft.AspNet.Identity;
using PagedList;
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
        public ActionResult IndexUser()
        {
            return View();
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
                int right = 0;
                foreach (string item in tempAllRightOrWrong)
                {
                    var temp = item.Split('|').ToList();
                    temp.RemoveAll(RemoveNull);
                    right += temp.Count;
                }
                var allQuestionsThisQualification = dbContext.Questions.Where(u => u.IdQualification == i).Count();
                int value = right * 100 / allQuestionsThisQualification;
                if (value != 0)
                    RightQ.Add(value);
                else if (value > 100)
                    RightQ.Add(100);
                else
                    RightQ.Add(0);
            }
            ViewBag.QualificationRight = RightQ;

            return PartialView();
        }
        private bool RemoveNull(String s)
        {
            return s == "0";
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
        public ActionResult Groups()
        {
            //var groups = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Course + u.Group).Distinct().ToList();
            var groups = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Group).Distinct().ToList();
            if (groups == null)
                RedirectToAction("Index", "Home");
            ViewBag.Groups = groups //Выпадающий список групп
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
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
                                                        //.Where(u => u.Course + u.Group == Groups)
                            .Where(u => u.Group == Groups)
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
                            //.Where(u => u.Course + u.Group == Groups)
                            .Where(u => u.Group == Groups)
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
                    .Where(u => u.Group == Groups)
                                        //.Where(u => u.Course + u.Group == Groups)

                    .Select(u => u.Id).ToList();
                int range = 0;

                List<bool> selected = new List<bool>() { false, false, false };

                switch (DateRange)
                {
                    case 1:
                        range = 1;
                        selected[0] = true;
                        ViewBag.Range = "за последний месяц";
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
                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
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
                foreach (var item in users)
                {
                    var temp = dbContext.SelfyTestQualifications.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyQualifications = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();
                    foreach (var date in allSelfyQualifications)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYQ[index] += 1;
                    }
                    temp = dbContext.SelfyTestDisciplines.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyDisciplines = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();

                    foreach (var date in allSelfyDisciplines)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYD[index] += 1;
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
                 new SelectListItem{ Value="1",Text="За последний месяц", Selected=selected[0]},
                 new SelectListItem{ Value="2",Text="За последние 3 месяца", Selected=selected[1]},
                 new SelectListItem{ Value="3",Text="За последние 6 месяцев", Selected=selected[2]},
                }.ToList();

                return PartialView("MonthDateRangeGroups");
            }
            return PartialView();
        }
        public ActionResult Courses()
        {
            //var courses = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Course + u.Group.Substring(0, 1)).Distinct().ToList();
            var courses = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Group.Substring(0, 1)).Distinct().ToList();
            if (courses == null)
                RedirectToAction("Index", "Home");
            ViewBag.Courses = courses //Выпадающий список курсов
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
            return View();
        }
        [HttpPost]
        public PartialViewResult Courses(string Courses, int Statistics, int? DateRange)
        {
            ViewBag.TitleChart = Courses;
            if (DateRange == null || Statistics != 2)
            {
                switch (Statistics)
                {
                    case 0:
                        var users = dbContext.Users
                            .Where(u => u.Course != 100)
                            //.Where(u => u.Course + u.Group.Substring(0, 1) == Courses)
                            .Where(u => u.Group.Substring(0, 1) == Courses)
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

                        ViewBag.BattlesLose = count - winCount;
                        ViewBag.BattlesWin = winCount;
                        return PartialView("CoursesBattles");
                    case 1:
                        var users2 = dbContext.Users
                            .Where(u => u.Course != 100)
                            //.Where(u => u.Course + u.Group.Substring(0, 1) == Courses)
                            .Where(u => u.Group.Substring(0, 1) == Courses)

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
                    //.Where(u => u.Course + u.Group.Substring(0, 1) == Courses)
                    .Where(u => u.Group.Substring(0, 1) == Courses)
                    .Select(u => u.Id).ToList();
                int range = 0;

                List<bool> selected = new List<bool>() { false, false, false };

                switch (DateRange)
                {
                    case 1:
                        range = 1;
                        selected[0] = true;
                        ViewBag.Range = "за последний месяц";
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

                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
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
                foreach (var item in users)
                {
                    var temp = dbContext.SelfyTestQualifications.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyQualifications = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();
                    foreach (var date in allSelfyQualifications)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYQ[index] += 1;
                    }
                    temp = dbContext.SelfyTestDisciplines.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyDisciplines = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();

                    foreach (var date in allSelfyDisciplines)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYD[index] += 1;
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
                 new SelectListItem{ Value="1",Text="За последний месяц", Selected=selected[0]},
                 new SelectListItem{ Value="2",Text="За последние 3 месяца", Selected=selected[1]},
                 new SelectListItem{ Value="3",Text="За последние 6 месяцев", Selected=selected[2]},
                }.ToList();

                return PartialView("MonthDateRangeCourses");
            }
            return PartialView();
        }
        public ActionResult Users()
        {
            Session.Clear();
            return View();
        }
        public PartialViewResult UsersAjax(string currentFilter, string searchString, int? page, string Group)
        {
            if (Session["Group"] != null)
                if ((string)Session["Group"] != Group)
                    page = 1;
            Session["Group"] = Group;
            if (!string.IsNullOrEmpty(searchString))
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            //var groups = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Course + u.Group).Distinct().ToList();
            var groups = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Group).Distinct().ToList();
            if (groups == null)
                RedirectToAction("Index", "Home");
            ViewBag.Group = groups //Выпадающий список групп
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                    Selected = u == Group
                }).ToList();
            if (string.IsNullOrEmpty(Group))
                ViewBag.Group.Insert(0, new SelectListItem { Text = " -- Выберите группу -- ", Selected = true, Disabled = true });
            else
                ViewBag.Group.Insert(0, new SelectListItem { Text = " -- Выберите группу -- ", Disabled = true });

            List<UsersForAdmin> model = new List<UsersForAdmin>(); //т.к. нам надо только имя и ид
            //var users = dbContext.Users.Where(u => u.Course != 100).Where(u => u.Course + u.Group == Group).
            var users = dbContext.Users.Where(u => u.Course != 100).Where(u => u.Group == Group).
            Select(u => new {
                    Id = u.Id,
                    Name = u.Family + " " + u.Name + " " + u.SubName,
                }).ToList();

            foreach (var item in users)
            {
                UsersForAdmin user = new UsersForAdmin();
                if (!String.IsNullOrEmpty(searchString) && item.Name.ToLower().Contains(searchString.ToLower()))
                {
                    user.Id = item.Id;
                    user.Name = item.Name;
                    model.Add(user);
                }
                if (String.IsNullOrEmpty(searchString))
                {
                    user.Id = item.Id;
                    user.Name = item.Name;
                    model.Add(user);
                }
            }

            model = model.OrderBy(u => u.Name).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.page = pageNumber;

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult UsersStatistics(string id)
        {
            var user = dbContext.Users.Find(id);
            if (user == null)
                RedirectToAction("Index", "Home");
            ViewBag.Id = user.Id;

            Decliner decliner = new Decliner();
            string[] declineText = decliner.Decline(user.Family, user.Name, user.SubName, 4);//Меняем падеж
            ViewBag.Name = declineText[0] + " " + declineText[1] + " " + declineText[2];
            return View();
        }
        [HttpPost]
        public PartialViewResult UsersStatistics(string Id, int Statistics, int? DateRange)
        {
            Decliner decliner = new Decliner();
            var user = dbContext.Users.Find(Id);
            string[] declineText = decliner.Decline(user.Family, user.Name, user.SubName, 4);//Меняем падеж
            ViewBag.TitleChart = declineText[1];

            if (DateRange == null || Statistics != 2)
            {
                switch (Statistics)
                {
                    case 0:
                        ViewBag.BattlesLose = user.BattleCount - user.BattleWinCount;
                        ViewBag.BattlesWin = user.BattleWinCount;
                        return PartialView("UserBattles");
                    case 1:
                        ViewBag.BattlesLose = user.AnswersCount - user.CorrectAnswersCount;
                        ViewBag.BattlesWin = user.CorrectAnswersCount;
                        return PartialView("UserAnswers");
                    case 2:
                        return PartialView("SelectDateRangeUser");
                }
            }
            else
            {
                int range = 0;
                List<bool> selected = new List<bool>() { false, false, false };

                switch (DateRange)
                {
                    case 1:
                        range = 1;
                        selected[0] = true;
                        ViewBag.Range = "за последний месяц";
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

                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
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

                var allSelfyQualifications = dbContext.SelfyTestQualifications.
                    Where(u => u.IdUser == user.Id).
                    Where(u => u.TimeStart >= beforeMonth).
                    Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                foreach (var date in allSelfyQualifications)
                {
                    var tempDate = new DateTime(today.Year, date.Month, date.Day);
                    var index = allDates.IndexOf(tempDate);
                    osYQ[index] += 1;
                }
                var allSelfyDisciplines = dbContext.SelfyTestDisciplines.
                    Where(u => u.IdUser == user.Id).
                    Where(u => u.TimeStart >= beforeMonth).
                    Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                foreach (var date in allSelfyDisciplines)
                {
                    var tempDate = new DateTime(today.Year, date.Month, date.Day);
                    var index = allDates.IndexOf(tempDate);
                    osYD[index] += 1;
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
                 new SelectListItem{ Value="1",Text="За последний месяц", Selected=selected[0]},
                 new SelectListItem{ Value="2",Text="За последние 3 месяца", Selected=selected[1]},
                 new SelectListItem{ Value="3",Text="За последние 6 месяцев", Selected=selected[2]},
                }.ToList();
                ViewBag.TitleChart = user.Name;

                return PartialView("MonthDateRangeUser");
            }
            return PartialView();
        }
        public ActionResult CompareGroups()
        {
            //var groups = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Course + u.Group).Distinct().ToList();
            var groups = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Group).Distinct().ToList();
            if (groups == null)
                RedirectToAction("Index", "Home");
            ViewBag.Group1 = groups //Выпадающий список групп
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
            ViewBag.Group2 = ViewBag.Group1;
            return View();
        }
        [HttpPost]
        public PartialViewResult CompareGroups(string Group1, string Group2, int Statistics, int? DateRange)
        {
            ViewBag.Group1 = Group1;
            ViewBag.Group2 = Group2;
            if (DateRange == null || Statistics != 2)
            {
                switch (Statistics)
                {
                    case 1:
                        var users1 = dbContext.Users
                           .Where(u => u.Course != 100)
                           //.Where(u => u.Course + u.Group == Group1)
                           .Where(u => u.Group == Group1)

                           .Select(u => new
                           {
                               AnswersCount = u.AnswersCount,
                               CorrectAnswersCount = u.CorrectAnswersCount
                           }).ToList();
                        var users2 = dbContext.Users
                           .Where(u => u.Course != 100)
                           //.Where(u => u.Course + u.Group == Group2)
                           .Where(u => u.Group == Group2)
                           .Select(u => new
                           {
                               AnswersCount = u.AnswersCount,
                               CorrectAnswersCount = u.CorrectAnswersCount
                           }).ToList();

                        int answers = 0;
                        int correctAnswers = 0;
                        foreach (var item in users1)
                        {
                            answers += item.AnswersCount;
                            correctAnswers += item.CorrectAnswersCount;
                        }
                        ViewBag.Answers1 = answers - correctAnswers;
                        ViewBag.AnswersCorrect1 = correctAnswers;
                        answers = 0;
                        correctAnswers = 0;
                        foreach (var item in users2)
                        {
                            answers += item.AnswersCount;
                            correctAnswers += item.CorrectAnswersCount;
                        }
                        ViewBag.Answers2 = answers - correctAnswers;
                        ViewBag.AnswersCorrect2 = correctAnswers;
                        return PartialView("CompareGroupsAnswers");
                    case 2:
                        return PartialView("SelectDateRangeGroups");
                }
            }
            else
            {
                var users1 = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Group == Group1)
                                //  .Where(u => u.Course + u.Group == Group1)
      .Select(u => u.Id).ToList();
                var users2 = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Group == Group2)
                          //  .Where(u => u.Course + u.Group == Group2)
            .Select(u => u.Id).ToList();
                int range = 0;

                List<bool> selected = new List<bool>() { false, false, false };

                switch (DateRange)
                {
                    case 1:
                        range = 1;
                        selected[0] = true;
                        ViewBag.Range = "за последний месяц";
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
                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
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
                foreach (var item in users1)
                {
                    var temp = dbContext.SelfyTestQualifications.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyQualifications = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();
                    foreach (var date in allSelfyQualifications)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYQ[index] += 1;
                    }
                    temp = dbContext.SelfyTestDisciplines.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyDisciplines = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();

                    foreach (var date in allSelfyDisciplines)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYD[index] += 1;
                    }
                }
                ViewBag.osXD = osX;
                ViewBag.osYD1 = "";
                foreach (var item in osYD)
                    ViewBag.osYD1 += item + ",";

                ViewBag.osXQ = osX;
                ViewBag.osYQ1 = "";
                foreach (var item in osYQ)
                    ViewBag.osYQ1 += item + ",";                

                osYD = new List<int>();
                osYQ = new List<int>();
                foreach (var item in allDates)
                {
                    osYD.Add(0);
                    osYQ.Add(0);
                }
                foreach (var item in users2)
                {
                    var temp = dbContext.SelfyTestQualifications.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyQualifications = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();
                    foreach (var date in allSelfyQualifications)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYQ[index] += 1;
                    }
                    temp = dbContext.SelfyTestDisciplines.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyDisciplines = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();

                    foreach (var date in allSelfyDisciplines)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYD[index] += 1;
                    }
                }
                ViewBag.osYD2 = "";
                foreach (var item in osYD)
                    ViewBag.osYD2 += item + ",";

                ViewBag.osYQ2 = "";
                foreach (var item in osYQ)
                    ViewBag.osYQ2 += item + ",";
                ViewBag.DateRange = new[]{
                 new SelectListItem{ Text=" -- Выберите период -- ", Disabled=true},
                 new SelectListItem{ Value="1",Text="За последний месяц", Selected=selected[0]},
                 new SelectListItem{ Value="2",Text="За последние 3 месяца", Selected=selected[1]},
                 new SelectListItem{ Value="3",Text="За последние 6 месяцев", Selected=selected[2]},
                }.ToList();

                return PartialView("CompareGroupsRange");
            }
            return PartialView();
        }
        public ActionResult CompareCourses()
        {
            //var courses = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Course + u.Group.Substring(0, 1)).Distinct().ToList();
            var courses = dbContext.Users.Where(u => u.Course != 100).Select(u => u.Group.Substring(0, 1)).Distinct().ToList();
            if (courses == null)
                RedirectToAction("Index", "Home");
            ViewBag.Course1 = courses //Выпадающий список курсов
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
            ViewBag.Course2 = ViewBag.Course1;
            return View();
        }
        [HttpPost]
        public PartialViewResult CompareCourses(string Course1, string Course2, int Statistics, int? DateRange)
        {
            ViewBag.Course1 = Course1;
            ViewBag.Course2 = Course2;
            if (DateRange == null || Statistics != 2)
            {
                switch (Statistics)
                {
                    case 1:
                        var users1 = dbContext.Users
                           .Where(u => u.Course != 100)
                           .Where(u => u.Group.Substring(0, 1) == Course1)
                                                     //.Where(u => u.Course + u.Group.Substring(0, 1) == Course1)
                           .Select(u => new
                           {
                               AnswersCount = u.AnswersCount,
                               CorrectAnswersCount = u.CorrectAnswersCount
                           }).ToList();
                        var users2 = dbContext.Users
                           .Where(u => u.Course != 100)
                           //.Where(u => u.Course + u.Group.Substring(0, 1) == Course2)
                           .Where(u => u.Group.Substring(0, 1) == Course2)

                           .Select(u => new
                           {
                               AnswersCount = u.AnswersCount,
                               CorrectAnswersCount = u.CorrectAnswersCount
                           }).ToList();

                        int answers = 0;
                        int correctAnswers = 0;
                        foreach (var item in users1)
                        {
                            answers += item.AnswersCount;
                            correctAnswers += item.CorrectAnswersCount;
                        }
                        ViewBag.Answers1 = answers - correctAnswers;
                        ViewBag.AnswersCorrect1 = correctAnswers;
                        answers = 0;
                        correctAnswers = 0;
                        foreach (var item in users2)
                        {
                            answers += item.AnswersCount;
                            correctAnswers += item.CorrectAnswersCount;
                        }
                        ViewBag.Answers2 = answers - correctAnswers;
                        ViewBag.AnswersCorrect2 = correctAnswers;
                        return PartialView("CompareCoursesAnswers");
                    case 2:
                        return PartialView("SelectDateRangeCourses");
                }
            }
            else
            {
                var users1 = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Group.Substring(0, 1) == Course1)
                                 //.Where(u => u.Course + u.Group.Substring(0, 1) == Course1)
       .Select(u => u.Id).ToList();
                var users2 = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Group.Substring(0, 1) == Course2)
                          //.Where(u => u.Course + u.Group.Substring(0, 1) == Course2)
              .Select(u => u.Id).ToList();
                int range = 0;

                List<bool> selected = new List<bool>() { false, false, false };

                switch (DateRange)
                {
                    case 1:
                        range = 1;
                        selected[0] = true;
                        ViewBag.Range = "за последний месяц";
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
                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
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
                foreach (var item in users1)
                {
                    var temp = dbContext.SelfyTestQualifications.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyQualifications = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();
                    foreach (var date in allSelfyQualifications)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYQ[index] += 1;
                    }
                    temp = dbContext.SelfyTestDisciplines.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyDisciplines = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();

                    foreach (var date in allSelfyDisciplines)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYD[index] += 1;
                    }
                }
                ViewBag.osXD = osX;
                ViewBag.osYD1 = "";
                foreach (var item in osYD)
                    ViewBag.osYD1 += item + ",";

                ViewBag.osXQ = osX;
                ViewBag.osYQ1 = "";
                foreach (var item in osYQ)
                    ViewBag.osYQ1 += item + ",";

                osYD = new List<int>();
                osYQ = new List<int>();
                foreach (var item in allDates)
                {
                    osYD.Add(0);
                    osYQ.Add(0);
                }
                foreach (var item in users2)
                {
                    var temp = dbContext.SelfyTestQualifications.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyQualifications = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();
                    foreach (var date in allSelfyQualifications)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYQ[index] += 1;
                    }
                    temp = dbContext.SelfyTestDisciplines.
                        Where(u => u.IdUser == item).
                        Where(u => u.TimeStart >= beforeMonth).
                        Where(u => u.TimeStart < today).Select(u => u.TimeStart).ToList();
                    var allSelfyDisciplines = temp.Select(u => u.ToString("dd.MM.yyyy")).Distinct().ToList();

                    foreach (var date in allSelfyDisciplines)
                    {
                        var tempDate = new DateTime(today.Year, DateTime.Parse(date).Month, DateTime.Parse(date).Day);
                        var index = allDates.IndexOf(tempDate);
                        osYD[index] += 1;
                    }
                }
                ViewBag.osYD2 = "";
                foreach (var item in osYD)
                    ViewBag.osYD2 += item + ",";

                ViewBag.osYQ2 = "";
                foreach (var item in osYQ)
                    ViewBag.osYQ2 += item + ",";
                ViewBag.DateRange = new[]{
                 new SelectListItem{ Text=" -- Выберите период -- ", Disabled=true},
                 new SelectListItem{ Value="1",Text="За последний месяц", Selected=selected[0]},
                 new SelectListItem{ Value="2",Text="За последние 3 месяца", Selected=selected[1]},
                 new SelectListItem{ Value="3",Text="За последние 6 месяцев", Selected=selected[2]},
                }.ToList();

                return PartialView("CompareCoursesRange");
            }
            return PartialView();
        }
    }
}