using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Question_Application.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace Question_Application.Controllers
{
    public class UserController : Controller
    {

        public IConfiguration Configuration { get; set; }

        public UserController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            ViewData["Title"] = "Login";

            CheckCredentials(Username, Password);

            return View();
        }

        public IActionResult Register()
        {
            ViewData["Title"] = "Registration";
            return View(new User());
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            ViewData["Title"] = "Registration";
            if (ModelState.IsValid)
            {
                if (user != null)
                {
                    AddUserToDatabase(user);
                    ViewData["is_registered"] = "true";

                    HttpContext.Session.SetInt32("UserID", user.ID);
                    HttpContext.Session.SetString("UserSession", user.Username);
                    if (user.IsAdmin)
                        HttpContext.Session.SetInt32("IsAdmin", 1);
                }
            }

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return View("~/Views/Home/Index.cshtml");
        }

        public List<User> ImportUsers()
        {
            List<User> tmpUsersList = new List<User>();

            DatabaseController database = new DatabaseController(Configuration);

            try
            {
                database.OpenConnection();

                string query = "select * from users";
                SqlCommand command = new SqlCommand(query, database.Connection);
                SqlDataReader reader = command.ExecuteReader();

                using(reader)
                {
                    while(reader.Read())
                    {
                        int id = (int)reader["id"];
                        string username = (string)reader["username"];
                        string password = (string)reader["password"];
                        bool isAdmin = (bool)reader["is_admin"];
                        string password_salt = (string)reader["password_salt"];

                        tmpUsersList.Add(new User(id, username, password, isAdmin));
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

            return tmpUsersList;
        }

        public void AddUserToDatabase(User user)
        {
            DatabaseController database = new DatabaseController(Configuration);

            try
            {
                database.OpenConnection();

                string query = "insert into users(username, password, is_admin, password_salt) " +
                                "values(@username, @password, @isAdmin, @salt)";
                SqlCommand command = new SqlCommand(query, database.Connection);
                command.Parameters.AddWithValue("@username", user.Username);

                PasswordHasher hashedPassword = GenerateHash(user.Password);
                command.Parameters.AddWithValue("@password", hashedPassword.Hash);
                command.Parameters.AddWithValue("@salt", hashedPassword.Salt);

                command.Parameters.AddWithValue("@isAdmin", user.IsAdmin);

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

        public void CheckCredentials(string username, string password)
        {
            DatabaseController database = new DatabaseController(Configuration);

            try
            {
                database.OpenConnection();

                string query = "select * from users where username=@username";
                SqlCommand command = new SqlCommand(query, database.Connection);
                command.Parameters.AddWithValue("@username", username);

                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    if (!reader.HasRows)
                        ViewData["message_login"] = "User with entered username does not exist! Please try again or create an account.";
                    else
                    {
                        while (reader.Read())
                        {
                            if (!VerifyPassword(password, (string)reader["password_salt"], (string)reader["password"]))
                                ViewData["message_login"] = "Password is incorrect!\nPlease try again.";
                            else
                            {
                                ViewData["message_login"] = "Welcome back!";

                                HttpContext.Session.SetInt32("UserID", (int)reader["id"]);
                                HttpContext.Session.SetString("UserSession", (string)reader["username"]);
                                if ((bool)reader["is_admin"])
                                    HttpContext.Session.SetInt32("IsAdmin", 1);
                            }
                        }
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
        }

        public PasswordHasher GenerateHash(string password)
        {
            PasswordHasher hasher = new PasswordHasher();

            var saltBytes = new byte[64];
            var provider = new RNGCryptoServiceProvider();
            provider.GetNonZeroBytes(saltBytes);
            hasher.Salt = Convert.ToBase64String(saltBytes);

            var rfc2898 = new Rfc2898DeriveBytes(password, saltBytes, 10000);
            hasher.Hash = Convert.ToBase64String(rfc2898.GetBytes(256));

            return hasher;
        }

        public bool VerifyPassword(string password, string hashedSalt, string hashedHash)
        {
            var saltBytes = Convert.FromBase64String(hashedSalt);
            var rfc2898 = new Rfc2898DeriveBytes(password, saltBytes, 10000);
            return Convert.ToBase64String(rfc2898.GetBytes(256)) == hashedHash;
        }
    }
}
