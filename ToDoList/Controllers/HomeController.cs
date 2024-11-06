using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private readonly ToDoContext _context;

        public HomeController(ToDoContext context) => _context = context;

        public ViewResult Index(string id)
        {
            ToDoViewModel model = new ToDoViewModel();

            var filters = new Filters(id);

           
            model.Filters = filters;
            model.Categories = _context.Categories.ToList(); 
            model.Statuses = _context.Statuses.ToList(); 
            model.DueFilters = Filters.DueFilterValues; 

            IQueryable<ToDo> query = _context.ToDos
                .Include(t => t.Category)  
                .Include(t => t.Status);   

           
            if (filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            }
            if (filters.HasStatus)
            {
                query = query.Where(t => t.StatusId == filters.StatusId);
            }
            if (filters.HasDue)
            {
                var today = DateTime.Today;
                if (filters.IsPast)
                    query = query.Where(t => t.DueDate < today);
                else if (filters.IsFuture)
                    query = query.Where(t => t.DueDate > today);
                else if (filters.IsToday)
                    query = query.Where(t => t.DueDate == today);
            }

            model.Tasks = query.OrderBy(t => t.DueDate).ToList();

            return View(model);
        }

        [HttpGet]
        public ViewResult Add()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Statuses = _context.Statuses.ToList();

            var task = new ToDo { StatusId = "open" }; 
            return View(task);
        }

        [HttpPost]
        public IActionResult Add(ToDo task)
        {
            if (ModelState.IsValid)
            {
                _context.ToDos.Add(task);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Statuses = _context.Statuses.ToList();
                return View(task);
            }
        }

        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new { id });
        }

        [HttpPost]
        public IActionResult MarkComplete([FromRoute] string id, ToDo selected)
        {
            selected = _context.ToDos.Find(selected.Id); 
            if (selected != null)
            {
                selected.StatusId = "closed"; 
                _context.SaveChanges();
            }
            return RedirectToAction("Index", new { id });
        }

        [HttpPost]
        public IActionResult DeleteComplete(string id)
        {
            var toDelete = _context.ToDos
                .Where(t => t.StatusId == "closed").ToList();

            foreach (var task in toDelete)
            {
                _context.ToDos.Remove(task);
            }
            _context.SaveChanges();

            return RedirectToAction("Index", new { id });
        }
    }
}
