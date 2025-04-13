using EFArchiver.WebTestApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace EFArchiver.WebTestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PartitionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PartitionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Partition()
        {
            var manager = new EFArchiverManager(_context);
            await manager.PartitionAllAsync("Storage");
            return Ok("Archiviazione completata");
        }
    }
}
