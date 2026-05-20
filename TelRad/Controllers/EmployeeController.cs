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

        public IActionResult Search()
        {
            var employees = _context.Employees.ToList();

            return View(employees);
        }

        [HttpPost]
        public IActionResult Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                ViewBag.Message = "Please enter a search keyword";

                var employees = _context.Employees.ToList();

                return View(employees);
            }

            var employee = _context.Employees
                .FirstOrDefault(e =>
                    (!string.IsNullOrEmpty(e.FullName) &&
                     e.FullName.Trim().ToLower() == keyword.Trim().ToLower())
                );

            if (employee == null)
            {
                ViewBag.Message = "Employee not found";

                var employees = _context.Employees.ToList();

                return View(employees);
            }

            var vm = new SearchResultViewModel
            {
                Employee = employee
            };

            return View("Result", vm);
        }

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

        [HttpPost]
        public IActionResult UpdateEmployee(Employee employee)
        {
            var existingEmployee =
                _context.Employees.Find(employee.Id);

            if (existingEmployee != null)
            {
                existingEmployee.FullName =
                    employee.FullName;

                existingEmployee.Branch =
                    employee.Branch;

                existingEmployee.Department =
                    employee.Department;

                existingEmployee.AssignedTelrad =
                    employee.AssignedTelrad;

                existingEmployee.NearestTelrad =
                    employee.NearestTelrad;

                _context.SaveChanges();
            }

            return RedirectToAction("Search");
        }
    }
}