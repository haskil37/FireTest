using FireTest.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FireTest.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdministratorController : Controller
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Users()
        {
            return View();
        }

        public PartialViewResult UsersAjax(string currentFilter, string searchString, int? page)
        {
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
            foreach (var item in users)
            {
                UsersForAdmin temp = new UsersForAdmin();
                temp.Avatar = item.Avatar;
                temp.Name = item.Family + " " + item.Name + " " + item.SubName;
                temp.Teacher = User.IsInRole("TEACHER");
                temp.Administrator = User.IsInRole("ADMIN");
            }

            model = model.OrderBy(s => s.Name).ToList();

            int pageSize = 2;
            int pageNumber = (page ?? 1);
            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }

    }
}