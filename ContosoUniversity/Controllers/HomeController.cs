using System.Collections.Generic;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.ViewModels.SchoolViewModel;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class HomeController : Controller
    {
        private readonly SchoolContext context;

        public HomeController(SchoolContext context) => this.context = context;

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            var groups = new List<EnrollmentDateGroup>();
            var connection = context.Database.GetDbConnection();

            try
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();

                var query = "SELECT EnrollmentDate, Count(*) AS StudentCount " +
                            "FROM Person " +
                            "WHERE Discriminator = 'Student' " +
                            "GROUP BY EnrollmentDate";
                command.CommandText = query;

                var dbReader = await command.ExecuteReaderAsync();
                if (dbReader.HasRows)
                {
                    while (await dbReader.ReadAsync())
                    {
                        groups.Add(new EnrollmentDateGroup()
                            { EnrollmentDate = dbReader.GetDateTime(0), StudentCount = dbReader.GetInt32(1) });
                    }
                }

                await dbReader.DisposeAsync();
            }
            finally
            {
                await connection.CloseAsync();
            }

            return View(groups);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
