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
            var tests = dbContext.TeacherTests.ToList().Where(u => u.TeacherId == User.Identity.GetUserId()).Where(u=>!string.IsNullOrEmpty(u.NameTest));

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
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TeacherTest teacherTest = dbContext.TeacherTests.Find(id);
            if (teacherTest == null)
            {
                return HttpNotFound();
            }
            return View(teacherTest);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TeacherTest teacherTest = dbContext.TeacherTests.Find(id);
            dbContext.TeacherTests.Remove(teacherTest);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult CreateTest()
        {
            Session.Clear();

            string userId = User.Identity.GetUserId();
            var result = dbContext.TeacherTests
                .Where(u => u.TeacherId == userId)
                .Where(u => string.IsNullOrEmpty(u.NameTest))
                .Where(u => string.IsNullOrEmpty(u.Questions))
                .Select(u => u.Id).SingleOrDefault();
            if (result != 0)
            {
                Session["Test"] = result;
                return View();
            }
            TeacherTest test = new TeacherTest();
            test.TeacherId = userId;
            dbContext.TeacherTests.Add(test);
            dbContext.SaveChanges();
            Session["Test"] = test.Id;
            return View();
        }
        public PartialViewResult CreateTestAjax(string currentFilter, string searchString, int? page, string NameTest, int? Subjects, string Tags, int? submitButton)
        {
            if ((int)Session["Subjects"] != Subjects || (string)Session["Tags"] != Tags)
                page = 1;
            Session["Subjects"] = Subjects;
            Session["Tags"] = Tags;

            string userId = User.Identity.GetUserId();
            int sessionId = (int)Session["Test"]; //Берем ид теста
            TeacherTest test = dbContext.TeacherTests.Find(sessionId);
            ViewBag.NameTest = test.NameTest;
            if (!string.IsNullOrEmpty(NameTest))
            {
                if (test != null && test.NameTest != NameTest)
                {
                    test.NameTest = NameTest; //Сохраняем название если оно изменилось
                    ViewBag.NameTest = NameTest;
                    dbContext.SaveChanges();
                }
            }

            List<string> subjects = new List<string>();
            if (test != null && test.Questions != null)
                subjects = test.Questions.Split('|').ToList();

            if (submitButton != null)
            {
                string newSubjects = "";
                int index = subjects.IndexOf(submitButton.ToString());
                if (index == -1)
                {
                    subjects.Add(submitButton.ToString());
                    if (test != null && test.Questions != null && test.Questions.Count() != 0)
                        newSubjects = test.Questions + "|" + submitButton.ToString();
                    else
                        newSubjects = submitButton.ToString();
                }
                else
                {
                    subjects.RemoveAt(index);
                    foreach (string item in subjects)
                    {
                        if (newSubjects.Length != 0)
                            newSubjects = newSubjects + "|" + item;
                        else
                            newSubjects = item;
                    }
                }
                if (test != null)
                {
                    test.Questions = newSubjects;
                    if (string.IsNullOrEmpty(test.NameTest))
                        test.NameTest = "Без названия " + sessionId;
                    ViewBag.NameTest = test.NameTest;
                }
                else
                {
                    TeacherTest newtest = new TeacherTest();
                    newtest.TeacherId = userId;
                    newtest.NameTest = "Без названия " + sessionId;
                    newtest.Questions = newSubjects;
                    dbContext.TeacherTests.Add(newtest);
                    Session["Test"] = newtest.Id;
                    ViewBag.NameTest = newtest.NameTest;
                }
                dbContext.SaveChanges();
            }

            if (!string.IsNullOrEmpty(searchString))
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            List<string> userSubjects = new List<string>(); //Берем в массив ид предметов у которых у нас доступ
            if (!string.IsNullOrEmpty(temp))
                userSubjects = temp.Split('|').ToList();

            var tempSubjects = dbContext.Subjects.Where(u => userSubjects.Contains(u.Id.ToString())).Select(u => new
            {
                Id = u.Id,
                Name = u.Name
            }); //Записываем предметы в список
            var selectList = tempSubjects //Добавляем выпадающий список из разрешенных предметов
                    .Select(u => new SelectListItem()
                    {
                        Text = u.Name,
                        Value = u.Id.ToString(),
                        Selected = u.Id == Subjects
                    }).ToList();
            ViewBag.Subjects = selectList;

            List<string> tempTags;

            int sub; //Если выбран предмет то используем его, если нет, то выбираем первый в списке разрешенных
            if (Subjects != null)
                sub = Subjects.Value;
            else
            {
                userSubjects.Sort();
                sub = Convert.ToInt32(userSubjects[0]);
            }

            tempTags = dbContext.Questions
                .Where(u => u.IdSubject == sub)
                .Select(u => u.Tag).Distinct().ToList(); //Берем все разделы
            tempTags[0] = "Все разделы";
            tempTags.Add("Без раздела");
            selectList = tempTags //Выпадающий список разделов
                .Select(u => new SelectListItem()
                {
                    Value = u,
                    Text = u,
                    Selected = u == Tags
                }).ToList();
            ViewBag.Tags = selectList;

            var tempQuestions = dbContext.Questions //Берем все вопросы дисциплины
                .Where(u => u.IdSubject == sub)
                .Select(u => new {
                    Id = u.Id,
                    Text = u.QuestionText
                }).ToList();
            if (Tags != "Все разделы" && Tags != "Без раздела")
            {
                tempQuestions = dbContext.Questions //Берем все вопросы дисциплины нужного раздела
                    .Where(u => u.IdSubject == sub)
                    .Where(u => u.Tag == Tags)
                    .Select(u => new {
                        Id = u.Id,
                        Text = u.QuestionText
                    }).ToList();
            }
            if (Tags == "Без раздела")
            {
                tempQuestions = dbContext.Questions //Берем все вопросы дисциплины без раздела
                    .Where(u => u.IdSubject == sub)
                    .Where(u => string.IsNullOrEmpty(u.Tag))
                    .Select(u => new {
                        Id = u.Id,
                        Text = u.QuestionText
                    }).ToList();
            }
            List<SubjectAccess> model = new List<SubjectAccess>(); //Использую это т.к. надо точно такие же поля
            foreach (var item in tempQuestions)
            {
                SubjectAccess subject = new SubjectAccess();
                if (!String.IsNullOrEmpty(searchString) && item.Text.ToLower().Contains(searchString.ToLower()))
                {
                    subject.Id = item.Id;
                    subject.Name = item.Text;
                    if (subjects.Contains(item.Id.ToString()))
                        subject.Access = true;
                    else
                        subject.Access = false;
                    model.Add(subject);
                }
                if(String.IsNullOrEmpty(searchString))
                {
                    subject.Id = item.Id;
                    subject.Name = item.Text;
                    if (subjects.Contains(item.Id.ToString()))
                        subject.Access = true;
                    else
                        subject.Access = false;
                    model.Add(subject);
                }
            }

            model = model.OrderBy(u => u.Name).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.page = pageNumber;

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TeacherTest teacherTest = dbContext.TeacherTests.Find(id);
            if (teacherTest == null)
            {
                return HttpNotFound();
            }
            TeacherTestDetails testDetails = new TeacherTestDetails();
            testDetails.Id = teacherTest.Id;
            testDetails.NameTest = teacherTest.NameTest;
            List<string> questions = teacherTest.Questions.Split('|').ToList();
            testDetails.Questions = new List<Question>();
            foreach (string item in questions)
                testDetails.Questions.Add(dbContext.Questions.Find(Convert.ToInt32(item)));
            Session["IdTest"] = id;
            return View(testDetails);
        }
        public PartialViewResult EditOld()
        {
            return PartialView();
        }
        public PartialViewResult EditOldAjax(string currentFilter, string searchString, int? page, string NameTest, int? submitButton)
        {
            string userId = User.Identity.GetUserId();
            int IdTest = (int)Session["IdTest"];
            TeacherTest test = dbContext.TeacherTests.Find(IdTest);

            if (!string.IsNullOrEmpty(NameTest))
            {
                if (test != null && test.NameTest != NameTest)
                {
                    test.NameTest = NameTest; //Сохраняем название если оно изменилось
                    dbContext.SaveChanges();
                }
            }
            TeacherTestDetails testDetails = new TeacherTestDetails();
            ViewBag.NameTest = test.NameTest;
            List<int> questions = test.Questions.Split('|').Select(int.Parse).ToList();
            if (submitButton != null)
            {
                int index = questions.IndexOf(submitButton.Value);
                questions.RemoveAt(index);
                string newQuestions = "";
                foreach (int item in questions)
                {
                    if (newQuestions.Length != 0)
                        newQuestions = newQuestions + "|" + item;
                    else
                        newQuestions = item.ToString();
                }
                test.Questions = newQuestions;
                dbContext.SaveChanges();
            }

            if (!string.IsNullOrEmpty(searchString))
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            var questionsOld = dbContext.Questions.Where(u => questions.Contains(u.Id)).ToList();
            List<SubjectAccess> model = new List<SubjectAccess>(); //Использую это т.к. надо точно такие же поля
            foreach (var item in questionsOld)
            {
                SubjectAccess subject = new SubjectAccess();
                if (!String.IsNullOrEmpty(searchString) && item.QuestionText.ToLower().Contains(searchString.ToLower()))
                {
                    subject.Id = item.Id;
                    subject.Name = item.QuestionText;
                    model.Add(subject);
                }
                if (String.IsNullOrEmpty(searchString))
                {
                    subject.Id = item.Id;
                    subject.Name = item.QuestionText;
                    model.Add(subject);
                }
            }

            model = model.OrderBy(u => u.Name).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (model.ToPagedList(pageNumber, pageSize).PageCount < page)
                pageNumber = model.ToPagedList(pageNumber, pageSize).PageCount;
            ViewBag.page = pageNumber;

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