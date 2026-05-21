using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelRad.Data;
using TelRad.Models;

namespace TelRad.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Search()
        {
            var employees = _context.Employees.ToList();
            bool isAdmin = User.Identity != null && User.IsInRole("Admin");
            ViewData["IsAdmin"] = isAdmin;

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
        public IActionResult UpdateEmployee(Employee model)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == model.Id);
            if (employee != null)
            {
                employee.FullName = model.FullName;
                employee.Branch = model.Branch;
                employee.Department = model.Department;
                employee.AssignedTelrad = model.AssignedTelrad;
                employee.NearestTelrad = model.NearestTelrad;
                employee.IsMainHandler = model.IsMainHandler; // now it will be true if checkbox checked

                _context.SaveChanges();
            }

            return RedirectToAction("Search");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateMainHandler([FromBody] MainHandlerUpdateModel model)
        {
            // Unset all employees for this AssignedTelrad
            var employees = _context.Employees
                                    .Where(e => e.AssignedTelrad == model.AssignedTelrad)
                                    .ToList();

            foreach (var emp in employees)
            {
                emp.IsMainHandler = emp.Id == model.EmployeeId ? model.IsMainHandler : false;
            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateActiveStatus([FromBody] UpdateActiveStatusRequest request)
        {
            var employee = await _context.Employees.FindAsync(request.EmployeeId);
            if (employee == null) return NotFound();

            employee.IsActive = request.IsActive;
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class UpdateActiveStatusRequest
        {
            public int EmployeeId { get; set; }
            public bool IsActive { get; set; }
        }
    }
}