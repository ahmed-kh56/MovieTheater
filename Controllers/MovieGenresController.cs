using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRatingApp.Extensions;
using MovieRatingApp.Models.Genres;
using MovieRatingApp.Requests;

namespace MovieRatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieGenresController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public MovieGenresController(MovieDbContext context)
        {
            _context = context;
        }

        [HttpPost("{movieId}/{genreId}")]
        public async Task<IActionResult> Add(
            [FromRoute] Guid movieId,
            [FromRoute] Guid genreId)
        {
            try
            {
                if (movieId == Guid.Empty || genreId == Guid.Empty)
                    return BadRequest();

                var movie = _context.Movies.FirstOrDefault(m=> m.Id==movieId);
                var genre = _context.Genres.FirstOrDefault(m=> m.Id==genreId);

                if (movie is null || genre is null)
                    return BadRequest();

                var moviegenre = new MovieGenre(movieId, genreId);

                await _context.AddAsync(moviegenre);

                await _context.SaveChangesAsync();

                return Ok(moviegenre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Server Error", Details = ex.Message });
            }
        }
        [HttpDelete("{movieId}/{genreId}")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid movieId,
            [FromRoute] Guid genreId)
        {
            try
            {
                if (movieId == Guid.Empty || genreId == Guid.Empty)
                    return BadRequest();

                var moviegenre = await _context.MovieGenres.FirstOrDefaultAsync(mg=>mg.MovieId ==movieId && mg.GenreId== genreId);

                if (moviegenre is null)
                    return BadRequest();


                _context.Remove(moviegenre);

                await _context.SaveChangesAsync();

                return Ok(moviegenre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Server Error", Details = ex.Message });
            }
        }
    }
}
