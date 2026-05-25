using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelRad.Data;
using TelRad.Models;
using static TelRad.Controllers.EmployeeController;

namespace TelRad.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageEmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public ManageEmployeeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View("Search");
        }

        // =========================
        // ADD EMPLOYEE
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest(new
                {
                    error = "Invalid employee data."
                });
            }

            // TRIM VALUES
            employee.FullName = (employee.FullName ?? "").Trim();
            employee.Branch = (employee.Branch ?? "").Trim();
            employee.Department = (employee.Department ?? "").Trim();
            employee.AssignedTelrad = (employee.AssignedTelrad ?? "").Trim();
            employee.NearestTelrad = (employee.NearestTelrad ?? "").Trim();

            // =========================================
            // FUZZY DUPLICATE NAME CHECK
            // =========================================

            string[] Tokenize(string name) =>
                name.ToUpperInvariant()
                    .Split(new char[] { ' ', ',', '.', '-' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(t => t.Length > 1) // ignore single initials like "P"
                    .ToArray();

            var incomingTokens = Tokenize(employee.FullName ?? "").ToHashSet();

            var allNames = await _context.Employees
                .Select(e => e.FullName)
                .Distinct()
                .ToListAsync();

            var similarName = allNames.FirstOrDefault(existing => {
                var existingTokens = Tokenize(existing ?? "").ToHashSet();
                return incomingTokens.Intersect(existingTokens).Count() >= 2;
            });

            if (similarName != null)
            {
                return Conflict(new
                {
                    error = $"A similar name already exists: \"{similarName}\". Please verify before adding."
                });
            }

            // REQUIRED ONLY FOR TELRAD
            if (string.IsNullOrWhiteSpace(employee.AssignedTelrad))
            {
                return BadRequest(new
                {
                    error = "Assigned Telrad is required."
                });
            }

            // =========================================
            // BLOCK IF TELRAD IS INACTIVE
            // =========================================

            bool inactiveTelradExists = await _context.Employees
                .AnyAsync(e =>
                    e.AssignedTelrad == employee.AssignedTelrad &&
                    !e.IsActive);

            if (inactiveTelradExists)
            {
                return Conflict(new
                {
                    error = "This Telrad is inactive and cannot be assigned."
                });
            }

            // =========================================
            // EXACT DUPLICATE CHECK
            // =========================================

            bool exactDuplicateExists = await _context.Employees
                .AnyAsync(e =>

                    e.FullName == employee.FullName &&
                    e.Branch == employee.Branch &&
                    e.Department == employee.Department &&
                    e.AssignedTelrad == employee.AssignedTelrad
                );

            if (exactDuplicateExists)
            {
                return Conflict(new
                {
                    error = "Duplicate employee record already exists."
                });
            }

            // =========================================
            // ALLOW SAME EMPLOYEE INFO
            // IF TELRAD IS DIFFERENT + ACTIVE
            // =========================================

            employee.IsActive = true;
            employee.IsMainHandler = false;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                id = employee.Id,
                fullName = employee.FullName,
                branch = employee.Branch,
                department = employee.Department,
                assignedTelrad = employee.AssignedTelrad,
                nearestTelrad = employee.NearestTelrad
            });
        }

        // =========================
        // UPDATE EMPLOYEE (GROUP SAFE SAVE FIX)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployee([FromBody] Employee dto)
        {
            var employees = await _context.Employees
                .Where(e => e.AssignedTelrad == dto.AssignedTelrad)
                .ToListAsync();

            if (!employees.Any())
                return NotFound();

            foreach (var emp in employees)
            {
                emp.FullName = dto.FullName;
                emp.Branch = dto.Branch;
                emp.Department = dto.Department;
                emp.NearestTelrad = dto.NearestTelrad;
                emp.IsMainHandler = dto.IsMainHandler;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}