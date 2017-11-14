﻿using FireTest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize]
    public class QualificationController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        private static Random random = new Random();
        private static int[,] relation = new int[5, 5]{
            {100, 0, 0, 0, 0},
            {40, 60, 0, 0, 0},
            {20, 20, 60, 0, 0},
            {10, 10, 20, 60, 0},
            {10, 10, 10, 20, 50}};

        public ActionResult Index(int id = 5, int course = 5)
        {
            if (!ModelState.IsValid || id < 1 || id > 5 || course < 1 || course > 5)
                return RedirectToAction("Index", "Home");

            if (id < course) //Чтоб курс был равен квалификации
                course = id;
            ViewBag.Course = "";
            for (int i = 1; i <= course;)
            {
                ViewBag.Course += i;
                i++;
                if (i > course)
                    break;
                //if (i < course)
                //    ViewBag.Course += ", ";
                //else
                //    ViewBag.Course += " и ";
                //Медленнее, но выглядит приятнее
                ViewBag.Course += i < course ? ", " : " и ";
            }

            ViewBag.QualificationName = dbContext.Qualifications.Find(id).Name;

            string user = User.Identity.GetUserId();

            var end = dbContext.SelfyTestQualifications //Есть ли незаконченный или пустой тест
                .Where(u => u.IdUser == user)
                .Where(u => u.End == false)
                .Select(u => new {
                    id = u.Id,
                    questions = u.Questions,
                    idQualification = u.IdQualification,
                }).FirstOrDefault();
            ViewBag.Сompleted = true;

            if (end != null)
            {
                if (!string.IsNullOrEmpty(end.questions)) //Если незаконченный тест, то предлагаем закончить
                {
                    ViewBag.QualificationName = dbContext.Qualifications.Find(end.idQualification).Name;
                    ViewBag.Id = end.id;
                    ViewBag.EndId = id;
                    ViewBag.EndCourse = course;
                    ViewBag.Сompleted = false;
                }
                else  //Если пустой тест, то меняем значения курса и квалификации
                {
                    SelfyTestQualification continueTest = dbContext.SelfyTestQualifications.Find(end.id);
                    continueTest.IdQualification = id;
                    continueTest.Course = course;
                    dbContext.SaveChanges();
                    ViewBag.Id = end.id;
                }
            }
            else //Если нет, то создаем пустой
            {
                SelfyTestQualification newTest = new SelfyTestQualification()
                {
                    IdUser = user,
                    IdQualification = id,
                    Course = course,
                    TimeStart = DateTime.Now,
                    TimeEnd = DateTime.Now
                };
                dbContext.SelfyTestQualifications.Add(newTest);
                dbContext.SaveChanges();
                ViewBag.Id = newTest.Id;
            }

            //От квалификации считаем ползунок
            int value = 0;
            for (int i = 1; i <= id; i++)
            {
                var countQuestions = dbContext.Questions.Where(u => u.IdQualification == i).Count();
                ViewBag.CountCurrent = countQuestions;
                if (value != 0)
                    value = Math.Min(value, countQuestions * 100 / relation[id - 1, i - 1]);
                else
                    value = countQuestions * 100 / relation[id - 1, i - 1];
            }
            ViewBag.CountMax = Math.Truncate(value / 10.0);
            ViewBag.TextQ = id;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int? EndId, int? EndCourse, int id = 0, string submitButton = "Exit", int count = 1)
        {
            if (!ModelState.IsValid || id == 0 || submitButton == "Exit")
                return RedirectToAction("Index", "Home");

            string user = User.Identity.GetUserId();

            var test = dbContext.SelfyTestQualifications //Проверяем правильность id
                            .Where(u => u.IdUser == user)
                            .Where(u => u.Id == id)
                            .Select(u => new {
                                questions = u.Questions,
                                idQualification = u.IdQualification,
                                course = u.Course,
                            }).FirstOrDefault();

            if (test == null) //Если id туфта, то выходим
                return RedirectToAction("Index", "Home");

            if (submitButton == "Cancel") //Удаляем пустой или незаконченный тесты при отмене
            {
                var deleteTest = dbContext.SelfyTestQualifications.Find(id);
                if (string.IsNullOrEmpty(deleteTest.Questions))
                    return RedirectToAction("Index", "Home");
                dbContext.SelfyTestQualifications.Remove(deleteTest);
                dbContext.SaveChanges();
                return RedirectToAction("Index", new { id = EndId, course = EndCourse });
            }

            if (submitButton != "Accept") //Т.к. Cancel мы обработали, то если будет любой текст кроме согласия, то это фигня и мы выкидываем пользователя на главную
                return RedirectToAction("Index", "Home");


            int value = 0;//Проверяем что count не больше чем можно
            for (int i = 1; i <= test.idQualification; i++)
            {
                var countQuestions2 = dbContext.Questions.Where(u => u.IdQualification == i).Count();
                if (value != 0)
                    value = Math.Min(value, countQuestions2 * 100 / relation[test.idQualification - 1, i - 1]);
                else
                    value = countQuestions2 * 100 / relation[test.idQualification - 1, i - 1];
            }
            if (count > Math.Truncate(value / 10.0))
                count = Convert.ToInt32(Math.Truncate(value / 10.0));

            if (string.IsNullOrEmpty(test.questions)) //Создаем новый тест
            {
                count = count * 10;
                if (!AutoSelectQuestion(id, test.idQualification, test.course, count))
                    return RedirectToAction("Index", "Home"); //Если что-то пошло не так, то на главную
            }
            else //Продолжаем старый тест
            {
                //Ничего не делаем :)
            }

            ApplicationUser userBusy = dbContext.Users.Find(User.Identity.GetUserId()); //Начали тест - делаем юзера занятым, чтобы нельзя было вызвать на бой
            userBusy.Busy = true;
            dbContext.SaveChanges();

            return RedirectToAction("QualificationTest", "Qualification");
        }
        public ActionResult QualificationTest()
        {
            string user = User.Identity.GetUserId();
            var qualificationTest = dbContext.SelfyTestQualifications
                .Where(u => u.IdUser == user)
                .Where(u => string.IsNullOrEmpty(u.Questions) != true)
                .Where(u => u.End == false)
                .Select(u => new {
                    id = u.Id,
                    idQualification = u.IdQualification,
                    questions = u.Questions,
                    answers = u.Answers
                }).FirstOrDefault();
            if (qualificationTest == null) //Если нет теста, то на главную
                return RedirectToAction("Index", "Home");

            int count = qualificationTest.questions.Split('|').ToList().Count(); //Общее количество вопросов
            int number = 0;
            if (!string.IsNullOrEmpty(qualificationTest.answers))
                number = qualificationTest.answers.Split('|').ToList().Count(); //Общее количество ответов

            if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
                return RedirectToAction("QualificationTestEnd");

            ViewBag.QualificationName = dbContext.Qualifications.Find(qualificationTest.idQualification).Name;
            Questions model = new Questions();
            model = SelectQuestion(qualificationTest.id);
            if (model == null)
                return RedirectToAction("QualificationTestEnd");

            ViewBag.Count = qualificationTest.questions.Split('|').ToList().Count();
            if (!string.IsNullOrEmpty(qualificationTest.answers))
                ViewBag.Number = qualificationTest.answers.Split('|').ToList().Count() + 1;
            else
            {
                SelfyTestQualification test = dbContext.SelfyTestQualifications.Find(qualificationTest.id); //Обновляем время начала теста, т.к. это первый запуск
                test.TimeStart = DateTime.Now;
                test.TimeEnd = DateTime.Now;
                dbContext.SaveChanges();
                ViewBag.Number = 1;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult QualificationTestQuestions(List<int> AnswersIDs)
        {
            string user = User.Identity.GetUserId();
            var qualificationTest = dbContext.SelfyTestQualifications //Берем id теста
                .Where(u => u.IdUser == user)
                .Where(u => string.IsNullOrEmpty(u.Questions) != true)
                .Where(u => u.End == false)
                .Select(u => new {
                    id = u.Id,
                    idQualification = u.IdQualification,
                    questions = u.Questions,
                    answers = u.Answers
                }).SingleOrDefault();

            if (!SaveAnswer(qualificationTest.id, AnswersIDs))
                return PartialView();

            int count = qualificationTest.questions.Split('|').ToList().Count(); //Общее количество вопросов
            int number = 0;
            if (!string.IsNullOrEmpty(qualificationTest.answers))
                number = qualificationTest.answers.Split('|').ToList().Count() + 1; //Общее количество ответов +1, т.к. запрос был раньше чем добавлен в базу новый ответ.
            else
                number = 1; //Общее количество ответов 1, т.к. запрос был раньше чем добавлен в базу новый ответ.

            if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
            {
                ViewBag.QualificationTestEnd = true;
                return PartialView();
            }
            else //Иначе продолжаем тестирование
            {
                ViewBag.Count = count;
                ViewBag.Number = number + 1;
                ViewBag.QualificationName = dbContext.Qualifications.Find(qualificationTest.idQualification).Name;
                ViewBag.QualificationTestEnd = false;

                Questions model = new Questions();
                model = SelectQuestion(qualificationTest.id);
                if (model == null)
                {
                    ViewBag.QualificationTestEnd = true;
                    return PartialView();
                }
                return PartialView(model);
            }
        }
        public ActionResult QualificationTestEnd()
        {
            string user = User.Identity.GetUserId();
            var qualificationTest = dbContext.SelfyTestQualifications
                .Where(u => u.IdUser == user)
                .Where(u => string.IsNullOrEmpty(u.Questions) != true)
                .Where(u => u.End == false)
                .Select(u => new {
                    id = u.Id,
                    idQualification = u.IdQualification,
                    questions = u.Questions,
                    answers = u.Answers
                }).SingleOrDefault();
            if (qualificationTest == null) //Если нет теста, то на главную
                return RedirectToAction("Index", "Home");

            int questions = qualificationTest.questions.Split('|').Count();
            int answers = qualificationTest.answers.Split('|').Count();
            if (questions != answers) //Если тест еще не закончен то шлем на тест
                return RedirectToAction("QualificationTest");

            SelfyTestQualification test = dbContext.SelfyTestQualifications.Find(qualificationTest.id); //Заканчиваем тест
            test.End = true;
            test.TimeEnd = DateTime.Now;
            ViewBag.Days = (test.TimeEnd - test.TimeStart).Days;
            ViewBag.Hours = (test.TimeEnd - test.TimeStart).Hours;
            ViewBag.Min = (test.TimeEnd - test.TimeStart).Minutes;
            ViewBag.Sec = (test.TimeEnd - test.TimeStart).Seconds;
            ViewBag.QualificationName = dbContext.Qualifications.Find(qualificationTest.idQualification).Name;
            dbContext.SaveChanges();

            List<string> right = new List<string>(); //Берем правильные и неправильные ответы и из каких они дисциплин
            List<string> wrong = new List<string>();
            List<string> idQuestions = test.Questions.Split('|').ToList();
            int countId = 0;
            foreach (string item in test.RightOrWrong.Split('|').ToList())
            {
                if (item == "0")
                    wrong.Add(idQuestions[countId]);
                else
                    right.Add(item);
                countId++;
            }
            List<TestWrongAnswers> AllTestWrongAnswers = new List<TestWrongAnswers>();
            if (wrong.Count() > 0)
            {
                List<int> wrongSubjects = new List<int>();
                List<int> idSubjects = new List<int>();
                List<int> idSubjectsCount = new List<int>();
                foreach (string item in wrong)
                {
                    int temp = Convert.ToInt32(item);
                    int wrongSubjectsId = dbContext.Questions.Find(temp).IdSubject;
                    wrongSubjects.Add(wrongSubjectsId);
                }

                //wrongSubjects.GroupBy(v => v).Where(g => g.Count() > 1).Select(g => g.Key);
                foreach (int val in wrongSubjects.Distinct())
                {
                    idSubjects.Add(val);
                    idSubjectsCount.Add(wrongSubjects.Where(x => x == val).Count());
                }
                countId = 0;
                foreach (int item in idSubjects)
                {
                    TestWrongAnswers TestWrongAnswers = new TestWrongAnswers()
                    {
                        Subject = dbContext.Subjects.Find(item).Name,
                        Count = idSubjectsCount[countId]
                    };
                    AllTestWrongAnswers.Add(TestWrongAnswers);
                    countId++;
                }
            }
            ViewBag.RightP = right.Count() * 100 / (right.Count() + wrong.Count());
            ViewBag.Right = right.Count();
            ViewBag.Wrong = wrong.Count();
            ViewBag.Count = right.Count() + wrong.Count();

            ApplicationUser userBusy = dbContext.Users.Find(User.Identity.GetUserId()); //Закончили тест - делаем юзера свободным
            userBusy.Busy = false;
            userBusy.Rating += (right.Count() * 100 / (right.Count() + wrong.Count())) / 2.0 + right.Count() / 2.0;
            dbContext.SaveChanges();
            ViewBag.Avatar = "/Images/Avatars/" + userBusy.Avatar;

            ViewBag.Details = qualificationTest.id;
            return View(AllTestWrongAnswers);
        }
        public ActionResult QualificationWrongDetails(int id)
        {
            string user = User.Identity.GetUserId();
            var qualificationTest = dbContext.SelfyTestQualifications
                .Where(u => u.IdUser == user)
                .Where(u => u.Id == id)
                .Where(u => u.End == true)
                .Select(u => new {
                    rightOrWrong = u.RightOrWrong,
                    questions = u.Questions,
                    answers = u.Answers
                }).SingleOrDefault();
            if (qualificationTest == null) //Если нет теста, то на главную
                return RedirectToAction("Index", "Home");
            int questions = qualificationTest.questions.Split('|').Count();
            int answers = qualificationTest.answers.Split('|').Count();
            
            //Берем неправильные ответы и из каких они дисциплин
            List<string> wrong = new List<string>();
            List<string> idQuestions = qualificationTest.questions.Split('|').ToList();
            int countId = 0;
            foreach (string item in qualificationTest.rightOrWrong.Split('|').ToList())
            {
                if (item == "0")
                    wrong.Add(idQuestions[countId]);
                countId++;
            }
            List<TestWrongAnswersDetails> AllTestWrongAnswersDetails = new List<TestWrongAnswersDetails>();
            if (wrong.Count() > 0)
            {
                foreach (string item in wrong)
                {
                    int temp = Convert.ToInt32(item);
                    var wrongDetails = new TestWrongAnswersDetails()
                    {
                        Question = dbContext.Questions.Find(temp).QuestionText
                    };
                    List<string> idCorrect = dbContext.Questions.Find(temp).IdCorrect.Split(',').ToList();
                    
                    if (idCorrect[0][0] == '~') //Если вопрос на последовательность
                    {
                        wrongDetails.TypeQuestion = "sequence";
                        var allAnswers = dbContext.Answers.Where(u => u.IdQuestion == temp).Select(u => new {
                            answerText = u.AnswerText,
                            answerId = u.Id
                        }).ToList();
                        idCorrect[0] = idCorrect[0].Remove(0, 1);

                        var correctAnswers = new List<string>();
                        for (int i = 0; i < idCorrect.Count(); i++)
                        {
                            //var y = allAnswers.Find(u => u.answerId.ToString() == idCorrect[i]).answerText;
                            var text = allAnswers.Where(u => u.answerId.ToString() == idCorrect[i]).Select(u => u.answerText).SingleOrDefault();
                            correctAnswers.Add(text);
                        }
                        wrongDetails.CorrectAnswers = correctAnswers;
                        AllTestWrongAnswersDetails.Add(wrongDetails);
                    }
                    else if (idCorrect[0][0] == '#') //Если вопрос на соответствие
                    {
                        wrongDetails.TypeQuestion = "conformity";
                        var allAnswers = dbContext.Answers.Where(u => u.IdQuestion == temp).Select(u => new {
                            answerText = u.AnswerText,
                            answerId = u.Id
                        }).ToList();
                        idCorrect[0] = idCorrect[0].Remove(0, 1);

                        var correctAnswers = new List<string>();
                        var wrongAnswers = new List<string>();
                        for (int i = 0; i < idCorrect.Count; i++)
                        {
                            var leftRight = idCorrect[i].Split('=');
                            correctAnswers.Add(allAnswers.Where(u => u.answerId.ToString() == leftRight[0]).Select(u => u.answerText).SingleOrDefault());
                            wrongAnswers.Add(allAnswers.Where(u => u.answerId.ToString() == leftRight[1]).Select(u => u.answerText).SingleOrDefault());
                        }
                        wrongDetails.WrongAnswers = wrongAnswers;
                        wrongDetails.CorrectAnswers = correctAnswers;
                        AllTestWrongAnswersDetails.Add(wrongDetails);
                    }
                    else
                    {
                        wrongDetails.TypeQuestion = "standart";
                        var allAnswers = dbContext.Answers.Where(u => u.IdQuestion == temp).Select(u => new {
                            answerText = u.AnswerText,
                            answerId = u.Id
                        }).ToList();

                        var correctAnswers = new List<string>();
                        var wrongAnswers = new List<string>();
                        foreach (var answersItem in allAnswers)
                        {
                            if (idCorrect.Contains(answersItem.answerId.ToString()))
                                correctAnswers.Add(answersItem.answerText);
                            else
                                wrongAnswers.Add(answersItem.answerText);
                        }
                        wrongDetails.WrongAnswers = wrongAnswers;
                        wrongDetails.CorrectAnswers = correctAnswers;
                        AllTestWrongAnswersDetails.Add(wrongDetails);
                    }
                }
            }
            return View(AllTestWrongAnswersDetails);
        }
        #region Вспомогательные приложения
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }
            base.Dispose(disposing);
        }
        private bool AutoSelectQuestion(int id, int idQualification, int course, int count)
        {
            try
            {
                List<int> idQuestions = new List<int>();
                string questions = ""; //Сохраняем вопросы в тест

                for (int i = 1; i <= idQualification; i++)
                {
                    var value = relation[idQualification - 1, i - 1] * count / 100;
                    idQuestions = dbContext.Questions.
                        Where(u => u.IdQualification == i).
                        OrderBy(u => Guid.NewGuid()).
                        Take(value).
                        Select(u => u.Id).ToList();
                    foreach (int item in idQuestions)
                        questions += item.ToString() + "|";
                }
                questions = questions.Substring(0, questions.Length - 1);
                var test = dbContext.SelfyTestQualifications.Find(id);
                test.Questions = questions;
                dbContext.SaveChanges();
            }
            catch
            {
                return false;
            }
            return true;
        }
        static void Shuffle<T>(List<T> array)
        {
            int n = array.Count();
            for (int i = 0; i < n; i++)
            {
                int r = i + (int)(random.NextDouble() * (n - i));
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }
        private Questions SelectQuestion(int id)
        {
            Questions question = new Questions();
            Question questionDB;
            int CurrentQuestion = CountAnswers(id);
            do
            {
                questionDB = dbContext.Questions.Find(CurrentQuestion);
                if (questionDB == null) //Если вопрос удален, делаем ответ верным и идем дальше
                {
                    SelfyTestQualification test = dbContext.SelfyTestQualifications.Find(id);
                    if (!string.IsNullOrEmpty(test.Answers))
                        test.Answers += "|0";
                    else
                        test.Answers = "0";

                    if (!string.IsNullOrEmpty(test.RightOrWrong))
                        test.RightOrWrong += "|1";
                    else
                        test.RightOrWrong = "1";
                    dbContext.SaveChanges();
                    int count = test.Questions.Split('|').ToList().Count(); //Общее количество вопросов
                    int number = test.Answers.Split('|').ToList().Count(); //Общее количество ответов

                    if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
                        return null;
                    CurrentQuestion = CountAnswers(id);
                }
            } while (questionDB == null);

            //Делаем запрос без прибавления 1, т.к. списки начинаются с 0, а не с 1.

            var allanswers = dbContext.Answers.Find(CurrentQuestion);
            question.QuestionText = questionDB.QuestionText;

            if (!string.IsNullOrEmpty(questionDB.QuestionImage) && questionDB.QuestionImage != "NULL")
                question.QuestionImage = "/Images/Questions/" + questionDB.QuestionImage;

            var allAnswers = dbContext.Answers
                  .Where(u => u.IdQuestion == CurrentQuestion)
                  .Select(u => new
                  {
                      id = u.Id,
                      text = u.AnswerText
                  }).ToList();
            List<Answers> Answers = new List<Answers>();

            ViewBag.TypeQuestion = "standart";
            if (questionDB.IdCorrect[0] != '~' && questionDB.IdCorrect[0] != '#' && questionDB.IdCorrect.Split(',').Count() > 1)
                ViewBag.Multiple = "Выберите несколько вариантов ответа";
            else
                ViewBag.Multiple = "Выберите один вариант ответа";
            if (questionDB.IdCorrect[0] == '~') //Если вопрос на последовательность
                ViewBag.TypeQuestion = "sequence";
            if (questionDB.IdCorrect[0] == '#') //Если вопрос на соответствие
            {
                ViewBag.TypeQuestion = "conformity";
                string correctAnswers = questionDB.IdCorrect;
                correctAnswers = correctAnswers.Remove(0, 1);

                List<string> answDragUser = new List<string>(); //Массив двигающихся ответов
                List<string> answNoDragUser = new List<string>(); //Массив других ответов
                List<string> coupleAnsw = correctAnswers.Split(',').ToList(); //Временная переменная
                foreach (var item in coupleAnsw)
                {
                    List<string> temp = item.Split('=').ToList();
                    answDragUser.Add(temp[0]);
                    answNoDragUser.Add(temp[1]);
                }
                Shuffle(answDragUser); //Перемешиваем массив двигающихся ответов
                Shuffle(answNoDragUser); //Перемешиваем массив других ответов
                //Соединяем ответы друг за другом и помещаем в переменную ответов
                foreach (var idAnsw in answDragUser)
                    foreach (var answ in allAnswers)
                        if (Convert.ToInt32(idAnsw) == answ.id)
                            Answers.Add(new Answers { AnswerText = answ.text, AnswerId = answ.id });
                foreach (var idAnsw in answNoDragUser)
                    foreach (var answ in allAnswers)
                        if (Convert.ToInt32(idAnsw) == answ.id)
                            Answers.Add(new Answers { AnswerText = answ.text, AnswerId = answ.id });
            }
            else
            {
                for (int i = 0; i < allAnswers.Count(); i++)
                {
                    Answers.Add(new Answers { AnswerText = allAnswers[i].text, AnswerId = allAnswers[i].id });
                    Shuffle(Answers);
                }
            }

            questionDB.CountAll += 1; //Прибавляем +1 к тому что вопрос был задан (для статистики)
            dbContext.SaveChanges();
            question.QuestionAnswers = Answers;

            return question;
        }
        private int CountAnswers(int id)
        {
            SelfyTestQualification test = dbContext.SelfyTestQualifications.Find(id);
            List<string> questions = new List<string>();
            List<string> answers = new List<string>();

            questions = test.Questions.Split('|').ToList();
            if (!string.IsNullOrEmpty(test.Answers))
                answers = test.Answers.Split('|').ToList();

            return Convert.ToInt32(questions[answers.Count()]);
        }
        private bool SaveAnswer(int id, List<int> AnswersIDs)
        {
            SelfyTestQualification test = dbContext.SelfyTestQualifications.Find(id);
            int CurrentQuestion = CountAnswers(id);
            Question question = dbContext.Questions.Find(CurrentQuestion);
            string correct = question.IdCorrect;
            if (AnswersIDs != null)
            {
                if (correct[0] != '~' && correct[0] != '#') //Если обычный вопрос то сортируем ответы
                    AnswersIDs.Sort();
            }
            else
                AnswersIDs = new List<int>() { 0 };

            string answer = "";
            if (correct[0] != '#')
            {
                if (correct[0] == '~') //Если последовательность - прибавляем ~
                    answer = "~";
                for (int i = 0; i < AnswersIDs.Count(); i++)
                {
                    if (i > 0)
                        answer += "," + AnswersIDs[i];
                    else
                        answer += AnswersIDs[i];
                }
            }
            else //Если соответствие - прибавляем # и проверяем по частям на правильность ответа. 
            {
                answer = "#";
                int halfCount = AnswersIDs.Count() / 2;
                for (int i = 0; i < halfCount; i++)
                {
                    if (i > 0)
                        answer += "," + AnswersIDs[i] + "=" + AnswersIDs[i + halfCount];
                    else
                        answer += AnswersIDs[i] + "=" + AnswersIDs[i + halfCount];
                }
                List<string> tempCorrect = new List<string>();
                List<string> tempAnswer = new List<string>();
                tempCorrect = correct.Remove(0, 1).Split(',').ToList();
                tempAnswer = answer.Remove(0, 1).Split(',').ToList();
                int tempCount = 0;
                foreach(string item in tempAnswer)
                {
                    if (tempCorrect.Contains(item))
                    {
                        tempCount++;
                    }
                }
                if (tempCount == tempCorrect.Count())
                    answer = correct;
            }
            if (!string.IsNullOrEmpty(test.Answers))
                test.Answers += "|" + answer;
            else
                test.Answers += answer;

            var user = dbContext.Users.Find(User.Identity.GetUserId()); //Сохранение правильных ответов и кол-ва ответов в таблицу юзера
            user.AnswersCount += 1;
            if (correct == answer)
            {
                user.CorrectAnswersCount += 1;  //Прибавляем +1 к тому что пользователь правильно ответил (для статистики)
                question.CountCorrect += 1;  //Прибавляем +1 к тому что на вопрос правильно ответили (для статистики)

                if (!string.IsNullOrEmpty(test.RightOrWrong))
                    test.RightOrWrong += "|1";
                else
                    test.RightOrWrong = "1";
            }
            else
            {
                if (!string.IsNullOrEmpty(test.RightOrWrong))
                    test.RightOrWrong += "|0";
                else
                    test.RightOrWrong = "0";
            }

            dbContext.SaveChanges();
            return true;
        }
        #endregion
    }
}