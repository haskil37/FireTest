using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace FireTest.Models
{
    public class IndexViewModel
    {
        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Отчество")]
        public string SubName { get; set; }
        [Required]
        [Display(Name = "Фамилия")]
        public string Family { get; set; }
        [Required]
        [StringLength(4, ErrorMessage = "Год поступления должен состоять из 4 цифр.", MinimumLength = 4)]
        [Display(Name = "Год поступления")]
        public string Year { get; set; }
        [Required]
        [Display(Name = "Учебная группа")]
        public string Group { get; set; }
        [Required]
        [StringLength(4, ErrorMessage = "Год рождения должен состоять из 4 цифр.", MinimumLength = 4)]
        [Display(Name = "Год рождения")]
        public string Age { get; set; }
        [Required]
        [Display(Name = "Пол")]
        public string Sex { get; set; }
        [Required]
        [Display(Name = "Регион поступления")]
        public string Region { get; set; }
        [Required]
        [Display(Name = "Специальность")]
        public string Speciality { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Пароль должен содержать не менее 8 символов.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }
}