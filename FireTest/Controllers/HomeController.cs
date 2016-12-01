using FireTest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FireTest.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        private IdentityResult UpdateCourseAndGroup(string userID)
        {
            try
            {
                ApplicationUser user = dbContext.Users.Find(userID);
                DateTime entrance = DateTime.Parse("1 Sep " + user.Year);
                TimeSpan diff = DateTime.Now.Subtract(entrance);
                DateTime zeroTime = new DateTime(1, 1, 1);
                if (diff.Days > 0)
                {
                    //Григорианский календарь начинается с 1. 
                    //То должны вычитать 1, но т.к. начинаем мы не с 0 курса, а с первого то прибавляем 1.
                    //Итого получается, что мы не вычитаем и не прибавляем
                    int course = (zeroTime + diff).Year;
                    if (course <= 6)
                    {
                        if (user.Group.Substring(0, 1) == "1" && course < 6) //Если ПБ
                            user.Course = course;

                        if (user.Group.Substring(0, 1) == "2") //Если ТБ
                        {
                            user.Course = course;
                        }
                        if (user.Group.Substring(0, 1) == "0" && course < 5) //Если платно
                        {
                            user.Course = course;
                        }
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception exception)
            {
                return IdentityResult.Failed(exception.Message);
            }
            return IdentityResult.Success;
        }
        private bool NewUser(string userID)
        {
            ApplicationUser user = dbContext.Users.Find(User.Identity.GetUserId());
            if (string.IsNullOrEmpty(user.Name) ||
                string.IsNullOrEmpty(user.Family) ||
                string.IsNullOrEmpty(user.SubName) ||
                user.Course == 0 ||
                user.Year == 0)
                return true;
            return false;
        }
        public PartialViewResult UserInfo()
        {
            switch (NewUser(User.Identity.GetUserId()))
            {
                case true:
                    ViewBag.Name = "Регистрация";
                    ViewBag.Avatar = "/Images/Avatars/NoAvatar.png";
                    ViewBag.New = true;
                    break;
                default:
                    ApplicationUser user = dbContext.Users.Find(User.Identity.GetUserId());
                    ViewBag.New = false;
                    ViewBag.Name = user.Name;
                    ViewBag.Avatar = "/Images/Avatars/" + user.Avatar;
                    ViewBag.Battles = user.BattleCount;
                    ViewBag.BattlesWin = user.BattleWinCount;
                    ViewBag.Correct = user.CorrectAnswersCount * 100 / user.AnswersCount;
                    var role = dbContext.Users.Find(User.Identity.GetUserId()).Roles.SingleOrDefault();
                    if (dbContext.Roles.Find(role.RoleId).Name == "USER")
                    {
                        var result = UpdateCourseAndGroup(user.Id);
                        if (!result.Succeeded)
                            ViewBag.Name = "Ошибка";
                    }
                    break;
            }
            return PartialView();
        }

        public ActionResult Index(ManageController.ManageMessageId? message)
        {
            switch (NewUser(User.Identity.GetUserId()))
            {
                case true:
                    return RedirectToAction("Index", "Manage", new { Message = ManageController.ManageMessageId.NewUser });
                default:
                    string userId = User.Identity.GetUserId();
                    ApplicationUser user = dbContext.Users.Find(userId);
                    user.Busy = false;
                    dbContext.SaveChanges();
                    ViewBag.Images = new SIC().SelectImagesCache(SIC.type.quote);
                    var role = dbContext.Users.Find(userId).Roles.SingleOrDefault();
                    switch(dbContext.Roles.Find(role.RoleId).Name)
                    {
                        case "TEACHER":
                            return RedirectToAction("Index", "Teacher");
                        default:
                            break;
                    }
                    return View();
            }
        }
        public ActionResult Load()
        {
            ViewBag.Images = new SIC().SelectImagesCache(SIC.type.img);
            return View();
        }
        [ChildActionOnly]
        public ActionResult Menu()
        {
            var role = dbContext.Users.Find(User.Identity.GetUserId()).Roles.SingleOrDefault();           
            ViewBag.Access = dbContext.Roles.Find(role.RoleId).Name;
            return PartialView();
        }
        public ActionResult Rating()
        {
            var top = dbContext.Users
                    .Select(u => new
                    {
                        avatar = u.Avatar,
                        name = u.Name,
                        family = u.Family,
                        rating = u.Rating,
                    }).OrderByDescending(s => s.rating).Take(5);
            List<Rating> users = new List<Rating>();
            foreach (var item in top)
            {
                Rating temp = new Rating();
                temp.Avatar = "/Images/Avatars/" + item.avatar;
                temp.Name = item.name;
                temp.Family = item.family;
                users.Add(temp);
            }
            return View(users);
        }
        public ActionResult RatingPlace()
        {
            ApplicationUser you = dbContext.Users.Find(User.Identity.GetUserId());

            string YouId = you.Id;

            int numbertop = dbContext.Users
                    .Where(u => u.Rating > you.Rating)
                    .Where(u => u.Group == you.Group)
                    .Select(u => u.Rating).OrderByDescending(u => u).Count();
            int numberbottom = dbContext.Users
                    .Where(u => u.Rating <= you.Rating)
                    .Where(u => u.Id != YouId)
                    .Where(u => u.Group == you.Group)
                    .Select(u => u.Rating).OrderByDescending(u => u).Count();

            int taketop = 2;
            int takebottom = 2;
            switch (numberbottom)
            {
                case 0:
                    taketop = 4;
                    takebottom = 0;
                    break;
                case 1:
                    taketop = 3;
                    takebottom = 1;
                    break;
                default:
                    switch (numbertop)
                    {
                        case 0:
                            taketop = 0;
                            takebottom = 4;
                            break;
                        case 1:
                            taketop = 1;
                            takebottom = 3;
                            break;
                    }
                    break;
            }

            if (taketop > numbertop)
                taketop = numbertop;
            if (takebottom > numberbottom)
                takebottom = numberbottom;

            ViewBag.Start = numbertop + 1 - taketop;

            var top = dbContext.Users
                    .Where(u => u.Rating > you.Rating)
                    .Where(u => u.Group == you.Group)
                    .Select(u => new
                    {
                        Avatar = u.Avatar,
                        Name = u.Name,
                        Family = u.Family,
                        Rating = u.Rating,
                    }).OrderByDescending(s => s.Rating).Take(taketop);

            List<Rating> users = new List<Rating>();

            foreach (var item in top)
            {
                users.Add(new Rating()
                {
                    Avatar = "/Images/Avatars/" + item.Avatar,
                    Name = item.Name,
                    Family = item.Family
                });
            }

            ViewBag.You = users.Count();

            users.Add(new Rating()
            {
                Avatar = "/Images/Avatars/" + you.Avatar,
                Name = you.Name,
                Family = you.Family
            });

            var bottom = dbContext.Users
                    .Where(u => u.Rating <= you.Rating)
                    .Where(u => u.Group == you.Group)
                    .Where(u => u.Id != YouId)
                    .Select(u => new
                    {
                        Avatar = u.Avatar,
                        Name = u.Name,
                        Family = u.Family,
                        Rating = u.Rating,
                    }).OrderByDescending(s => s.Rating).Take(takebottom);

            foreach (var item in bottom)
            {
                users.Add(new Rating()
                {
                    Avatar = "/Images/Avatars/" + item.Avatar,
                    Name = item.Name,
                    Family = item.Family
                });
            }

            return View(users);
        }
        public PartialViewResult Departments()
        {
            DepartmentsAndSubjects DepartmentsAndSubjects = new DepartmentsAndSubjects();
            DepartmentsAndSubjects.Department = dbContext.Departments.ToList();            
            DepartmentsAndSubjects.Subject = dbContext.Subjects.OrderBy(u => u.Name).ToList(); 
            return PartialView(DepartmentsAndSubjects);
        }
    }
}