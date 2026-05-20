using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelRad.Data;
using TelRad.Models;

namespace TelRad.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // 👤 PUBLIC (GUEST + ADMIN can use)
        [AllowAnonymous]
        public IActionResult Search()
        {
            var employees = _context.Employees.ToList();
            return View(employees);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Search(string keyword)
        {
            var employees = _context.Employees.ToList();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ViewBag.Message = "Please enter a search keyword";
                return View(employees);
            }

            var employee = _context.Employees
                .FirstOrDefault(e => e.FullName.Trim().ToLower() == keyword.Trim().ToLower());

            if (employee == null)
            {
                ViewBag.Message = "Employee not found";
                return View(employees);
            }

            return View("Result", new SearchResultViewModel
            {
                Employee = employee
            });
        }

        // 🔐 ADMIN ONLY ACTIONS

        [Authorize(Roles = "Admin")]
        public IActionResult AllEmployees()
        {
            return View(_context.Employees.ToList());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult NearestTelrad()
        {
            return View(_context.Employees.ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AssignNearestTelrad(int id, string nearestTelrad)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);

            if (employee != null)
            {
                employee.NearestTelrad = nearestTelrad;
                _context.SaveChanges();
            }

            return RedirectToAction("Search");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee != null)
            {
                _context.Employees.Remove(employee);
                _context.SaveChanges();
            }

            return RedirectToAction("Search");
        }
    }
}