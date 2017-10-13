using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using FireTest.Models;
using System.Text.RegularExpressions;

namespace FireTest.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string message = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            if (message != "")
                ViewBag.Message = message;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string login = model.Snils.Replace(" ", string.Empty);
            login = login.Replace("-", string.Empty);
            var result = await SignInManager.PasswordSignInAsync(login, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    if (UserManager.FindByName(login).EmailConfirmed)
                        return RedirectToLocal(returnUrl);
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    ModelState.AddModelError("", "Учетная запись не подтверждена. Если хотите получить письмо подтверждения повторно, то нажмите <a href='EmailReconfirm/" + UserManager.FindByName(login).Id + "'>здесь</a>.");
                    return View(model);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Неудачная попытка входа.");
                    return View(model);
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> EmailReconfirm(string id)
        {
            try
            {
                var user = UserManager.FindById(id);
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Подтверждение учетной записи", "Подтвердите вашу учетную запись, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Login");
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string login = model.Snils.Replace(" ", string.Empty);
                login = login.Replace("-", string.Empty);

                var confirmOrNo = await UserManager.FindByNameAsync(login);//Если юзер есть, но почта не активирована, можно удалить и создать заново
                if (confirmOrNo != null && !confirmOrNo.EmailConfirmed)
                    UserManager.Delete(confirmOrNo);

                var user = new ApplicationUser { UserName = login, Email = model.Email, Snils = model.Snils, Avatar = "NoAvatar.png", LastActivity = DateTime.Now, QualificationPoint = "_|_|_|_|_" };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Подтверждение учетной записи", "Подтвердите вашу учетную запись, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");
                    await UserManager.AddToRoleAsync(user.Id, "USER");
                    ViewBag.Mail = model.Email;
                    return View("DisplayEmail");
                }
                AddErrors(result, user.UserName); //Мой текст ошибки в регистрации
            }
            return View(model);
        }
        private void AddErrors(IdentityResult result, string user)
        {
            foreach (string error in result.Errors)
            {
                string localizedError = error;
                if (localizedError.Contains("Имя \"" + user + "\" уже используется."))
                    localizedError = "Введенный СНИЛС уже зарегистрирован.";
                ModelState.AddModelError("", localizedError);
            }
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return View("Error");

            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (!result.Succeeded)
                return View("Error");

            await SignInManager.SignInAsync(UserManager.FindById(userId), isPersistent: true, rememberBrowser: true);

            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string login = model.Snils.Replace(" ", string.Empty);
                login = login.Replace("-", string.Empty);
                var user = await UserManager.FindByNameAsync(login);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                    return View("ForgotPasswordConfirmation");

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Сброс пароля", "Сбросьте ваш пароль, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string login = model.Snils.Replace(" ", string.Empty);
            login = login.Replace("-", string.Empty);
            var user = await UserManager.FindByNameAsync(login);
            if (user == null)
                return RedirectToAction("Login", "Account", new { message = "Пароль сброшен" });

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
                return RedirectToAction("Login", "Account", new { message = "Пароль сброшен" });

            AddErrors(result);
            return View();
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}