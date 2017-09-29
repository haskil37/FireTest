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
                currentFilter = value[3];
                if (value[0] == "Administrator")
                {
                    var userEdit = dbContext.Users.Find(value[1]);
                    if (value[2] == "true")
                    {
                        var temp = userManager.RemoveFromRoles(userEdit.Id, "TEACHER");
                        temp = userManager.RemoveFromRoles(userEdit.Id, "USER");
                        userManager.AddToRole(userEdit.Id, "ADMIN");
                        userEdit.Course = 100;
                        userEdit.Group = "-1";
                    }
                    else
                    {
                        var temp = userManager.RemoveFromRole(userEdit.Id, "ADMIN");
                        userEdit.Group = "";
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
                        userEdit.Group = "-1";
                    }
                    else
                    {
                        var temp = userManager.RemoveFromRoles(userEdit.Id, "TEACHER");
                        userEdit.Group = "";
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
                                       || u.SubName.Contains(searchString)
                                       || u.Group.Contains(searchString));
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
                    temp.Group = item.Group;
                    if (temp.Teacher)
                        temp.Group = "Преподаватель";
                    if (temp.Administrator)
                        temp.Group = "Администратор";
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
        public PartialViewResult SelectTeacherAjax(string currentFilter, string searchString, int? page, string submitButton)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = pageNumber;

            if (!string.IsNullOrEmpty(submitButton))
            {
                var search = submitButton.Split('|');
                currentFilter = search[1];
                submitButton = search[0];
                var access = dbContext.TeachersAccess.SingleOrDefault(u => u.TeacherId == submitButton);
                if (access != null)
                    access.TeacherQualifications = !access.TeacherQualifications;
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
            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
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
            users = users.OrderBy(u => u.Family + " " + u.Name + " " + u.SubName);
            var emptycount = 1;
            foreach (var item in users.ToList())
            {
                UsersForAdmin temp = new UsersForAdmin();
                if (emptycount >= (pageNumber - 1) * pageSize + 1 && emptycount <= pageNumber * pageSize)
                {
                    temp.Id = item.Id + "|" + searchString;
                    temp.Avatar = item.Avatar;
                    temp.Name = item.Family + " " + item.Name + " " + item.SubName;
                    var teacherQ = dbContext.TeachersAccess.SingleOrDefault(u => u.TeacherId == item.Id);
                    if (teacherQ != null)
                        temp.Qualification = teacherQ.TeacherQualifications;
                    else
                        temp.Qualification = false;
                }
                model.Add(temp);
                emptycount++;
            }

            model = model.ToList();
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
        public ActionResult AddSubject()
        {
            ViewBag.Department = dbContext.Departments //Выпадающий список кафедр
                .Select(u => new SelectListItem()
                {
                    Value = u.Id.ToString(),
                    Text = u.Name,
                }).ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSubject(string NameSubject, int Department)
        {
            ViewBag.StatusMessage = "Вы должны указать название новой дисциплины";
            if (dbContext.Departments.Find(Department) == null)
                Department = 1;

            ViewBag.Department = dbContext.Departments //Выпадающий список кафедр
               .Select(u => new SelectListItem()
               {
                   Value = u.Id.ToString(),
                   Text = u.Name,
                   Selected = u.Id == Department
               }).ToList();

            if (string.IsNullOrEmpty(NameSubject))
                return View();

            ViewBag.NameSubject = NameSubject;

            dbContext.Subjects.Add(new Subject()
            {
                IdDepartment = Department,
                Name = NameSubject
            });
            dbContext.SaveChanges();

            ViewBag.StatusMessage = "Дисциплина успешно добавлена";
            return View();
        }
        public ActionResult DeleteSubject(string StatusMessage)
        {
            ViewBag.StatusMessage = StatusMessage;
            ViewBag.SubjectsDelete = dbContext.Subjects //Выпадающие списки дисциплин
                .Select(u => new SelectListItem()
                {
                    Value = u.Id.ToString(),
                    Text = u.Name,
                }).ToList();
            ViewBag.SubjectsTransfer = dbContext.Subjects 
                .Select(u => new SelectListItem()
                {
                    Value = u.Id.ToString(),
                    Text = u.Name,
                }).ToList();
            ViewBag.Count = dbContext.Questions
                .Where(u => u.IdSubject == dbContext.Subjects.Select(s => s.Id).FirstOrDefault())
                .Count();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult DeleteSubjectCount(int SubjectsDelete, int SubjectsTransfer, string submitButton)
        {
            ViewBag.Count = dbContext.Questions.Where(u => u.IdSubject == SubjectsDelete).Count();
            if (submitButton != null)
            {
                ViewBag.Confrim = true;
                ViewBag.SubjectsDelete = SubjectsDelete;
                ViewBag.SubjectsTransfer = SubjectsTransfer;
                return PartialView();
            }
            ViewBag.SubjectsDelete = dbContext.Subjects //Выпадающий список дисциплин
                .Select(u => new SelectListItem()
                {
                    Value = u.Id.ToString(),
                    Text = u.Name,
                    Selected = u.Id == SubjectsDelete
                }).ToList();
            ViewBag.SubjectsTransfer = dbContext.Subjects //Выпадающий список дисциплин
                .Select(u => new SelectListItem()
                {
                    Value = u.Id.ToString(),
                    Text = u.Name,
                    Selected = u.Id == SubjectsTransfer
                }).ToList();
            return PartialView();
        }
        public ActionResult DeleteSubjectConfirm(string value)
        {
            var temp = value.Split('|');
            ViewBag.SubjectDelete = dbContext.Subjects.Find(Convert.ToInt32(temp[0])).Name;
            ViewBag.SubjectTransfer = dbContext.Subjects.Find(Convert.ToInt32(temp[1])).Name;
            return View(temp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSubjectConfirm(int SubjectsDelete, int SubjectsTransfer)
        {
            var SubjectDelete = dbContext.Subjects.Find(SubjectsDelete).Id;
            var SubjectTransfer = dbContext.Subjects.Find(SubjectsTransfer).Id;

            var allQ = dbContext.Questions.Where(u => u.IdSubject == SubjectDelete);
            foreach (var item in allQ)
            {
                item.IdSubject = SubjectTransfer;
            }
            dbContext.Subjects.Remove(dbContext.Subjects.Find(SubjectDelete));
            dbContext.SaveChanges();
            return RedirectToAction("DeleteSubject", new { StatusMessage = "Дисциплина успешно удалена" });
        }
        public ActionResult SelectTeacherStatistic()
        {
            return View();
        }
        public PartialViewResult SelectTeacherStatisticAjax(string currentFilter, string searchString, int? page, int? Page, string submitButton)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.Page = pageNumber;

            if (!string.IsNullOrEmpty(submitButton))
            {
                var access = dbContext.TeachersAccess.SingleOrDefault(u => u.TeacherId == submitButton);
                if (access != null)
                    access.TeacherQualifications = !access.TeacherQualifications;
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

            users = users.OrderBy(u => u.Family + " " + u.Name + " " + u.SubName);

            var emptycount = 1;
            foreach (var item in users)
            {
                UsersForAdmin temp = new UsersForAdmin();
                if (emptycount >= (pageNumber - 1) * pageSize + 1 && emptycount <= pageNumber * pageSize)
                {
                    temp.Id = item.Id;
                    temp.Avatar = item.Avatar;
                    temp.Name = item.Family + " " + item.Name + " " + item.SubName;
                }
                model.Add(temp);
                emptycount++;
            }
            model = model.ToList();

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult TeacherStatistic(string userId)
        {
            var tests = dbContext.Examinations.Where(u => u.TeacherId == userId).
                Select(u => new {
                    Id = u.Id,
                    Date = u.Date,
                    Group = u.Group,
                    FinishTest = u.FinishTest,
                    IdTest = u.IdTest
                }).OrderBy(u => u.Date).ToList();
            var model = new List<TeacherStatistics>();
            foreach (var item in tests)
            {
                var stats = new TeacherStatistics
                {
                    Date = item.Date,
                    Group = item.Group,
                };
                if (item.FinishTest)
                {
                    stats.Qualification = dbContext.Qualifications
                        .Find(dbContext.TeacherFinishTests
                                    .Find(item.IdTest).IdQualification)
                        .Name;
                    if (!string.IsNullOrEmpty(dbContext.TestQualificationAccess.SingleOrDefault(u => u.IdExamination == item.Id).IdUsers))
                        stats.IsOver = "Тестирование прошло";
                    else
                    {
                        if (stats.Date > DateTime.Today)
                            stats.IsOver = "Тестирование запланировано";
                        else
                            stats.IsOver = "Тестирование просрочено";
                    }
                }
                else
                {
                    var questions = dbContext.TeacherTests.Find(item.IdTest).Questions.Split('|');
                    List<int> subj = new List<int>();
                    foreach (var i in questions)
                    {
                        int idSubj = dbContext.Questions.Find(Convert.ToInt32(i)).IdSubject;
                        if (!subj.Contains(idSubj))
                            subj.Add(idSubj);
                    }
                    for (int i = 1; i <= subj.Count; i++)
                    {
                        stats.Qualification += dbContext.Subjects.Find(i).Name;
                        if (subj.Count > 1 && i != subj.Count)
                            stats.Qualification += ", ";
                    }
                    if (!string.IsNullOrEmpty(dbContext.TestQualificationAccess.SingleOrDefault(u => u.IdExamination == item.Id).IdUsers))
                        stats.IsOver = "Тестирование прошло";
                    else
                    {
                        if (stats.Date > DateTime.Today)
                            stats.IsOver = "Тестирование запланировано";
                        else
                            stats.IsOver = "Тестирование просрочено";
                    }
                }
                model.Add(stats);
            }
            
            var userValue = dbContext.Users.Find(userId);
            ViewBag.Avatar = userValue.Avatar;
            Decliner decliner = new Decliner();
            string[] declineText = decliner.Decline(userValue.Family, userValue.Name, userValue.SubName, 2);//Меняем падеж
            ViewBag.Name = declineText[0] + " " + declineText[1] + " " + declineText[2];
            return View(model);
        }
    }
}