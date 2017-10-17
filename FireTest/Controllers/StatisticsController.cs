using FireTest.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        public ActionResult IndexUser()
        {
            return View();
        }
        [Authorize(Roles = "ADMIN, TEACHER")]
        public ActionResult Index()
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
                var allQuestionsThisQualification = dbContext.Questions.Where(u => u.IdQualification <= i).Count();
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
            var groups = dbContext.Users
                .Where(u => u.Course != 100)
                .Where(u => u.Group != "-1")
                .Where(u => u.Group != null).Select(u => u.Group).Distinct().ToList();
            if (groups == null)
                RedirectToAction("Index", "Home");
            ViewBag.Groups = groups //Выпадающий список групп
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
            ViewBag.Groups.Insert(0, new SelectListItem { Text = " -- Выберите группу -- ", Selected = true, Disabled = true });
            return View();
        }
        [HttpPost]
        public PartialViewResult Groups(string Groups, int Statistics, int? DateRange)
        {
            ViewBag.TitleChart = Groups;
            if (DateRange == null || Statistics != 2)
            {
                switch (Statistics)
                {
                    case 0:
                        {
                            var users = dbContext.Users
                              .Where(u => u.Course != 100)
                              .Where(u => u.Group == Groups)
                              .Select(u => new
                              {
                                  BattleCount = u.BattleCount,
                                  BattleWinCount = u.BattleWinCount
                              });
                            int count = 0;
                            int winCount = 0;
                            foreach (var item in users)
                            {
                                count += item.BattleCount;
                                winCount += item.BattleWinCount;
                            }
                            ViewBag.BattlesLose = count - winCount;
                            ViewBag.BattlesWin = winCount;
                            return PartialView("GroupsBattles");
                        }
                    case 1:
                        {
                            var users = dbContext.Users
                              .Where(u => u.Course != 100)
                              .Where(u => u.Group == Groups)
                              .Select(u => new
                              {
                                  AnswersCount = u.AnswersCount,
                                  CorrectAnswersCount = u.CorrectAnswersCount
                              });
                            int answers = 0;
                            int correctAnswers = 0;
                            foreach (var item in users)
                            {
                                answers += item.AnswersCount;
                                correctAnswers += item.CorrectAnswersCount;
                            }
                            ViewBag.Answers = answers - correctAnswers;
                            ViewBag.AnswersCorrect = correctAnswers;
                            return PartialView("GroupsAnswers");
                        }
                    case 2:
                        return PartialView("SelectDateRangeGroups");
                }
            }
            else
            {
                var users = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Group == Groups)
                    .Select(u => u.Id).ToList();

                int range = DateRange.Value * (DateRange.Value + 1) / 2;
                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                Dictionary<DateTime, int> SelfyD = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyQ = new Dictionary<DateTime, int>();

                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
                {
                    SelfyD.Add(date, 0);
                    SelfyQ.Add(date, 0);
                }
                foreach (var item in users)
                {
                    var allSelfyQCount = dbContext.SelfyTestQualifications
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();
                    var allSelfyDCount = dbContext.SelfyTestDisciplines
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();

                    foreach (var date in allSelfyQCount)
                        SelfyQ[date] += 1;
                    foreach (var date in allSelfyDCount)
                        SelfyD[date] += 1;
                }
                foreach (var item in SelfyQ)
                {
                    ViewBag.osXQ += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYQ += item.Value + ",";
                }
                foreach (var item in SelfyD)
                {
                    ViewBag.osXD += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYD += item.Value + ",";
                }
                ViewBag.DateRange = ListDataRange(DateRange.Value); //Выпадающий список периодов
                return PartialView("MonthDateRangeGroups");
            }
            return PartialView();
        }
        public ActionResult Courses()
        {
            var courses = dbContext.Users
                .Where(u => u.Course != 100)
                .Where(u => u.Group != "-1")
                .Where(u => u.Group != null).Select(u => u.Group.Substring(0, 1)).Distinct().ToList();
            if (courses == null)
                RedirectToAction("Index", "Home");
            ViewBag.Courses = courses //Выпадающий список курсов
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
            ViewBag.Courses.Insert(0, new SelectListItem { Text = " -- Выберите курс -- ", Selected = true, Disabled = true });
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
                        {
                            var users = dbContext.Users
                               .Where(u => u.Course != 100)
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
                        }
                    case 1:
                        {
                            var users2 = dbContext.Users
                              .Where(u => u.Course != 100)
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
                        }
                    case 2:
                        return PartialView("SelectDateRangeCourses");
                }
            }
            else
            {
                var users = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Group.Substring(0, 1) == Courses)
                    .Select(u => u.Id).ToList();

                int range = DateRange.Value * (DateRange.Value + 1) / 2;
                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                Dictionary<DateTime, int> SelfyD = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyQ = new Dictionary<DateTime, int>();

                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
                {
                    SelfyD.Add(date, 0);
                    SelfyQ.Add(date, 0);
                }
                foreach (var item in users)
                {
                    var allSelfyQCount = dbContext.SelfyTestQualifications
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();
                    var allSelfyDCount = dbContext.SelfyTestDisciplines
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();

                    foreach (var date in allSelfyQCount)
                        SelfyQ[date] += 1;
                    foreach (var date in allSelfyDCount)
                        SelfyD[date] += 1;
                }
                foreach (var item in SelfyQ)
                {
                    ViewBag.osXQ += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYQ += item.Value + ",";
                }
                foreach (var item in SelfyD)
                {
                    ViewBag.osXD += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYD += item.Value + ",";
                }
                ViewBag.DateRange = ListDataRange(DateRange.Value); //Выпадающий список периодов
                return PartialView("MonthDateRangeCourses");
            }
            return PartialView();
        }
        public ActionResult Users()
        {
            var groups = dbContext.Users
                .Where(u => u.Course != 100)
                .Where(u => u.Group != "-1")
                .Where(u => u.Group != null).Select(u => u.Group).Distinct().ToList();
            if (groups == null)
                RedirectToAction("Index", "Home");
            ViewBag.Group = groups //Выпадающий список групп
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
            ViewBag.Group.Insert(0, new SelectListItem { Text = " -- Выберите группу -- ", Selected = true, Disabled = true });
            return View();
        }
        public PartialViewResult UsersAjax(string currentFilter, string searchString, int? page, string Group)
        {
            if (!string.IsNullOrEmpty(Group))
                Session["Group"] = Group;
            else
            {
                if (Session["Group"] != null)
                    Group = (string)Session["Group"];
            }

            if (!string.IsNullOrEmpty(searchString))
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            var users = dbContext.Users.Where(u => !string.IsNullOrEmpty(u.Group)).Where(u => u.Group == Group);

            if (!string.IsNullOrEmpty(searchString))
                foreach (var item in searchString.ToLower().Split(' '))
                    if (!string.IsNullOrEmpty(item))
                        users = users.Where(u => u.Family.ToLower().Contains(item)
                                           || u.Name.ToLower().Contains(item)
                                           || u.SubName.ToLower().Contains(item));

            users = users.OrderBy(u => u.Family + " " + u.Name + " " + u.SubName);
            List<UsersForAdmin> model = new List<UsersForAdmin>(); //т.к. нам надо только имя и ид
            foreach (var item in users)
            {
                model.Add(new UsersForAdmin
                {
                    Id = item.Id,
                    Name = item.Family + " " + item.Name + " " + item.SubName
                });
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
            string[] declineText = decliner.Decline(user.Family, user.Name, user.SubName, 2);//Меняем падеж
            ViewBag.Name = declineText[0] + " " + declineText[1] + " " + declineText[2];
            return View();
        }
        [HttpPost]
        public PartialViewResult UsersStatistics(string Id, int Statistics, int? DateRange)
        {
            Decliner decliner = new Decliner();
            var user = dbContext.Users.Find(Id);
            string[] declineText = decliner.Decline(user.Family, user.Name, user.SubName, 2);//Меняем падеж
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
                        ViewBag.Answers = user.AnswersCount - user.CorrectAnswersCount;
                        ViewBag.AnswersCorrect = user.CorrectAnswersCount;
                        return PartialView("UserAnswers");
                    case 2:
                        return PartialView("SelectDateRangeUser");
                }
            }
            else
            {
                int range = DateRange.Value * (DateRange.Value + 1) / 2;
                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                Dictionary<DateTime, int> SelfyD = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyQ = new Dictionary<DateTime, int>();

                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
                {
                    SelfyD.Add(date, 0);
                    SelfyQ.Add(date, 0);
                }

                var allSelfyQCount = dbContext.SelfyTestQualifications
                    .Where(u => u.IdUser == user.Id)
                    .Where(u => u.TimeStart >= beforeMonth)
                    .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();
                var allSelfyDCount = dbContext.SelfyTestDisciplines
                    .Where(u => u.IdUser == user.Id)
                    .Where(u => u.TimeStart >= beforeMonth)
                    .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();

                foreach (var date in allSelfyQCount)
                    SelfyQ[date] += 1;
                foreach (var date in allSelfyDCount)
                    SelfyD[date] += 1;

                foreach (var item in SelfyQ)
                {
                    ViewBag.osXQ += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYQ += item.Value + ",";
                }
                foreach (var item in SelfyD)
                {
                    ViewBag.osXD += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYD += item.Value + ",";
                }
                ViewBag.TitleChart = user.Name;
                ViewBag.DateRange = ListDataRange(DateRange.Value); //Выпадающий список периодов
                return PartialView("MonthDateRangeUser");
            }
            return PartialView();
        }
        public ActionResult CompareGroups()
        {
            var groups = dbContext.Users
                .Where(u => u.Course != 100)
                .Where(u => u.Group != "-1")
                .Where(u => u.Group != null).Select(u => u.Group).Distinct().ToList();
            if (groups == null)
                RedirectToAction("Index", "Home");
            ViewBag.Group = groups //Выпадающий список групп
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
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
                           .Where(u => u.Group == Group1)
                           .Select(u => new
                           {
                               AnswersCount = u.AnswersCount,
                               CorrectAnswersCount = u.CorrectAnswersCount
                           }).ToList();
                        var users2 = dbContext.Users
                           .Where(u => u.Course != 100)
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
                        ViewBag.Answers1 = answers;
                        ViewBag.AnswersCorrect1 = correctAnswers;
                        answers = 0;
                        correctAnswers = 0;
                        foreach (var item in users2)
                        {
                            answers += item.AnswersCount;
                            correctAnswers += item.CorrectAnswersCount;
                        }
                        ViewBag.Answers2 = answers;
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
                    .Select(u => u.Id).ToList();
                var users2 = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Group == Group2)
                    .Select(u => u.Id).ToList();

                int range = DateRange.Value * (DateRange.Value + 1) / 2;
                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                Dictionary<DateTime, int> SelfyD1 = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyQ1 = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyD2 = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyQ2 = new Dictionary<DateTime, int>();

                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
                {
                    SelfyD1.Add(date, 0);
                    SelfyQ1.Add(date, 0);
                    SelfyD2.Add(date, 0);
                    SelfyQ2.Add(date, 0);
                }
                foreach (var item in users1)
                {
                    var allSelfyQCount = dbContext.SelfyTestQualifications
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();
                    var allSelfyDCount = dbContext.SelfyTestDisciplines
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();

                    foreach (var date in allSelfyQCount)
                        SelfyQ1[date] += 1;
                    foreach (var date in allSelfyDCount)
                        SelfyD1[date] += 1;
                }
                foreach (var item in users2)
                {
                    var allSelfyQCount = dbContext.SelfyTestQualifications
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();
                    var allSelfyDCount = dbContext.SelfyTestDisciplines
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();

                    foreach (var date in allSelfyQCount)
                        SelfyQ2[date] += 1;
                    foreach (var date in allSelfyDCount)
                        SelfyD2[date] += 1;
                }

                foreach (var item in SelfyQ1)
                {
                    ViewBag.osXQ += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYQ1 += item.Value + ",";
                }
                foreach (var item in SelfyD1)
                {
                    ViewBag.osXD += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYD1 += item.Value + ",";
                }
                foreach (var item in SelfyQ2)
                    ViewBag.osYQ2 += item.Value + ",";
                foreach (var item in SelfyD2)
                    ViewBag.osYD2 += item.Value + ",";

                ViewBag.DateRange = ListDataRange(DateRange.Value); //Выпадающий список периодов
                return PartialView("CompareGroupsRange");
            }
            return PartialView();
        }
        public ActionResult CompareCourses()
        {
            var courses = dbContext.Users
                .Where(u => u.Course != 100)
                .Where(u => u.Group != "-1")
                .Where(u => u.Group != null).Select(u => u.Group.Substring(0, 1)).Distinct().ToList();
            if (courses == null)
                RedirectToAction("Index", "Home");
            ViewBag.Course = courses //Выпадающий список курсов
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                }).ToList();
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
                           .Select(u => new
                           {
                               AnswersCount = u.AnswersCount,
                               CorrectAnswersCount = u.CorrectAnswersCount
                           }).ToList();
                        var users2 = dbContext.Users
                           .Where(u => u.Course != 100)
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
                        ViewBag.Answers1 = answers;
                        ViewBag.AnswersCorrect1 = correctAnswers;
                        answers = 0;
                        correctAnswers = 0;
                        foreach (var item in users2)
                        {
                            answers += item.AnswersCount;
                            correctAnswers += item.CorrectAnswersCount;
                        }
                        ViewBag.Answers2 = answers;
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
                    .Select(u => u.Id).ToList();
                var users2 = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Where(u => u.Group.Substring(0, 1) == Course2)
                    .Select(u => u.Id).ToList();
                int range = DateRange.Value * (DateRange.Value + 1) / 2;
                var today = DateTime.Today.AddDays(1);
                var beforeMonth = new DateTime(today.Year, today.Month - range, today.Day);

                Dictionary<DateTime, int> SelfyD1 = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyQ1 = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyD2 = new Dictionary<DateTime, int>();
                Dictionary<DateTime, int> SelfyQ2 = new Dictionary<DateTime, int>();

                for (DateTime date = beforeMonth; date < today; date = date.AddDays(1))
                {
                    SelfyD1.Add(date, 0);
                    SelfyQ1.Add(date, 0);
                    SelfyD2.Add(date, 0);
                    SelfyQ2.Add(date, 0);
                }
                foreach (var item in users1)
                {
                    var allSelfyQCount = dbContext.SelfyTestQualifications
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();
                    var allSelfyDCount = dbContext.SelfyTestDisciplines
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();

                    foreach (var date in allSelfyQCount)
                        SelfyQ1[date] += 1;
                    foreach (var date in allSelfyDCount)
                        SelfyD1[date] += 1;
                }
                foreach (var item in users2)
                {
                    var allSelfyQCount = dbContext.SelfyTestQualifications
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();
                    var allSelfyDCount = dbContext.SelfyTestDisciplines
                        .Where(u => u.IdUser == item)
                        .Where(u => u.TimeStart >= beforeMonth)
                        .Where(u => u.TimeStart < today).Future().Select(u => u.TimeStart.Date).Distinct();

                    foreach (var date in allSelfyQCount)
                        SelfyQ2[date] += 1;
                    foreach (var date in allSelfyDCount)
                        SelfyD2[date] += 1;
                }

                foreach (var item in SelfyQ1)
                {
                    ViewBag.osXQ += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYQ1 += item.Value + ",";
                }
                foreach (var item in SelfyD1)
                {
                    ViewBag.osXD += "'" + item.Key.Date.ToString("d MMMM") + "',";
                    ViewBag.osYD1 += item.Value + ",";
                }
                foreach (var item in SelfyQ2)
                    ViewBag.osYQ2 += item.Value + ",";
                foreach (var item in SelfyD2)
                    ViewBag.osYD2 += item.Value + ",";

                ViewBag.DateRange = ListDataRange(DateRange.Value); //Выпадающий список периодов
                return PartialView("CompareCoursesRange");
            }
            return PartialView();
        }
        private List<SelectListItem> ListDataRange(int DataRange)
        {
            Dictionary<int, string> DateRangeDB = new Dictionary<int, string>
            {
                {1,"За последний месяц"},
                {2,"За последние 3 месяца" },
                {3,"За последние 6 месяцев" },
            };
            List<SelectListItem> ListDataRange = new List<SelectListItem>
            {
                new SelectListItem { Text = " -- Выберите период -- ", Disabled = true }
            };
            foreach (var item in DateRangeDB)
                ListDataRange.Add(new SelectListItem { Value = item.Key.ToString(), Text = item.Value, Selected = item.Key == DataRange ? true : false });
            return ListDataRange;
        }
    }
}