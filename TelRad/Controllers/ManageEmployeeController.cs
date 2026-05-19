using Microsoft.AspNetCore.Mvc;
using TelRad.Data;
using TelRad.Models;

namespace TelRad.Controllers
{
    public class ManageEmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public ManageEmployeeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Employee employee)
        {
            if (ModelState.IsValid)
            {
                var lastEmployee = _context.Employees
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefault();

                int nextNumber = 1;

                if (lastEmployee != null)
                {
                    nextNumber = lastEmployee.Id + 1;
                }

                _context.Employees.Add(employee);

                _context.SaveChanges();

                TempData["Success"] = "Employee added successfully";

                return RedirectToAction("Index");
            }

            return View(employee);
        }
    }
}