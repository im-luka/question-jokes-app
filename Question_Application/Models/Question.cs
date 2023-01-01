using Question_Application.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Question_Application.Models
{
    public class Question
    {
        public int ID { get; set; }

        [Display(Name = "Question")]
        public string Question_ { get; set; }
        public string Answer { get; set; }
        public DateTime Date { get; set; }
        public int UserID { get; set; }

        public Question()
        {

        }

        public Question(int id, string question, string answer, DateTime date, int userID)
        {
            this.ID = id;
            this.Question_ = question;
            this.Answer = answer;
            this.Date = date;
            this.UserID = userID;
        }

        public static explicit operator Question(QuestionAndAuthor v)
        {
            throw new NotImplementedException();
        }
    }
}
