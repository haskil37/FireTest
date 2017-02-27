using System.Collections.Generic;

namespace FireTest.Models
{
    public class Answers
    {
        public int AnswerId { set; get; }
        public string AnswerText { set; get; }
    }
    public class Questions
    {
        public string QuestionText { set; get; }
        public string QuestionImage { set; get; }
        public System.Collections.Generic.List<Answers> QuestionAnswers { set; get; }
    }
    public class TestWrongAnswers
    {
        public string Subject { set; get; }
        public int Count { set; get; }
    }
    public class TestWrongAnswersDetails
    {
        public string Question { set; get; }
        public string TypeQuestion { set; get; }
        public List<string> CorrectAnswers { set; get; }
        public List<string> WrongAnswers { set; get; }
    }
    public class Rating
    {
        public string Name { set; get; }
        public string Family { set; get; }
        public string Avatar { set; get; }
    }
}