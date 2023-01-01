using Question_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Question_Application.ViewModel
{
    public class QuestionAndAuthor
    {
        public Question Question { get; set; }
        public User Author { get; set; }

        public QuestionAndAuthor()
        {

        }
    }
}
