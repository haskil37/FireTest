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
        public async Task<ActionResult> Index(ManageMessageId? message, string NewFaculty)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : message == ManageMessageId.ChangeSuccess ? "Ваши данные сохранены."
                : "";
            ViewBag.Editable = true;
            if (!string.IsNullOrEmpty(NewFaculty))
                ViewBag.StatusMessage = NewFaculty;

            string UserId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(UserId);
            IndexViewModel model = new IndexViewModel
            {
                Name = user.Name,
                SubName = user.SubName,
                Family = user.Family,
                Avatar = "/Images/Avatars/" + user.Avatar,
                Year = user.Year != 0 ? user.Year.ToString() : "",
                Group = user.Group,
                Age = user.Age == 0 ? "" : user.Age.ToString(),
                Sex = user.Sex,
                //SpecialityOptions = Speciality(user.Speciality),
                FacultyOptions = Faculty(user.Faculty),
                RegionOptions = Region(user.Region)
            };

            if (user.Course == 100)
            {
                var role = dbContext.Users.Find(UserId).Roles.SingleOrDefault();
                if (dbContext.Roles.Find(role.RoleId).Name == "USER")
                {
                    model.Group = "Выпускник";
                    ViewBag.Editable = false;
                }
                else
                    model.Group = "Преподаватель";
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(IndexViewModel model)
        {
            string UserId = User.Identity.GetUserId();
            if (!ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(UserId);
                model.Avatar = "/Images/Avatars/" + user.Avatar;
                //model.SpecialityOptions = Speciality(user.Speciality);
                model.FacultyOptions = Faculty(user.Faculty);
                model.RegionOptions = Region(user.Region);

                ViewBag.Editable = true;
                if (user.Course == 100)
                {
                    var role = dbContext.Users.Find(UserId).Roles.SingleOrDefault();
                    if (dbContext.Roles.Find(role.RoleId).Name == "USER")
                    {
                        model.Group = "Выпускник";
                        ViewBag.Editable = false;
                    }
                    else
                        model.Group = "Преподаватель";
                }
                return View(model);
            }

            var result = await UpdateUserAsync(UserId, model);
            if (result.Succeeded)
            {
                var role = dbContext.Users.Find(User.Identity.GetUserId()).Roles.SingleOrDefault();
                if (dbContext.Roles.Find(role.RoleId).Name == "USER")
                {
                    var update = UpdateCourse(UserId);
                    if (!update.Succeeded)
                        return RedirectToAction("Index", new { Message = ManageMessageId.Error });
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangeSuccess });
            }
            AddErrors(result); 
            return View(model);
        }
        private IdentityResult UpdateCourse(string userID)
        {
            try
            {
                ApplicationUser user = dbContext.Users.Find(userID);
                TimeSpan diff = DateTime.Now - DateTime.Parse("1 Sep " + user.Year);
                DateTime zeroTime = new DateTime(1, 1, 1);
                if (diff.Days > 0)
                {
                    //Григорианский календарь начинается с 1. 
                    //То должны вычитать 1, но т.к. начинаем мы не с 0 курса, а с первого то прибавляем 1.
                    //Итого получается, что мы не вычитаем и не прибавляем
                    int course = (zeroTime + diff).Year;
                    user.Course = course;
                    //if (user.Group.Length > 1)
                    //{
                    //var vipusk = user.Group[1];
                    //if (vipusk == '1' && course >= 6)
                    //    user.Course = 100;
                    //if (vipusk == '2' && course >= 5)
                    //    user.Course = 100;
                    var vipusk = dbContext.Faculties.Find(Convert.ToInt32(user.Faculty)).Bachelor;
                    if (user.Master) //если пошел на магистра, то к выпуску добавляем срок для магистра
                        vipusk += dbContext.Faculties.Find(user.Faculty).Master;
                    if (course >= vipusk + 1)
                        user.Course = 100;

                    //}
                    //if (course > 6)
                    //    user.Course = 100;

                    if (user.Course != 100)
                        user.Group = user.Course + user.Group.Remove(0, 1);

                    dbContext.SaveChanges();
                }
            }
            catch (Exception exception)
            {
                return IdentityResult.Failed(exception.Message);
            }
            return IdentityResult.Success;
        }
        private async Task<IdentityResult> UpdateUserAsync(string userId, IndexViewModel model)
        {
            try
            {
                ApplicationUser user = dbContext.Users.Find(userId);
                user.Name = model.Name.Trim();
                user.SubName = model.SubName.Trim();
                user.Family = model.Family.Trim();
                user.Faculty = model.Faculty.Trim();
                //user.Speciality = model.Speciality.Trim();
                user.Group = model.Group.Trim();
                user.Year = Convert.ToInt32(model.Year.Trim());
                model.Age = model.Age.Trim();
                model.Age = model.Age.Replace("_", "");
                user.Age = Convert.ToInt32(model.Age);
                user.Sex = model.Sex;
                user.Region = model.Region.Trim();
                await dbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                return IdentityResult.Failed(exception.Message);
            }
            return IdentityResult.Success;
        }
        public ActionResult ChangePassword()
        {
            return View();
        }
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

        #region Аватар
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
        #endregion

        #region Вспомогательные приложения
        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            ChangeSuccess,
            Error
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
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        private List<SelectListItem> Region(string userRegion)
        {
            Dictionary<string, string> RegionDataBase = new Dictionary<string, string>
            {
                {"01","Республика Адыгея" },
                {"02, 102","Республика Башкортостан"},
                {"03","Республика Бурятия"},
                {"04","Республика Алтай (Горный Алтай)"},
                {"05","Республика Дагестан"},
                {"06","Республика Ингушетия"},
                {"07","Кабардино-Балкарская Республика"},
                {"08","Республика Калмыкия"},
                {"09","Республика Карачаево-Черкессия"},
                {"10","Республика Карелия"},
                {"11","Республика Коми"},
                {"82","Республика Крым"},
                {"12","Республика Марий Эл"},
                {"13, 113","Республика Мордовия"},
                {"14","Республика Саха (Якутия)"},
                {"15","Республика Северная Осетия-Алания"},
                {"16, 116","Республика Татарстан"},
                {"17","Республика Тыва"},
                {"18","Удмуртская Республика"},
                {"19","Республика Хакасия"},
                {"20","утилизировано (бывшая Чечня)"},
                {"21, 121","Чувашская Республика"},
                {"22","Алтайский край"},
                {"23, 93","Краснодарский край"},
                {"24, 84, 88, 124","Красноярский край"},
                {"25, 125","Приморский край"},
                {"26","Ставропольский край"},
                {"27","Хабаровский край"},
                {"28","Амурская область"},
                {"29","Архангельская область"},
                {"30","Астраханская область"},
                {"31","Белгородская область"},
                {"32","Брянская область"},
                {"33","Владимирская область"},
                {"34","Волгоградская область"},
                {"35","Вологодская область"},
                {"36","Воронежская область"},
                {"37","Ивановская область"},
                {"38, 85, 138","Иркутская область"},
                {"39, 91","Калининградская область"},
                {"40","Калужская область"},
                {"41, 82","Камчатский край"},
                {"42","Кемеровская область"},
                {"43","Кировская область"},
                {"44","Костромская область"},
                {"45","Курганская область"},
                {"46","Курская область"},
                {"47","Ленинградская область"},
                {"48","Липецкая область"},
                {"49","Магаданская область"},
                {"50, 90, 150,190","Московская область"},
                {"51","Мурманская область"},
                {"52, 152","Нижегородская область"},
                {"53","Новгородская область"},
                {"54, 154","Новосибирская область"},
                {"55","Омская область"},
                {"56","Оренбургская область"},
                {"57","Орловская область"},
                {"58","Пензенская область"},
                {"59, 81, 159","Пермский край"},
                {"60","Псковская область"},
                {"61, 161","Ростовская область"},
                {"62","Рязанская область"},
                {"63, 163","Самарская область"},
                {"64, 164","Саратовская область"},
                {"65","Сахалинская область"},
                {"66, 96","Свердловская область"},
                {"67","Смоленская область"},
                {"68","Тамбовская область"},
                {"69","Тверская область"},
                {"70","Томская область"},
                {"71","Тульская область"},
                {"72","Тюменская область"},
                {"73, 173","Ульяновская область"},
                {"74, 174","Челябинская область"},
                {"75, 80","Забайкальский край"},
                {"76","Ярославская область"},
                {"77, 97, 99, 177, 199, 197","г. Москва"},
                {"78, 98, 198","г. Санкт-Петербург"},
                {"79","Еврейская автономная область"},
                {"83","Ненецкий автономный округ"},
                {"86","Ханты-Мансийский автономный округ - Югра"},
                {"87","Чукотский автономный округ"},
                {"89","Ямало-Ненецкий автономный округ"},
                {"92","Севастополь"},
                {"94","Территории, которые находятся вне РФ и обслуживаются Департаментом режимных объектов МВД. Пример – Байконур"},
                {"95","Чеченская республика"},
            };
            List<SelectListItem> regionList = new List<SelectListItem>();
            foreach (var item in RegionDataBase)
            {
                if (item.Key == userRegion)
                    regionList.Add(new SelectListItem { Value = item.Key, Text = item.Value, Selected = true });
                else
                    regionList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            return regionList;
        }
        private List<SelectListItem> Faculty(string userFaculty)
        {
            List<SelectListItem> facultyList = new List<SelectListItem>();
            var allFaculties = dbContext.Faculties.ToList();
            foreach (var item in allFaculties)
            {
                if (item.Id.ToString() == userFaculty)
                    facultyList.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Name, Selected = true });
                else
                    facultyList.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Name });
            }
            return facultyList;

        }
        private List<SelectListItem> Speciality(string userSpeciality)
        {
            Dictionary<string, string> SpecialityDataBase = new Dictionary<string, string>
            {
                {"0","Пожарная безопасность"},
                {"1","Техносферная безопасность" },
            };
            List<SelectListItem> specialityList = new List<SelectListItem>();
            foreach (var item in SpecialityDataBase)
            {
                if (item.Key == userSpeciality)
                    specialityList.Add(new SelectListItem { Value = item.Key, Text = item.Value, Selected = true });
                else
                    specialityList.Add(new SelectListItem { Value = item.Key, Text = item.Value });
            }
            return specialityList;
        }
        #endregion
    }
}