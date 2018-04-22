using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FireTest.Models
{
    public interface IFireTestService
    {
        bool New(FireTestType type, bool delete = false);
        SkillIndexViewModel SkillIndexViewModel(int skill);
    }
    public enum FireTestType
    {
        Skill,
        Discipline,
        Battle,
        Test,
        Exam
    }
    public class FireTestService : IFireTestService
    {
        public bool New(FireTestType type, bool delete = false)
        {
            switch (type)
            {
                case FireTestType.Skill:
                    var selfyQualification = Find(dbContext.SelfyTestQualifications);
                    if (selfyQualification != null)
                    {
                        if (!string.IsNullOrEmpty(selfyQualification.Questions) && !delete)
                            return false;
                        dbContext.SelfyTestQualifications.Remove(selfyQualification);
                    }
                    break;
                case FireTestType.Discipline:
                    var selfyDiscipline = Find(dbContext.SelfyTestDisciplines);
                    if (selfyDiscipline != null)
                    {
                        if (!string.IsNullOrEmpty(selfyDiscipline.Questions) && !delete)
                            return false;
                        dbContext.SelfyTestDisciplines.Remove(selfyDiscipline);
                    }
                    break;
            }
            dbContext.SaveChanges();
            return true;
        }
        #region SkillIndexViewModel
        public SkillIndexViewModel SkillIndexViewModel(int skill)
        {
            var count = Count(skill);
            return new SkillIndexViewModel
            {
                Skill = skill,
                SkillCourse = CourseText(skill),
                SkillName = GetSkillName(skill),
                CountCurrent = count[0],
                CountMax = count[1]
            };
        }
        private string GetSkillName(int Skill)
        {
            return dbContext.Qualifications.Find(Skill).Name;
        }
        private string CourseText(int skill)
        {
            string value = "";
            for (int i = 1; i <= skill;)
            {
                value += i;
                i++;
                if (i > skill)
                    break;
                value += i < skill ? ", " : " и ";
            }
            return value;
        }
        private List<int> Count(int skill)
        {
            int[,] relation = new int[5, 5]{
                {100, 0, 0, 0, 0},
                {40, 60, 0, 0, 0},
                {20, 20, 60, 0, 0},
                {10, 10, 20, 60, 0},
                {10, 10, 10, 20, 50}};

            int value = 0;
            int current = 0;
            for (int i = 1; i <= skill; i++)
            {
                var countQuestions = dbContext.Questions.Where(u => u.IdQualification == i).Count();
                current = countQuestions;
                if (value != 0)
                    value = Math.Min(value, countQuestions * 100 / relation[skill - 1, i - 1]);
                else
                    value = countQuestions * 100 / relation[skill - 1, i - 1];
            }
            return new List<int>()
            {
                current,
                (int)Math.Truncate(value / 10.0)
            };
        }
        #endregion
        #region Вспомогательные
        private ApplicationDbContext dbContext = new ApplicationDbContext();
        string userID;
        public FireTestService(string userId)
        {
            userID = userId;
        }
        private SelfyTestQualification Find(DbSet<SelfyTestQualification> selfyTestQualifications)
        {
            return dbContext.SelfyTestQualifications
                .Where(u => u.IdUser == userID)
                .Where(u => u.End == false).FirstOrDefault();
        }
        private SelfyTestDiscipline Find(DbSet<SelfyTestDiscipline> SelfyTestDiscipline)
        {
            return dbContext.SelfyTestDisciplines
                .Where(u => u.IdUser == userID)
                .Where(u => u.End == false).FirstOrDefault();
        }
        #endregion
    }
}