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
                        var temp = userManager.RemoveFromRoles(userEdit.Id, "TEACHER");
                        temp = userManager.RemoveFromRoles(userEdit.Id, "USER");
                        userManager.AddToRole(userEdit.Id, "ADMIN");
                        userEdit.Course = 100;
                    }
                    else
                    {
                        var temp = userManager.RemoveFromRole(userEdit.Id, "ADMIN");
                        userManager.AddToRole(userEdit.Id, "USER");
                    }
                }
                else
                {
                    var userEdit = dbContext.Users.Find(value[1]);
                    if (value[2] == "true")
                    {
                       var temp = userManager.RemoveFromRoles(userEdit.Id, "USER");
                        userManager.AddToRole(userEdit.Id, "TEACHER");
                        userEdit.Course = 100;
                    }
                    else
                    {
                        var temp = userManager.RemoveFromRoles(userEdit.Id, "TEACHER");
                        userManager.AddToRole(userEdit.Id, "USER");
                    }
                }
                dbContext.SaveChanges();         
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
        public ActionResult TeacherSubjects(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
                ViewBag.userId = userId;
            return View();
        }
        public PartialViewResult TeacherSubjectsAjax(string currentFilter, string searchString, int? page, int? submitButton, int? Page, string userId)
        {
            ViewBag.userId = userId;
            var access = dbContext.TeachersAccess
                .Where(u => u.TeacherId == userId)
                .Select(u => new
                {
                    Id = u.Id,
                    TeacherSubjects = u.TeacherSubjects
                }).SingleOrDefault();
            List<string> subjectsAccess = new List<string>();
            if (access != null && access.TeacherSubjects != null)
                subjectsAccess = access.TeacherSubjects.Split('|').ToList();
            if (submitButton != null)
            {
                string newAccess = "";
                int index = subjectsAccess.IndexOf(submitButton.ToString());
                if (index == -1)
                {
                    subjectsAccess.Add(submitButton.ToString());
                    if (access != null && access.TeacherSubjects != null && access.TeacherSubjects.Count() != 0)
                        newAccess = access.TeacherSubjects + "|" + submitButton.ToString();
                    else
                        newAccess = submitButton.ToString();
                }
                else
                {
                    subjectsAccess.RemoveAt(index);
                    foreach (string item in subjectsAccess)
                    {
                        if (newAccess.Length != 0)
                            newAccess = newAccess + "|" + item;
                        else
                            newAccess = item;
                    }
                }
                if (access != null)
                {
                    var temp = dbContext.TeachersAccess.Find(access.Id);
                    temp.TeacherSubjects = newAccess;
                }
                else
                {
                    var temp = new TeacherAccess();
                    temp.TeacherId = userId;
                    temp.TeacherQualifications = false;
                    temp.TeacherSubjects = newAccess;
                    dbContext.TeachersAccess.Add(temp);
                }
                dbContext.SaveChanges();
            }
            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

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
        public ActionResult SelectTeacher()
        {
            return View();
        }
        public PartialViewResult SelectTeacherAjax(string currentFilter, string searchString, int? page, int? Page, string submitButton)
        {
            if (!string.IsNullOrEmpty(submitButton))
            {
                var access = dbContext.TeachersAccess
                    .Where(u => u.TeacherId == submitButton)
                    .Select(u => new
                    {
                        Id = u.Id,
                        TeacherQualifications = u.TeacherQualifications
                    }).SingleOrDefault();

                if (access != null)
                {
                    var temp = dbContext.TeachersAccess.Find(access.Id);
                    if (temp.TeacherQualifications)
                        temp.TeacherQualifications = false;
                    else
                        temp.TeacherQualifications = true;
                }
                else
                {
                    var temp = new TeacherAccess();
                    temp.TeacherId = submitButton;
                    temp.TeacherQualifications = true;
                    dbContext.TeachersAccess.Add(temp);
                }
                dbContext.SaveChanges();
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
                if (userManager.IsInRole(item.Id, "TEACHER") || userManager.IsInRole(item.Id, "ADMIN"))
                {
                    UsersForAdmin temp = new UsersForAdmin();
                    temp.Id = item.Id;
                    temp.Avatar = item.Avatar;
                    temp.Name = item.Family + " " + item.Name + " " + item.SubName;
                    model.Add(temp);
                }
            }
            foreach(var item in model)
            {
                var access = dbContext.TeachersAccess
                    .Where(u => u.TeacherId == item.Id)
                    .Select(u => new
                    {
                        TeacherQualifications = u.TeacherQualifications
                    }).SingleOrDefault();
                if (access != null)
                    item.Qualification = access.TeacherQualifications;
                else
                    item.Qualification = false;
            }

            model = model.OrderBy(u => u.Name).ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = pageNumber;
            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
    }
}