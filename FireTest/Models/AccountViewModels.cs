using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FireTest.Models
{
    public class ValidateSnils : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string text = Regex.Replace(((string)value), @"[^\d]", "", RegexOptions.Compiled);
                decimal snils;
                bool intResultTryParse = decimal.TryParse(text, out snils);

                if (text.Length == 11 && intResultTryParse == true)
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult("Некорректный СНИЛС");
        }
    }

    public class LoginViewModel
    {
        [Required]
        [ValidateSnils]
        [StringLength(14, ErrorMessage = "СНИЛС должен содержать не менее 11 символов.", MinimumLength = 14)]
        [Display(Name = "СНИЛС")]
        public string Snils { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Пароль должен содержать не менее 8 символов.", MinimumLength = 8)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [ValidateSnils]
        [StringLength(14, ErrorMessage = "СНИЛС должен содержать не менее 11 символов.", MinimumLength = 14)]
        [Display(Name = "СНИЛС")]
        public string Snils { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Пароль должен содержать не менее 8 символов.", MinimumLength = 8)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [ValidateSnils]
        [StringLength(14, ErrorMessage = "СНИЛС должен содержать не менее 11 символов.", MinimumLength = 14)]
        [Display(Name = "СНИЛС")]
        public string Snils { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Пароль должен содержать не менее 8 символов.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [ValidateSnils]
        [StringLength(14, ErrorMessage = "СНИЛС должен содержать не менее 11 символов.", MinimumLength = 14)]
        [Display(Name = "СНИЛС")]
        public string Snils { get; set; }
    }
}