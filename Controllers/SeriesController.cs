using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRatingApp.Models.Common;
using MovieRatingApp.Models.Movies;
using System.ComponentModel.DataAnnotations;

namespace MovieRatingApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SeriesController : ControllerBase
    {
        private readonly MovieDbContext _context;
        public SeriesController(MovieDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody]
            [Required]
            string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("Name can't be empty.");

                var series = new FilmSeries(name);

                await _context.AddAsync(series);

                await _context.SaveChangesAsync();

                return Ok(series);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var series = await _context.FilmSeries
                    .Include(g => g.Movies)
                    .Select(
                                g => new
                                {
                                    g.Id,
                                    g.Name,
                                    MoviesCount = g.Movies.Count,
                                    Movies = g.Movies.Select(m => new
                                    {
                                        m.Id,
                                        m.Title,
                                        m.Description,
                                        PhotoUrl = m.PhotoShowUrl
                                     })
                                }
                            )
                    .ToListAsync();

                return Ok(series);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] string name)
        {
            try
            {
                var series = await _context.FilmSeries.FirstOrDefaultAsync(g => g.Id == id);
                if (series == null)
                {
                    return NotFound($"Genre with id {id} not found.");
                }
                var audit = new AuditLog(AuditActionType.Updated, series);

                series.Update(name);
                await _context.AddAsync(audit);
                _context.Update(series);

                await _context.SaveChangesAsync();

                return Ok(new { series, AuditId = audit.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
