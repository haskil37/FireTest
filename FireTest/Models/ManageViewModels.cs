﻿using System.Collections.Generic;
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
        [Display(Name = "Учебная группа (без года обучения и факультета)")]
        public string Group { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать символов не менее: {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }
}