using FireTest.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
        UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

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
            var users = dbContext.Users.Where(u => u.Avatar != user);
            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Family.Contains(searchString)
                                       || u.Name.Contains(searchString)
                                       || u.SubName.Contains(searchString));
            }








           var rr= userManager.AddToRole("82f261b9-0c40-4843-b7e5-1adf322d5bc6", "TEACHER");





            foreach (var item in users)
            {
                UsersForAdmin temp = new UsersForAdmin();
                temp.Id = item.Id;
                temp.Avatar = item.Avatar;
                temp.Name = item.Family + " " + item.Name + " " + item.SubName;
                temp.Teacher = userManager.IsInRole(item.Id, "TEACHER");
                temp.Administrator = userManager.IsInRole(item.Id, "ADMIN");
                model.Add(temp);
            }

            model = model.OrderBy(u => u.Name).ToList();

            int pageSize = 2;
            int pageNumber = (page ?? 1);
            return PartialView(model.ToPagedList(pageNumber, pageSize));
        }
        [HttpPost]
        public PartialViewResult UsersAjaxSave(List<string> Administrator, List<string> Teacher)
        {

            if (Administrator != null)
            {
                foreach(string item in Administrator)
                {
                    var user = dbContext.Users.Find(item);
                    if (user != null && !User.IsInRole("ADMIN"))
                        userManager.AddToRole(user.Id, "ADMIN");
                }
            }
            if (Teacher != null)
            {
                foreach (string item in Teacher)
                {
                    var user = dbContext.Users.Find(item);
                    if (user != null && !User.IsInRole("TEACHER"))
                        userManager.AddToRole(user.Id, "TEACHER");
                }
            }
            return PartialView();
        }
    }
}