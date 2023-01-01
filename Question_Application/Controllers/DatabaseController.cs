using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Question_Application.Controllers
{
    public class DatabaseController : Controller
    {
        public SqlConnection Connection { get; set; }

        public DatabaseController(IConfiguration configuration)
        {
            string connString = configuration.GetConnectionString("connectionString");
            Connection = new SqlConnection(connString);
        }

        public void OpenConnection()
        {
            Connection.Open();
        }

        public void CloseConnection()
        {
            Connection.Close();
        }
    }
}
