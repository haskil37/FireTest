using FireTest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize]
    public class BattlesController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        private static Random random = new Random();
        const int battleQuestions = 20;
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            var unfinishedBattle = dbContext.Battles //Если идет схватка пересылаем на нее (первый игрок)
                .Where(u => u.FirstPlayer == userId)
                .Where(u => string.IsNullOrEmpty(u.Winner) == true)
                .Where(u => u.AgreementOtherPlayer == true)
                .Select(u => new
                {
                    id = u.Id,
                    Row = u.RightOrWrongFirstPlayer,
                    lastActivity = u.TimeEndFirstPlayer
                }).SingleOrDefault();
            if (unfinishedBattle != null)
            {
                Battle battle = dbContext.Battles.Find(unfinishedBattle.id);
                double time = 0;
                if (unfinishedBattle.Row == null) //Если не дал ниодного ответа
                {
                    double timeElapsed = (DateTime.Now - unfinishedBattle.lastActivity).TotalSeconds;
                    int counAnswersLost = 0;
                    for (int i = 0; i < battleQuestions; i++)
                    {
                        time = time + 60 / Math.Pow(2, i);
                        if (time < timeElapsed)
                            counAnswersLost++;
                    }
                    if (counAnswersLost != 0)
                    {
                        string answers = "0";
                        for (int i = 1; i < counAnswersLost; i++)
                            answers = answers + "|0";
                        battle.RightOrWrongFirstPlayer = answers;
                        battle.AnswersFirstPlayer = answers;
                    }
                }
                else
                {
                    double timeElapsed = (DateTime.Now - unfinishedBattle.lastActivity).TotalSeconds;
                    int counAnswersLost = 0;
                    for (int i = 0; i < battleQuestions - unfinishedBattle.Row.Split('|').Count(); i++)
                    {
                        time = time + 60 / Math.Pow(2, i);
                        if (time < timeElapsed)
                            counAnswersLost++;
                    }
                    if (counAnswersLost != 0)
                    {
                        for (int i = 1; i < counAnswersLost; i++)
                        {
                            battle.RightOrWrongFirstPlayer += "|0";
                            battle.AnswersFirstPlayer += "|0";
                        }
                    }
                }
                ApplicationUser user = dbContext.Users.Find(userId); //Пересылаем и делаем его занятым.
                user.Busy = true;
                dbContext.SaveChanges();
                return RedirectToAction("Fight", new { id = unfinishedBattle.id });
            }
            unfinishedBattle = dbContext.Battles //Если идет схватка пересылаем на нее (второй игрок)
                .Where(u => u.SecondPlayer == userId)
                .Where(u => string.IsNullOrEmpty(u.Winner) == true)
                .Where(u => u.AgreementOtherPlayer == true)
                .Select(u => new
                {
                    id = u.Id,
                    Row = u.RightOrWrongSecondPlayer,
                    lastActivity = u.TimeEndSecondPlayer
                }).SingleOrDefault();
            if (unfinishedBattle != null)
            {
                Battle battle = dbContext.Battles.Find(unfinishedBattle.id);
                double time = 0;
                if (unfinishedBattle.Row == null) //Если не дал ниодного ответа
                {
                    double timeElapsed = (DateTime.Now - unfinishedBattle.lastActivity).TotalSeconds;
                    int counAnswersLost = 0;
                    for (int i = 0; i < battleQuestions; i++)
                    {
                        time = time + 60 / Math.Pow(2, i);
                        if (time < timeElapsed)
                            counAnswersLost++;
                    }
                    if (counAnswersLost != 0)
                    {
                        string answers = "0";
                        for (int i = 1; i < counAnswersLost; i++)
                            answers = answers + "|0";
                        battle.RightOrWrongSecondPlayer = answers;
                        battle.AnswersSecondPlayer = answers;
                    }
                }
                else
                {
                    double timeElapsed = (DateTime.Now - unfinishedBattle.lastActivity).TotalSeconds;
                    int counAnswersLost = 0;
                    for (int i = 0; i < battleQuestions - unfinishedBattle.Row.Split('|').Count(); i++)
                    {
                        time = time + 60 / Math.Pow(2, i);
                        if (time < timeElapsed)
                            counAnswersLost++;
                    }
                    if (counAnswersLost != 0)
                    {
                        for (int i = 1; i < counAnswersLost; i++)
                        {
                            battle.RightOrWrongSecondPlayer += "|0";
                            battle.AnswersSecondPlayer += "|0";
                        }
                    }
                }

                ApplicationUser user = dbContext.Users.Find(userId); //Пересылаем и делаем его занятым.
                user.Busy = true;
                dbContext.SaveChanges();
                return RedirectToAction("Fight", new { id = unfinishedBattle.id });
            }
            return View();
        }
        public PartialViewResult Users()
        {
            string user = User.Identity.GetUserId();
            DateTime activity = DateTime.Now.AddSeconds(-30); //30 секунд небыло активности - значит оффлайн
            var otherUsers = dbContext.Users
                 .Where(u => u.Id != user)
                 .Where(u => u.Busy != true)
                 .Where(u => u.LastActivity >= activity)
                 .Select(u => new {
                     Id = u.Id,
                     Avatar = u.Avatar,
                     Name = u.Name,
                     Family = u.Family,
                     Battles = u.BattleCount,
                     CorrectAnswers = u.CorrectAnswersCount,
                     AllAnswers = u.AnswersCount,
                     Rating = u.Rating,
                     Course = u.Course,
                     Group = u.Group
                 }).ToList();

            List<UsersData> model = new List<UsersData>();

            foreach (var item in otherUsers)
            {
                int numbertop = dbContext.Users
                       .Where(u => u.Rating > item.Rating)
                       .Select(u => u.Rating).OrderByDescending(u => u).Count();

                UsersData temp = new UsersData()
                {
                    Id = item.Id,
                    Avatar = "/Images/Avatars/" + item.Avatar,
                    Name = item.Name,
                    Family = item.Family,
                    Battles = item.Battles,
                    Rating = numbertop + 1,
                };

                if (item.Course != 100)
                    temp.Group = item.Course + item.Group;

                if (item.AllAnswers != 0)
                    temp.CorrectAnswers = 100 * item.CorrectAnswers / item.AllAnswers;
                else
                    temp.CorrectAnswers = 0;

                model.Add(temp);
            }
            return PartialView(model);
        }
        [HttpPost]
        public PartialViewResult BattleInvite()
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser user = dbContext.Users.Find(userId);
            if (user == null)
                return PartialView(false);

            ViewBag.UpdateCalendar = user.Update;

            user.LastActivity = DateTime.Now;
            dbContext.SaveChanges();

            bool invited = user.Invited;        
            if (invited == false)
                return PartialView(false);

            int battleId = user.IdBattleInvite;
            var battle = dbContext.Battles
                 .Where(u => u.Id == battleId)
                 .Where(u => u.SecondPlayer == userId)
                 .Where(u => u.Winner == null)
                 .Select(u => new {
                     Id = u.Id,
                     Time = u.TimeStartFirstPlayer,
                 }).SingleOrDefault();
            if (battle == null)
                return PartialView(false);

            if ((DateTime.Now - battle.Time).Minutes >= 1)
            {
                user.Invited = false;
                dbContext.SaveChanges();
                return PartialView(false);
            }
            return PartialView(true);
        }
        [HttpPost]
        public PartialViewResult BattleInvitePopup()
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser user = dbContext.Users.Find(userId);

            Battle battle = dbContext.Battles.Find(user.IdBattleInvite);

            ViewBag.Course = battle.Course;
            ViewBag.Qualification = dbContext.Qualifications
                    .Where(u => u.Id == battle.Qualification)
                    .Select(u => u.Name).Single();

            ApplicationUser otheruser = dbContext.Users.Find(battle.FirstPlayer.ToString());
            List<UsersData> invite = new List<UsersData>();

            int numbertop = dbContext.Users
                    .Where(u => u.Rating > user.Rating)
                    .Select(u => u.Rating).OrderByDescending(u => u).Count();

            int temp = 0;
            if (user.AnswersCount != 0)
                temp = 100 * user.CorrectAnswersCount / user.AnswersCount;

            invite.Add(new UsersData
            {
                Avatar = "/Images/Avatars/" + user.Avatar,
                Family = user.Family,
                Name = user.Name,
                Battles = user.BattleCount,
                Group = user.Course + user.Group,
                Rating = numbertop + 1,
                CorrectAnswers = temp,
            });
            temp = 0;
            if (otheruser.AnswersCount != 0)
                temp = 100 * otheruser.CorrectAnswersCount / otheruser.AnswersCount;

            invite.Add(new UsersData
            {
                Avatar = "/Images/Avatars/" + otheruser.Avatar,
                Family = otheruser.Family,
                Name = otheruser.Name,
                Battles = user.BattleCount,
                Group = user.Course + user.Group,
                Rating = numbertop + 1,
                CorrectAnswers = temp,
            });

            ViewBag.Id = user.IdBattleInvite;
            return PartialView(invite);
        }

        public ActionResult Prepare(string id)
        {
            if (id == null) 
                return RedirectToAction("Index", "Home");

            ApplicationUser otherUser = dbContext.Users.Find(id); //Есть ли такой юзер, если нет - домой
            if (otherUser == null)
                return RedirectToAction("Index", "Home");
            string userId = User.Identity.GetUserId();

            ApplicationUser user = dbContext.Users.Find(userId); //Юзера делаем занятым
            user.Busy = true;
            dbContext.SaveChanges();

            var unfinishedBattle = dbContext.Battles //Если идет схватка пересылаем на нее (первый игрок)
                .Where(u => u.FirstPlayer == userId)
                .Where(u => string.IsNullOrEmpty(u.Winner) == true)
                .Where(u => u.AgreementOtherPlayer == true)
                .Select(u => new
                {
                    id = u.Id,
                    Row = u.RightOrWrongFirstPlayer,
                    lastActivity = u.TimeEndFirstPlayer
                }).SingleOrDefault();
            if (unfinishedBattle != null)
            {
                Battle battleReturn = dbContext.Battles.Find(unfinishedBattle.id);
                double time = 0;
                if (unfinishedBattle.Row == null) //Если не дал ниодного ответа
                {
                    for (int i = 0; i < battleQuestions; i++)
                    {
                        time = time + 60 / Math.Pow(2, i);
                    }
                    if ((DateTime.Now - unfinishedBattle.lastActivity).TotalSeconds >= time)
                    {
                        string answers = "0";
                        for (int i = 1; i < battleQuestions; i++)
                            answers = answers + "|0";
                        battleReturn.RightOrWrongFirstPlayer = answers;
                        battleReturn.AnswersFirstPlayer = answers;

                        battleReturn.TimeEndFirstPlayer = DateTime.Now;
                        battleReturn.EndFirstPlayer = true;
                        dbContext.SaveChanges();
                    }
                }
                else
                {
                    //Считаем сколько осталось
                    for (int i = 0; i < battleQuestions - unfinishedBattle.Row.Split('|').Count(); i++)
                    {
                        time = time + 60 / Math.Pow(2, i);
                    }
                    if ((DateTime.Now - unfinishedBattle.lastActivity).TotalSeconds >= time)
                    {
                        for (int i = battleReturn.RightOrWrongFirstPlayer.Split('|').Count() - 1; i < battleQuestions; i++)
                        {
                            battleReturn.RightOrWrongFirstPlayer += "|0";
                            battleReturn.AnswersFirstPlayer += "|0";
                        }
                        battleReturn.TimeEndFirstPlayer = DateTime.Now;
                        battleReturn.EndFirstPlayer = true;
                        dbContext.SaveChanges();
                    }
                }
                return RedirectToAction("Fight", new { id = unfinishedBattle.id });
            }
            unfinishedBattle = dbContext.Battles //Если идет схватка пересылаем на нее (второй игрок)
                .Where(u => u.SecondPlayer == userId)
                .Where(u => string.IsNullOrEmpty(u.Winner) == true)
                .Where(u => u.AgreementOtherPlayer == true)
                .Select(u => new
                {
                    id = u.Id,
                    Row = u.RightOrWrongSecondPlayer,
                    lastActivity = u.TimeEndSecondPlayer
                }).SingleOrDefault();
            if (unfinishedBattle != null)
            {
                Battle battleReturn = dbContext.Battles.Find(unfinishedBattle.id);
                double time = 0;
                if (unfinishedBattle.Row == null) //Если не дал ниодного ответа
                {
                    for (int i = 0; i < battleQuestions; i++)
                    {
                        time = time + 60 / Math.Pow(2, i);
                    }
                    if ((DateTime.Now - unfinishedBattle.lastActivity).TotalSeconds >= time)
                    {
                        string answers = "0";
                        for (int i = 1; i < battleQuestions; i++)
                            answers = answers + "|0";
                        battleReturn.RightOrWrongFirstPlayer = answers;
                        battleReturn.AnswersFirstPlayer = answers;

                        battleReturn.TimeEndSecondPlayer = DateTime.Now;
                        battleReturn.EndSecondPlayer = true;
                        dbContext.SaveChanges();
                    }
                }
                else
                {
                    //Считаем сколько осталось
                    for (int i = 0; i < battleQuestions - unfinishedBattle.Row.Split('|').Count(); i++)
                    {
                        time = time + 60 / Math.Pow(2, i);
                    }
                    if ((DateTime.Now - unfinishedBattle.lastActivity).TotalSeconds >= time)
                    {
                        for (int i = battleReturn.RightOrWrongFirstPlayer.Split('|').Count() - 1; i < battleQuestions; i++)
                        {
                            battleReturn.RightOrWrongFirstPlayer += "|0";
                            battleReturn.AnswersFirstPlayer += "|0";
                        }
                        battleReturn.TimeEndSecondPlayer = DateTime.Now;
                        battleReturn.EndSecondPlayer = true;
                        dbContext.SaveChanges();
                    }
                }
                return RedirectToAction("Fight", new { id = unfinishedBattle.id });
            }
            var emptyBattle = dbContext.Battles //Есть ли пустая запись в бд то используем ее
                .Where(u => u.FirstPlayer == userId)
                .Where(u => string.IsNullOrEmpty(u.Winner) == true)
                .Where(u => string.IsNullOrEmpty(u.QuestionsFirstPlayer) == true)
                .Select(u => new
                {
                    id = u.Id
                }).SingleOrDefault();

            Battle battle;

            if (emptyBattle != null)
                battle = dbContext.Battles.Find(emptyBattle.id);
            else
                battle = new Battle();

            battle.FirstPlayer = userId;
            battle.SecondPlayer = id;
            battle.TimeStartFirstPlayer = DateTime.Now;
            battle.TimeStartSecondPlayer = DateTime.Now;
            battle.TimeEndFirstPlayer = DateTime.Now;
            battle.TimeEndSecondPlayer = DateTime.Now;
            if (emptyBattle == null)
                dbContext.Battles.Add(battle);

            dbContext.SaveChanges();
            ViewBag.Id = battle.Id;

            Decliner decliner = new Decliner();
            string[] declineText = decliner.Decline(otherUser.Family, otherUser.Name, "", 4);//Меняем падеж
            ViewBag.SecondPlayer = declineText[1] + " " + declineText[0];
            
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Prepare(BattleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (model.Id == 0)
                    return RedirectToAction("Index", "Home");

                string otherUser = dbContext.Battles.Find(model.Id).SecondPlayer;
                if (string.IsNullOrEmpty(otherUser))
                    return RedirectToAction("Index", "Home");
                Decliner decliner = new Decliner();
                string[] declineText = decliner.Decline(dbContext.Users.Find(otherUser).Family, dbContext.Users.Find(otherUser).Name, "", 4);//Меняем падеж
                ViewBag.SecondPlayer = declineText[1] + " " + declineText[0];
                ViewBag.Id = model.Id;

                return View();
            }

            var battle = dbContext.Battles.Find(model.Id);
            if (battle == null)
                return RedirectToAction("Index", "Home");

            battle.Course = model.Course;
            battle.Qualification = model.Qualification;
            battle.TimeStartFirstPlayer = DateTime.Now;
            battle.TimeStartSecondPlayer = DateTime.Now;
            battle.TimeEndFirstPlayer = DateTime.Now;
            battle.TimeEndSecondPlayer = DateTime.Now;
            dbContext.SaveChanges();

            if (!AutoSelectQuestion(model.Id)) //подготовим за людей вопросы. 
                return RedirectToAction("Index", "Home");

            var secondPlayer = dbContext.Users.Find(battle.SecondPlayer); //Приглашаем второго юзера
            secondPlayer.Invited = true;
            secondPlayer.IdBattleInvite = model.Id;
            dbContext.SaveChanges();

            return RedirectToAction("Wait", new { model.Id });
        }
        public ActionResult inviteAnswer(int id, string submitButton) //Сюда попадает только тот кого вызвали
        {
            string user = User.Identity.GetUserId();
            Battle result = dbContext.Battles.Find(id);
            if (result == null)
                return RedirectToAction("Index", "Home");

            if (result.SecondPlayer != user)
                return RedirectToAction("Index", "Home");

            var player = dbContext.Users.Find(user);

            switch (submitButton)
            {
                case "Accept":
                    result.AgreementOtherPlayer = true;
                    player.Invited = false; //т.к. второй игрок согласился удаляем инвайт, чтоб подсказка слева не вылезала.
                    player.Busy = true;
                    dbContext.SaveChanges();
                    break;
                default:
                    player.Invited = false;
                    dbContext.Battles.Remove(result);
                    dbContext.SaveChanges();
                    return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Wait", new { id });
        }
        public ActionResult Wait(int id)
        {
            var result = dbContext.Battles.Find(id);
            if (result == null)
                return RedirectToAction("Index", "Home");

            var player = dbContext.Users.Find(result.FirstPlayer);
            ViewBag.FirstPlayer = player.Name + " " + player.Family;
            ViewBag.FirstPlayerAvatar = "/Images/Avatars/" + player.Avatar;
            //+еще куча данных

            player = dbContext.Users.Find(result.SecondPlayer);
            ViewBag.SecondPlayer = player.Name + " " + player.Family;
            ViewBag.SecondPlayerAvatar = "/Images/Avatars/" + player.Avatar;

            ViewBag.Id = id;
            ViewBag.Time = 60 - (DateTime.Now - result.TimeStartFirstPlayer).Seconds;
            return View();
        }
        [HttpPost]
        public PartialViewResult WaitConfirm(int id)
        {
            var result = dbContext.Battles.Find(id);
            ViewBag.Result = false;
            if (result == null)
                return PartialView();

            ViewBag.Result = true;
            ViewBag.Agree = true;
            if (result != null && result.AgreementOtherPlayer != true)
            {
                ViewBag.Agree = false;
                if ((DateTime.Now - result.TimeStartFirstPlayer).TotalMinutes > 1)
                {
                    ViewBag.Result = false;
                    ApplicationUser OtherPlayer = new ApplicationUser();
                    if (result.FirstPlayer == User.Identity.GetUserId())
                        OtherPlayer = dbContext.Users.Find(result.SecondPlayer);
                    else
                        OtherPlayer = dbContext.Users.Find(result.FirstPlayer);

                    OtherPlayer.Invited = false;
                    dbContext.Battles.Remove(result);
                    dbContext.SaveChanges();
                }
            }
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Wait(int id, string submitButton)
        {
            var result = dbContext.Battles.Find(id);
            if (result == null)
                return RedirectToAction("Index", "Home");
            switch (submitButton)
            {
                case "Accept":
                    return RedirectToAction("Fight", new { id });
                default:
                    Battle battle = dbContext.Battles.Find(id);
                    var player = dbContext.Users.Find(battle.SecondPlayer);
                    player.Invited = false;
                    dbContext.Battles.Remove(battle);
                    dbContext.SaveChanges();
                    return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult Fight(int? id)
        {
            if (id == null) //Левый заход на страницу
                return RedirectToAction("Index", "Home");
            int fightId = id.Value;

            var result = dbContext.Battles.Find(fightId); 
            if (!result.AgreementOtherPlayer) //Если попали сюда сами и второго юзера нет, то домой
                return RedirectToAction("Index", "Home");

            if (result.Winner != null) //Если есть победитель - домой
                return RedirectToAction("Index", "Home");

            string user = User.Identity.GetUserId();
            if (user != result.FirstPlayer && user != result.SecondPlayer) //Вообще не понятно кто зашел сюда - домой
                return RedirectToAction("Index", "Home");

            var player = dbContext.Users.Find(result.FirstPlayer);
            ViewBag.FirstPlayer = player.Name + " " + player.Family;
            ViewBag.FirstPlayerAvatar = "/Images/Avatars/" + player.Avatar;
            //+еще куча данных

            player = dbContext.Users.Find(result.SecondPlayer);
            ViewBag.SecondPlayer = player.Name + " " + player.Family;
            ViewBag.SecondPlayerAvatar = "/Images/Avatars/" + player.Avatar;

            player = dbContext.Users.Find(user);
            player.BattleCount += 1;
            DateTime tempTime = new DateTime(1971, 1, 1);
            List<string> answers = new List<string>();

            if (result.FirstPlayer == user) //Сюда первый юзер
            {
                int count = result.QuestionsFirstPlayer.Split('|').ToList().Count(); //Общее количество вопросов 
                int number = 0;
                if (result.AnswersFirstPlayer != null)
                    number = result.AnswersFirstPlayer.Split('|').ToList().Count(); //Общее количество ответов

                if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
                    return RedirectToAction("BattleEnd", new { id = fightId });

                if (result.TimeStartFirstPlayer == result.TimeEndFirstPlayer) //Значит только начали, но время end надо тоже обновить для мониторинга АФК
                {
                    result.TimeStartFirstPlayer = DateTime.Now; //Это больше не обновляется потом
                    result.TimeEndFirstPlayer = DateTime.Now;
                }

                if (result.AnswersFirstPlayer != null)
                    answers = result.AnswersFirstPlayer.Split('|').ToList();

                ViewBag.Min = (DateTime.Now - result.TimeStartFirstPlayer).Minutes;
                ViewBag.Sec = (DateTime.Now - result.TimeStartFirstPlayer).Seconds;
            }
            else //А сюда второй
            {
                int count = result.QuestionsFirstPlayer.Split('|').ToList().Count(); //Общее количество вопросов 
                int number = 0;
                if (result.AnswersSecondPlayer != null)
                    number = result.AnswersSecondPlayer.Split('|').ToList().Count(); //Общее количество ответов

                if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
                    return RedirectToAction("BattleEnd", new { id = fightId });

                if (result.TimeStartSecondPlayer == result.TimeEndSecondPlayer) //Значит только начали, но время end надо тоже обновить для мониторинга АФК
                {
                    result.TimeStartSecondPlayer = DateTime.Now; //Это больше не обновляется потом
                    result.TimeEndSecondPlayer = DateTime.Now;
                }
                if (result.AnswersSecondPlayer != null)
                    answers = result.AnswersSecondPlayer.Split('|').ToList();

                ViewBag.Min = (DateTime.Now - result.TimeStartSecondPlayer).Minutes;
                ViewBag.Sec = (DateTime.Now - result.TimeStartSecondPlayer).Seconds;
            }

            dbContext.SaveChanges();
            ViewBag.Id = fightId;
            ViewBag.Number = answers.Count() + 1;

            Questions model = new Questions();
            model = SelectQuestion(fightId);
            ViewBag.Afk = 60;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public PartialViewResult FightQuestions(int id, List<int> AnswersIDs, string afk, int timeAfk = 60)
        {
            if (!string.IsNullOrEmpty(afk))
                ViewBag.Afk = Math.Floor(timeAfk / 2.0); //Округляем секунды, например 7,5 => 7
            else
                ViewBag.Afk = 60;

            var battle = dbContext.Battles.Find(id);
            if (!battle.AgreementOtherPlayer) //Если попали сюда сами и второго юзера нет, то домой
                return PartialView();
            ViewBag.Id = id;
            ViewBag.Result = false;

            if (battle.EndFirstPlayer && battle.EndSecondPlayer) //Значит закончили без тебя :) (по таймеру)
                return PartialView();

            if (!SaveAnswer(id, AnswersIDs))
                return PartialView();

            string user = User.Identity.GetUserId();
            if(user == battle.FirstPlayer) //Сюда первый юзер
            {
                //Если время на ответ уже меньше 2 секунд, то заканчиваем
                if (timeAfk / 2 <= 2)
                {
                    for (int i = battle.RightOrWrongFirstPlayer.Split('|').Count(); i < battleQuestions; i++)
                    {
                        battle.RightOrWrongFirstPlayer += "|0";
                        battle.AnswersFirstPlayer += "|0";
                    }
                    battle.TimeEndFirstPlayer = DateTime.Now;
                    battle.EndFirstPlayer = true;
                    dbContext.SaveChanges();
                }

                int count = battle.QuestionsFirstPlayer.Split('|').ToList().Count(); //Общее количество вопросов 
                int number = 0;
                if (battle.AnswersFirstPlayer != null)
                    number = battle.AnswersFirstPlayer.Split('|').ToList().Count(); //Общее количество ответов

                if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
                {
                    battle.TimeEndFirstPlayer = DateTime.Now;
                    battle.EndFirstPlayer = true;
                    dbContext.SaveChanges();
                    return PartialView();
                }
            }
            else //Сюда второй
            {
                //Если время на ответ уже меньше 2 секунд, то заканчиваем
                if (timeAfk / 2 <= 2)
                {
                    for (int i = battle.RightOrWrongSecondPlayer.Split('|').Count(); i < battleQuestions; i++)
                    {
                        battle.RightOrWrongSecondPlayer += "|0";
                        battle.AnswersSecondPlayer += "|0";
                    }
                    battle.TimeEndSecondPlayer = DateTime.Now;
                    battle.EndSecondPlayer = true;
                    dbContext.SaveChanges();
                }

                int count = battle.QuestionsSecondPlayer.Split('|').ToList().Count(); //Общее количество вопросов 
                int number = 0;
                if (battle.AnswersSecondPlayer != null)
                    number = battle.AnswersSecondPlayer.Split('|').ToList().Count(); //Общее количество ответов

                if (count == number) //Если ответов столько же сколько и вопросов то идем на страницу статистики.
                {
                    battle.TimeEndSecondPlayer = DateTime.Now;
                    battle.EndSecondPlayer = true;
                    dbContext.SaveChanges();
                    return PartialView();
                }
            }

            Questions model = new Questions();
            model = SelectQuestion(id);

            List<string> answers = new List<string>();
            if (battle.FirstPlayer == User.Identity.GetUserId())
            {
                if (battle.AnswersFirstPlayer != null)
                    answers = battle.AnswersFirstPlayer.Split('|').ToList();
            }
            else
            {
                if (battle.AnswersSecondPlayer != null)
                    answers = battle.AnswersSecondPlayer.Split('|').ToList();
            }

            ViewBag.Number = answers.Count() + 1;
            ViewBag.Result = true;
            return PartialView(model);
        }
        [HttpPost]
        public PartialViewResult FightQuestionsRoW(int id)
        {
            Battle battle = dbContext.Battles.Find(id);
            string RoWFirst = battle.RightOrWrongFirstPlayer;
            string RoWSecond = battle.RightOrWrongSecondPlayer;

            List<List<string>> RoW = new List<List<string>>();
            if (RoWFirst != null)
                RoW.Add(RoWFirst.Split('|').ToList());
            else
                ViewBag.First = 0;
            if (RoWSecond != null)
                RoW.Add(RoWSecond.Split('|').ToList());
            else
                ViewBag.Second = 0;
            return PartialView(RoW);
        }

        public ActionResult BattleEnd(int id)
        {
            Battle battle = dbContext.Battles.Find(id);

            string answers = "";
            DateTime start = new DateTime();
            DateTime end = new DateTime();
            string user = User.Identity.GetUserId();
            if (battle.FirstPlayer == user && battle.EndFirstPlayer == true) //Если первый юзер
            {
                answers = battle.RightOrWrongFirstPlayer;
                start = battle.TimeStartFirstPlayer;
                end = battle.TimeEndFirstPlayer;
            }
            if (battle.SecondPlayer == user && battle.EndSecondPlayer == true) //Если второй
            {
                answers = battle.RightOrWrongSecondPlayer;
                start = battle.TimeStartSecondPlayer;
                end = battle.TimeEndSecondPlayer;
            }
            dbContext.SaveChanges();

            List<string> right = new List<string>();
            foreach (string item in answers.Split('|').ToList())
            {
                if (item != "0")
                    right.Add(item);
            }
            ViewBag.Right = right.Count();
            ViewBag.TimeSec = (end - start).Seconds;
            ViewBag.TimeMin = (end - start).Minutes;

            var player = dbContext.Users.Find(user);
            ViewBag.You = player.Name + " " + player.Family;
            ViewBag.Avatar = "/Images/Avatars/" + player.Avatar;
            ViewBag.Id = id;

            return View();
        }
        [HttpPost]
        public PartialViewResult BattleEndWait(int id)
        {
            Battle battle = dbContext.Battles.Find(id);

            if (battle.EndFirstPlayer == battle.EndSecondPlayer)
                ViewBag.Result = true;
            else
            {
                string idOtherPlayer = "";
                if (battle.FirstPlayer == User.Identity.GetUserId())
                {
                    idOtherPlayer = battle.SecondPlayer;
                }
                else
                {
                    idOtherPlayer = battle.FirstPlayer;
                }
                ApplicationUser otherUser = dbContext.Users.Find(idOtherPlayer);
                DateTime lastActivity;
                if (battle.FirstPlayer == otherUser.Id)
                    lastActivity = battle.TimeEndFirstPlayer;
                else
                    lastActivity = battle.TimeEndSecondPlayer;

                //Если 60 секунд небыло ответа
                if (lastActivity < DateTime.Now.AddSeconds(-60))
                {
                    //Если второго игрока нет - считаем время которое ждать до конца, в зависимости от кол-ва его ответов.
                    double time = 0;
                    if (battle.FirstPlayer == otherUser.Id)
                    {
                        if (battle.RightOrWrongFirstPlayer == null) //Если противник не дал ниодного ответа
                        {
                            for (int i = 0; i < battleQuestions; i++)
                            {
                                time = time + 60 / Math.Pow(2, i);
                            }
                            if ((DateTime.Now - lastActivity).TotalSeconds >= time)
                            {
                                string answers = "0";
                                for (int i = 1; i < battleQuestions; i++)
                                    answers = answers + "|0";
                                battle.RightOrWrongFirstPlayer = answers;
                                battle.AnswersFirstPlayer = answers;

                                battle.TimeEndFirstPlayer = DateTime.Now;
                                battle.EndFirstPlayer = true;
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            //Считаем сколько осталось
                            for (int i = 0; i < battleQuestions - battle.RightOrWrongFirstPlayer.Split('|').Count(); i++)
                            {
                                time = time + 60 / Math.Pow(2, i);
                            }
                            if ((DateTime.Now - lastActivity).TotalSeconds >= time)
                            {
                                for (int i = battle.RightOrWrongFirstPlayer.Split('|').Count(); i < battleQuestions; i++)
                                {
                                    battle.RightOrWrongFirstPlayer += "|0";
                                    battle.AnswersFirstPlayer += "|0";
                                }
                                battle.TimeEndFirstPlayer = DateTime.Now;
                                battle.EndFirstPlayer = true;
                                dbContext.SaveChanges();
                            }
                        }
                    }
                    else //Тоже самое если другой игрок вышел
                    {
                        if (battle.RightOrWrongSecondPlayer == null)
                        {
                            for (int i = 0; i < battleQuestions; i++)
                            {
                                var temp = Math.Pow(2, i);
                                time = time + 60 / temp;
                            }
                            if ((DateTime.Now - lastActivity).TotalSeconds >= time) 
                            {
                                string answers = "0";
                                for (int i = 1; i < battleQuestions; i++)
                                    answers = answers + "|0";
                                battle.RightOrWrongSecondPlayer = answers;
                                battle.AnswersSecondPlayer = answers;

                                battle.TimeEndSecondPlayer = DateTime.Now;
                                battle.EndSecondPlayer = true;
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < battleQuestions - battle.RightOrWrongSecondPlayer.Split('|').Count(); i++)
                            {
                                time = time + 60 / Math.Pow(2, i);
                            }
                            if ((DateTime.Now - lastActivity).TotalSeconds >= time)
                            {
                                for (int i = battle.RightOrWrongSecondPlayer.Split('|').Count(); i < battleQuestions; i++)
                                {
                                    battle.RightOrWrongSecondPlayer += "|0";
                                    battle.AnswersSecondPlayer += "|0";
                                }
                                battle.TimeEndSecondPlayer = DateTime.Now;
                                battle.EndSecondPlayer = true;
                                dbContext.SaveChanges();
                            }
                        }
                    }                    
                }
            }
            ViewBag.Id = id;
            return PartialView();
        }
        public ActionResult Finish(int id)
        {
            Battle battle = dbContext.Battles.Find(id);
            var user1 = dbContext.Users.Find(battle.FirstPlayer);
            user1.Busy = false;
            var user2 = dbContext.Users.Find(battle.SecondPlayer);
            user2.Busy = false;
            dbContext.SaveChanges();
            //Берем данные первого игрока
            double time1 = (battle.TimeEndFirstPlayer - battle.TimeStartFirstPlayer).TotalSeconds;
            List<string> rightFirst = new List<string>();
            foreach (string item in battle.RightOrWrongFirstPlayer.Split('|').ToList())
            {
                if (item != "0")
                    rightFirst.Add(item);
            }

            //Берем данные второго игрока
            double time2 = (battle.TimeEndSecondPlayer - battle.TimeStartSecondPlayer).TotalSeconds;
            List<string> rightSecond = new List<string>();
            foreach (string item in battle.RightOrWrongSecondPlayer.Split('|').ToList())
            {
                if (item != "0")
                    rightSecond.Add(item);
            }
            //Сравниванием
            double points1 = rightFirst.Count() * 10;
            ViewBag.RightFirst = rightFirst.Count();
            double points2 = rightSecond.Count() * 10;
            ViewBag.RightSecond = rightSecond.Count();
            points1 = points1 - time1 / 100;
            points2 = points2 - time2 / 100;

            ApplicationUser winner = new ApplicationUser();
            if (points1 > points2)
            {
                battle.Winner = battle.FirstPlayer;
                ViewBag.Winner = 1;
                if (battle.FirstPlayer == User.Identity.GetUserId())
                    winner = dbContext.Users.Find(battle.FirstPlayer);
            }
            else
            {
                battle.Winner = battle.SecondPlayer;
                ViewBag.Winner = 2;
                if (battle.FirstPlayer == User.Identity.GetUserId())
                    winner = dbContext.Users.Find(battle.SecondPlayer);
            }
            winner.BattleWinCount++;
            winner.Rating += 1;
            dbContext.SaveChanges();
            var player = dbContext.Users.Find(battle.FirstPlayer);
            ViewBag.AvatarFirst = "/Images/Avatars/" + player.Avatar;
            ViewBag.NameFirst = player.Name + " " + player.Family;
            ViewBag.TimeMinFirst = (battle.TimeEndFirstPlayer - battle.TimeStartFirstPlayer).Minutes;
            ViewBag.TimeSecFirst = (battle.TimeEndFirstPlayer - battle.TimeStartFirstPlayer).Seconds;
            if (ViewBag.Winner == 1)
                ViewBag.NameWinner = ViewBag.NameFirst;

            player = dbContext.Users.Find(battle.SecondPlayer);
            ViewBag.AvatarSecond = "/Images/Avatars/" + player.Avatar;
            ViewBag.NameSecond = player.Name + " " + player.Family;
            ViewBag.TimeMinSecond = (battle.TimeEndSecondPlayer - battle.TimeStartSecondPlayer).Minutes;
            ViewBag.TimeSecSecond = (battle.TimeEndSecondPlayer - battle.TimeStartSecondPlayer).Seconds;
            if (ViewBag.Winner == 2)
                ViewBag.NameWinner = ViewBag.NameSecond;

            ViewBag.You = -1;
            if (battle.Winner == User.Identity.GetUserId())
                ViewBag.You = 1;

            return View();
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
        private bool AutoSelectQuestion(int id)
        {
            try
            {
                Battle battle = dbContext.Battles.Find(id);

                List<int> idQuestions = new List<int>();
                var questionsId = dbContext.Questions
                     .Where(u => u.IdQualification <= battle.Qualification)
                     .Where(u => u.IdCourse <= battle.Course)
                     .Select(u => new
                     {
                         id = u.Id
                     }).ToList();

                if (battleQuestions >= questionsId.Count) //Если вопросов меньше или столько же сколько в базе, то делаем что есть
                {
                    for (int i = 0; i < questionsId.Count; i++)
                        idQuestions.Add(questionsId.ElementAt(i).id);
                    Shuffle(idQuestions); //Берем все вопросы без рандома и перемешиваем.
                }
                else //Иначе набираем рандомно вопросы
                {
                    Random rnd = new Random();
                    while (idQuestions.Count != battleQuestions)
                    {
                        int value = rnd.Next(questionsId.Count());
                        int item = questionsId.ElementAt(value).id;

                        if (!idQuestions.Contains(item))
                            idQuestions.Add(item);
                    }
                }

                string questions = ""; //Сохраняем вопросы в тест
                foreach (int item in idQuestions)
                {
                    questions += item.ToString() + "|";
                }
                questions = questions.Substring(0, questions.Length - 1);
                battle.QuestionsFirstPlayer = questions;
                battle.QuestionsSecondPlayer = questions;
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
            Battle battle = dbContext.Battles.Find(id);
            List<string> questions = new List<string>();
            List<string> answers = new List<string>();

            string user = User.Identity.GetUserId();
            if (user == battle.FirstPlayer) //Сюда первый юзер
            {
                questions = battle.QuestionsFirstPlayer.Split('|').ToList();
                if (battle.AnswersFirstPlayer != null)
                    answers = battle.AnswersFirstPlayer.Split('|').ToList();
            }
            else //Сюда второй
            {
                questions = battle.QuestionsSecondPlayer.Split('|').ToList();
                if (battle.AnswersSecondPlayer != null)
                    answers = battle.AnswersSecondPlayer.Split('|').ToList();
            }

            return Convert.ToInt32(questions[answers.Count()]);
        }
        private bool SaveAnswer(int id, List<int> AnswersIDs)
        {
            Battle battle = dbContext.Battles.Find(id);
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

            string userId = User.Identity.GetUserId();
            if (userId == battle.FirstPlayer) //Сюда первый юзер
            {
                battle.TimeEndFirstPlayer = DateTime.Now; //Для отслеживания
                if (battle.AnswersFirstPlayer != null && battle.AnswersFirstPlayer.Count() > 0)
                    battle.AnswersFirstPlayer += "|" + answer;
                else
                    battle.AnswersFirstPlayer += answer;
            }
            else //Сюда второй
            {
                battle.TimeEndSecondPlayer = DateTime.Now; //Для отслеживания
                if (battle.AnswersSecondPlayer != null && battle.AnswersSecondPlayer.Count() > 0)
                    battle.AnswersSecondPlayer += "|" + answer;
                else
                    battle.AnswersSecondPlayer += answer;
            }

            var user = dbContext.Users.Find(userId); //Сохранение правильных ответов и кол-ва ответов в таблицу юзера
            user.AnswersCount += 1;
            if (correct == answer)
            {
                user.CorrectAnswersCount += 1;  //Прибавляем +1 к тому что пользователь правильно ответил (для статистики)
                question.CountCorrect += 1;  //Прибавляем +1 к тому что на вопрос правильно ответили (для статистики)

                if (userId == battle.FirstPlayer) //Сюда первый юзер
                {
                    if (battle.RightOrWrongFirstPlayer != null)
                        battle.RightOrWrongFirstPlayer += "|1";
                    else
                        battle.RightOrWrongFirstPlayer = "1";
                }
                else //Сюда второй
                {
                    if (battle.RightOrWrongSecondPlayer != null)
                        battle.RightOrWrongSecondPlayer += "|1";
                    else
                        battle.RightOrWrongSecondPlayer = "1";
                }
            }
            else
            {
                if (userId == battle.FirstPlayer) //Сюда первый юзер
                {
                    if (battle.RightOrWrongFirstPlayer != null)
                        battle.RightOrWrongFirstPlayer += "|0";
                    else
                        battle.RightOrWrongFirstPlayer = "0";
                }
                else //Сюда второй
                {
                    if (battle.RightOrWrongSecondPlayer != null)
                        battle.RightOrWrongSecondPlayer += "|0";
                    else
                        battle.RightOrWrongSecondPlayer = "0";
                }
            }
            dbContext.SaveChanges();
            return true;
        }
        #endregion
    }
}