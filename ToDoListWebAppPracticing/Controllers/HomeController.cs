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

        public IActionResult Index(string id, string? searchString)
        {
            var filter = new Filter(id);
            ViewBag.Filter = filter;
            IQueryable<ToDoListItems> query = dbContext.ToDoLists.Include(t => t.Category).Include(t => t.Status);

            ViewBag.Categories = dbContext.Categories.ToList() ?? new List<Category>();
            ViewBag.Statuses = dbContext.Statuses.ToList() ?? new List<Status>();
            ViewBag.DueFilterValues = Filter.DueFilterValues;

            if (!string.IsNullOrEmpty(searchString))
            {
                string lowerSearch = searchString.ToLower();
                query = query.Where(t => t.Description.ToLower().Contains(lowerSearch));

                ViewBag.SearchString = searchString;
            }

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
                // ต้องมั่นใจว่า Id = 0 สำหรับ Add ใหม่
                tasks.Id = 0;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ToDoListItems tasks)
        {
            string filterId = TempData["currentFilterId"]?.ToString() ?? "all-all-all";
            TempData["currentFilterId"] = filterId;
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                if (tasks.Id > 0)
                {
                    dbContext.ToDoLists.Update(tasks);
                    dbContext.SaveChanges();
                    TempData.Remove("currentFilterId");
                    return RedirectToAction("Index", new { ID = filterId });
                }

                ModelState.AddModelError("", "ไม่พบ ID ของรายการที่ต้องการแก้ไข");
            }

            ViewBag.Categories = dbContext.Categories.ToList();
            ViewBag.Statuses = dbContext.Statuses.ToList();
            return View("Add", tasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSelected(string id, int[] selectedTasks)
        {
            if (selectedTasks == null || selectedTasks.Length != 1)
            {
                TempData["message"] = "กรุณาเลือก Task ที่ต้องการแก้ไขเพียง 1 รายการ";
                return RedirectToAction("Index", new { ID = id });
            }

            int taskIdToEdit = selectedTasks[0];
            var taskToEdit = dbContext.ToDoLists.Include(t => t.Category).Include(t => t.Status).FirstOrDefault(t => t.Id == taskIdToEdit);

            if (taskToEdit == null)
            {
                TempData["message"] = "ไม่พบ Task ที่ระบุ";
                return RedirectToAction("Index", new { ID = id });
            }

            TempData["currentFilterId"] = id;
            ViewBag.Categories = dbContext.Categories.ToList();
            ViewBag.Statuses = dbContext.Statuses.ToList();

            ModelState.Remove("id");
            return View("Add", taskToEdit);
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
