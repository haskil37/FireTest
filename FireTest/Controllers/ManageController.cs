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
                //: message == ManageMessageId.NewUser ? "Заполните свой профиль."
                : message == ManageMessageId.ChangeSuccess ? "Ваши данные сохранены."
                : "";

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            string year = "";
            if (user.Year != 0)
                year = user.Year.ToString();
            ViewBag.Avatar = "/Images/Avatars/" + user.Avatar;
            var role = dbContext.Users.Find(User.Identity.GetUserId()).Roles.SingleOrDefault();

            IndexViewModel model = new IndexViewModel();
            List<bool> selectedF = new List<bool>() { false, false, false };

            ViewBag.Editable = true;
            if (dbContext.Roles.Find(role.RoleId).Name == "USER" && !string.IsNullOrEmpty(user.Group))
            {
                if (!string.IsNullOrEmpty(user.Faculty))
                {
                    switch (user.Faculty)
                    {
                        case "0":
                            selectedF[0] = true;
                            break;
                        case "1":
                            selectedF[1] = true;
                            break;
                        case "2":
                            selectedF[2] = true;
                            break;
                    }
                }
                else
                    selectedF[1] = true;
                if (user.Course != 100)
                {
                    //if (user.Group.Substring(0, 1) == "2" && user.Course > 4)
                    //    ViewBag.FinalGroup = (user.Course + 1) + user.Group;
                    //else
                    //    ViewBag.FinalGroup = user.Course + user.Group;
                    //ViewBag.FinalGroup = user.Group;
                }
                else
                {
                    ViewBag.FinalGroup = "Выпускник";
                    ViewBag.Editable = false;
                }
                model = new IndexViewModel
                {
                    Name = user.Name,
                    SubName = user.SubName,
                    Family = user.Family,
                    Year = year,
                    //Group = user.Group.Remove(0, 1)
                    Group = user.Group,
                    Age = user.Age.ToString()
                };
            }
            else
            {
                if (!string.IsNullOrEmpty(user.Group))
                {
                    model = new IndexViewModel
                    {
                        Name = user.Name,
                        SubName = user.SubName,
                        Family = user.Family,
                        Year = year,
                        Group = user.Group,
                        Age = user.Age.ToString()
                    };
                    ViewBag.FinalGroup = "Преподаватель";
                }
            }

            ViewBag.SexM = false;
            ViewBag.SexF = false;
            if (user.Sex)
                ViewBag.SexM = true;
            else
                ViewBag.SexF = true;

            var facultyList = new[]{
                     new SelectListItem{ Value="1",Text="Факультет пожарной безопасности", Selected=selectedF[1]},
                     new SelectListItem{ Value="2",Text="Факультет техносферной безопасности", Selected=selectedF[2]},
                     new SelectListItem{ Value="0",Text="Факультет платных образовательных услуг", Selected=selectedF[0]},
                 };
            ViewBag.Faculty = facultyList.ToList();
            List<bool> selectedR = new List<bool>();
            for (int i = 0; i < 86; i++)
            {
                selectedR.Add(false);
            }
            switch (user.Region)
            {
                case "01":
                    selectedR[0] = true;
                    break;
                case "02, 102":
                    selectedR[1] = true;
                    break;
                case "03":
                    selectedR[2] = true;
                    break;
                case "04":
                    selectedR[3] = true;
                    break;
                case "05":
                    selectedR[4] = true;
                    break;
                case "06":
                    selectedR[5] = true;
                    break;
                case "07":
                    selectedR[6] = true;
                    break;
                case "08":
                    selectedR[7] = true;
                    break;
                case "09":
                    selectedR[8] = true;
                    break;
                case "10":
                    selectedR[9] = true;
                    break;
                case "11":
                    selectedR[10] = true;
                    break;
                case "12":
                    selectedR[11] = true;
                    break;
                case "13, 113":
                    selectedR[12] = true;
                    break;
                case "14":
                    selectedR[13] = true;
                    break;
                case "15":
                    selectedR[14] = true;
                    break;
                case "16, 116":
                    selectedR[15] = true;
                    break;
                case "17":
                    selectedR[16] = true;
                    break;
                case "18":
                    selectedR[17] = true;
                    break;
                case "19":
                    selectedR[18] = true;
                    break;
                case "20":
                    selectedR[19] = true;
                    break;
                case "21, 121":
                    selectedR[20] = true;
                    break;
                case "22":
                    selectedR[21] = true;
                    break;
                case "23, 93":
                    selectedR[22] = true;
                    break;
                case "24, 84, 88, 124":
                    selectedR[23] = true;
                    break;
                case "25, 125":
                    selectedR[24] = true;
                    break;
                case "26":
                    selectedR[25] = true;
                    break;
                case "27":
                    selectedR[26] = true;
                    break;
                case "28":
                    selectedR[27] = true;
                    break;
                case "29":
                    selectedR[28] = true;
                    break;
                case "30":
                    selectedR[29] = true;
                    break;
                case "31":
                    selectedR[30] = true;
                    break;
                case "32":
                    selectedR[31] = true;
                    break;
                case "33":
                    selectedR[32] = true;
                    break;
                case "34":
                    selectedR[33] = true;
                    break;
                case "35":
                    selectedR[34] = true;
                    break;
                case "36":
                    selectedR[35] = true;
                    break;
                case "37":
                    selectedR[36] = true;
                    break;
                case "38, 85, 138":
                    selectedR[37] = true;
                    break;
                case "39, 91":
                    selectedR[38] = true;
                    break;
                case "40":
                    selectedR[39] = true;
                    break;
                case "41, 82":
                    selectedR[40] = true;
                    break;
                case "42":
                    selectedR[41] = true;
                    break;
                case "43":
                    selectedR[42] = true;
                    break;
                case "44":
                    selectedR[43] = true;
                    break;
                case "45":
                    selectedR[44] = true;
                    break;
                case "46":
                    selectedR[45] = true;
                    break;
                case "47":
                    selectedR[46] = true;
                    break;
                case "48":
                    selectedR[47] = true;
                    break;
                case "49":
                    selectedR[48] = true;
                    break;
                case "50, 90, 150,190":
                    selectedR[49] = true;
                    break;
                case "51":
                    selectedR[50] = true;
                    break;
                case "52, 152":
                    selectedR[51] = true;
                    break;
                case "53":
                    selectedR[52] = true;
                    break;
                case "54, 154":
                    selectedR[53] = true;
                    break;
                case "55":
                    selectedR[54] = true;
                    break;
                case "56":
                    selectedR[55] = true;
                    break;
                case "57":
                    selectedR[56] = true;
                    break;
                case "58":
                    selectedR[57] = true;
                    break;
                case "59, 81, 159":
                    selectedR[58] = true;
                    break;
                case "60":
                    selectedR[59] = true;
                    break;
                case "61, 161":
                    selectedR[60] = true;
                    break;
                case "62":
                    selectedR[61] = true;
                    break;
                case "63, 163":
                    selectedR[62] = true;
                    break;
                case "64, 164":
                    selectedR[63] = true;
                    break;
                case "65":
                    selectedR[64] = true;
                    break;
                case "66, 96":
                    selectedR[65] = true;
                    break;
                case "67":
                    selectedR[66] = true;
                    break;
                case "68":
                    selectedR[67] = true;
                    break;
                case "69":
                    selectedR[68] = true;
                    break;
                case "70":
                    selectedR[69] = true;
                    break;
                case "71":
                    selectedR[70] = true;
                    break;
                case "72":
                    selectedR[71] = true;
                    break;
                case "73, 173":
                    selectedR[72] = true;
                    break;
                case "74, 174":
                    selectedR[73] = true;
                    break;
                case "75, 80":
                    selectedR[74] = true;
                    break;
                case "76":
                    selectedR[75] = true;
                    break;
                case "77, 97, 99, 177, 199, 197":
                    selectedR[76] = true;
                    break;
                case "78, 98, 198":
                    selectedR[77] = true;
                    break;
                case "79":
                    selectedR[78] = true;
                    break;
                case "83":
                    selectedR[79] = true;
                    break;
                case "86":
                    selectedR[80] = true;
                    break;
                case "87":
                    selectedR[81] = true;
                    break;
                case "89":
                    selectedR[82] = true;
                    break;
                case "92":
                    selectedR[83] = true;
                    break;
                case "94":
                    selectedR[84] = true;
                    break;
                case "95":
                    selectedR[85] = true;
                    break;
            }

            var regionList = new[]{
                    new SelectListItem{ Value="01",Text="Республика Адыгея", Selected=selectedR[0]},
                    new SelectListItem{ Value="02, 102",Text="Республика Башкортостан", Selected=selectedR[1]},
                    new SelectListItem{ Value="03",Text="Республика Бурятия", Selected=selectedR[2]},
                    new SelectListItem{ Value="04",Text="Республика Алтай (Горный Алтай)", Selected=selectedR[3]},
                    new SelectListItem{ Value="05",Text="Республика Дагестан", Selected=selectedR[4]},
                    new SelectListItem{ Value="06",Text="Республика Ингушетия", Selected=selectedR[5]},
                    new SelectListItem{ Value="07",Text="Кабардино-Балкарская Республика", Selected=selectedR[6]},
                    new SelectListItem{ Value="08",Text="Республика Калмыкия", Selected=selectedR[7]},
                    new SelectListItem{ Value="09",Text="Республика Карачаево-Черкессия", Selected=selectedR[8]},
                    new SelectListItem{ Value="10",Text="Республика Карелия", Selected=selectedR[9]},
                    new SelectListItem{ Value="11",Text="Республика Коми", Selected=selectedR[10]},
                    new SelectListItem{ Value="12",Text="Республика Марий Эл", Selected=selectedR[11]},
                    new SelectListItem{ Value="13, 113",Text="Республика Мордовия", Selected=selectedR[12]},
                    new SelectListItem{ Value="14",Text="Республика Саха (Якутия)", Selected=selectedR[13]},
                    new SelectListItem{ Value="15",Text="Республика Северная Осетия-Алания", Selected=selectedR[14]},
                    new SelectListItem{ Value="16, 116",Text="Республика Татарстан", Selected=selectedR[15]},
                    new SelectListItem{ Value="17",Text="Республика Тыва", Selected=selectedR[16]},
                    new SelectListItem{ Value="18",Text="Удмуртская Республика", Selected=selectedR[17]},
                    new SelectListItem{ Value="19",Text="Республика Хакасия", Selected=selectedR[18]},
                    new SelectListItem{ Value="20",Text="утилизировано (бывшая Чечня)", Selected=selectedR[19]},
                    new SelectListItem{ Value="21, 121",Text="Чувашская Республика", Selected=selectedR[20]},
                    new SelectListItem{ Value="22",Text="Алтайский край", Selected=selectedR[21]},
                    new SelectListItem{ Value="23, 93",Text="Краснодарский край", Selected=selectedR[22]},
                    new SelectListItem{ Value="24, 84, 88, 124",Text="Красноярский край", Selected=selectedR[23]},
                    new SelectListItem{ Value="25, 125",Text="Приморский край", Selected=selectedR[24]},
                    new SelectListItem{ Value="26",Text="Ставропольский край", Selected=selectedR[25]},
                    new SelectListItem{ Value="27",Text="Хабаровский край", Selected=selectedR[26]},
                    new SelectListItem{ Value="28",Text="Амурская область", Selected=selectedR[27]},
                    new SelectListItem{ Value="29",Text="Архангельская область", Selected=selectedR[28]},
                    new SelectListItem{ Value="30",Text="Астраханская область", Selected=selectedR[29]},
                    new SelectListItem{ Value="31",Text="Белгородская область", Selected=selectedR[30]},
                    new SelectListItem{ Value="32",Text="Брянская область", Selected=selectedR[31]},
                    new SelectListItem{ Value="33",Text="Владимирская область", Selected=selectedR[32]},
                    new SelectListItem{ Value="34",Text="Волгоградская область", Selected=selectedR[33]},
                    new SelectListItem{ Value="35",Text="Вологодская область", Selected=selectedR[34]},
                    new SelectListItem{ Value="36",Text="Воронежская область", Selected=selectedR[35]},
                    new SelectListItem{ Value="37",Text="Ивановская область", Selected=selectedR[36]},
                    new SelectListItem{ Value="38, 85, 138",Text="Иркутская область", Selected=selectedR[37]},
                    new SelectListItem{ Value="39, 91",Text="Калининградская область", Selected=selectedR[38]},
                    new SelectListItem{ Value="40",Text="Калужская область", Selected=selectedR[39]},
                    new SelectListItem{ Value="41, 82",Text="Камчатский край", Selected=selectedR[40]},
                    new SelectListItem{ Value="42",Text="Кемеровская область", Selected=selectedR[41]},
                    new SelectListItem{ Value="43",Text="Кировская область", Selected=selectedR[42]},
                    new SelectListItem{ Value="44",Text="Костромская область", Selected=selectedR[43]},
                    new SelectListItem{ Value="45",Text="Курганская область", Selected=selectedR[44]},
                    new SelectListItem{ Value="46",Text="Курская область", Selected=selectedR[45]},
                    new SelectListItem{ Value="47",Text="Ленинградская область", Selected=selectedR[46]},
                    new SelectListItem{ Value="48",Text="Липецкая область", Selected=selectedR[47]},
                    new SelectListItem{ Value="49",Text="Магаданская область", Selected=selectedR[48]},
                    new SelectListItem{ Value="50, 90, 150,190",Text="Московская область", Selected=selectedR[49]},
                    new SelectListItem{ Value="51",Text="Мурманская область", Selected=selectedR[50]},
                    new SelectListItem{ Value="52, 152",Text="Нижегородская область", Selected=selectedR[51]},
                    new SelectListItem{ Value="53",Text="Новгородская область", Selected=selectedR[52]},
                    new SelectListItem{ Value="54, 154",Text="Новосибирская область", Selected=selectedR[53]},
                    new SelectListItem{ Value="55",Text="Омская область", Selected=selectedR[54]},
                    new SelectListItem{ Value="56",Text="Оренбургская область", Selected=selectedR[55]},
                    new SelectListItem{ Value="57",Text="Орловская область", Selected=selectedR[56]},
                    new SelectListItem{ Value="58",Text="Пензенская область", Selected=selectedR[57]},
                    new SelectListItem{ Value="59, 81, 159",Text="Пермский край", Selected=selectedR[58]},
                    new SelectListItem{ Value="60",Text="Псковская область", Selected=selectedR[59]},
                    new SelectListItem{ Value="61, 161",Text="Ростовская область", Selected=selectedR[60]},
                    new SelectListItem{ Value="62",Text="Рязанская область", Selected=selectedR[61]},
                    new SelectListItem{ Value="63, 163",Text="Самарская область", Selected=selectedR[62]},
                    new SelectListItem{ Value="64, 164",Text="Саратовская область", Selected=selectedR[63]},
                    new SelectListItem{ Value="65",Text="Сахалинская область", Selected=selectedR[64]},
                    new SelectListItem{ Value="66, 96",Text="Свердловская область", Selected=selectedR[65]},
                    new SelectListItem{ Value="67",Text="Смоленская область", Selected=selectedR[66]},
                    new SelectListItem{ Value="68",Text="Тамбовская область", Selected=selectedR[67]},
                    new SelectListItem{ Value="69",Text="Тверская область", Selected=selectedR[68]},
                    new SelectListItem{ Value="70",Text="Томская область", Selected=selectedR[69]},
                    new SelectListItem{ Value="71",Text="Тульская область", Selected=selectedR[70]},
                    new SelectListItem{ Value="72",Text="Тюменская область", Selected=selectedR[71]},
                    new SelectListItem{ Value="73, 173",Text="Ульяновская область", Selected=selectedR[72]},
                    new SelectListItem{ Value="74, 174",Text="Челябинская область", Selected=selectedR[73]},
                    new SelectListItem{ Value="75, 80",Text="Забайкальский край", Selected=selectedR[74]},
                    new SelectListItem{ Value="76",Text="Ярославская область", Selected=selectedR[75]},
                    new SelectListItem{ Value="77, 97, 99, 177, 199, 197",Text="г. Москва", Selected=selectedR[76]},
                    new SelectListItem{ Value="78, 98, 198",Text="г. Санкт-Петербург", Selected=selectedR[77]},
                    new SelectListItem{ Value="79",Text="Еврейская автономная область", Selected=selectedR[78]},
                    new SelectListItem{ Value="83",Text="Ненецкий автономный округ", Selected=selectedR[79]},
                    new SelectListItem{ Value="86",Text="Ханты-Мансийский автономный округ - Югра", Selected=selectedR[80]},
                    new SelectListItem{ Value="87",Text="Чукотский автономный округ", Selected=selectedR[81]},
                    new SelectListItem{ Value="89",Text="Ямало-Ненецкий автономный округ", Selected=selectedR[82]},
                    new SelectListItem{ Value="92",Text="Резерв МВД Российской Федерации", Selected=selectedR[83]},
                    new SelectListItem{ Value="94",Text="Территории, которые находятся вне РФ и обслуживаются Департаментом режимных объектов МВД. Пример – Байконур", Selected=selectedR[84]},
                    new SelectListItem{ Value="95",Text="Чеченская республика", Selected=selectedR[85]},
                 };
            ViewBag.Region = regionList.ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(IndexViewModel model, string Faculty)
        {

            if (!ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                ViewBag.Avatar = "/Images/Avatars/" + user.Avatar;
                List<bool> selectedF = new List<bool>() { false, false, false };
                List<bool> selectedR = new List<bool>();
                for (int i = 0; i < 86; i++)
                {
                    selectedR.Add(false);
                }
                if (!string.IsNullOrEmpty(user.Group))
                {
                    if (!string.IsNullOrEmpty(user.Faculty))
                    {
                        switch (user.Faculty)
                        {
                            case "0":
                                selectedF[0] = true;
                                break;
                            case "1":
                                selectedF[1] = true;
                                break;
                            case "2":
                                selectedF[2] = true;
                                break;
                        }
                    }
                    else
                        selectedF[1] = true;
                }

                ViewBag.SexM = false;
                ViewBag.SexF = false;
                if (user.Sex)
                    ViewBag.SexM = true;
                else
                    ViewBag.SexF = true;

                string year = "";
                if (user.Year != 0)
                    year = user.Year.ToString();

                var role = dbContext.Users.Find(User.Identity.GetUserId()).Roles.SingleOrDefault();
                ViewBag.Editable = true;
                if (dbContext.Roles.Find(role.RoleId).Name == "USER" && !string.IsNullOrEmpty(user.Group))
                {
                    if (!string.IsNullOrEmpty(user.Faculty))
                    {
                        switch (user.Faculty)
                        {
                            case "0":
                                selectedF[0] = true;
                                break;
                            case "1":
                                selectedF[1] = true;
                                break;
                            case "2":
                                selectedF[2] = true;
                                break;
                        }
                    }
                    else
                        selectedF[1] = true;

                    if (user.Course != 100)
                    {
                        //if (user.Group.Substring(0, 1) == "2" && user.Course > 4)
                        //    ViewBag.FinalGroup = (user.Course + 1) + user.Group;
                        //else
                        //    ViewBag.FinalGroup = user.Course + user.Group;
                        //ViewBag.FinalGroup = user.Group;
                    }
                    else
                    {
                        ViewBag.FinalGroup = "Выпускник";
                        ViewBag.Editable = false;
                    }
                    model = new IndexViewModel
                    {
                        Name = user.Name,
                        SubName = user.SubName,
                        Family = user.Family,
                        Year = year,
                        //Group = user.Group.Remove(0, 1)
                        Group = user.Group,
                        Age = user.Age.ToString()
                    };
                }
                else
                {
                    if (!string.IsNullOrEmpty(user.Group))
                    {
                        model = new IndexViewModel
                        {
                            Name = user.Name,
                            SubName = user.SubName,
                            Family = user.Family,
                            Year = year,
                            Group = user.Group,
                            Age = user.Age.ToString()
                        };
                        ViewBag.FinalGroup = "Преподаватель";
                    }
                }






                ViewBag.Faculty = new[]{
                     new SelectListItem{ Value="1",Text="Факультет пожарной безопасности", Selected=selectedF[1]},
                     new SelectListItem{ Value="2",Text="Факультет техносферной безопасности", Selected=selectedF[2]},
                     new SelectListItem{ Value="0",Text="Факультет платных образовательных услуг", Selected=selectedF[0]},
                 }.ToList();
                //ViewBag.Faculty = facultyList.ToList();

                if (!string.IsNullOrEmpty(user.Region))
                {

                    switch (user.Region)
                    {
                        case "01":
                            selectedR[0] = true;
                            break;
                        case "02, 102":
                            selectedR[1] = true;
                            break;
                        case "03":
                            selectedR[2] = true;
                            break;
                        case "04":
                            selectedR[3] = true;
                            break;
                        case "05":
                            selectedR[4] = true;
                            break;
                        case "06":
                            selectedR[5] = true;
                            break;
                        case "07":
                            selectedR[6] = true;
                            break;
                        case "08":
                            selectedR[7] = true;
                            break;
                        case "09":
                            selectedR[8] = true;
                            break;
                        case "10":
                            selectedR[9] = true;
                            break;
                        case "11":
                            selectedR[10] = true;
                            break;
                        case "12":
                            selectedR[11] = true;
                            break;
                        case "13, 113":
                            selectedR[12] = true;
                            break;
                        case "14":
                            selectedR[13] = true;
                            break;
                        case "15":
                            selectedR[14] = true;
                            break;
                        case "16, 116":
                            selectedR[15] = true;
                            break;
                        case "17":
                            selectedR[16] = true;
                            break;
                        case "18":
                            selectedR[17] = true;
                            break;
                        case "19":
                            selectedR[18] = true;
                            break;
                        case "20":
                            selectedR[19] = true;
                            break;
                        case "21, 121":
                            selectedR[20] = true;
                            break;
                        case "22":
                            selectedR[21] = true;
                            break;
                        case "23, 93":
                            selectedR[22] = true;
                            break;
                        case "24, 84, 88, 124":
                            selectedR[23] = true;
                            break;
                        case "25, 125":
                            selectedR[24] = true;
                            break;
                        case "26":
                            selectedR[25] = true;
                            break;
                        case "27":
                            selectedR[26] = true;
                            break;
                        case "28":
                            selectedR[27] = true;
                            break;
                        case "29":
                            selectedR[28] = true;
                            break;
                        case "30":
                            selectedR[29] = true;
                            break;
                        case "31":
                            selectedR[30] = true;
                            break;
                        case "32":
                            selectedR[31] = true;
                            break;
                        case "33":
                            selectedR[32] = true;
                            break;
                        case "34":
                            selectedR[33] = true;
                            break;
                        case "35":
                            selectedR[34] = true;
                            break;
                        case "36":
                            selectedR[35] = true;
                            break;
                        case "37":
                            selectedR[36] = true;
                            break;
                        case "38, 85, 138":
                            selectedR[37] = true;
                            break;
                        case "39, 91":
                            selectedR[38] = true;
                            break;
                        case "40":
                            selectedR[39] = true;
                            break;
                        case "41, 82":
                            selectedR[40] = true;
                            break;
                        case "42":
                            selectedR[41] = true;
                            break;
                        case "43":
                            selectedR[42] = true;
                            break;
                        case "44":
                            selectedR[43] = true;
                            break;
                        case "45":
                            selectedR[44] = true;
                            break;
                        case "46":
                            selectedR[45] = true;
                            break;
                        case "47":
                            selectedR[46] = true;
                            break;
                        case "48":
                            selectedR[47] = true;
                            break;
                        case "49":
                            selectedR[48] = true;
                            break;
                        case "50, 90, 150,190":
                            selectedR[49] = true;
                            break;
                        case "51":
                            selectedR[50] = true;
                            break;
                        case "52, 152":
                            selectedR[51] = true;
                            break;
                        case "53":
                            selectedR[52] = true;
                            break;
                        case "54, 154":
                            selectedR[53] = true;
                            break;
                        case "55":
                            selectedR[54] = true;
                            break;
                        case "56":
                            selectedR[55] = true;
                            break;
                        case "57":
                            selectedR[56] = true;
                            break;
                        case "58":
                            selectedR[57] = true;
                            break;
                        case "59, 81, 159":
                            selectedR[58] = true;
                            break;
                        case "60":
                            selectedR[59] = true;
                            break;
                        case "61, 161":
                            selectedR[60] = true;
                            break;
                        case "62":
                            selectedR[61] = true;
                            break;
                        case "63, 163":
                            selectedR[62] = true;
                            break;
                        case "64, 164":
                            selectedR[63] = true;
                            break;
                        case "65":
                            selectedR[64] = true;
                            break;
                        case "66, 96":
                            selectedR[65] = true;
                            break;
                        case "67":
                            selectedR[66] = true;
                            break;
                        case "68":
                            selectedR[67] = true;
                            break;
                        case "69":
                            selectedR[68] = true;
                            break;
                        case "70":
                            selectedR[69] = true;
                            break;
                        case "71":
                            selectedR[70] = true;
                            break;
                        case "72":
                            selectedR[71] = true;
                            break;
                        case "73, 173":
                            selectedR[72] = true;
                            break;
                        case "74, 174":
                            selectedR[73] = true;
                            break;
                        case "75, 80":
                            selectedR[74] = true;
                            break;
                        case "76":
                            selectedR[75] = true;
                            break;
                        case "77, 97, 99, 177, 199, 197":
                            selectedR[76] = true;
                            break;
                        case "78, 98, 198":
                            selectedR[77] = true;
                            break;
                        case "79":
                            selectedR[78] = true;
                            break;
                        case "83":
                            selectedR[79] = true;
                            break;
                        case "86":
                            selectedR[80] = true;
                            break;
                        case "87":
                            selectedR[81] = true;
                            break;
                        case "89":
                            selectedR[82] = true;
                            break;
                        case "92":
                            selectedR[83] = true;
                            break;
                        case "94":
                            selectedR[84] = true;
                            break;
                        case "95":
                            selectedR[85] = true;
                            break;
                    }
                }
                ViewBag.Region = new[]{
                    new SelectListItem{ Value="01",Text="Республика Адыгея", Selected=selectedR[0]},
                    new SelectListItem{ Value="02, 102",Text="Республика Башкортостан", Selected=selectedR[1]},
                    new SelectListItem{ Value="03",Text="Республика Бурятия", Selected=selectedR[2]},
                    new SelectListItem{ Value="04",Text="Республика Алтай (Горный Алтай)", Selected=selectedR[3]},
                    new SelectListItem{ Value="05",Text="Республика Дагестан", Selected=selectedR[4]},
                    new SelectListItem{ Value="06",Text="Республика Ингушетия", Selected=selectedR[5]},
                    new SelectListItem{ Value="07",Text="Кабардино-Балкарская Республика", Selected=selectedR[6]},
                    new SelectListItem{ Value="08",Text="Республика Калмыкия", Selected=selectedR[7]},
                    new SelectListItem{ Value="09",Text="Республика Карачаево-Черкессия", Selected=selectedR[8]},
                    new SelectListItem{ Value="10",Text="Республика Карелия", Selected=selectedR[9]},
                    new SelectListItem{ Value="11",Text="Республика Коми", Selected=selectedR[10]},
                    new SelectListItem{ Value="12",Text="Республика Марий Эл", Selected=selectedR[11]},
                    new SelectListItem{ Value="13, 113",Text="Республика Мордовия", Selected=selectedR[12]},
                    new SelectListItem{ Value="14",Text="Республика Саха (Якутия)", Selected=selectedR[13]},
                    new SelectListItem{ Value="15",Text="Республика Северная Осетия-Алания", Selected=selectedR[14]},
                    new SelectListItem{ Value="16, 116",Text="Республика Татарстан", Selected=selectedR[15]},
                    new SelectListItem{ Value="17",Text="Республика Тыва", Selected=selectedR[16]},
                    new SelectListItem{ Value="18",Text="Удмуртская Республика", Selected=selectedR[17]},
                    new SelectListItem{ Value="19",Text="Республика Хакасия", Selected=selectedR[18]},
                    new SelectListItem{ Value="20",Text="утилизировано (бывшая Чечня)", Selected=selectedR[19]},
                    new SelectListItem{ Value="21, 121",Text="Чувашская Республика", Selected=selectedR[20]},
                    new SelectListItem{ Value="22",Text="Алтайский край", Selected=selectedR[21]},
                    new SelectListItem{ Value="23, 93",Text="Краснодарский край", Selected=selectedR[22]},
                    new SelectListItem{ Value="24, 84, 88, 124",Text="Красноярский край", Selected=selectedR[23]},
                    new SelectListItem{ Value="25, 125",Text="Приморский край", Selected=selectedR[24]},
                    new SelectListItem{ Value="26",Text="Ставропольский край", Selected=selectedR[25]},
                    new SelectListItem{ Value="27",Text="Хабаровский край", Selected=selectedR[26]},
                    new SelectListItem{ Value="28",Text="Амурская область", Selected=selectedR[27]},
                    new SelectListItem{ Value="29",Text="Архангельская область", Selected=selectedR[28]},
                    new SelectListItem{ Value="30",Text="Астраханская область", Selected=selectedR[29]},
                    new SelectListItem{ Value="31",Text="Белгородская область", Selected=selectedR[30]},
                    new SelectListItem{ Value="32",Text="Брянская область", Selected=selectedR[31]},
                    new SelectListItem{ Value="33",Text="Владимирская область", Selected=selectedR[32]},
                    new SelectListItem{ Value="34",Text="Волгоградская область", Selected=selectedR[33]},
                    new SelectListItem{ Value="35",Text="Вологодская область", Selected=selectedR[34]},
                    new SelectListItem{ Value="36",Text="Воронежская область", Selected=selectedR[35]},
                    new SelectListItem{ Value="37",Text="Ивановская область", Selected=selectedR[36]},
                    new SelectListItem{ Value="38, 85, 138",Text="Иркутская область", Selected=selectedR[37]},
                    new SelectListItem{ Value="39, 91",Text="Калининградская область", Selected=selectedR[38]},
                    new SelectListItem{ Value="40",Text="Калужская область", Selected=selectedR[39]},
                    new SelectListItem{ Value="41, 82",Text="Камчатский край", Selected=selectedR[40]},
                    new SelectListItem{ Value="42",Text="Кемеровская область", Selected=selectedR[41]},
                    new SelectListItem{ Value="43",Text="Кировская область", Selected=selectedR[42]},
                    new SelectListItem{ Value="44",Text="Костромская область", Selected=selectedR[43]},
                    new SelectListItem{ Value="45",Text="Курганская область", Selected=selectedR[44]},
                    new SelectListItem{ Value="46",Text="Курская область", Selected=selectedR[45]},
                    new SelectListItem{ Value="47",Text="Ленинградская область", Selected=selectedR[46]},
                    new SelectListItem{ Value="48",Text="Липецкая область", Selected=selectedR[47]},
                    new SelectListItem{ Value="49",Text="Магаданская область", Selected=selectedR[48]},
                    new SelectListItem{ Value="50, 90, 150,190",Text="Московская область", Selected=selectedR[49]},
                    new SelectListItem{ Value="51",Text="Мурманская область", Selected=selectedR[50]},
                    new SelectListItem{ Value="52, 152",Text="Нижегородская область", Selected=selectedR[51]},
                    new SelectListItem{ Value="53",Text="Новгородская область", Selected=selectedR[52]},
                    new SelectListItem{ Value="54, 154",Text="Новосибирская область", Selected=selectedR[53]},
                    new SelectListItem{ Value="55",Text="Омская область", Selected=selectedR[54]},
                    new SelectListItem{ Value="56",Text="Оренбургская область", Selected=selectedR[55]},
                    new SelectListItem{ Value="57",Text="Орловская область", Selected=selectedR[56]},
                    new SelectListItem{ Value="58",Text="Пензенская область", Selected=selectedR[57]},
                    new SelectListItem{ Value="59, 81, 159",Text="Пермский край", Selected=selectedR[58]},
                    new SelectListItem{ Value="60",Text="Псковская область", Selected=selectedR[59]},
                    new SelectListItem{ Value="61, 161",Text="Ростовская область", Selected=selectedR[60]},
                    new SelectListItem{ Value="62",Text="Рязанская область", Selected=selectedR[61]},
                    new SelectListItem{ Value="63, 163",Text="Самарская область", Selected=selectedR[62]},
                    new SelectListItem{ Value="64, 164",Text="Саратовская область", Selected=selectedR[63]},
                    new SelectListItem{ Value="65",Text="Сахалинская область", Selected=selectedR[64]},
                    new SelectListItem{ Value="66, 96",Text="Свердловская область", Selected=selectedR[65]},
                    new SelectListItem{ Value="67",Text="Смоленская область", Selected=selectedR[66]},
                    new SelectListItem{ Value="68",Text="Тамбовская область", Selected=selectedR[67]},
                    new SelectListItem{ Value="69",Text="Тверская область", Selected=selectedR[68]},
                    new SelectListItem{ Value="70",Text="Томская область", Selected=selectedR[69]},
                    new SelectListItem{ Value="71",Text="Тульская область", Selected=selectedR[70]},
                    new SelectListItem{ Value="72",Text="Тюменская область", Selected=selectedR[71]},
                    new SelectListItem{ Value="73, 173",Text="Ульяновская область", Selected=selectedR[72]},
                    new SelectListItem{ Value="74, 174",Text="Челябинская область", Selected=selectedR[73]},
                    new SelectListItem{ Value="75, 80",Text="Забайкальский край", Selected=selectedR[74]},
                    new SelectListItem{ Value="76",Text="Ярославская область", Selected=selectedR[75]},
                    new SelectListItem{ Value="77, 97, 99, 177, 199, 197",Text="г. Москва", Selected=selectedR[76]},
                    new SelectListItem{ Value="78, 98, 198",Text="г. Санкт-Петербург", Selected=selectedR[77]},
                    new SelectListItem{ Value="79",Text="Еврейская автономная область", Selected=selectedR[78]},
                    new SelectListItem{ Value="83",Text="Ненецкий автономный округ", Selected=selectedR[79]},
                    new SelectListItem{ Value="86",Text="Ханты-Мансийский автономный округ - Югра", Selected=selectedR[80]},
                    new SelectListItem{ Value="87",Text="Чукотский автономный округ", Selected=selectedR[81]},
                    new SelectListItem{ Value="89",Text="Ямало-Ненецкий автономный округ", Selected=selectedR[82]},
                    new SelectListItem{ Value="92",Text="Резерв МВД Российской Федерации", Selected=selectedR[83]},
                    new SelectListItem{ Value="94",Text="Территории, которые находятся вне РФ и обслуживаются Департаментом режимных объектов МВД. Пример – Байконур", Selected=selectedR[84]},
                    new SelectListItem{ Value="95",Text="Чеченская республика", Selected=selectedR[85]},
                 }.ToList();
                //ViewBag.Region = regionList.ToList();

                return View(model);
            }

            string userId = User.Identity.GetUserId();
            var result = await UpdateUserAsync(userId, model, Faculty);
            if (result.Succeeded)
            {
                var update = UpdateCourse(userId);
                if (!update.Succeeded)
                    return RedirectToAction("Index");
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
                DateTime entrance = DateTime.Parse("1 Sep " + user.Year);
                TimeSpan diff = DateTime.Now.Subtract(entrance);
                DateTime zeroTime = new DateTime(1, 1, 1);
                if (diff.Days > 0)
                {
                    //Григорианский календарь начинается с 1. 
                    //То должны вычитать 1, но т.к. начинаем мы не с 0 курса, а с первого то прибавляем 1.
                    //Итого получается, что мы не вычитаем и не прибавляем
                    int course = (zeroTime + diff).Year;
                    if (course <= 6)
                    {
                        if (user.Group.Substring(1, 1) == "1") //Если ПБ
                        {
                            if (course < 6)
                                user.Course = course;
                            else
                                user.Course = 100;
                        }
                        if (user.Group.Substring(1, 1) == "2") //Если ТБ
                            user.Course = course;
                        if (user.Group.Substring(1, 1) == "0") //Если платно
                        {
                            if (course < 5)
                                user.Course = course;
                            else
                                user.Course = 100;
                        }
                    }
                    else
                        user.Course = 100;
                    dbContext.SaveChanges();
                }
            }
            catch (Exception exception)
            {
                return IdentityResult.Failed(exception.Message);
            }
            return IdentityResult.Success;
        }

        private async Task<IdentityResult> UpdateUserAsync(string userId, IndexViewModel model, string Faculty)
        {
            try
            {
                ApplicationUser user = dbContext.Users.Find(userId);
                user.Name = model.Name.Trim();
                user.SubName = model.SubName.Trim();
                user.Family = model.Family.Trim();
                user.Faculty = Faculty;
                //user.Group = Faculty + model.Group.Trim();
                user.Group = model.Group.Trim();
                user.Year = Convert.ToInt32(model.Year.Trim());
                model.Age = model.Age.Trim();
                model.Age = model.Age.Replace("_", "");
                user.Age = Convert.ToInt32(model.Age);
                user.Sex = Convert.ToBoolean(model.Sex.Trim());
                user.Region = model.Region.Trim();
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