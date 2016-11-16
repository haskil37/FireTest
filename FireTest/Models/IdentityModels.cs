using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public bool Invited { get; set; }
        public int BattleCount { get; set; }
        public int BattleWinCount { get; set; }
        public int Rating { get; set; }
        public int AnswersCount { get; set; }
        public int CorrectAnswersCount { get; set; }
        public bool Busy { get; set; }
        public int IdBattleInvite { get; set; }
        public System.DateTime LastActivity { get; set; }

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
        public bool Teacher { get; set; }
        public bool Administrator { get; set; }
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
    }
    public class Examination
    {
        public int Id { get; set; }
        public string TeacherId { get; set; }
        public int IdTest { get; set; }
        public string Name { get; set; }
        public int Group { get; set; }
        public System.DateTime Date { get; set; }
        public string classroom { get; set; }
        public string annotations { get; set; }
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
        public DbSet<Examination> Examinations { get; set; }
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