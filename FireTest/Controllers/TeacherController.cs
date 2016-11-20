using FireTest.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize(Roles = "ADMIN, TEACHER")]
    public class TeacherController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        public ActionResult Index()
        {
            var tests = dbContext.TeacherTests.ToList().Where(u => u.TeacherId == User.Identity.GetUserId());

            return View(tests);
        }
        [ChildActionOnly]
        public ActionResult IndexExams()
        {
            var exams = dbContext.Examinations.ToList().Where(u => u.TeacherId == User.Identity.GetUserId());
            return PartialView(exams);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            TeacherTest teacherTest = dbContext.TeacherTests.Find(id);
            if (teacherTest == null)
                return HttpNotFound();

            TeacherTestDetails testDetails = new TeacherTestDetails();
            testDetails.Id = teacherTest.Id;
            testDetails.NameTest = teacherTest.NameTest;
            List<string> questions = teacherTest.Questions.Split(',').ToList();
            testDetails.Questions = new List<Question>();
            foreach (string item in questions)
                testDetails.Questions.Add(dbContext.Questions.Find(Convert.ToInt32(item)));
            return View(testDetails);
        }
        public ActionResult CreateTest()
        {
            return View();
        }
        public PartialViewResult CreateTestSelect()
        {
            string userId = User.Identity.GetUserId();
            string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            List<string> userSubjects = temp.Split('|').ToList();
            var tempSubjects = dbContext.Subjects.Where(u => userSubjects.Contains(u.Id.ToString())).Select(u => new
            {
                Id = u.Id,
                Name = u.Name
            });
            var selectList = (from item in tempSubjects
                              select new SelectListItem()
                              {
                                  Text = item.Name,
                                  Value = item.Id.ToString(),
                              }).ToList();
            ViewBag.Subjects = selectList;
            return PartialView();
        }
        public PartialViewResult CreateTestAjax(int Subjects)
        {
            string userId = User.Identity.GetUserId();
            string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            List<string> userSubjects = temp.Split('|').ToList();
            var tempSubjects = dbContext.Subjects.Where(u => userSubjects.Contains(u.Id.ToString())).Select(u => new
            {
                Id = u.Id,
                Name = u.Name
            });
            var selectList = (from item in tempSubjects
                              select new SelectListItem()
                              {
                                  Text = item.Name,
                                  Value = item.Id.ToString(),
                                  Selected = item.Id == Subjects
                              }).ToList();
            ViewBag.Subjects = selectList;

            return PartialView();
        }
        public ActionResult SelectSubjects()
        {
            return View();
        }

        public PartialViewResult SelectSubjectsAjax(string currentFilter, string searchString, int? page, string NameTest, int? Subjects)
        {
            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            string userId = User.Identity.GetUserId();
            string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            List<string> userSubjects = temp.Split('|').ToList();
            var tempSubjects = dbContext.Subjects.Where(u => userSubjects.Contains(u.Id.ToString())).Select(u => new
            {
                Id = u.Id,
                Name = u.Name
            });
            if (!String.IsNullOrEmpty(searchString))
            {
                tempSubjects = dbContext.Subjects.Where(u => userSubjects.Contains(u.Id.ToString())).Where(u => u.Name.Contains(searchString)).Select(u => new
                {
                    Id = u.Id,
                    Name = u.Name
                });
            }
            List<Subject> model = new List<Subject>();
            foreach(var item in tempSubjects)
            {
                Subject subject = new Subject();
                subject.Id = item.Id;
                subject.Name = item.Name;
                model.Add(subject);
            }
            var selectList = (from item in model
                              select new SelectListItem()
                              {
                                  Text = item.Name,
                                  Value = item.Id.ToString(),
                                  Selected = item.Id == Subjects
                              }).ToList();
            ViewBag.Subjects = selectList;

            model = model.OrderBy(u => u.Name).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult SelectTag(int? id)
        {
            if (id == null)
                return RedirectToAction("SelectSubjects");

            ViewBag.Subject = id;
            return View();
        }
        public PartialViewResult SelectTagAjax(string currentFilter, string searchString, int? page, int subject)
        {
            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            var model = dbContext.Questions
                .Where(u => u.IdSubject == subject)
                .Where(u => u.Tag != null)
                .Select(u => u.Tag).ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}