using FireTest.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;

namespace FireTest.Controllers
{
    [Authorize(Roles = "ADMIN, TEACHER")]
    public class TeacherController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        public ActionResult Index(string message)
        {
            var userId = User.Identity.GetUserId();
            var tests = dbContext.TeacherTests.ToList().Where(u => u.TeacherId == userId).Where(u => !string.IsNullOrEmpty(u.NameTest));
            ViewBag.Message = message;

            var access = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherQualifications).SingleOrDefault();
            ViewBag.Access = false;
            if (access)
                ViewBag.Access = true;

            return View(tests);
        }
        [ChildActionOnly]
        public ActionResult IndexExams()
        {
            var exams = dbContext.Examinations.ToList().Where(u => u.TeacherId == User.Identity.GetUserId());
            return PartialView(exams);
        }
        [ChildActionOnly]
        public ActionResult IndexQualification()
        {
            var finishTest = dbContext.TeacherFinishTests.ToList().Where(u => u.TeacherId == User.Identity.GetUserId()).Where(u => !string.IsNullOrEmpty(u.NameTest));
            return PartialView(finishTest);
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
            testDetails.Eval5 = teacherTest.Eval5;
            testDetails.Eval4 = teacherTest.Eval4;
            testDetails.Eval3 = teacherTest.Eval3;

            if (!string.IsNullOrEmpty(teacherTest.Questions))
            {
                List<string> questions = teacherTest.Questions.Split('|').ToList();
                testDetails.Questions = new List<Question>();
                foreach (string item in questions)
                    testDetails.Questions.Add(dbContext.Questions.Find(Convert.ToInt32(item)));
            }
            return View(testDetails);
        }
        public ActionResult DetailsFinish(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            TeacherFinishTest teacherFinishTest = dbContext.TeacherFinishTests.Find(id);
            if (teacherFinishTest == null)
                return HttpNotFound();

            TeacherTestDetails testDetails = new TeacherTestDetails();
            testDetails.Id = teacherFinishTest.Id;
            testDetails.NameTest = teacherFinishTest.NameTest;
            testDetails.Eval5 = teacherFinishTest.Eval5;
            testDetails.Eval4 = teacherFinishTest.Eval4;
            testDetails.Eval3 = teacherFinishTest.Eval3;

            if (!string.IsNullOrEmpty(teacherFinishTest.Questions))
            {
                List<string> questions = teacherFinishTest.Questions.Split('|').ToList();
                testDetails.Questions = new List<Question>();
                foreach (string item in questions)
                    testDetails.Questions.Add(dbContext.Questions.Find(Convert.ToInt32(item)));
            }
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
        public ActionResult DeleteFinish(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TeacherFinishTest teacherFinishTest = dbContext.TeacherFinishTests.Find(id);
            if (teacherFinishTest == null)
            {
                return HttpNotFound();
            }
            return View(teacherFinishTest);
        }
        [HttpPost, ActionName("DeleteFinish")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedFinish(int id)
        {
            TeacherFinishTest teacherFinishTest = dbContext.TeacherFinishTests.Find(id);
            dbContext.TeacherFinishTests.Remove(teacherFinishTest);
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
        public PartialViewResult CreateTestAjax(string currentFilter, string searchString, int? page, string NameTest, int? Subjects, string Tags, int? submitButton, List<int> Eval)
        {
            if (Session["Subjects"] != null && Session["Tags"] != null)
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
            if (Eval != null)//Сохраняем оценки
            {
                if (Eval.Count() == 3)
                {
                    test.Eval5 = Eval[0];
                    test.Eval4 = Eval[1];
                    test.Eval3 = Eval[2];
                }
                else
                {
                    test.Eval5 = 0;
                    test.Eval4 = 0;
                    test.Eval3 = 0;
                }
                dbContext.SaveChanges();
            }
            ViewBag.Eval5 = test.Eval5;
            ViewBag.Eval4 = test.Eval4;
            ViewBag.Eval3 = test.Eval3;

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
            if (tempTags.Count == 0)
                tempTags.Add("Все разделы");
            else
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
                if (String.IsNullOrEmpty(searchString))
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
            Session.Clear();
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
            testDetails.Eval5 = teacherTest.Eval5;
            testDetails.Eval4 = teacherTest.Eval4;
            testDetails.Eval3 = teacherTest.Eval3;
            if (!string.IsNullOrEmpty(teacherTest.Questions))
            {
                List<string> questions = teacherTest.Questions.Split('|').ToList();
                testDetails.Questions = new List<Question>();
                foreach (string item in questions)
                    testDetails.Questions.Add(dbContext.Questions.Find(Convert.ToInt32(item)));
            }
            Session["IdTest"] = id;
            return View(testDetails);
        }
        public PartialViewResult EditOldAjax(string currentFilter, string searchString, int? page, string NameTest, int? submitButton, List<int> Eval)
        {
            string userId = User.Identity.GetUserId();
            if (Session["IdTest"] == null)
                return PartialView();

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
            if (Eval != null)//Сохраняем оценки
            {
                if (Eval.Count() == 3)
                {
                    test.Eval5 = Eval[0];
                    test.Eval4 = Eval[1];
                    test.Eval3 = Eval[2];
                }
                else
                {
                    test.Eval5 = 0;
                    test.Eval4 = 0;
                    test.Eval3 = 0;
                }
                dbContext.SaveChanges();
            }
            ViewBag.Eval5 = test.Eval5;
            ViewBag.Eval4 = test.Eval4;
            ViewBag.Eval3 = test.Eval3;
            List<int> questions = new List<int>();
            if (!string.IsNullOrEmpty(test.Questions))
                questions = test.Questions.Split('|').Select(int.Parse).ToList();

            ViewBag.Submit = false;
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
                ViewBag.Submit = true; //Чтобы обновить таблицу с вопросами (не работает)
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
            if (pageNumber == 0)
                pageNumber = 1;
            ViewBag.page = pageNumber;

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public PartialViewResult EditNewAjax(string currentFilter, string searchString, int? page, int? Subjects, string Tags, int? submitButton)
        {
            string userId = User.Identity.GetUserId();
            if (Session["IdTest"] == null)
                return PartialView();
            int IdTest = (int)Session["IdTest"];
            if (Session["Subjects"] != null && Session["Tags"] != null)
                if ((int)Session["Subjects"] != Subjects || (string)Session["Tags"] != Tags)
                    page = 1;
            Session["Subjects"] = Subjects;
            Session["Tags"] = Tags;

            TeacherTest test = dbContext.TeacherTests.Find(IdTest);
            ViewBag.Submit = false;
            if (submitButton != null)
            {
                if (test.Questions != null && test.Questions.Count() != 0)
                    test.Questions += "|" + submitButton;
                else
                    test.Questions = submitButton.ToString();

                ViewBag.Submit = true; //Чтобы обновить таблицу с вопросами теста
                dbContext.SaveChanges();
            }

            List<string> questions = new List<string>();
            if (test != null && test.Questions != null)
                questions = test.Questions.Split('|').ToList();

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
            if (tempTags.Count == 0)
                tempTags.Add("Все разделы");
            else
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

            var tempQuestions = dbContext.Questions //Берем все вопросы дисциплины, которых нет в тесте
                .Where(u => u.IdSubject == sub)
                .Where(u => !questions.Contains(u.Id.ToString()))
                .Select(u => new {
                    Id = u.Id,
                    Text = u.QuestionText,
                    Tag = u.Tag
                }).ToList();

            if (Tags != "Все разделы" && Tags != "Без раздела")
            {
                tempQuestions = tempQuestions //Берем все вопросы дисциплины нужного раздела
                    .Where(u => u.Tag == Tags)
                    .Select(u => new {
                        Id = u.Id,
                        Text = u.Text,
                        Tag = u.Tag
                    }).ToList();
            }
            if (Tags == "Без раздела")
            {
                tempQuestions = tempQuestions //Берем все вопросы дисциплины без раздела
                    .Where(u => string.IsNullOrEmpty(u.Tag))
                    .Select(u => new {
                        Id = u.Id,
                        Text = u.Text,
                        Tag = u.Tag
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
                    if (questions.Contains(item.Id.ToString()))
                        subject.Access = true;
                    else
                        subject.Access = false;
                    model.Add(subject);
                }
                if (String.IsNullOrEmpty(searchString))
                {
                    subject.Id = item.Id;
                    subject.Name = item.Text;
                    if (questions.Contains(item.Id.ToString()))
                        subject.Access = true;
                    else
                        subject.Access = false;
                    model.Add(subject);
                }
            }

            model = model.OrderBy(u => u.Name).ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (model.ToPagedList(pageNumber, pageSize).PageCount < page)
                pageNumber = model.ToPagedList(pageNumber, pageSize).PageCount;
            if (pageNumber == 0)
                pageNumber = 1;
            ViewBag.page = pageNumber;

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }




        public ActionResult CreateTestFinishPrepare()
        {
            ViewBag.Qualifications = dbContext.Qualifications //Добавляем выпадающий список квалификаций
                    .Select(u => new SelectListItem()
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }).ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTestFinishPrepare(TeacherFinishTestPrepareViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Qualifications = dbContext.Qualifications //Добавляем выпадающий список квалификаций
                       .Select(u => new SelectListItem()
                       {
                           Text = u.Name,
                           Value = u.Id.ToString()
                       }).ToList();
                return View(model);
            }

            string userId = User.Identity.GetUserId();
            TeacherFinishTest newtest = new TeacherFinishTest();
            newtest.TeacherId = userId;
            newtest.NameTest = model.NameTest;
            newtest.Eval3 = model.Eval3;
            newtest.Eval4 = model.Eval4;
            newtest.Eval5 = model.Eval5;
            dbContext.TeacherFinishTests.Add(newtest);
            dbContext.SaveChanges();
            //        return RedirectToAction("CreateTestFinish", new System.Web.Routing.RouteValueDictionary(
            //new { idTest = newtest.Id }));
            return RedirectToAction("CreateTestFinish", new { idTest = newtest.Id });
        }
        public ActionResult CreateTestFinish(int idTest)
        {
            Session.Clear();
            //string userId = User.Identity.GetUserId();
            //var result = dbContext.TeacherFinishTests
            //    .Where(u => u.TeacherId == userId)
            //    .Where(u => u.Id == idTest)
            //    .Select(u => u.Id).SingleOrDefault();
            //if (result != 0)
            //{
                Session["idFinishTest"] = idTest;
            //    return View();
            //}
            //TeacherFinishTest test = new TeacherFinishTest();
            //test.TeacherId = userId;
            //dbContext.TeacherFinishTests.Add(test);
            //dbContext.SaveChanges();
            //Session["Test"] = test.Id;
            return View();
        }
        public PartialViewResult CreateTestFinishAjax(string currentFilter, string searchString, int? page, int? submitButton, bool submitButtonAll = false)
        {
            //if (Session["Subjects"] != null && Session["Tags"] != null)
            //    if ((int)Session["Subjects"] != Subjects || (string)Session["Tags"] != Tags)
            //        page = 1;
            //Session["Subjects"] = Subjects;
            //Session["Tags"] = Tags;

            string userId = User.Identity.GetUserId();
            int sessionId = (int)Session["idFinishTest"]; //Берем ид теста
            TeacherFinishTest test = dbContext.TeacherFinishTests.Find(sessionId);
            ViewBag.NameTest = test.NameTest;
            //if (!string.IsNullOrEmpty(NameTest))
            //{
            //    if (test != null && test.NameTest != NameTest)
            //    {
            //        test.NameTest = NameTest; //Сохраняем название если оно изменилось
            //        ViewBag.NameTest = NameTest;
            //        dbContext.SaveChanges();
            //    }
            //}
            //if (Eval != null)//Сохраняем оценки
            //{
            //    if (Eval.Count() == 3)
            //    {
            //        test.Eval5 = Eval[0];
            //        test.Eval4 = Eval[1];
            //        test.Eval3 = Eval[2];
            //    }
            //    else
            //    {
            //        test.Eval5 = 0;
            //        test.Eval4 = 0;
            //        test.Eval3 = 0;
            //    }
            //    dbContext.SaveChanges();
            //}
            ViewBag.Eval5 = test.Eval5;
            ViewBag.Eval4 = test.Eval4;
            ViewBag.Eval3 = test.Eval3;

            List<string> subjects = new List<string>();
            if (submitButtonAll)
            {
                var Subjects = "";
                var allSubjects = dbContext.Questions.Where(u => u.IdQualification == test.IdQualification).ToList();
                foreach (var item in allSubjects)
                {
                    if (Subjects.Length > 0)
                        Subjects += "|" + item.Id;
                    else
                        Subjects += item.Id;
                    subjects.Add(item.Id.ToString());
                }
                test.Questions = Subjects;
                dbContext.SaveChanges();
            }
            else
            {
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
                    test.Questions = newSubjects;
                    dbContext.SaveChanges();
                }
                //if (test != null)
                //{
                //    test.Questions = newSubjects;
                //    if (string.IsNullOrEmpty(test.NameTest))
                //        test.NameTest = "Без названия " + sessionId;
                //    ViewBag.NameTest = test.NameTest;
                //}
                //else
                //{
                //    TeacherFinishTest newtest = new TeacherFinishTest();
                //    newtest.TeacherId = userId;
                //    newtest.NameTest = "Без названия " + sessionId;
                //    newtest.Questions = newSubjects;
                //    dbContext.TeacherFinishTests.Add(newtest);
                //    Session["Test"] = newtest.Id;
                //    ViewBag.NameTest = newtest.NameTest;
                //}
            }

            if (!string.IsNullOrEmpty(searchString))
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            //string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            //List<string> userSubjects = new List<string>(); //Берем в массив ид предметов у которых у нас доступ
            //if (!string.IsNullOrEmpty(temp))
            //    userSubjects = temp.Split('|').ToList();

            //var tempSubjects = dbContext.Subjects.Where(u => userSubjects.Contains(u.Id.ToString())).Select(u => new
            //{
            //    Id = u.Id,
            //    Name = u.Name
            //}); //Записываем предметы в список
            //var selectList = tempSubjects //Добавляем выпадающий список из разрешенных предметов
            //        .Select(u => new SelectListItem()
            //        {
            //            Text = u.Name,
            //            Value = u.Id.ToString(),
            //            Selected = u.Id == Subjects
            //        }).ToList();
            //ViewBag.Subjects = selectList;

            //List<string> tempTags;

            //int sub; //Если выбран предмет то используем его, если нет, то выбираем первый в списке разрешенных
            //if (Subjects != null)
            //    sub = Subjects.Value;
            //else
            //{
            //    userSubjects.Sort();
            //    sub = Convert.ToInt32(userSubjects[0]);
            //}

            //tempTags = dbContext.Questions
            //    .Where(u => u.IdSubject == sub)
            //    .Select(u => u.Tag).Distinct().ToList(); //Берем все разделы
            //if (tempTags.Count == 0)
            //    tempTags.Add("Все разделы");
            //else
            //    tempTags[0] = "Все разделы";
            //tempTags.Add("Без раздела");
            //selectList = tempTags //Выпадающий список разделов
            //    .Select(u => new SelectListItem()
            //    {
            //        Value = u,
            //        Text = u,
            //        Selected = u == Tags
            //    }).ToList();
            //ViewBag.Tags = selectList;

            var tempQuestions = dbContext.Questions //Берем все вопросы дисциплины
                .Where(u => u.IdQualification == test.IdQualification)
                .Select(u => new {
                    Id = u.Id,
                    Text = u.QuestionText
                }).ToList();
            //if (Tags != "Все разделы" && Tags != "Без раздела")
            //{
            //    tempQuestions = dbContext.Questions //Берем все вопросы дисциплины нужного раздела
            //        .Where(u => u.IdSubject == sub)
            //        .Where(u => u.Tag == Tags)
            //        .Select(u => new {
            //            Id = u.Id,
            //            Text = u.QuestionText
            //        }).ToList();
            //}
            //if (Tags == "Без раздела")
            //{
            //    tempQuestions = dbContext.Questions //Берем все вопросы дисциплины без раздела
            //        .Where(u => u.IdSubject == sub)
            //        .Where(u => string.IsNullOrEmpty(u.Tag))
            //        .Select(u => new {
            //            Id = u.Id,
            //            Text = u.QuestionText
            //        }).ToList();
            //}
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
                if (String.IsNullOrEmpty(searchString))
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



        
        public ActionResult NewQuestion(string Message, int? Department, int? Subject, int? Course)
        {
            ViewBag.StatusMessage = Message;
            string userId = User.Identity.GetUserId();
            string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            List<string> userSubjects = new List<string>(); //Берем в массив ид предметов у которых у нас доступ
            if (!string.IsNullOrEmpty(temp))
                userSubjects = temp.Split('|').ToList();
            else
                return RedirectToAction("Error");

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
                        Selected = u.Id == Subject
                    }).ToList();
            ViewBag.Subjects = selectList;

            selectList = dbContext.Departments //Добавляем выпадающий список из кафедр
               .Select(u => new SelectListItem()
               {
                   Text = u.Name,
                   Value = u.Id.ToString(),
                   Selected = u.Id == Department
               }).ToList();
            ViewBag.Departments = selectList;

            List<int> tempCourses = new List<int>() { 1, 2, 3, 4, 5 }; //Добавляем выпадающий список курсов
            selectList = tempCourses
               .Select(u => new SelectListItem()
               {
                   Text = "Курс " + u.ToString(),
                   Value = u.ToString(),
                   Selected = u == Course
               }).ToList();
            ViewBag.Courses = selectList;

            ViewBag.Type = 1;
            return View();
        }
        [HttpPost]
        public ActionResult NewQuestion(ViewCreateQuestion model, int Subjects, int Departments, int Courses, int Type, List<string> AnswersSequence, List<string> AnswersConformity, HttpPostedFileBase uploadfile)
        {
            if (!ModelState.IsValid || Type != 1)
            {
                string userId = User.Identity.GetUserId();
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

                selectList = dbContext.Departments //Добавляем выпадающий список из кафедр
                   .Select(u => new SelectListItem()
                   {
                       Text = u.Name,
                       Value = u.Id.ToString(),
                       Selected = u.Id == Departments
                   }).ToList();
                ViewBag.Departments = selectList;

                List<int> tempCourses = new List<int>() { 1, 2, 3, 4, 5 }; //Добавляем выпадающий список курсов
                selectList = tempCourses
                   .Select(u => new SelectListItem()
                   {
                       Text = "Курс "+u.ToString(),
                       Value = u.ToString(),
                       Selected = u == Courses
                   }).ToList();
                ViewBag.Courses = selectList;
                ViewBag.Type = Type;
                if (Type == 1)
                    return View(model);
                if (Type == 2)
                {
                    int countAnswers = 0;
                    foreach (string item in AnswersSequence)
                        if (string.IsNullOrEmpty(item))
                            countAnswers++;
                    if (countAnswers == 8)
                    {
                        ViewBag.Error = "Должен быть хотя бы один ответ";
                        return View(model);
                    }
                    if (string.IsNullOrEmpty(model.QuestionText))
                    {
                        ViewBag.Answers = AnswersSequence;
                        return View(model);
                    }
                }
                if (Type == 3)
                {
                    int countAnswers = 0;
                    for (int i = 0; i < 4; i++)
                        if (!string.IsNullOrEmpty(AnswersConformity[i]) && !string.IsNullOrEmpty(AnswersConformity[i + 4]))
                            countAnswers++;
                    if (string.IsNullOrEmpty(model.QuestionText))
                    {
                        ViewBag.Answers = AnswersConformity;
                        return View(model);
                    }
                    if (countAnswers == 0) //Если хоть 1 пара есть, то норм
                    {
                        ViewBag.Error = "Должна быть хотя бы одна пара значений";
                        return View(model);
                    }
                }
            }

            Question newQuestion = new Question();
            newQuestion.QuestionText = model.QuestionText;
            newQuestion.IdCourse = Courses;
            newQuestion.IdDepartment = Departments;
            newQuestion.IdSubject = Subjects;
            newQuestion.Tag = model.Tag;
            if (uploadfile != null) //Загрузка картинки вопроса, если есть
            {
                string extension = Path.GetExtension(uploadfile.FileName).ToLower();
                MD5 md5 = MD5.Create();
                byte[] avatar = new byte[uploadfile.ContentLength];
                string ImageName = BitConverter.ToString(md5.ComputeHash(avatar)).Replace("-", "").ToLower();
                uploadfile.SaveAs(Server.MapPath("~/Images/Questions/" + ImageName + extension));
                newQuestion.QuestionImage = ImageName + extension;
            }
            dbContext.Questions.Add(newQuestion);
            dbContext.SaveChanges();
            int index = newQuestion.Id;

            if (Type == 1)
            {
                List<string> answers = new List<string>();
                List<int> answersCorrect = new List<int>();
                int correct = 0;
                foreach (var item in model.Answers)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        answers.Add(item);
                        if (model.AnswersCorrects.Contains(correct))
                            answersCorrect.Add(answers.Count() - 1);
                    }
                    correct++;
                }

                List<int> indexAnswers = new List<int>();
                Answer newAnswer = new Answer();
                for (int i = 0; i < answers.Count(); i++)
                {
                    newAnswer.IdQuestion = index;
                    newAnswer.AnswerText = answers[i];
                    dbContext.Answers.Add(newAnswer);
                    dbContext.SaveChanges();
                    indexAnswers.Add(newAnswer.Id);
                }
                int count = 0;
                string answerCorrectIndex = "";
                foreach (int item in indexAnswers)
                {
                    if (answersCorrect.Contains(count))
                    {
                        if (answerCorrectIndex.Length > 0)
                            answerCorrectIndex += "," + item;
                        else
                            answerCorrectIndex = "" + item;
                    }
                    count++;
                }
                newQuestion.IdCorrect = answerCorrectIndex;
                dbContext.SaveChanges();
            }
            if (Type == 2)
            {
                List<string> answers = new List<string>();
                foreach (var item in AnswersSequence)
                    if (!String.IsNullOrEmpty(item))
                        answers.Add(item);

                List<int> indexAnswers = new List<int>();
                Answer newAnswer = new Answer();
                for (int i = 0; i < answers.Count(); i++)
                {
                    newAnswer.IdQuestion = index;
                    newAnswer.AnswerText = answers[i];
                    dbContext.Answers.Add(newAnswer);
                    dbContext.SaveChanges();
                    indexAnswers.Add(newAnswer.Id);
                }
                string answerCorrectIndex = "~";
                foreach (int item in indexAnswers)
                    answerCorrectIndex += item + ",";
                answerCorrectIndex = answerCorrectIndex.Substring(0, answerCorrectIndex.Length - 1);
                newQuestion.IdCorrect = answerCorrectIndex;
                dbContext.SaveChanges();
            }
            if (Type == 3)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (string.IsNullOrEmpty(AnswersConformity[i]) || string.IsNullOrEmpty(AnswersConformity[i + 4]))
                    {
                        AnswersConformity[i] = null;
                        AnswersConformity[i + 4] = null;
                    }
                }
                List<string> answers = new List<string>();

                foreach (var item in AnswersConformity)
                    if (!String.IsNullOrEmpty(item))
                        answers.Add(item);
                List<int> indexAnswers = new List<int>();
                Answer newAnswer = new Answer();
                for (int i = 0; i < answers.Count(); i++)
                {
                    newAnswer.IdQuestion = index;
                    newAnswer.AnswerText = answers[i];
                    dbContext.Answers.Add(newAnswer);
                    dbContext.SaveChanges();
                    indexAnswers.Add(newAnswer.Id);
                }
                string answerCorrectIndex = "#";
                int halfCount = indexAnswers.Count() / 2;
                for (int i = 0; i < halfCount; i++)
                {
                    if (i > 0)
                        answerCorrectIndex += "," + indexAnswers[i] + "=" + indexAnswers[i + halfCount];
                    else
                        answerCorrectIndex += indexAnswers[i] + "=" + indexAnswers[i + halfCount];
                }
                newQuestion.IdCorrect = answerCorrectIndex;
                dbContext.SaveChanges();
            }
            return RedirectToAction("NewQuestion", new { Message = "Вопрос был успешно добавлен", Department = newQuestion.IdDepartment, Subject = newQuestion.IdSubject, Course = newQuestion.IdCourse });
        }
        public ActionResult EditQuestionSelect(string message)
        {
            Session.Clear();

            string userId = User.Identity.GetUserId();
            string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            if (string.IsNullOrEmpty(temp))
                return RedirectToAction("Error");

            ViewBag.DeleteQualifications = message;
            return View();
        }
        public PartialViewResult EditQuestionSelectAjax(string currentFilter, string searchString, int? page, string NameTest, int? Subjects, string Tags)
        {
            string userId = User.Identity.GetUserId();
            if (Session["Subjects"] != null && Session["Tags"] != null)
                if ((int)Session["Subjects"] != Subjects || (string)Session["Tags"] != Tags)
                    page = 1;
            Session["Subjects"] = Subjects;
            Session["Tags"] = Tags;
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
            if (tempTags.Count == 0)
                tempTags.Add("Все разделы");
            else
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
                    Text = u.QuestionText,
                    Qualification = u.IdQualification
                }).ToList();
            if (Tags != "Все разделы" && Tags != "Без раздела")
            {
                tempQuestions = dbContext.Questions //Берем все вопросы дисциплины нужного раздела
                    .Where(u => u.IdSubject == sub)
                    .Where(u => u.Tag == Tags)
                    .Select(u => new {
                        Id = u.Id,
                        Text = u.QuestionText,
                        Qualification = u.IdQualification
                    }).ToList();
            }
            if (Tags == "Без раздела")
            {
                tempQuestions = dbContext.Questions //Берем все вопросы дисциплины без раздела
                    .Where(u => u.IdSubject == sub)
                    .Where(u => string.IsNullOrEmpty(u.Tag))
                    .Select(u => new {
                        Id = u.Id,
                        Text = u.QuestionText,
                        Qualification = u.IdQualification
                    }).ToList();
            }

            List<SubjectsAndQualification> model = new List<SubjectsAndQualification>();
            foreach (var item in tempQuestions)
            {
                SubjectsAndQualification subject = new SubjectsAndQualification();
                if (!String.IsNullOrEmpty(searchString) && item.Text.ToLower().Contains(searchString.ToLower()))
                {
                    subject.Id = item.Id;
                    subject.Name = item.Text;
                    subject.Qualification = item.Qualification;
                    if (item.Qualification != 0)
                        subject.QualificationName = dbContext.Qualifications.Find(item.Qualification).Name;
                    model.Add(subject);
                }
                if (String.IsNullOrEmpty(searchString))
                {
                    subject.Id = item.Id;
                    subject.Name = item.Text;
                    subject.Qualification = item.Qualification;
                    if (item.Qualification != 0)
                        subject.QualificationName = dbContext.Qualifications.Find(item.Qualification).Name;
                    model.Add(subject);
                }
            }

            model = model.OrderBy(u => u.Name).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.page = pageNumber;

            if (dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherQualifications).SingleOrDefault())
                ViewBag.Access = true;

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult EditQuestion(int id, string Message)
        {
            ViewBag.Id = id;
            ViewBag.StatusMessage = Message;
            string userId = User.Identity.GetUserId();
            string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            List<string> userSubjects = new List<string>(); //Берем в массив ид предметов у которых у нас доступ
            if (!string.IsNullOrEmpty(temp))
                userSubjects = temp.Split('|').ToList();
            else
                return RedirectToAction("Error");

            var question = dbContext.Questions.Find(id); //Данные вопроса
            if (!string.IsNullOrEmpty(question.QuestionImage) && question.QuestionImage != "NULL")
                ViewBag.QuestionImage = question.QuestionImage;

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
                        Selected = u.Id == question.IdSubject
                    }).ToList();
            ViewBag.Subjects = selectList;

            selectList = dbContext.Departments //Добавляем выпадающий список из кафедр
               .Select(u => new SelectListItem()
               {
                   Text = u.Name,
                   Value = u.Id.ToString(),
                   Selected = u.Id == question.IdDepartment
               }).ToList();
            ViewBag.Departments = selectList;

            List<int> tempCourses = new List<int>() { 1, 2, 3, 4, 5 }; //Добавляем выпадающий список курсов
            selectList = tempCourses
               .Select(u => new SelectListItem()
               {
                   Text = "Курс " + u.ToString(),
                   Value = u.ToString(),
                   Selected = u == question.IdCourse
               }).ToList();
            ViewBag.Courses = selectList;

            var model = new ViewEditQuestion();
            model.QuestionText = question.QuestionText;
            model.Tag = question.Tag;            
            model.Answers = new List<Answer>();
            List<bool> tempAnswersCorrects = new List<bool>();
            string correct = question.IdCorrect;
            if (correct[0] != '~' && correct[0] != '#') //Если обычный вопрос
            {
                ViewBag.Type = 1;
                List<string> correctAnswers = correct.Split(',').ToList();
                List<int> AnswersTrue = new List<int>();
                var Answers = dbContext.Answers
                    .Where(u => u.IdQuestion == id)
                    .Select(u => new
                    {
                        Id = u.Id,
                        AnswerText = u.AnswerText
                    }).ToList();
                correctAnswers.Sort();
                int i = 0;
                foreach (var item in Answers)
                {
                    if (correctAnswers.Contains(item.Id.ToString()))
                    {
                        tempAnswersCorrects.Add(true);
                        
                        i++;
                    }
                    else
                        tempAnswersCorrects.Add(false);
                    Answer tempAnswer = new Answer();
                    tempAnswer.Id = item.Id;
                    tempAnswer.AnswerText = item.AnswerText;
                    model.Answers.Add(tempAnswer);
                }
                ViewBag.Corrects = tempAnswersCorrects;
            }
            if (correct[0] == '~')
            {
                ViewBag.Type = 2;
                var Answers = dbContext.Answers
                    .Where(u => u.IdQuestion == id)
                    .Select(u => new
                    {
                        Id = u.Id,
                        AnswerText = u.AnswerText
                    }).ToList();
                foreach (var item in Answers)
                {
                    Answer tempAnswer = new Answer();
                    tempAnswer.Id = item.Id;
                    tempAnswer.AnswerText = item.AnswerText;
                    model.Answers.Add(tempAnswer);
                }
            }
            if (correct[0] == '#')
            {
                ViewBag.Type = 3;
                correct = correct.Substring(1);
                List<string> correctAnswersCouple = correct.Split(',').ToList();
                foreach (string item in correctAnswersCouple)
                {
                    List<string> correctAnswers = item.Split('=').ToList();
                    foreach (string item2 in correctAnswers)
                    {
                        var Answers = dbContext.Answers
                                .Where(u => u.Id.ToString() == item2)
                                .Select(u => new
                                {
                                    Id = u.Id,
                                    AnswerText = u.AnswerText
                                }).ToList();
                        foreach (var item3 in Answers)
                        {
                            Answer tempAnswer = new Answer();
                            tempAnswer.Id = item3.Id;
                            tempAnswer.AnswerText = item3.AnswerText;
                            model.Answers.Add(tempAnswer);
                        }
                    }
                }
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult EditQuestion(ViewEditQuestion Question, int Type, int id, int Subjects, int Departments, int Courses, string Delete, HttpPostedFileBase uploadfile)
        {
            if (!ModelState.IsValid)
                return View(Question);

            var question = dbContext.Questions.Find(id);
            question.IdCourse = Courses;
            question.IdDepartment = Departments;
            question.IdSubject = Subjects;
            question.QuestionText = Question.QuestionText;
            question.Tag = Question.Tag;

            var NewAnswersId = dbContext.Answers
                    .Where(u => u.IdQuestion == id)
                    .Select(u => u.Id).ToList();
            int i = 0;
            string NewAnswersCorrect = question.IdCorrect;
            string tempNewAnswersCorrect = "";
            int count = 0;
            foreach (var item in Question.Answers)
            {
                if (item != null && !string.IsNullOrEmpty(item.AnswerText))
                    count++;
            }
            if (Type == 1)
            {
                if (count == NewAnswersId.Count())
                {
                    foreach (var item in NewAnswersId)
                    {
                        var NewAnswers = dbContext.Answers.Find(item);
                        NewAnswers.AnswerText = Question.Answers[i].AnswerText;
                        if (Question.AnswersCorrects.Contains(i))
                        {
                            if (tempNewAnswersCorrect.Length > 0)
                                tempNewAnswersCorrect += "," + NewAnswers.Id;
                            else
                                tempNewAnswersCorrect = "" + NewAnswers.Id;
                        }
                        i++;
                    }
                }
                if (count < NewAnswersId.Count())
                {
                    foreach (var item in NewAnswersId)
                    {
                        var delete = dbContext.Answers.Find(item);
                        dbContext.Answers.Remove(delete);
                    }
                    for (int j = 0; j < 8;)
                    {
                        if (!string.IsNullOrEmpty(Question.Answers[j].AnswerText))
                        {
                            Answer NewAnswers = new Answer();
                            NewAnswers.IdQuestion = id;
                            NewAnswers.AnswerText = Question.Answers[j].AnswerText;
                            dbContext.Answers.Add(NewAnswers);
                            dbContext.SaveChanges();

                            if (Question.AnswersCorrects.Contains(j))
                            {
                                if (tempNewAnswersCorrect.Length > 0)
                                    tempNewAnswersCorrect += "," + NewAnswers.Id;
                                else
                                    tempNewAnswersCorrect = "" + NewAnswers.Id;
                            }
                        }
                        j++;
                    }
                }
                if (count > NewAnswersId.Count())
                {
                    for (int j = 0; j < 8;)
                    {
                        if (!string.IsNullOrEmpty(Question.Answers[j].AnswerText))
                        {
                            Answer NewAnswers = new Answer();
                            if (NewAnswersId.Count() > j)
                            {
                                NewAnswers = dbContext.Answers.Find(NewAnswersId[j]);
                                NewAnswers.AnswerText = Question.Answers[j].AnswerText;
                            }
                            else
                            {
                                NewAnswers.IdQuestion = id;
                                NewAnswers.AnswerText = Question.Answers[j].AnswerText;
                                dbContext.Answers.Add(NewAnswers);
                                dbContext.SaveChanges();
                            }
                            if (Question.AnswersCorrects.Contains(j))
                            {
                                if (tempNewAnswersCorrect.Length > 0)
                                    tempNewAnswersCorrect += "," + NewAnswers.Id;
                                else
                                    tempNewAnswersCorrect = "" + NewAnswers.Id;
                            }
                        }
                        j++;
                    }
                }
            }
            if (Type == 2)
            {
                if (count == NewAnswersId.Count())
                {
                    tempNewAnswersCorrect = "~";
                    foreach (var item in NewAnswersId)
                    {
                        var NewAnswers = dbContext.Answers.Find(item);
                        NewAnswers.AnswerText = Question.Answers[i].AnswerText;
                        tempNewAnswersCorrect += NewAnswers.Id + ",";
                        i++;
                    }
                    tempNewAnswersCorrect = tempNewAnswersCorrect.Substring(0, tempNewAnswersCorrect.Length - 1);
                }
                if (count > NewAnswersId.Count())
                {
                    tempNewAnswersCorrect = "~";
                    for (int j = 0; j < 8;)
                    {
                        if (!string.IsNullOrEmpty(Question.Answers[j].AnswerText))
                        {
                            Answer NewAnswers = new Answer();
                            if (NewAnswersId.Count() > j)
                            {
                                NewAnswers = dbContext.Answers.Find(NewAnswersId[j]);
                                NewAnswers.AnswerText = Question.Answers[j].AnswerText;
                                tempNewAnswersCorrect += NewAnswers.Id + ",";
                            }
                            else
                            {
                                NewAnswers.IdQuestion = id;
                                NewAnswers.AnswerText = Question.Answers[j].AnswerText;
                                dbContext.Answers.Add(NewAnswers);
                                dbContext.SaveChanges();
                                tempNewAnswersCorrect += NewAnswers.Id + ",";
                            }
                        }
                        j++;
                    }
                    tempNewAnswersCorrect = tempNewAnswersCorrect.Substring(0, tempNewAnswersCorrect.Length - 1);
                }
                if (count < NewAnswersId.Count())
                {
                    foreach (var item in NewAnswersId)
                    {
                        var delete = dbContext.Answers.Find(item);
                        dbContext.Answers.Remove(delete);
                    }
                    tempNewAnswersCorrect = "~";
                    for (int j = 0; j < 8;)
                    {
                        if (!string.IsNullOrEmpty(Question.Answers[j].AnswerText))
                        {
                            Answer NewAnswers = new Answer();
                            NewAnswers.IdQuestion = id;
                            NewAnswers.AnswerText = Question.Answers[j].AnswerText;
                            dbContext.Answers.Add(NewAnswers);
                            dbContext.SaveChanges();
                            tempNewAnswersCorrect += NewAnswers.Id + ",";
                        }
                        j++;
                    }
                    tempNewAnswersCorrect = tempNewAnswersCorrect.Substring(0, tempNewAnswersCorrect.Length - 1);
                }
            }
            if (Type == 3)
            {
                if (count == NewAnswersId.Count())
                {
                    List<int> indexAnswers = new List<int>();
                    foreach (var item in NewAnswersId)
                    {
                        var NewAnswers = dbContext.Answers.Find(item);
                        NewAnswers.AnswerText = Question.Answers[i].AnswerText;
                        indexAnswers.Add(NewAnswers.Id);
                        i++;
                    }
                    tempNewAnswersCorrect = "#";
                    int halfCount = indexAnswers.Count() / 2;
                    for (int j = 0; j < halfCount; j++)
                    {
                        if (j > 0)
                            tempNewAnswersCorrect += "," + indexAnswers[j] + "=" + indexAnswers[j + halfCount];
                        else
                            tempNewAnswersCorrect += indexAnswers[j] + "=" + indexAnswers[j + halfCount];
                    }
                }
                else
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (string.IsNullOrEmpty(Question.Answers[j].AnswerText) || string.IsNullOrEmpty(Question.Answers[j + 4].AnswerText))
                        {
                            Question.Answers[j].AnswerText = null;
                            Question.Answers[j + 4].AnswerText = null;
                        }
                    }
                    foreach (var item in NewAnswersId)
                    {
                        var delete = dbContext.Answers.Find(item);
                        dbContext.Answers.Remove(delete);
                    }

                    List<string> answers = new List<string>();

                    foreach (var item in Question.Answers)
                        if (!String.IsNullOrEmpty(item.AnswerText))
                            answers.Add(item.AnswerText);
                    List<int> indexAnswers = new List<int>();
                    Answer newAnswer = new Answer();
                    for (int j = 0; j < answers.Count(); j++)
                    {
                        newAnswer.IdQuestion = id;
                        newAnswer.AnswerText = answers[j];
                        dbContext.Answers.Add(newAnswer);
                        dbContext.SaveChanges();
                        indexAnswers.Add(newAnswer.Id);
                    }
                    tempNewAnswersCorrect = "#";
                    int halfCount = indexAnswers.Count() / 2;
                    for (int j = 0; j < halfCount; j++)
                    {
                        if (j > 0)
                            tempNewAnswersCorrect += "," + indexAnswers[j] + "=" + indexAnswers[j + halfCount];
                        else
                            tempNewAnswersCorrect += indexAnswers[j] + "=" + indexAnswers[j + halfCount];
                    }
                }
            }
            question.IdCorrect = tempNewAnswersCorrect;
            if (!string.IsNullOrEmpty(Delete))
            {
                System.IO.File.Delete(Server.MapPath("~/Images/Questions/" + question.QuestionImage));
                question.QuestionImage = null;

            }
            if (uploadfile != null) //Загрузка картинки вопроса, если есть
            {
                string extension = Path.GetExtension(uploadfile.FileName).ToLower();
                MD5 md5 = MD5.Create();
                byte[] avatar = new byte[uploadfile.ContentLength];
                string ImageName = BitConverter.ToString(md5.ComputeHash(avatar)).Replace("-", "").ToLower();
                uploadfile.SaveAs(Server.MapPath("~/Images/Questions/" + ImageName + extension));
                question.QuestionImage = ImageName + extension;
            }
            dbContext.SaveChanges();
            return RedirectToAction("EditQuestion", new { id, Message = "Вопрос был успешно изменен" });
        }
        public ActionResult DeleteQuestionSelect()
        {
            Session.Clear();

            string userId = User.Identity.GetUserId();
            string temp = dbContext.TeachersAccess.Where(u => u.TeacherId == userId).Select(u => u.TeacherSubjects).SingleOrDefault();
            if (string.IsNullOrEmpty(temp))
                return RedirectToAction("Error");

            return View();
        }
        public PartialViewResult DeleteQuestionSelectAjax(string currentFilter, string searchString, int? page, string NameTest, int? Subjects, string Tags)
        {
            string userId = User.Identity.GetUserId();
            if (Session["Subjects"] != null && Session["Tags"] != null)
                if ((int)Session["Subjects"] != Subjects || (string)Session["Tags"] != Tags)
                    page = 1;
            Session["Subjects"] = Subjects;
            Session["Tags"] = Tags;
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
            if (tempTags.Count == 0)
                tempTags.Add("Все разделы");
            else
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
                    model.Add(subject);
                }
                if (String.IsNullOrEmpty(searchString))
                {
                    subject.Id = item.Id;
                    subject.Name = item.Text;
                    model.Add(subject);
                }
            }

            model = model.OrderBy(u => u.Name).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.page = pageNumber;

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult DeleteQuestion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = dbContext.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }
        [HttpPost, ActionName("DeleteQuestion")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQuestionConfirmed(int id)
        {
            Question question = dbContext.Questions.Find(id);
            dbContext.Questions.Remove(question);
            List<Answer> answers = dbContext.Answers.Where(u => u.IdQuestion == id).ToList();
            foreach(Answer item in answers)
                dbContext.Answers.Remove(item);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult QuestionQualification(int id)
        {
            Question question = dbContext.Questions.Find(id);
            if (question == null)
                return RedirectToAction("EditQuestionSelect");
            ViewBag.Question = question.QuestionText;
            ViewBag.Del = true;
            if (question.IdQualification != 0)
                ViewBag.Qualification = dbContext.Qualifications.Find(question.IdQualification).Name;
            else
            {
                ViewBag.Del = false;
                var selectList = dbContext.Qualifications
                        .Select(u => new SelectListItem()
                        {
                            Text = u.Name,
                            Value = u.Id.ToString(),
                        }).ToList();
                ViewBag.QualificationsList = selectList;
            }
            return View();
        }
        [HttpPost]
        public ActionResult QuestionQualification(int id, int? QualificationsList)
        {
            var question = dbContext.Questions.Find(id);
            if (question == null)
                return RedirectToAction("EditQuestionSelect");
            if (QualificationsList == null)
            {
                question.IdQualification = 0;
                dbContext.SaveChanges();
                return RedirectToAction("EditQuestionSelect", new { message = "Вопрос был успешно убран из квалификации" });
            }
            question.IdQualification = QualificationsList.Value;
            dbContext.SaveChanges();
            return RedirectToAction("EditQuestionSelect", new { message = "Вопрос был успешно добавлен в квалификацию" });
        }
        public ActionResult CreateExam()
        {
            string userId = User.Identity.GetUserId();
            var tests = dbContext.TeacherTests
                .Where(u => u.TeacherId == userId)
                .Where(u => !string.IsNullOrEmpty(u.NameTest))
                .Where(u => !string.IsNullOrEmpty(u.Questions))
                .Select(u => new SelectListItem()
                {
                    Text = u.NameTest,
                    Value = u.Id.ToString(),
                }).ToList();

            var finishTests = dbContext.TeacherFinishTests
                .Where(u => u.TeacherId == userId)
                .Where(u => !string.IsNullOrEmpty(u.NameTest))
                .Where(u => !string.IsNullOrEmpty(u.Questions))
                .Select(u => new SelectListItem()
                {
                    Text = u.NameTest,
                    Value = u.Id.ToString(),
                }).ToList();
            if (finishTests == null || finishTests.Count == 0)
            {
                if ((tests == null || tests.Count == 0))
                    return RedirectToAction("Index", new { message = "Нет доступных тестов для создания экзамена" });
            }
            else
            {
                if ((tests == null || tests.Count == 0))
                    tests = finishTests;
                else
                    foreach (var item in finishTests)
                        tests.Add(item);
            }

            ViewBag.Test = tests;
            ViewBag.Group = dbContext.Users
                .Where(u => u.Course != 100)
                .Select(u => new SelectListItem()
                {
                    //Text = u.Course + u.Group,
                    //Value = u.Course + u.Group,
                    Text = u.Group,
                    Value = u.Group,

                })
                .Distinct().ToList();
            return View(new ExaminationViewModel() { Date = DateTime.Now });
        }
        [HttpPost]
        public ActionResult CreateExam(ExaminationViewModel model, string Group, int Test, int Time)
        {
            string userId = User.Identity.GetUserId();

            if (!ModelState.IsValid)
            {
                ViewBag.Test = dbContext.TeacherTests
                    .Where(u => u.TeacherId == userId)
                    .Select(u => new SelectListItem()
                    {
                        Text = u.NameTest,
                        Value = u.Id.ToString(),
                    }).ToList();

                ViewBag.Group = dbContext.Users
                    .Where(u => u.Course != 100)
                    .Select(u => new SelectListItem()
                    {
                        //Text = u.Course + u.Group,
                        //Value = u.Course + u.Group,
                        Text = u.Group,
                        Value = u.Group,
                    })
                    .Distinct().ToList();

                return View(model);
            }
            Examination exam = new Examination();
            exam.Name = model.Name;
            exam.Annotations = model.Annotations;
            exam.Classroom = model.Classroom;
            exam.Group = Group;
            exam.IdTest = Test;
            exam.TeacherId = userId;
            exam.Date = model.Date;
            exam.Time = Time;
            dbContext.Examinations.Add(exam);
            dbContext.SaveChanges();
            var allUsers = dbContext.Users.
                                //Where(u => u.Course + u.Group == Group).
                Where(u => u.Group == Group).
                Select(u => u.Id).ToList();
            foreach (var item in allUsers)
            {
                var user = dbContext.Users.Find(item);
                user.Update = true;
            }
            TestQualificationAccess testQualificationAccess = new TestQualificationAccess();
            testQualificationAccess.IdExamination = exam.Id;
            dbContext.TestQualificationAccess.Add(testQualificationAccess);
            dbContext.SaveChanges();
            return RedirectToAction("EditExams", new { id = exam.Id, message = "Экзамен успешно создан" });
        }
        public ActionResult EditExams(int? id, string message)
        {
            if (id == null)
                return RedirectToAction("Index");

            string userId = User.Identity.GetUserId();
            ExaminationViewModel model = new ExaminationViewModel();
            Examination exam = dbContext.Examinations.Find(id);
            model.Annotations = exam.Annotations;
            model.Classroom = exam.Classroom;
            model.Date = exam.Date;
            model.Name = exam.Name;
            model.Time = exam.Time;

            ViewBag.Test = dbContext.TeacherTests
                .Where(u => u.TeacherId == userId)
                .Select(u => new SelectListItem()
                {
                    Text = u.NameTest,
                    Value = u.Id.ToString(),
                    Selected = u.Id == exam.IdTest
                }).ToList();

            ViewBag.Group = dbContext.Users
                .Select(u => new SelectListItem()
                {
                    //Text = u.Course + u.Group,
                    //Value = u.Course + u.Group,
                    //Selected = u.Course + u.Group == exam.Group
                    Text = u.Group,
                    Value = u.Group,
                    Selected = u.Group == exam.Group
                })
                .Distinct().ToList();
            ViewBag.Id = exam.Id;
            ViewBag.Message = message;
            return View(model);
        }
        [HttpPost]
        public ActionResult EditExams(ExaminationViewModel model, string Group, int Test, int id)
        {
            string userId = User.Identity.GetUserId();

            if (!ModelState.IsValid)
            {
                ViewBag.Test = dbContext.TeacherTests
                    .Where(u => u.TeacherId == userId)
                    .Select(u => new SelectListItem()
                    {
                        Text = u.NameTest,
                        Value = u.Id.ToString(),
                    }).ToList();

                ViewBag.Group = dbContext.Users
                    .Select(u => new SelectListItem()
                    {
                        //Text = u.Course + u.Group,
                        //Value = u.Course + u.Group,
                        Text = u.Group,
                        Value = u.Group,
                    })
                    .Distinct().ToList();

                return View(model);
            }
            Examination exam = dbContext.Examinations.Find(id);
            exam.Name = model.Name;
            exam.Annotations = model.Annotations;
            exam.Classroom = model.Classroom;
            exam.Group = Group;
            exam.IdTest = Test;
            exam.TeacherId = userId;
            exam.Date = model.Date;
            exam.Time = model.Time;

            dbContext.SaveChanges();
            var allUsers = dbContext.Users.
                                //Where(u => u.Course + u.Group == Group).
                Where(u => u.Group == Group).
                Select(u => u.Id).ToList();
            foreach (var item in allUsers)
            {
                var user = dbContext.Users.Find(item);
                user.Update = true;
                dbContext.SaveChanges();
            }
            return RedirectToAction("EditExams", new { message = "Изменения сохранены" });
        }
        public ActionResult DeleteExams(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Examination exam = dbContext.Examinations.Find(id);
            if (exam == null)
            {
                return HttpNotFound();
            }
            return View(exam);
        }
        [HttpPost, ActionName("DeleteExams")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteExamsConfirmed(int id)
        {
            Examination exam = dbContext.Examinations.Find(id);
            TestQualificationAccess testQualificationAccess = dbContext.TestQualificationAccess.Where(u => u.IdExamination == id).SingleOrDefault();
            dbContext.TestQualificationAccess.Remove(testQualificationAccess);
            dbContext.Examinations.Remove(exam);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DetailsExams(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Examination exams = dbContext.Examinations.Find(id);
            if (exams == null)
                return HttpNotFound();

            ViewBag.NameTest = dbContext.TeacherTests.Find(exams.IdTest).NameTest;

            return View(exams);
        }

        public ActionResult ExamAccess(int? id)
        {
            Session.Clear();
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Session["IdTest"] = id;
            return View();
        }
        public PartialViewResult ExamAccessAjax(string currentFilter, string searchString, int? page, string submitButton)
        {
            if (Session["IdTest"] == null)
                return PartialView();
            int id = (int)Session["IdTest"];

            if (!string.IsNullOrEmpty(submitButton))
            {
                var exam = dbContext.TestQualificationAccess.Where(u => u.IdExamination == id).SingleOrDefault();
                if (!string.IsNullOrEmpty(exam.IdUsers) && exam.IdUsers.Contains(submitButton))
                {
                    List<string> temp = exam.IdUsers.Split(',').ToList();
                    temp.Remove(submitButton);
                    string tempIdUsers = "";
                    foreach (string item in temp)
                        tempIdUsers += "," + item;
                    if (temp.Count() != 0)
                        tempIdUsers = tempIdUsers.Remove(0, 1);
                    exam.IdUsers = tempIdUsers;
                }
                else
                {
                    if (string.IsNullOrEmpty(exam.IdUsers))
                        exam.IdUsers = submitButton;
                    else
                        exam.IdUsers += "," + submitButton;
                }
                dbContext.SaveChanges();
            }

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            List<UsersForAdmin> model = new List<UsersForAdmin>();
            string examGroup = dbContext.Examinations.Find(id).Group;
            //var users = dbContext.Users.Where(u => u.Course + u.Group == examGroup);
            var users = dbContext.Users.Where(u => u.Group == examGroup);

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
                model.Add(temp);
            }
            var tempAccess = dbContext.TestQualificationAccess.
                Where(u => u.IdExamination == id).
                Select(u => u.IdUsers).SingleOrDefault();
            List<string> usersAccess = new List<string>();
            if (tempAccess != null)
                usersAccess = tempAccess.Split(',').ToList();

            foreach (var item in model)
            {
                if (usersAccess.Contains(item.Id))
                    item.Qualification = true;
                else
                    item.Qualification = false;
            }

            model = model.OrderBy(u => u.Name).ToList();

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (model.ToPagedList(pageNumber, pageSize).PageCount < page)
                pageNumber = model.ToPagedList(pageNumber, pageSize).PageCount;
            if (pageNumber == 0)
                pageNumber = 1;
            ViewBag.Page = pageNumber;

            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Error()
        {
            return View();
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