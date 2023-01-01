using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Question_Application.Models
{
    public class PasswordHasher
    {
        public string Hash { get; set; }
        public string Salt { get; set; }
    }
}
