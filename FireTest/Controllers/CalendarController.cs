using FireTest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            var user = dbContext.Users.Find(userId);
            user.Update = false;
            dbContext.SaveChanges();
            var exams = dbContext.Examinations
                .Where(u => u.Group == user.Group)
                .Select(u => new {
                    Name = u.Name,
                    Classroom = u.Classroom,
                    Date = u.Date,
                    Annotations = u.Annotations
                }).ToList();
            if (exams.Count == 0)
                exams = dbContext.Examinations
                   .Where(u => u.TeacherId == userId)
                   .Select(u => new
                   {
                       Name = u.Name,
                       Classroom = u.Classroom,
                       Date = u.Date,
                       Annotations = u.Annotations
                   }).ToList();

            ViewBag.Dates = "";
            foreach(var item in exams)
            {
                string temp = "{title: '" + item.Name;
                if (!string.IsNullOrEmpty(item.Classroom))
                {
                    temp += " (Аудитория: " + item.Classroom;
                    if (!string.IsNullOrEmpty(item.Annotations))
                        temp += ". Комментарий: " + item.Annotations + ".)', start:'";
                    else
                        temp += ".)', start:'";
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.Annotations))
                        temp += "(Комментарий: " + item.Annotations + ".)', start:'";
                    else
                        temp += "', start:'";
                }
                ViewBag.Dates += temp + item.Date.ToString("yyyy-MM-dd") + "', textEscape: 'false'},";
            }
            return View();
        }
    }
}