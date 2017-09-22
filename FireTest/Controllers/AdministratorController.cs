using FireTest.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
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
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = pageNumber;

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
            users = users.OrderBy(u => u.Family + " " + u.Name + " " + u.SubName);
            var emptycount = 1;
            foreach (var item in users)
            {
                UsersForAdmin temp = new UsersForAdmin();
                if (emptycount >= (pageNumber - 1) * pageSize + 1 && emptycount <= pageNumber * pageSize)
                {
                    temp.Id = item.Id;
                    temp.Snils = item.Snils;
                    temp.Avatar = item.Avatar;
                    temp.Name = item.Family + " " + item.Name + " " + item.SubName;
                    temp.Teacher = userManager.IsInRole(item.Id, "TEACHER");
                    temp.Administrator = userManager.IsInRole(item.Id, "ADMIN");
                }
                model.Add(temp);
                emptycount++;
            }

            model = model.ToList();

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult TeacherSubjects(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
                ViewBag.userId = userId;
            return View();
        }
        public PartialViewResult TeacherSubjectsAjax(string currentFilter, string searchString, int? page, int? submitButton, int? Page, string userId, bool submitButtonAll = false)
        {
            ViewBag.userId = userId;
            List<string> subjectsAccess = new List<string>();
            if (submitButtonAll)
            {
                var access = dbContext.TeachersAccess
                    .Where(u => u.TeacherId == userId)
                    .Select(u => new
                    {
                        Id = u.Id,
                        TeacherSubjects = u.TeacherSubjects,
                        TeacherQualifications = u.TeacherQualifications
                    }).SingleOrDefault();
                bool TeacherQualificationsAccess = false;
                if (access != null)
                {
                    TeacherQualificationsAccess = access.TeacherQualifications;
                    var delete = dbContext.TeachersAccess.Find(access.Id);
                    dbContext.TeachersAccess.Remove(delete);
                    var newAccess = new TeacherAccess
                    {
                        TeacherId = userId,
                        TeacherQualifications = TeacherQualificationsAccess
                    };

                    var Subjects = "";
                    var allSubjects = dbContext.Subjects.ToList();
                    foreach (var item in allSubjects)
                    {
                        subjectsAccess.Add(item.Id.ToString());
                        if (Subjects.Length > 0)
                            Subjects += "|" + item.Id;
                        else
                            Subjects += item.Id;
                    }
                    newAccess.TeacherSubjects = Subjects;
                    dbContext.TeachersAccess.Add(newAccess);
                    dbContext.SaveChanges();
                }
            }
            else
            {
                var access = dbContext.TeachersAccess
                    .Where(u => u.TeacherId == userId)
                    .Select(u => new
                    {
                        Id = u.Id,
                        TeacherSubjects = u.TeacherSubjects
                    }).SingleOrDefault();
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
                        dbContext.TeachersAccess.Add(new TeacherAccess
                        {
                            TeacherId = userId,
                            TeacherQualifications = false,
                            TeacherSubjects = newAccess
                        });
                    }
                    dbContext.SaveChanges();
                }
            }
            currentFilter = searchString;
            ViewBag.CurrentFilter = searchString;

            var subjects = dbContext.Subjects.ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                subjects = dbContext.Subjects.Where(u => u.Name.Contains(searchString)).ToList();
            }
            List<SubjectAccess> model = new List<SubjectAccess>();
            foreach (var item in subjects)
            {
                SubjectAccess temp = new SubjectAccess
                {
                    Id = item.Id,
                    Name = item.Name
                };
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
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = pageNumber;

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
                    dbContext.TeachersAccess.Add(new TeacherAccess
                    {
                        TeacherId = submitButton,
                        TeacherQualifications = true
                    });
                }
                dbContext.SaveChanges();
            }
            currentFilter = searchString;
            ViewBag.CurrentFilter = searchString;

            string user = User.Identity.GetUserId();
            List<UsersForAdmin> model = new List<UsersForAdmin>();

            var users = dbContext.Users
                     .Where(u => u.Roles.Any(r => r.RoleId != "3")) //3 - это USER
                     .Where(u => u.Id != user);

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Family.Contains(searchString)
                                       || u.Name.Contains(searchString)
                                       || u.SubName.Contains(searchString));
            }

            foreach (var item in users)
            {
                model.Add(new UsersForAdmin
                {
                    Id = item.Id,
                    Avatar = item.Avatar,
                    Name = item.Family + " " + item.Name + " " + item.SubName
                });
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

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult DeleteUser(string id)
        {
            var user = dbContext.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            Decliner decliner = new Decliner();
            string[] declineText = decliner.Decline(user.Family, user.Name, user.SubName, 4);//Меняем падеж
            ViewBag.User = declineText[0] + " " + declineText[1] + " " + declineText[2];

            return View();
        }
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUserConfirmed(string id)
        {
            var user = dbContext.Users.Find(id);
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
            return RedirectToAction("Users");
        }
    }
}