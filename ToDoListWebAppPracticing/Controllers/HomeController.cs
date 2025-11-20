using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListWebAppPracticing.Models;

namespace ToDoListWebAppPracticing.Controllers
{
    public class HomeController : Controller
    {
        private ToDoListDbContext dbContext = null!;

        public HomeController(ToDoListDbContext dbCxt)
        {
           dbContext = dbCxt ?? throw new ArgumentNullException(nameof(dbCxt));
        }

        public IActionResult Index(string id)
        {
            var filter = new Filter(id);
            ViewBag.Filter = filter;

            ViewBag.Categories = dbContext.Categories.ToList() ?? new List<Category>();
            ViewBag.Statuses = dbContext.Statuses.ToList() ?? new List<Status>();
            ViewBag.DueFilterValues = Filter.DueFilterValues;

            IQueryable<ToDoListDbContext> query = (IQueryable<ToDoListDbContext>)dbContext.ToDoLists
                .Include(t => t.Category)
                .Include(t => t.Status);

            if (filter.IsCategory) { query = query.Where(t => t.Category == filter.CategoryId); }

            if (filter.IsStatus) { query = query.Where(t => t.StatusId == filter.StatusId); }
            
            if (filter.IsDue)
            {
                if (filter.IsPast)
                {
                    query = query.Where(t => t.DueDate < DateTime.Today);
                }
                else if (filter.IsToday)
                {
                    query = query.Where(t => t.DueDate == DateTime.Today);
                }
                else if (filter.IsFuture)
                {
                    query = query.Where(t => t.DueDate > DateTime.Today);
                }
            }

            var tasks = query.OrderBy(t => t.DueDate).ToList();

            return View(tasks);
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
