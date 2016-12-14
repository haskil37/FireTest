using FireTest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize]
    public class ExaminationController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        private static Random random = new Random();
        public PartialViewResult Access()
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser user = dbContext.Users.Find(userId);
            var exams = dbContext.Examinations.
                Where(u => u.Date == DateTime.Today).
                Where(u => u.Group == user.Course + user.Group).
                Select(u => new {
                    Id = u.Id,
                    Name = u.Name,
                }).ToList();
            string start = "";
            foreach (var item in exams)
            {
                var temp = dbContext.TestQualificationAccess.
                    Where(u => u.IdExamination == item.Id).
                    Select(u => u.IdUsers).SingleOrDefault();
                var test = dbContext.TestQualification.
                     Where(u => u.IdExamination == item.Id).
                     Where(u => u.IdUser == userId).
                     Where(u => u.End == true).SingleOrDefault();

                if (temp != null && test == null)
                {
                    List<string> usersAccess = temp.Split(',').ToList();
                    if (usersAccess.Contains(userId))
                        start += "<a href=\"../Examination/Index/" + item.Id + "\">Приступить к экзамену: \"" + item.Name + "\"</a><br />";
                }
            }
            ViewBag.Start = start;
            return PartialView();
        }
        public ActionResult Index(int? id)
        {
            if (id == null)
                return RedirectToAction("Index", "Home");

            string userId = User.Identity.GetUserId();
            ApplicationUser userBusy = dbContext.Users.Find(userId); //Делаем юзера занятым
            userBusy.Busy = true;
            dbContext.SaveChanges();

            var end = dbContext.TestQualification //Есть ли незаконченный или пустой экзамен
                .Where(u => u.IdUser == userId)
                .Where(u => u.End == false)
                .Where(u => u.IdExamination == id.Value).SingleOrDefault();

            var exam = dbContext.Examinations
                .Where(u => u.Id == id.Value)
                .Select(u => new
                {
                    Id = u.IdTest,
                    Name = u.Name,
                    Time = u.Time
                }).SingleOrDefault();
            ViewBag.ExaminationName = exam.Name;
            int count = 0;
            int number = 0;
            if (end == null)//Если еще не начали экзамен, то создаем новый
            {
                TestQualification newExam = new TestQualification();
                newExam.IdUser = userId;
                newExam.IdExamination = id.Value;
                newExam.TimeStart = DateTime.Now;
                string examQuestions = dbContext.TeacherTests
                    .Where(u => u.Id == exam.Id)
                    .Select(u => u.Questions).SingleOrDefault();
                newExam.Questions = examQuestions;
                dbContext.TestQualification.Add(newExam);
                dbContext.SaveChanges();

                Questions model = new Questions();
                model = SelectQuestion(newExam.Id);

                ViewBag.Count = examQuestions.Split('|').ToList().Count(); //Общее количество вопросов
                ViewBag.Number = 1;
                return View(model);
            }
            else
            {
                if (end.TimeStart.AddMinutes(exam.Time) <= DateTime.Now) //Проверяем если превысили время 
                {
                    var elapsed = dbContext.TestQualification.Find(end.Id);
                    int questionsCount = elapsed.Questions.Split('|').ToList().Count();
                    int answersCount = 0;
                    if (!string.IsNullOrEmpty(elapsed.Answers))
                        answersCount = elapsed.Answers.Split('|').ToList().Count();
                    if (questionsCount - answersCount != 0) //Если ответов меньше чем вопросов
                    {
                        string answers;
                        if (answersCount == 0)
                            answers = "0";
                        else
                            answers = "|0";
                        for (int i = 1; i < questionsCount - answersCount; i++)
                            answers = answers + "|0";
                        elapsed.Answers += answers;
                        elapsed.RightOrWrong += answers;
                        dbContext.SaveChanges();
                        return RedirectToAction("End");
                    }
                }

                count = end.Questions.Split('|').ToList().Count(); //Общее количество вопросов
                if (!string.IsNullOrEmpty(end.Answers))
                    number = end.Answers.Split('|').ToList().Count(); //Общее количество ответов

                if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
                    return RedirectToAction("End");

                Questions model = new Questions();
                model = SelectQuestion(end.Id);

                ViewBag.Count = count;
                ViewBag.Number = number + 1;
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Questions(List<int> AnswersIDs)
        {
            string user = User.Identity.GetUserId();
            var test = dbContext.TestQualification //Берем id теста
                .Where(u => u.IdUser == user)
                .Where(u => string.IsNullOrEmpty(u.Questions) != true)
                .Where(u => u.End == false)
                .Select(u => new
                {
                    id = u.Id,
                    questions = u.Questions,
                    answers = u.Answers,
                    idExam = u.IdExamination,
                    start = u.TimeStart
                }).SingleOrDefault();
            var exam = dbContext.Examinations
                .Where(u => u.Id == test.idExam)
                .Select(u => new
                {
                    Name = u.Name,
                    Time = u.Time
                }).SingleOrDefault();
           
            if (test.start.AddMinutes(exam.Time) <= DateTime.Now) //Проверяем если превысили время 
            {
                var elapsed = dbContext.TestQualification.Find(test.id);
                int questionsCount = elapsed.Questions.Split('|').ToList().Count();
                int answersCount = 0;
                if (!string.IsNullOrEmpty(elapsed.Answers))
                    answersCount = elapsed.Answers.Split('|').ToList().Count();
                if (questionsCount - answersCount != 0) //Если ответов меньше чем вопросов
                {
                    string answers = "0";
                    for (int i = 1; i < questionsCount - answersCount; i++)
                        answers = answers + "|0";
                    elapsed.Answers += answers;
                    elapsed.RightOrWrong += answers;
                    ViewBag.ExaminationEnd = true;
                    dbContext.SaveChanges();
                    return PartialView();
                }
            }
            if (!SaveAnswer(test.id, AnswersIDs))
                return PartialView();

            int count = test.questions.Split('|').ToList().Count(); //Общее количество вопросов
            int number = 0;
            if (test.answers != null)
                number = test.answers.Split('|').ToList().Count() + 1; //Общее количество ответов +1, т.к. запрос был раньше чем добавлен в базу новый ответ.
            else
                number = 1; //Общее количество ответов 1, т.к. запрос был раньше чем добавлен в базу новый ответ.

            if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
            {
                ViewBag.ExaminationEnd = true;
                return PartialView();
            }
            else //Иначе продолжаем тестирование
            {
                ViewBag.Count = count;
                ViewBag.Number = number + 1;
                ViewBag.ExaminationName = exam.Name;
                ViewBag.ExaminationEnd = false;

                Questions model = new Questions();
                model = SelectQuestion(test.id);
                return PartialView(model);
            }
        }
        public ActionResult End()
        {
            string user = User.Identity.GetUserId();
            var test = dbContext.TestQualification
                .Where(u => u.IdUser == user)
                .Where(u => u.End == false)
                .Select(u => new {
                    id = u.Id,
                    questions = u.Questions,
                    answers = u.Answers,
                    idExam = u.IdExamination,
                    start = u.TimeStart
                }).SingleOrDefault();

            if (test == null) //Если нет теста, то на главную
                return RedirectToAction("Index", "Home");

            int questions = test.questions.Split('|').Count();
            int answers = test.answers.Split('|').Count();
            if (questions != answers) //Если тест еще не закончен то шлем куда надо
                return RedirectToAction("Index", new { id = test.idExam });

            var exam = dbContext.Examinations
                .Where(u => u.Id == test.idExam)
                .Select(u => new
                {
                    idTest = u.IdTest,
                    Name = u.Name,
                    Time = u.Time
                }).SingleOrDefault();
            if (test.start.AddMinutes(exam.Time) <= DateTime.Now)
                ViewBag.Time = exam.Time;
            else
                ViewBag.Time = (DateTime.Now - test.start).Minutes;

            TestQualification testEnd = dbContext.TestQualification.Find(test.id); //Заканчиваем тест
            testEnd.End = true;
            ViewBag.ExaminationName = exam.Name;

            List<string> right = new List<string>(); //Берем правильные и неправильные ответы и из каких они дисциплин
            List<string> wrong = new List<string>();
            List<string> idQuestions = testEnd.Questions.Split('|').ToList();
            int countId = 0;
            foreach (string item in testEnd.RightOrWrong.Split('|').ToList())
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
                    TestWrongAnswers TestWrongAnswers = new TestWrongAnswers();
                    TestWrongAnswers.Subject = dbContext.Subjects.Find(item).Name;
                    TestWrongAnswers.Count = idSubjectsCount[countId];

                    AllTestWrongAnswers.Add(TestWrongAnswers);
                    countId++;
                }
            }

            var eval = dbContext.TeacherTests.Find(exam.idTest);
            var rightP = right.Count() * 100 / (right.Count() + wrong.Count());
            testEnd.Score = rightP;
            if (rightP >= eval.Eval5)
                ViewBag.Eval = "Оценка 5";
            if (rightP < eval.Eval5)
                ViewBag.Eval = "Оценка 4";
            if (rightP < eval.Eval4)
                ViewBag.Eval = "Оценка 3";
            if (rightP < eval.Eval3)
                ViewBag.Eval = "Оценка 2";
            ViewBag.RightP = rightP;
            ViewBag.Right = right.Count();
            ViewBag.Wrong = wrong.Count();
            ViewBag.Count = right.Count() + wrong.Count();

            ApplicationUser userBusy = dbContext.Users.Find(User.Identity.GetUserId()); //Закончили тест - делаем юзера свободным
            userBusy.Busy = false;
            userBusy.Rating += (right.Count() * 100 / (right.Count() + wrong.Count())) / 2.0 + right.Count() / 2.0;
            dbContext.SaveChanges();
            ViewBag.Avatar = "/Images/Avatars/" + userBusy.Avatar;

            return View(AllTestWrongAnswers);
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
            int CurrentQuestion = CountAnswers(id);

            //Делаем запрос без прибавления 1, т.к. списки начинаются с 0, а не с 1.

            var questionDB = dbContext.Questions.Find(CurrentQuestion);
            var allanswers = dbContext.Answers.Find(CurrentQuestion);
            question.QuestionText = questionDB.QuestionText;

            if (questionDB.QuestionImage != "NULL")
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
            TestQualification test = dbContext.TestQualification.Find(id);
            List<string> questions = new List<string>();
            List<string> answers = new List<string>();

            questions = test.Questions.Split('|').ToList();
            if (test.Answers != null)
                answers = test.Answers.Split('|').ToList();

            return Convert.ToInt32(questions[answers.Count()]);
        }
        private bool SaveAnswer(int id, List<int> AnswersIDs)
        {
            TestQualification test = dbContext.TestQualification.Find(id);
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
                foreach (string item in tempAnswer)
                {
                    if (tempCorrect.Contains(item))
                    {
                        tempCount++;
                    }
                }
                if (tempCount == tempCorrect.Count())
                    answer = correct;
            }
            if (test.Answers != null && test.Answers.Count() > 0)
                test.Answers += "|" + answer;
            else
                test.Answers += answer;

            var user = dbContext.Users.Find(User.Identity.GetUserId()); //Сохранение правильных ответов и кол-ва ответов в таблицу юзера
            user.AnswersCount += 1;
            if (correct == answer)
            {
                user.CorrectAnswersCount += 1;  //Прибавляем +1 к тому что пользователь правильно ответил (для статистики)
                question.CountCorrect += 1;  //Прибавляем +1 к тому что на вопрос правильно ответили (для статистики)

                if (test.RightOrWrong != null)
                    test.RightOrWrong += "|1";
                else
                    test.RightOrWrong = "1";
            }
            else
            {
                if (test.RightOrWrong != null)
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