using CafeBackEndWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using CafeBackEndWebApi.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CafeBackEndWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CafeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CafeController(ApplicationDbContext context) => _context = context;

        [HttpGet("{id}")]
        public async Task<ActionResult<Cafe>> GetCafeById(Guid id)
        {
            var cafe = await _context.Cafes.FindAsync(id);
            if (cafe == null)
                return NotFound();

            return cafe;
        }
       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCafes([FromQuery] string? location)
        {
            var cafes = _context.Cafes.Include(c => c.Employees).AsQueryable();

            if (!string.IsNullOrWhiteSpace(location))
                cafes = cafes.Where(c => c.Location == location);

            var result = await cafes
                .Select(c => new {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Logo,
                    c.Location,
                    Employees = c.Employees.Count
                })
                .OrderByDescending(c => c.Employees)
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateCafe([FromForm] Cafe cafe, IFormFile logo)
        {
            cafe.Id = Guid.NewGuid();
            if (logo != null && logo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(logo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await logo.CopyToAsync(fileStream);
                }

                // Set the logo path to relative URL (for web display)
                cafe.Logo = "/uploads/" + uniqueFileName;
            }
            _context.Cafes.Add(cafe);
            await _context.SaveChangesAsync();
            return Ok(cafe);
        }

      
        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCafe([FromForm] Cafe cafe, IFormFile logo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCafe = await _context.Cafes.FindAsync(cafe.Id);
            if (existingCafe == null)
            {
                return NotFound("Cafe not found.");
            }

            // Save the new logo file if provided
            if (logo != null && logo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(logo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await logo.CopyToAsync(fileStream);
                }

                // Set the logo path to relative URL (for web display)
                existingCafe.Logo = "/uploads/" + uniqueFileName;
            }

            // Update other fields
            existingCafe.Name = cafe.Name;
            existingCafe.Description = cafe.Description;
            existingCafe.Location = cafe.Location;
           // existingCafe.Employees = cafe.Employees;

            await _context.SaveChangesAsync();

            return Ok(existingCafe);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCafe(Guid id)
        {
            var cafe = await _context.Cafes.Include(c => c.Employees).FirstOrDefaultAsync(c => c.Id == id);
            if (cafe == null) return NotFound();

            _context.Cafes.Remove(cafe);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}

