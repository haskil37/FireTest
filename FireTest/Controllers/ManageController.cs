using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using FireTest.Models;
using System.Collections.Generic;
using System.IO;
using System.Web.Helpers;
using System.Security.Cryptography;

namespace FireTest.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        ApplicationDbContext dbContext = new ApplicationDbContext();

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : message == ManageMessageId.NewUser ? "Заполните свой профиль."
                : message == ManageMessageId.ChangeSuccess ? "Ваши данные сохранены."
                : "";

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            string year = "";
            if (user.Year != 0)
                year = user.Year.ToString();
            ViewBag.Avatar = "/Images/Avatars/" + user.Avatar;
            List<bool> selected = new List<bool>() { false, false, false };
            string group = "";
            if (!string.IsNullOrEmpty(user.Group) && user.Group != "0" && user.Year != 0)
            {
                if (user.Group.Length < 3)
                    user.Group = user.Course + "00";
                user.Group = user.Group.Remove(0, 1);
                List<bool> selectedGroup = new List<bool>();
                switch (user.Group.Substring(0, 1))
                {
                    case "1":
                        selected[1] = true;
                        selectedGroup = new List<bool>() { false, false, false, false };
                        group = user.Group.Substring(user.Group.Length - 1);
                        switch (group)
                        {
                            case "1":
                                selectedGroup[0] = true;
                                break;
                            case "2":
                                selectedGroup[1] = true;
                                break;
                            case "3":
                                selectedGroup[2] = true;
                                break;
                            case "4":
                                selectedGroup[3] = true;
                                break;
                        }
                        var groupList1 = new[]{
                             new SelectListItem{ Value="1",Text="1 учебная группа", Selected=selectedGroup[0]},
                             new SelectListItem{ Value="2",Text="2 учебная группа", Selected=selectedGroup[1]},
                             new SelectListItem{ Value="3",Text="3 учебная группа", Selected=selectedGroup[2]},
                             new SelectListItem{ Value="4",Text="4 учебная группа", Selected=selectedGroup[3]},
                        };
                        ViewBag.Group = groupList1.ToList();
                        break;
                    case "2":
                        selected[2] = true;
                        if (user.Course == 1)
                        {
                            selectedGroup = new List<bool>() { false, false, false };
                            group = user.Group.Substring(user.Group.Length - 1);
                            switch (group)
                            {
                                case "1":
                                    selectedGroup[0] = true;
                                    break;
                                case "2":
                                    selectedGroup[1] = true;
                                    break;
                                case "3":
                                    selectedGroup[2] = true;
                                    break;
                            }
                            var groupList2 = new[]{
                             new SelectListItem{ Value="1",Text="1 учебная группа", Selected=selectedGroup[0]},
                             new SelectListItem{ Value="2",Text="2 учебная группа", Selected=selectedGroup[1]},
                             new SelectListItem{ Value="3",Text="3 учебная группа", Selected=selectedGroup[2]},
                            };
                            ViewBag.Group = groupList2.ToList();
                        }
                        else if (user.Course < 5)
                        {
                            selectedGroup = new List<bool>() { false, false, false, false };
                            group = user.Group.Substring(user.Group.Length - 1);
                            switch (group)
                            {
                                case "1":
                                    selectedGroup[0] = true;
                                    break;
                                case "2":
                                    selectedGroup[1] = true;
                                    break;
                                case "3":
                                    selectedGroup[2] = true;
                                    break;
                                case "4":
                                    selectedGroup[3] = true;
                                    break;
                            }
                            var groupList2 = new[]{
                             new SelectListItem{ Value="1",Text="1 учебная группа", Selected=selectedGroup[0]},
                             new SelectListItem{ Value="2",Text="2 учебная группа", Selected=selectedGroup[1]},
                             new SelectListItem{ Value="3",Text="3 учебная группа", Selected=selectedGroup[2]},
                             new SelectListItem{ Value="4",Text="4 учебная группа", Selected=selectedGroup[3]},
                            };
                            ViewBag.Group = groupList2.ToList();
                        }
                        else
                        {
                            user.Course++;
                            selectedGroup = new List<bool>() { false, false };
                            group = user.Group.Substring(user.Group.Length - 1);
                            switch (group)
                            {
                                case "1":
                                    selectedGroup[0] = true;
                                    break;
                                case "2":
                                    selectedGroup[1] = true;
                                    break;
                            }
                            var groupList2 = new[]{
                             new SelectListItem{ Value="1",Text="1 учебная группа", Selected=selectedGroup[0]},
                             new SelectListItem{ Value="2",Text="2 учебная группа", Selected=selectedGroup[1]},
                            };
                            ViewBag.Group = groupList2.ToList();
                        }
                        break;
                    default:
                        selected[0] = true;
                        if (user.Course == 1)
                        {
                            selectedGroup = new List<bool>() { false, false };
                            group = user.Group.Substring(user.Group.Length - 1);
                            switch (group)
                            {
                                case "2":
                                    selectedGroup[0] = true;
                                    break;
                                case "4":
                                    selectedGroup[1] = true;
                                    break;
                            }
                            var groupList3 = new[]{
                             new SelectListItem{ Value="2",Text="2 учебная группа", Selected=selectedGroup[0]},
                             new SelectListItem{ Value="4",Text="4 учебная группа", Selected=selectedGroup[1]},
                            };
                            ViewBag.Group = groupList3.ToList();
                        }
                        else if (user.Course == 4)
                        {
                            var groupList3 = new[]{
                             new SelectListItem{ Value="2",Text="2 учебная группа", Selected=true}
                            };
                            ViewBag.Group = groupList3.ToList();
                        }
                        else
                        {
                            selectedGroup = new List<bool>() { false, false };
                            group = user.Group.Substring(user.Group.Length - 1);
                            switch (group)
                            {
                                case "1":
                                    selectedGroup[0] = true;
                                    break;
                                case "2":
                                    selectedGroup[1] = true;
                                    break;
                            }
                            var groupList3 = new[]{
                             new SelectListItem{ Value="2-1",Text="2-1 учебная группа", Selected=selectedGroup[0]},
                             new SelectListItem{ Value="2-2",Text="2-2 учебная группа", Selected=selectedGroup[1]},
                            };
                            ViewBag.Group = groupList3.ToList();
                        }
                        break;
                }
                ViewBag.FinalGroup = user.Course + "" + user.Group;
            }
            var facultyList = new[]{
                     new SelectListItem{ Value="1",Text="Факультет пожарной безопасности", Selected=selected[1]},
                     new SelectListItem{ Value="2",Text="Факультет техносферной безопасности", Selected=selected[2]},
                     new SelectListItem{ Value="0",Text="Факультет платных образовательных услуг", Selected=selected[0]},
                 };
            ViewBag.Faculty = facultyList.ToList();

            ViewBag.Course = user.Course;
            var model = new IndexViewModel
            {
                Name = user.Name,
                SubName = user.SubName,
                Family = user.Family,
                Year = year,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(IndexViewModel model, string Faculty, string Group)
        {
            if (!ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                ViewBag.Avatar = "/Images/Avatars/" + user.Avatar;
                ViewBag.Course = user.Course;
                return View(model);
            }

            var result = await UpdateUserAsync(User.Identity.GetUserId(), model, Faculty + Group);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangeSuccess });
            }
            AddErrors(result); 
            return View(model);
        }
        private async Task<IdentityResult> UpdateUserAsync(string userId, IndexViewModel model, string Group)
        {
            try
            {
                ApplicationUser user = dbContext.Users.Find(userId);
                user.Name = model.Name;
                user.SubName = model.SubName;
                user.Family = model.Family;
                user.Group = user.Course + Group;
                user.Year = Convert.ToInt32(model.Year);
                DateTime entrance = DateTime.Parse("1 Sep " + user.Year);
                TimeSpan diff = DateTime.Now.Subtract(entrance);
                DateTime zeroTime = new DateTime(1, 1, 1);
                int course = 0;
                if (diff.Days > 0)
                {
                    //Григорианский календарь начинается с 1. 
                    //То должны вычитать 1, но т.к. начинаем мы не с 0 курса, а с первого то прибавляем 1.
                    //Итого получается, что мы не вычитаем и не прибавляем
                    course = (zeroTime + diff).Year;
                }
                user.Course = course;
                await dbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                return IdentityResult.Failed(exception.Message);
            }
            return IdentityResult.Success;
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }


        private const int AvatarStoredWidth = 400;  // ToDo - Change the size of the stored avatar image
        private const int AvatarStoredHeight = 400; // ToDo - Change the size of the stored avatar image
        private const int AvatarScreenWidth = 600;  // ToDo - Change the value of the width of the image on the screen

        private const string TempFolder = "/Temp";
        private const string MapTempFolder = "~" + TempFolder;
        private const string AvatarPath = "/Images/Avatars";

        private readonly string[] _imageFileExtensions = { ".jpg", ".png", ".gif", ".jpeg" };

        [HttpGet]
        public ActionResult AvatarUpload()
        {
            return PartialView();
        }
        [ValidateAntiForgeryToken]
        public ActionResult AvatarUpload(IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any()) return Json(new { success = false, errorMessage = "Фотография не загружена" });
            var file = files.FirstOrDefault();  // берем только одно первое
            if (file == null || !IsImage(file)) return Json(new { success = false, errorMessage = "Фотография имеет неверный формат" });
            if (file.ContentLength <= 0) return Json(new { success = false, errorMessage = "Размер фотографии не может быть нулевым" });
            var webPath = GetTempSavedFilePath(file);
            if (webPath == "false")
                return Json(new { success = false, errorMessage = "Фотография имеет неверный формат" });
            //Замена '\' на '//' для корректного изображения в firefox и IE,
            return Json(new { success = true, fileName = webPath.Replace("\\", "/") }); 
        }

        [HttpPost]
        public async Task<ActionResult> SaveAvatar(string t, string l, string h, string w, string fileName)
        {
            try
            {
                var top = Convert.ToInt32(t.Replace("-", "").Replace("px", ""));
                var left = Convert.ToInt32(l.Replace("-", "").Replace("px", ""));
                var height = Convert.ToInt32(h.Replace("-", "").Replace("px", ""));
                var width = Convert.ToInt32(w.Replace("-", "").Replace("px", ""));

                var fn = Path.Combine(Server.MapPath(MapTempFolder), Path.GetFileName(fileName));
                var img = new WebImage(fn);

                img.Resize(width, height);
                int bottom = img.Height - top - AvatarStoredHeight;
                int right = img.Width - left - AvatarStoredWidth;
                if (bottom < 0)
                {
                    top = top + bottom;
                    bottom = 0;
                }
                img.Crop(top, left, bottom, right);
                System.IO.File.Delete(fn);
                var FileName = Path.Combine(AvatarPath, Path.GetFileName(fn));
                var FileLocation = HttpContext.Server.MapPath(FileName);
                if (Directory.Exists(Path.GetDirectoryName(FileLocation)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FileLocation));
                }

                img.Save(FileLocation);
                MD5 md5 = MD5.Create();
                Stream file = System.IO.File.OpenRead(FileLocation);
                string newFile = BitConverter.ToString(md5.ComputeHash(file)).Replace("-", "").ToLower();
                string pathNewFile = HttpContext.Server.MapPath(AvatarPath + "\\" + newFile + Path.GetExtension(FileLocation));

                newFile = newFile + Path.GetExtension(FileLocation);
                if (System.IO.File.Exists(pathNewFile))
                    System.IO.File.Delete(pathNewFile);
                file.Close();
                System.IO.File.Move(FileLocation, pathNewFile);

                var user = dbContext.Users.Find(User.Identity.GetUserId());
                if (user != null)
                {
                    user.Avatar = newFile;
                    await dbContext.SaveChangesAsync();
                }
                return Json(new { success = true, avatarFileLocation = FileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = "Не удалось загрузить фотографию.\nERRORINFO: " + ex.Message });
            }
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("image") ||
                _imageFileExtensions.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        private string GetTempSavedFilePath(HttpPostedFileBase file)
        {
            var serverPath = HttpContext.Server.MapPath(TempFolder);
            if (Directory.Exists(serverPath) == false)
            {
                Directory.CreateDirectory(serverPath);
            }

            var fileName = Path.GetFileName(file.FileName);
            fileName = SaveTemporaryAvatarFileImage(file, serverPath, fileName);
            if (fileName == "false")
                return "false";
            CleanUpTempFolder(1);
            return Path.Combine(TempFolder, fileName);
        }

        private static string SaveTemporaryAvatarFileImage(HttpPostedFileBase file, string serverPath, string fileName)
        {
            WebImage img;
            try
            {
                img = new WebImage(file.InputStream);

                var ratio = img.Height / (double)img.Width;
                img.Resize(AvatarScreenWidth, (int)(AvatarScreenWidth * ratio));

                var fullFileName = Path.Combine(serverPath, fileName);
                if (System.IO.File.Exists(fullFileName))
                    System.IO.File.Delete(fullFileName);
                img.Save(fullFileName);
            }
            catch
            {
                return "false";
            }
            return Path.GetFileName(img.FileName);
        }

        private void CleanUpTempFolder(int hoursOld)
        {
            try
            {
                var currentUtcNow = DateTime.UtcNow;
                var serverPath = HttpContext.Server.MapPath(TempFolder);
                if (!Directory.Exists(serverPath)) return;
                var fileEntries = Directory.GetFiles(serverPath);
                foreach (var fileEntry in fileEntries)
                {
                    var fileCreationTime = System.IO.File.GetCreationTimeUtc(fileEntry);
                    var res = currentUtcNow - fileCreationTime;
                    if (res.TotalHours > hoursOld)
                    {
                        System.IO.File.Delete(fileEntry);
                    }
                }
            }
            catch
            {
                // Deliberately empty.
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            ChangeSuccess,
            NewUser,
            Error
        }

#endregion
    }
}