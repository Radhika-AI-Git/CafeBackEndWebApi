using CafeBackEndWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using CafeBackEndWebApi.Data;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace CafeBackEndWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public EmployeeController(ApplicationDbContext context) => _context = context;

        [HttpGet("{cafeId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetEmployees(Guid? cafeId)
        {
            var employees = _context.Employees.Include(e => e.Cafe).AsQueryable();

            if (cafeId!=Guid.Empty)
                employees = employees.Where(e => e.CafeId == cafeId);

            var result = await employees
                .Select(e => new

                {
                    e.Id,
                    e.Name,
                    e.EmailAddress,
                    e.PhoneNumber,
                    DaysWorked = e.StartDate.HasValue ? EF.Functions.DateDiffDay(e.StartDate.Value, DateTime.Now) : 0,
                    Cafe = e.Cafe != null ? e.Cafe.Name : ""
                })
                .OrderByDescending(e => e.DaysWorked)
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
        {
            if (await _context.Employees.AnyAsync(e => e.Id == employee.Id))
                return Conflict("Employee ID must be unique");

            if (employee.CafeId.HasValue)
            {
                if (await _context.Employees.AnyAsync(e => e.Id == employee.Id && e.CafeId != null))
                    return Conflict("Employee already assigned to a cafe");
            }
            employee.StartDate = DateTime.UtcNow;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return Ok(employee);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEmployee([FromBody] Employee employee)
        {
            var existing = await _context.Employees.FindAsync(employee.Id);
            if (existing == null) return NotFound();

            existing.Name = employee.Name;
            existing.EmailAddress = employee.EmailAddress;
            existing.PhoneNumber = employee.PhoneNumber;
            existing.Gender = employee.Gender;

            if (employee.CafeId != existing.CafeId)
            {
                if (employee.CafeId != null &&
                    await _context.Employees.AnyAsync(e => e.Id == employee.Id && e.CafeId != null))
                    return Conflict("Employee already assigned to a cafe");

                existing.CafeId = employee.CafeId;
                existing.StartDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}