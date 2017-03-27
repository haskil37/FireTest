using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace FireTest.Models
{
    #region Пользователь
    public class ApplicationUser : IdentityUser
    {
        public string Snils { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string SubName { get; set; }
        public string Avatar { get; set; }
        public int Course { get; set; }
        public int Year { get; set; }
        public string Group { get; set; }
        public string Faculty { get; set; }
        public string QualificationPoint { get; set; }
        public bool Invited { get; set; }
        public int BattleCount { get; set; }
        public int BattleWinCount { get; set; }
        public double Rating { get; set; }
        public int AnswersCount { get; set; }
        public int CorrectAnswersCount { get; set; }
        public bool Busy { get; set; }
        public int IdBattleInvite { get; set; }
        public System.DateTime LastActivity { get; set; }
        public bool Update { get; set; }
        public int Age { get; set; }
        public bool Sex { get; set; }
        public string Region { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
    }
    public class UsersData
    {
        public string Id { get; set; }
        public string Family { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int Battles { get; set; }
        public int CorrectAnswers { get; set; }
        public string Group { get; set; }
        public int Rating { get; set; }
    }
    public class UsersForAdmin
    {
        public string Id { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Snils { get; set; }
        public bool Teacher { get; set; }
        public bool Administrator { get; set; }
        public bool Qualification { get; set; }
    }
    public class Awards
    {
        public List<string> Qualification { get; set; }
        public List<string> Battles { get; set; }
    }
    #endregion
    #region Структура (факультеты, дисциплины, специальности, квалификации, группы)
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int IdDepartment { get; set; }
    }
    public class SubjectAccess
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Access { get; set; }
    }
    public class SubjectsAndQualification
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Qualification { get; set; }
        public string QualificationName { get; set; }
    }
    public class DepartmentsAndSubjects
    {
        public List<Department> Department { get; set; }
        public List<Subject> Subject { get; set; }
    }
    public class Specialty
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Qualification
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    #endregion
    #region Вопросы и ответы
    public class Question
    {
        public int Id { get; set; }
        public int IdDepartment { get; set; }
        public int IdSubject { get; set; }
        public int IdSpecialty { get; set; }
        public int IdQualification { get; set; }
        public int IdCourse { get; set; }
        public string QuestionText { get; set; }
        public string QuestionImage { get; set; }
        public string IdCorrect { get; set; }
        public int CountAll { get; set; }
        public int CountCorrect { get; set; }
        public string Tag { get; set; }
    }
    public class Answer
    {
        public int Id { get; set; }
        public int IdQuestion { get; set; }
        public string AnswerText { get; set; }
    }
    public class ViewCreateQuestion
    {
        [Required(ErrorMessage = "Вы не написали текст вопроса")]
        public string QuestionText { get; set; }
        public string Tag { get; set; }        
        [AnswersValidate]
        public List<string> Answers { get; set; }
        [Required(ErrorMessage = "Должен быть указан хотя бы один правильный ответ")]
        public List<int> AnswersCorrects { get; set; }
    }
    public class ViewEditQuestion
    {
        [Required(ErrorMessage = "Вы не написали текст вопроса")]
        public string QuestionText { get; set; }
        public string Tag { get; set; }
        public List<Answer> Answers { get; set; }
        public List<int> AnswersCorrects { get; set; }
    }
    public class AnswersValidate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var list = value as IList;
            int count = 0;
            foreach(string item in list)
                if (string.IsNullOrEmpty(item))
                    count++;
            if (count == 8)
                return new ValidationResult("Должен быть хотя бы один ответ");

            return ValidationResult.Success;
        }
    }
    #endregion
    #region Самотестирование
    public class SelfyTestQualification
    {
        public int Id { get; set; }
        public int IdQualification { get; set; }
        public int Course { get; set; }
        public string IdUser { get; set; }
        public string Questions { get; set; }
        public string Answers { get; set; }
        public string RightOrWrong { get; set; }
        public System.DateTime TimeStart { get; set; }
        public System.DateTime TimeEnd { get; set; }
        public bool End { get; set; }
    }
    public class SelfyTestDiscipline
    {
        public int Id { get; set; }
        public int IdSubject { get; set; }
        public string IdUser { get; set; }
        public string Questions { get; set; }
        public string Answers { get; set; }
        public string RightOrWrong { get; set; }
        public System.DateTime TimeStart { get; set; }
        public System.DateTime TimeEnd { get; set; }
        public bool End { get; set; }
    }
    #endregion
    #region Схватки
    public class Battle
    {
        public int Id { get; set; }
        public string FirstPlayer { get; set; }
        public string SecondPlayer { get; set; }
        public int Course { get; set; }
        public int Qualification { get; set; }
        public string QuestionsFirstPlayer { get; set; }
        public string QuestionsSecondPlayer { get; set; }
        public string AnswersFirstPlayer { get; set; }
        public string AnswersSecondPlayer { get; set; }
        public string RightOrWrongFirstPlayer { get; set; }
        public string RightOrWrongSecondPlayer { get; set; }
        public bool EndFirstPlayer { get; set; }
        public bool EndSecondPlayer { get; set; }
        public bool AgreementOtherPlayer { get; set; }
        public System.DateTime TimeStartFirstPlayer { get; set; }
        public System.DateTime TimeEndFirstPlayer { get; set; }
        public System.DateTime TimeStartSecondPlayer { get; set; }
        public System.DateTime TimeEndSecondPlayer { get; set; }
        public string Winner { get; set; }
    }
    public class ValidateBattle : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Произошла ошибка. Попробуйте выбрать еще раз.");

            if (validationContext.MemberName == "Qualification")
                if ((int)value < 1 || (int)value > 5)
                    return new ValidationResult("Не выбрана квалификация");

            if (validationContext.MemberName == "Course")
                if ((int)value < 1 || (int)value > 5)
                    return new ValidationResult("Не выбран курс");

            return ValidationResult.Success;
        }
    }
    public class BattleViewModel
    {
        [Required]
        [ValidateBattle]
        public int Id { get; set; }

        [Required]
        [ValidateBattle]
        public int Course { get; set; }

        [Required]
        [ValidateBattle]
        public int Qualification { get; set; }
    }
    #endregion
    #region Учительская
    public class TeacherAccess
    {
        public int Id { get; set; }
        public string TeacherId { get; set; }
        public string TeacherSubjects { get; set; }
        public bool TeacherQualifications { get; set; }
    }
    public class TeacherTest
    {
        public int Id { get; set; }
        public string TeacherId { get; set; }
        public string NameTest { get; set; }
        public string Questions { get; set; }
        public int Eval5 { get; set; }
        public int Eval4 { get; set; }
        public int Eval3 { get; set; }
    }
    public class TeacherFinishTest
    {
        public int Id { get; set; }
        public string TeacherId { get; set; }
        public int IdQualification { get; set; }
        public string NameTest { get; set; }
        public string Questions { get; set; }
        public int Eval5 { get; set; }
        public int Eval4 { get; set; }
        public int Eval3 { get; set; }
    }
    public class TeacherFinishTestPrepareViewModel
    {
        [Required(ErrorMessage = "Вы должны указать название итогового тестирования")]
        public string NameTest { get; set; }
        public int Qualifications { get; set; }
        [Required(ErrorMessage = "Вы должны указать критерий оценки для 5")]
        public int Eval5 { get; set; }
        [Required(ErrorMessage = "Вы должны указать критерий оценки для 4")]
        public int Eval4 { get; set; }
        [Required(ErrorMessage = "Вы должны указать критерий оценки для 3")]
        public int Eval3 { get; set; }
    }
    public class TeacherTestDetails
    {
        public int Id { get; set; }
        [Required]
        public string NameTest { get; set; }
        public string Qualification { get; set; }
        public int Eval5 { get; set; }
        public int Eval4 { get; set; }
        public int Eval3 { get; set; }

        public List<Question> Questions { get; set; }
    }
    public class Examination
    {
        public int Id { get; set; }
        public string TeacherId { get; set; }
        public int IdTest { get; set; }
        public bool FinishTest { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public System.DateTime Date { get; set; }
        public string Classroom { get; set; }
        public string Annotations { get; set; }
        public int Time { get; set; }
    }
    public class ExaminationViewModel
    {
        [Required(ErrorMessage = "Вы должны указать название экзамена")]
        [Display(Name = "Название экзамена")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Вы должны выбрать дату экзамена")]
        [Display(Name = "Дата экзамена")]
        [DataType(DataType.Date)]
        public System.DateTime Date { get; set; }
        [Display(Name = "Аудитория")]
        public string Classroom { get; set; }
        [Display(Name = "Комментарий")]
        public string Annotations { get; set; }
        [Required(ErrorMessage = "Нужно указать время тестирования")]
        [Display(Name = "Время тестирования, мин")]
        public int Time { get; set; }
    }
    #endregion
    #region Тестирование
    public class TestQualification
    {
        public int Id { get; set; }
        public int IdExamination { get; set; }
        public string IdUser { get; set; }
        public string Questions { get; set; }
        public string Answers { get; set; }
        public string RightOrWrong { get; set; }
        public System.DateTime TimeStart { get; set; }
        public int Score { get; set; }
        public bool End { get; set; }
    }
    public class TestQualificationAccess
    {
        public int Id { get; set; }
        public int IdExamination { get; set; }
        public string IdUsers { get; set; }
    }
    #endregion
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Qualification> Qualifications { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Specialty> Specialtys { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<SelfyTestQualification> SelfyTestQualifications { get; set; }
        public DbSet<SelfyTestDiscipline> SelfyTestDisciplines { get; set; }
        public DbSet<Battle> Battles { get; set; }
        public DbSet<TeacherAccess> TeachersAccess { get; set; }
        public DbSet<TeacherTest> TeacherTests { get; set; }
        public DbSet<TeacherFinishTest> TeacherFinishTests { get; set; }
        public DbSet<Examination> Examinations { get; set; }
        public DbSet<TestQualification> TestQualification { get; set; }
        public DbSet<TestQualificationAccess> TestQualificationAccess { get; set; }
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>().ToTable("Role");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UsersRole");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("_UsersClaim");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("_UsersLogin");

            modelBuilder.Entity<ApplicationUser>().Ignore(x => x.PhoneNumber);
            modelBuilder.Entity<ApplicationUser>().Ignore(x => x.PhoneNumberConfirmed);
            modelBuilder.Entity<ApplicationUser>().Ignore(x => x.TwoFactorEnabled);
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}