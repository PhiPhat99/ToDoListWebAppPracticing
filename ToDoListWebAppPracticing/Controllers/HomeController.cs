using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using ToDoListWebAppPracticing.Models;

namespace ToDoListWebAppPracticing.Controllers
{
    public class HomeController : Controller
    {
        private ToDoListDbContext dbContext = null!;

        public HomeController(ToDoListDbContext dbCtx)
        {
           dbContext = dbCtx ?? throw new ArgumentNullException(nameof(dbCtx));
        }

        public IActionResult Index(string id)
        {
            var filter = new Filter(id);
            ViewBag.Filter = filter;

            ViewBag.Categories = dbContext.Categories.ToList() ?? new List<Category>();
            ViewBag.Statuses = dbContext.Statuses.ToList() ?? new List<Status>();
            ViewBag.DueFilterValues = Filter.DueFilterValues;

            IQueryable<ToDoListItems> query = dbContext.ToDoLists.Include(t => t.Category).Include(t => t.Status);

            if (filter.IsCategory) { query = query.Where(t => t.CategoryId == filter.CategoryId); }

            if (filter.IsStatus) { query = query.Where(t => t.StatusId == filter.StatusId); }
            
            if (filter.IsDue)
            {
                if (filter.IsPast)
                {
                    query = query.Where(t => t.Duedate < DateTime.Today);
                }
                else if (filter.IsToday)
                {
                    query = query.Where(t => t.Duedate == DateTime.Today);
                }
                else if (filter.IsFuture)
                {
                    query = query.Where(t => t.Duedate > DateTime.Today);
                }
            }

            var tasks = query.OrderBy(t => t.Duedate).ToList();

            return View(tasks);
        }

        public IActionResult Add()
        {
            ViewBag.Categories = dbContext.Categories.ToList() ?? new List<Category>();
            ViewBag.Statuses = dbContext.Statuses.ToList() ?? new List<Status>();

            var tasks = new ToDoListItems { StatusId = "open" };
            return View(tasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(ToDoListItems tasks)
        {
            if (ModelState.IsValid)
            {
                dbContext.ToDoLists.Add(tasks);
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Categories = dbContext.Categories.ToList();
                ViewBag.Statuses = dbContext.Statuses.ToList();
                return View(tasks);
            }
        }

        public IActionResult Filters(string[] filter)
        {
            string id = string.Join("-", filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult MarkComplete([FromRoute] string id, ToDoListItems selected)
        {
            var task = dbContext.ToDoLists.Find(selected.Id);
            if (task != null)
            {
                task.StatusId = "closed";
                dbContext.SaveChanges();
            }
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSelected(string id, int[] selectedTasks)
        {
            if (selectedTasks != null && selectedTasks.Length > 0)
            {
                var tasksToDelete = dbContext.ToDoLists.Where(t => selectedTasks.Contains(t.Id)).ToList();

                if (tasksToDelete.Count > 0)
                {
                    dbContext.ToDoLists.RemoveRange(tasksToDelete);
                    dbContext.SaveChanges();
                }
            }

            return RedirectToAction("Index", new { ID = id });
        }
    }
}
