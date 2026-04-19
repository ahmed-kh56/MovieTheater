using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRatingApp.Models;

namespace MovieRatingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly MovieDbContext _context;
        public GenresController(MovieDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var genresDto = await _context.Genres
                    .Include(g => g.Movies)
                    .Select(
                                g => new
                                {
                                    Id = g.Id,
                                    Name = g.Name,
                                    CreatedAt = g.CreatedAt,
                                    LastUpdate = g.LastUpdate,
                                    MoviesCount = g.Movies.Count
                                }
                            )
                    .ToListAsync();

                return Ok(genresDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] string name)
        {
            try
            {
                var genre = await _context.Genres.FirstOrDefaultAsync(g=>g.Id == id);
                if (genre == null)
                {
                    return NotFound($"Genre with id {id} not found.");
                }
                var audit = new AuditLog(AuditActionType.Updated, genre);


                genre.Update(name);
                await _context.AddAsync(audit);
                _context.Update(genre);

                await _context.SaveChangesAsync();

                return Ok(genre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("Name can't be empty.");

                var genre = new Genre(name);

                await _context.Genres.AddAsync(genre);

                await _context.SaveChangesAsync();

                return Ok(genre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
