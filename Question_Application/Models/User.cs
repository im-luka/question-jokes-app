using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Question_Application.Models
{
    public class User
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Username is required!")]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Username must be between 3 - 15 characters!")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required!")]
        [StringLength(1000, MinimumLength = 8, ErrorMessage = "Password must include at least 8 characters!")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[-+_!@#$%^&*., ?]).+$", ErrorMessage = "Password must contain at least 1 uppercase letter, lowercase letter, number and special character.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password is not the same as original password!")]
        public string PasswordConfirmed { get; set; }

        public PasswordHasher HashedPassword { get; set; }

        public bool IsAdmin { get; set; }

        public User()
        {

        }

        public User(int id, string username, string password, bool isAdmin)
        {
            this.ID = id;
            this.Username = username;
            this.Password = password;
            this.IsAdmin = isAdmin;
        }
    }
}
