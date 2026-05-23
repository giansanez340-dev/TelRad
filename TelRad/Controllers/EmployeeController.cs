using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult UpdateEmployee([FromBody] UpdateEmployeeRequest model)
        {
            if (model == null) return BadRequest();

            var employee = _context.Employees.FirstOrDefault(e => e.Id == model.Id);
            if (employee == null) return NotFound();

            // Check if telrad is being changed
            var telradChanged = employee.AssignedTelrad?.ToLower() != model.AssignedTelrad?.ToLower();

            if (telradChanged && !string.IsNullOrWhiteSpace(model.AssignedTelrad))
            {
                // Check if the new telrad exists and is inactive
                var telradInactive = _context.Employees
                    .Where(e => e.AssignedTelrad == model.AssignedTelrad && !e.IsActive)
                    .Any();

                if (telradInactive)
                {
                    return BadRequest(new { error = "This Telrad is inactive and cannot be assigned." });
                }
            }

            employee.FullName = model.FullName;
            employee.Branch = model.Branch;
            employee.Department = model.Department;
            employee.Position = model.Position;
            employee.Number = model.Number;
            employee.AssignedTelrad = model.AssignedTelrad;
            employee.NearestTelrad = model.NearestTelrad;
            employee.IsMainHandler = model.IsMainHandler;

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult AddInlineEmployee(string fullName, string? assignedTelrad, string? nearestTelrad)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Name is required to add a new entry.";
                return RedirectToAction("Search");
            }

            var normalizedFullName = fullName.Trim();
            var duplicateExists = _context.Employees.Any(e =>
                !string.IsNullOrWhiteSpace(e.FullName) &&
                e.FullName!.Trim().ToLower() == normalizedFullName.ToLower());

            if (duplicateExists)
            {
                TempData["Error"] = "An entry with the same Full Name already exists.";
                return RedirectToAction("Search");
            }

            // Check if telrad is inactive
            if (!string.IsNullOrWhiteSpace(assignedTelrad))
            {
                var telradInactive = _context.Employees
                    .Where(e => e.AssignedTelrad == assignedTelrad.Trim() && !e.IsActive)
                    .Any();

                if (telradInactive)
                {
                    TempData["Error"] = "This Telrad is inactive and cannot be assigned.";
                    return RedirectToAction("Search");
                }
            }

            var employee = new Employee
            {
                FullName = normalizedFullName,
                Branch = "Unassigned",
                Department = "Unassigned",
                AssignedTelrad = string.IsNullOrWhiteSpace(assignedTelrad) ? null : assignedTelrad.Trim(),
                NearestTelrad = string.IsNullOrWhiteSpace(nearestTelrad) ? null : nearestTelrad.Trim(),
                IsActive = true
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();

            TempData["Success"] = "Entry added successfully.";
            return RedirectToAction("Search");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateMainHandler([FromBody] MainHandlerUpdateModel model)
        {
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateActiveStatus([FromBody] UpdateActiveStatusRequest request)
        {
            if (request == null) return BadRequest();

            var assignedTelrad = request.AssignedTelrad?.Trim();
            if (string.IsNullOrWhiteSpace(assignedTelrad) && request.EmployeeId > 0)
            {
                var emp = await _context.Employees.FindAsync(request.EmployeeId);
                if (emp != null)
                {
                    assignedTelrad = emp.AssignedTelrad;
                }
            }

            if (string.IsNullOrWhiteSpace(assignedTelrad))
            {
                return BadRequest(new { error = "AssignedTelrad is required." });
            }

            var employees = await _context.Employees
                .Where(e => e.AssignedTelrad == assignedTelrad)
                .ToListAsync();

            if (!employees.Any()) return NotFound();

            foreach (var emp in employees)
            {
                emp.IsActive = request.IsActive;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        public class UpdateEmployeeRequest
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string Branch { get; set; } = "";
            public string Department { get; set; } = "";
            public string Position { get; set; } = "";
            public string Number { get; set; } = "";
            public string AssignedTelrad { get; set; } = "";
            public string NearestTelrad { get; set; } = "";
            public bool IsMainHandler { get; set; }
        }

        public class UpdateActiveStatusRequest
        {
            public int EmployeeId { get; set; }
            public bool IsActive { get; set; }
            public string AssignedTelrad { get; set; } = "";
        }
    }
}
