using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Question_Application.Models;
using Question_Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Question_Application.Controllers
{
    public class QuestionsController : Controller
    {
        private List<QuestionAndAuthor> QuestionAndAuthorList { get; set; }

        private IConfiguration Configuration;

        public QuestionsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IActionResult ListOfQuestions()
        {
            QuestionAndAuthorList = ImportQuestions();
            ViewData["Title"] = "Questions";
            return View("Questions", QuestionAndAuthorList);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Create question";
            return View(new Question());
        }

        [HttpPost]
        public IActionResult Create(Question question)
        {
            ViewData["Title"] = "Create question";
            if(ModelState.IsValid)
            {
                if(question != null)
                {
                    AddQuestionToDatabase(question);
                    ViewData["is_added"] = "true";
                }
            }

            return View();
        }

        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Details";
            QuestionAndAuthorList = ImportQuestions();
            QuestionAndAuthor tmpQandA = QuestionAndAuthorList.FirstOrDefault(quest => quest.Question.ID == id);
            return View(tmpQandA);
        }

        public IActionResult Delete(int questionID)
        {
            DeleteQuestionFromDatabase(questionID);

            return RedirectToAction("ListOfQuestions");
        }

        public IActionResult Edit(int questionID)
        {
            ViewData["Title"] = "Edit";
            QuestionAndAuthorList = ImportQuestions();
            QuestionAndAuthor tmpQandA = QuestionAndAuthorList.FirstOrDefault(quest => quest.Question.ID == questionID);

            return View(tmpQandA);
        }

        [HttpPost]
        public IActionResult Edit(QuestionAndAuthor tmpQandA)
        {
            ViewData["Title"] = "Edit";
            EditQuestionToDatabase(tmpQandA);

            return View();
        }

        public List<QuestionAndAuthor> ImportQuestions()
        {
            List<QuestionAndAuthor> tmpQuestionAndAuthorList = new List<QuestionAndAuthor>();

            DatabaseController database = new DatabaseController(Configuration);

            try
            {
                database.OpenConnection();

                string query = "select * from questions";
                SqlCommand command = new SqlCommand(query, database.Connection);
                SqlDataReader reader = command.ExecuteReader();

                using(reader)
                {
                    while(reader.Read())
                    {
                        int id = (int)reader["id"];
                        string question = (string)reader["question"];
                        string answer = (string)reader["answer"];
                        DateTime date = (DateTime)reader["date"];
                        int userID = (int)reader["user_id"];

                        UserController tmpController = new UserController(Configuration);
                        List<User> tmpUsersList = tmpController.ImportUsers();

                        tmpQuestionAndAuthorList.Add(new QuestionAndAuthor()
                        {
                            Question = new Question(id, question, answer, date, userID),
                            Author = tmpUsersList.FirstOrDefault(user => user.ID == userID)
                        });
                    }
                }

            }
            catch(SqlException ex)
            {
                ViewData["sql_exception"] = "Error when importing data from database: " + ex.Message;
            }
            finally
            {
                database.CloseConnection();
            }

            return tmpQuestionAndAuthorList;
        }

        public void AddQuestionToDatabase(Question question)
        {
            DatabaseController database = new DatabaseController(Configuration);

            try
            {
                database.OpenConnection();

                string query = "insert into questions(question, answer, date, user_id) " +
                                "values(@question, @answer, @date, @userID)";
                SqlCommand command = new SqlCommand(query, database.Connection);
                command.Parameters.AddWithValue("@question", question.Question_);
                command.Parameters.AddWithValue("@answer", question.Answer);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.Parameters.AddWithValue("@userID", HttpContext.Session.GetInt32("UserID"));

                command.ExecuteNonQuery();
            }
            catch(SqlException ex)
            {
                ViewData["sql_exception"] = "Error when importing data from database: " + ex.Message;
            }
            finally
            {
                database.CloseConnection();
            }
        }

        public void DeleteQuestionFromDatabase(int id)
        {
            DatabaseController database = new DatabaseController(Configuration);

            try
            {
                database.OpenConnection();

                string query = "delete from questions where id=@questionID";
                SqlCommand command = new SqlCommand(query, database.Connection);
                command.Parameters.AddWithValue("@questionID", id);

                command.ExecuteNonQuery();
            }
            catch(SqlException ex)
            {
                ViewData["sql_exception"] = "Error when importing data from database: " + ex.Message;
            }
            finally
            {
                database.CloseConnection();
            }
        }

        public void EditQuestionToDatabase(QuestionAndAuthor instance)
        {
            DatabaseController database = new DatabaseController(Configuration);

            try
            {
                database.OpenConnection();

                string query = "update questions " +
                                "set question=@question, answer=@answer, date=@date, user_id=@userID " +
                                "where id=@questionID";
                SqlCommand command = new SqlCommand(query, database.Connection);
                command.Parameters.AddWithValue("@question", instance.Question.Question_);
                command.Parameters.AddWithValue("@answer", instance.Question.Answer);
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.Parameters.AddWithValue("@userID", HttpContext.Session.GetInt32("UserID"));
                command.Parameters.AddWithValue("@questionID", instance.Question.ID);

                command.ExecuteNonQuery();

                ViewData["is_edited"] = "You have successfully edited question!";
            }
            catch(SqlException ex)
            {
                ViewData["sql_exception"] = "Error when importing data from database: " + ex.Message;
            }
            finally
            {
                database.CloseConnection();
            }
        } 
    }
}
