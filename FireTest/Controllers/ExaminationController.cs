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
                Where(u => !u.FinishTest).
                Where(u => u.Group == user.Group).
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
                        start += "<br /><a href=\"../Examination/Index/" + item.Id + "\">Приступить к экзамену: \"" + item.Name + "\"</a><br />";
                }
            }
            exams = dbContext.Examinations.
                    Where(u => u.Date == DateTime.Today).
                    Where(u => u.FinishTest).
                    Where(u => u.Group == user.Group).
                    Select(u => new {
                        Id = u.Id,
                        Name = u.Name,
                    }).ToList();
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
                    {
                        if (start.Length != 0)
                            start += "<hr />";
                        start += "<br /><a href=\"../Examination/Index/" + item.Id + "\">Приступить к итоговому тестированию: \"" + item.Name + "\"</a><br />";
                    }
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
            ViewBag.IDEXAM = id.Value;
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
                    Time = u.Time,
                    Finish = u.FinishTest
                }).SingleOrDefault();
            ViewBag.ExaminationName = exam.Name;
            int count = 0;
            int number = 0;
            if (end == null)//Если еще не начали экзамен, то создаем новый
            {
                TestQualification newExam = new TestQualification()
                {
                    IdUser = userId,
                    IdExamination = id.Value,
                    TimeStart = DateTime.Now,
                    TimeEnd = DateTime.Now
                };
                string examQuestions = "";
                if (!exam.Finish)
                    examQuestions = dbContext.TeacherTests
                        .Where(u => u.Id == exam.Id)
                        .Select(u => u.Questions).SingleOrDefault();
                else
                    examQuestions = dbContext.TeacherFinishTests
                        .Where(u => u.Id == exam.Id)
                        .Select(u => u.Questions).SingleOrDefault();

                //Перемешаем вопросы
                var questionsShake = examQuestions.Split('|').OrderBy(u => random.Next());
                examQuestions = "";
                foreach (var item in questionsShake)
                    examQuestions += item + "|";
                examQuestions = examQuestions.Substring(0, examQuestions.Length - 1);
                newExam.Questions = examQuestions;
                dbContext.TestQualification.Add(newExam);
                dbContext.SaveChanges();

                Questions model = new Questions();
                model = SelectQuestion(newExam.Id);
                if (model == null)
                    return RedirectToAction("End", new { IDEXAM = id.Value });

                ViewBag.Count = examQuestions.Split('|').ToList().Count(); //Общее количество вопросов
                ViewBag.Number = 1;

                var tempTime = newExam.TimeStart.AddMinutes(exam.Time) - DateTime.Now;
                ViewBag.TimeMin = tempTime.TotalMinutes;
                ViewBag.TimeSec = tempTime.Seconds;

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
                        return RedirectToAction("End", new { IDEXAM = id.Value });
                    }
                }

                count = end.Questions.Split('|').ToList().Count(); //Общее количество вопросов
                if (!string.IsNullOrEmpty(end.Answers))
                    number = end.Answers.Split('|').ToList().Count(); //Общее количество ответов

                if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
                    return RedirectToAction("End", new { IDEXAM = id.Value });

                Questions model = new Questions();
                //if (exam.Finish)
                //    model = SelectQuestion(end.Id, true);
                //else
                //    model = SelectQuestion(end.Id, false);
                model = SelectQuestion(end.Id);
                if (model == null)
                    return RedirectToAction("End", new { IDEXAM = id.Value });

                ViewBag.Count = count;
                ViewBag.Number = number + 1;

                var tempTime = end.TimeStart.AddMinutes(exam.Time) - DateTime.Now;
                ViewBag.TimeMin = tempTime.TotalMinutes;
                ViewBag.TimeSec = tempTime.Seconds;

                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult Questions(List<string> AnswersIDs, int? IDEXAM)
        {
            ViewBag.IDEXAM = IDEXAM.Value;
            string user = User.Identity.GetUserId();
            if (dbContext.Users.Find(user).LastActivity < DateTime.Now.AddSeconds(-30)) //Проверка на оффлайн для ботов
                return PartialView();
            var test = dbContext.TestQualification //Берем id теста
                .Where(u => u.IdUser == user)
                .Where(u => string.IsNullOrEmpty(u.Questions) != true)
                .Where(u => u.End == false)
                .Where(u => u.IdExamination == IDEXAM)
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
                    Time = u.Time,
                }).SingleOrDefault();
            var tempTime = test.start.AddMinutes(exam.Time) - DateTime.Now;
            ViewBag.TimeMin = tempTime.Minutes;
            ViewBag.TimeSec = tempTime.Seconds;
            if (test.start.AddMinutes(exam.Time) <= DateTime.Now) //Проверяем если превысили время 
            {
                var elapsed = dbContext.TestQualification.Find(test.id);
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
                    ViewBag.ExaminationEnd = true;
                    dbContext.SaveChanges();
                    return PartialView();
                }
            }

            List<int> AnswersIDsINT = new List<int>();
            if (AnswersIDs != null)
            {
                foreach (var item in AnswersIDs)
                    AnswersIDsINT.Add(CodeDecode(item));
            }
            else
                AnswersIDsINT.Add(0);

            if (!SaveAnswer(test.id, AnswersIDsINT))
                return PartialView();

            int count = test.questions.Split('|').ToList().Count(); //Общее количество вопросов
            int number = 0;
            if (!string.IsNullOrEmpty(test.answers))
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
                if (model == null)
                {
                    ViewBag.QualificationTestEnd = true;
                    return PartialView();
                }
                return PartialView(model);
            }
        }
        public ActionResult End(int? IDEXAM)
        {
            string user = User.Identity.GetUserId();
            var test = dbContext.TestQualification
                .Where(u => u.IdUser == user)
                .Where(u => u.End == false)
                .Where(u => u.IdExamination == IDEXAM)
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
                    Time = u.Time,
                    FinishTest = u.FinishTest
                }).SingleOrDefault();

            TestQualification testEnd = dbContext.TestQualification.Find(test.id); //Заканчиваем тест
            testEnd.End = true;
            testEnd.TimeEnd = DateTime.Now;
            ViewBag.ExaminationName = exam.Name;

            if (test.start.AddMinutes(exam.Time) <= testEnd.TimeEnd)
                ViewBag.Time = exam.Time;
            else
                ViewBag.Time = (int)(testEnd.TimeEnd - test.start).TotalMinutes;

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
                    TestWrongAnswers TestWrongAnswers = new TestWrongAnswers()
                    {
                        Subject = dbContext.Subjects.Find(item).Name,
                        Count = idSubjectsCount[countId]
                    };
                    AllTestWrongAnswers.Add(TestWrongAnswers);
                    countId++;
                }
            }
            int Eval5, Eval4, Eval3, IdQua;
            if (exam.FinishTest)
            {
                var tempEval = dbContext.TeacherFinishTests.Find(exam.idTest);
                Eval5 = tempEval.Eval5;
                Eval4 = tempEval.Eval4;
                Eval3 = tempEval.Eval3;
                IdQua = tempEval.IdQualification;
            }
            else
            {
                var tempEval = dbContext.TeacherTests.Find(exam.idTest);
                Eval5 = tempEval.Eval5;
                Eval4 = tempEval.Eval4;
                Eval3 = tempEval.Eval3;
                IdQua = 0;
            }
            var rightP = right.Count() * 100 / (right.Count() + wrong.Count());
            testEnd.Score = rightP;
            int Eval = 0;
            if (rightP >= Eval5)
                Eval = 5;
            if (rightP < Eval5)
                Eval = 4;
            if (rightP < Eval4)
                Eval = 3;
            if (rightP < Eval3)
                Eval = 2;
            ViewBag.Eval = "Оценка " + Eval;

            ViewBag.RightP = rightP;
            ViewBag.Right = right.Count();
            ViewBag.Wrong = wrong.Count();
            ViewBag.Count = right.Count() + wrong.Count();

            ApplicationUser userBusy = dbContext.Users.Find(user); //Закончили тест - делаем юзера свободным
            userBusy.Busy = false;
            userBusy.Rating += (right.Count() * 100 / (right.Count() + wrong.Count())) / 2.0 + right.Count() / 2.0;
            if (IdQua != 0 && Eval != 2)
            {
                string quapoint = userBusy.QualificationPoint;
                //От идКвалификации зависит на какой позиции менять оценку. Маска _|_|_|_|_
                //                                                                0 2 4 6 8                
                var index = 2 * IdQua - 2; // 1->0; 2->2; 3->4; 4->6; 5->8
                quapoint = quapoint.Remove(index, 1).Insert(index, Eval.ToString());
                userBusy.QualificationPoint = quapoint;
            }
            dbContext.SaveChanges();
            ViewBag.Avatar = "/Images/Avatars/" + userBusy.Avatar;
            ViewBag.Name = userBusy.Family + " " + userBusy.Name + " " + userBusy.SubName;
            return View(AllTestWrongAnswers);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Issue(string QID, string Issue)
        {
            int que = CodeDecode(QID);
            string Result = "Произошел сбой. Сообщение не доставлено";
            if (que != 0)
            {
                if (string.IsNullOrEmpty(Issue.Trim()))
                    Result = "Вы должны описать проблему";
                else
                {
                    var result = dbContext.Questions.Find(que);
                    if (result != null)
                    {
                        dbContext.Issues.Add(new Issue
                        {
                            QuestionId = que,
                            SubjectId = result.IdSubject,
                            Message = Issue,
                            UserId = User.Identity.GetUserId()
                        });
                        dbContext.SaveChanges();
                        Result = "Сообщение доставлено";
                    }
                }
            }
            return Content(Result);
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
            Question questionDB;
            int CurrentQuestion = CountAnswers(id);
            do
            {
                questionDB = dbContext.Questions.Find(CurrentQuestion);
                if (questionDB == null) //Если вопрос удален, делаем ответ верным и идем дальше
                {
                    TestQualification test = dbContext.TestQualification.Find(id);
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
            ViewBag.QID = CodeDecode(CurrentQuestion);

            //Делаем запрос без прибавления 1, т.к. списки начинаются с 0, а не с 1.

            var allanswers = dbContext.Answers.Find(CurrentQuestion);
            question.QuestionText = TranslitText(questionDB.QuestionText);


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
                            Answers.Add(new Answers { AnswerText = TranslitText(answ.text), AnswerId = CodeDecode(answ.id) });
                foreach (var idAnsw in answNoDragUser)
                    foreach (var answ in allAnswers)
                        if (Convert.ToInt32(idAnsw) == answ.id)
                            Answers.Add(new Answers { AnswerText = TranslitText(answ.text), AnswerId = CodeDecode(answ.id) });
            }
            else
            {
                for (int i = 0; i < allAnswers.Count(); i++)
                {
                    Answers.Add(new Answers { AnswerText = TranslitText(allAnswers[i].text), AnswerId = CodeDecode(allAnswers[i].id) });
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
            if (!string.IsNullOrEmpty(test.Answers))
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
        static int CodeDecode(string value)
        {
            try
            {
                int randomInt = Convert.ToInt32(value.Substring(0, 2));
                switch (randomInt % 4)
                {
                    case 0:
                        {
                            string otherString = value.Substring(7, value.Length - 7);
                            var intValue10 = Convert.ToInt32(otherString, 16);
                            return intValue10 - randomInt;
                        }
                    case 1:
                        {
                            string otherString = value.Substring(2, value.Length - 5 - 2);
                            const string chars = "ABDHIJKLMN";
                            var stringValue = "";
                            for (int i = 0; i < otherString.Length; i++)
                                stringValue += chars.IndexOf(otherString[i]);
                            return Convert.ToInt32(stringValue) - randomInt;
                        }
                    case 2:
                        {
                            string otherString = value.Substring(7, value.Length - 7);
                            const string chars = "SDFGLHWXZJ";
                            var stringValue = "";
                            for (int i = 0; i < otherString.Length; i++)
                                stringValue += chars.IndexOf(otherString[i]);
                            return Convert.ToInt32(stringValue) - randomInt;
                        }
                    default:
                        {
                            string otherString = value.Substring(2, value.Length - 2);
                            const string Vowels = "AEIOU";
                            var stringValue = "";
                            for (int i = 0; i < otherString.Length; i++)
                            {
                                if (Vowels.Contains(otherString[i]))
                                    stringValue += 0;
                                else
                                    stringValue += 1;
                            }
                            var intValue10 = Convert.ToInt32(stringValue, 2);
                            return intValue10;
                        }
                }

            }
            catch (Exception)
            {
                return 0;
            }
        }
        static string CodeDecode(int intValue)
        {
            string value = intValue.ToString();
            string code = "";
            int randomInt = random.Next(10, 99);
            switch (randomInt % 4)
            {
                case 0:
                    {
                        var intValue16 = Convert.ToString(intValue + randomInt, 16);
                        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        string randomString = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
                        code = randomInt + randomString + intValue16;
                        break;
                    }
                case 1:
                    {
                        const string chars = "ABDHIJKLMN";
                        var stringValue = "";
                        value = (Convert.ToInt32(value) + randomInt).ToString();
                        for (int i = 0; i < value.Length; i++)
                            stringValue += chars[Convert.ToInt32(value[i].ToString())];
                        code = randomInt + stringValue + random.Next(10000, 99999);
                        break;
                    }
                case 2:
                    {
                        const string chars = "SDFGLHWXZJ";
                        var stringValue = "";
                        value = (Convert.ToInt32(value) + randomInt).ToString();
                        for (int i = 0; i < value.Length; i++)
                            stringValue += chars[Convert.ToInt32(value[i].ToString())];
                        code = randomInt + random.Next(10000, 99999).ToString() + stringValue;
                        break;
                    }
                default:
                    {
                        var intValue2 = Convert.ToString(intValue, 2);
                        const string Vowels = "AEIOU";
                        const string Consonants = "BCDFGHJKLMNPQRSTVWXYZ";
                        var stringValue = "";
                        for (int i = 0; i < intValue2.Length; i++)
                        {
                            if (intValue2[i] == '0')
                                stringValue += Vowels[random.Next(Vowels.Length)];
                            else
                                stringValue += Consonants[random.Next(Consonants.Length)];
                        }
                        code = randomInt + stringValue;
                        break;
                    }
            }
            return code.ToUpper();
        }
        private string TranslitText(string source)
        {
            Dictionary<string, string> dictionaryChar = new Dictionary<string, string>()
            {
                {"а","a"},
                {"е","e"},
                {"о","o"},
                {"р","p"},
                {"с","c"},
                {"х","x"},
                {"А","A"},
                {"С","C"},
                {"Р","P"},
                {"О","O"},
                {"Е","E"},
                {"М","M"},
                {"В","B"},
                {"Т","T"},
                {"Н","H"},
                {"Х","X"}
            };
            var result = "";
            foreach (var ch in source)
            {
                if (dictionaryChar.TryGetValue(ch.ToString(), out string ss))
                {
                    if (random.NextDouble() > 0.5)
                        result += ss;
                    else
                        result += ch;
                }
                else result += ch;
            }
            return result;
        }
        #endregion
    }
}