using FireTest.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdministratorController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Users()
        {
            return View();
        }

        public PartialViewResult UsersAjax(string currentFilter, string searchString, int? page, string submitButton, int? Page)
        {
            if (!string.IsNullOrEmpty(submitButton))
            {
                string[] value = submitButton.Split('|');
                if (value[0] == "Administrator")
                {
                    var userEdit = dbContext.Users.Find(value[1]);
                    if (value[2] == "true")
                    {
                        var tt = userManager.RemoveFromRoles(userEdit.Id, "TEACHER");
                        tt = userManager.RemoveFromRoles(userEdit.Id, "USER");
                        userManager.AddToRole(userEdit.Id, "ADMIN");
                    }
                    else
                    {
                        var tt = userManager.RemoveFromRole(userEdit.Id, "ADMIN");
                        userManager.AddToRole(userEdit.Id, "USER");
                    }
                }
                else
                {
                    var userEdit = dbContext.Users.Find(value[1]);
                    if (value[2] == "true")
                    {
                       var tt = userManager.RemoveFromRoles(userEdit.Id, "USER");
                        userManager.AddToRole(userEdit.Id, "TEACHER");
                    }
                    else
                    {
                        var tt = userManager.RemoveFromRoles(userEdit.Id, "TEACHER");
                        userManager.AddToRole(userEdit.Id, "USER");
                    }
                }                
                page = Page;
            }
            
            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            string user = User.Identity.GetUserId();
            List<UsersForAdmin> model = new List<UsersForAdmin>();
            var users = dbContext.Users.Where(u => u.Id != user);
            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Family.Contains(searchString)
                                       || u.Name.Contains(searchString)
                                       || u.SubName.Contains(searchString));
            }
            foreach (var item in users)
            {
                UsersForAdmin temp = new UsersForAdmin();
                temp.Id = item.Id;
                temp.Avatar = item.Avatar;
                temp.Name = item.Family + " " + item.Name + " " + item.SubName;
                temp.Teacher = userManager.IsInRole(item.Id, "TEACHER");
                temp.Administrator = userManager.IsInRole(item.Id, "ADMIN");
                model.Add(temp);
            }

            model = model.OrderBy(u => u.Name).ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = pageNumber;
            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult TeacherSubjects()
        {
            return View();
        }
        public PartialViewResult TeacherSubjectsAjax(string currentFilter, string searchString, int? page, int? submitButton, int? Page, string userId)
        {
            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            userId = "1"; //sdfdsfdsfdsaf

            var access = dbContext.TeachersAccess
                .Where(u => u.TeacherId == userId)
                .Select(u => new
                {
                    TeacherSubjects = u.TeacherSubjects
                }).SingleOrDefault();
            List<string> subjectsAccess = new List<string>();
            if (access != null)
                subjectsAccess = access.TeacherSubjects.Split('|').ToList();

            var subjects = dbContext.Subjects.ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                subjects = dbContext.Subjects.Where(u => u.Name.Contains(searchString)).ToList();
            }
            List<SubjectAccess> model = new List<SubjectAccess>();
            foreach (var item in subjects)
            {
                SubjectAccess temp = new SubjectAccess();
                temp.Id = item.Id;
                temp.Name = item.Name;
                if (subjectsAccess.Contains(item.Id.ToString()))
                    temp.Access = true;
                else
                    temp.Access = false;
                model.Add(temp);
            }

            model = model.OrderBy(u => u.Name).ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = pageNumber;
            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
    }
}