using EFArchiver.WebTestApp.Data;
using EFArchiver.WebTestApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EFArchiver;

namespace EFArchiver.WebTestApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeopleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PeopleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPeople()
        {
            var people = await _context.People.ToListAsync();
            return Ok(people);
        }

        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveOldPeople()
        {
            var archiver = new EntityArchiver<Person>(_context);

            await archiver.ArchiveAsync(p => p.CreatedAt < new DateTime(2025, 1, 1), "Storage");

            return Ok("archived");
        }

        [HttpPost]
        public async Task<IActionResult> AddPerson([FromBody] Person person)
        {
            _context.People.Add(person);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPeople), new { person.Id }, person);
        }
    }
}
