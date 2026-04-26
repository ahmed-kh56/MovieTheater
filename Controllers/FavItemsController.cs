using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRatingApp.Models.Common;
using MovieRatingApp.Models.FavLists;
using MovieRatingApp.Models.Movies;
using System.Security.Claims;

namespace MovieRatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavItemsController : ControllerBase
    {
        private readonly MovieDbContext _db;
        public FavItemsController(MovieDbContext db)
        {
            _db = db;
        }

        [HttpGet("Movies")]
        public async Task<IActionResult> GetMovies(
            [FromQuery] FavListAction? action = null,
            [FromQuery] int Page = 0,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);



                var favItems = await _db.FavListItems
                    .Where(f => f.Action == action || action == null)
                    .Include(f=>f.Movie)
                    .Select(f => new
                    {
                        f.Id,
                        f.MovieId,
                        f.Type,
                        f.Action,
                        f.Movie.Title,
                        f.Movie.Description,
                        f.Movie.PhotoShowUrl,
                        f.CreatedAt
                    })
                    .Skip(pageSize * Page)
                    .Take(pageSize)
                    .ToListAsync();
                return Ok(favItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
        [HttpGet("Series")]
        public async Task<IActionResult> GetSerieses(
            [FromQuery] FavListAction? action = null,
            [FromQuery] int Page = 0,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);



                var favItems = await _db.FavListItems
                    .Where(f => f.Action == action || action == null)
                    .Include(f => f.Movie)
                    .Select(f => new
                    {
                        f.Id,
                        f.MovieId,
                        f.Type,
                        f.Action,
                        f.Movie.Title,
                        f.Movie.Description,
                        f.Movie.PhotoShowUrl,
                        f.CreatedAt
                    })
                    .Skip(pageSize*Page)
                    .Take(pageSize)
                    .ToListAsync();
                return Ok(favItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("{id:guid}")]
        public async Task<IActionResult> AddToMyList(
            [FromRoute] Guid id,
            [FromQuery] FavListItemType type,
            [FromQuery] FavListAction action = FavListAction.ToWatch)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

                FilmSeries series = null;
                Movie movie = null;

                if (type == FavListItemType.Series)
                    series = await _db.FilmSeries.FirstOrDefaultAsync(s => s.Id == id);
                else
                    movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == id);


                var favListItem = new FavListItem(movie, series, userId, action);

                await _db.AddAsync(favListItem);
                await _db.SaveChangesAsync();

                return Ok(favListItem);


            }
            catch(Exception ex)
            {
                return StatusCode(500,new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateFavListItem(
            [FromRoute] Guid id,
            [FromQuery] FavListAction action)
        {
            try
            {
                var favListItem = await _db.FavListItems.FirstOrDefaultAsync(f => f.Id == id);
                if (favListItem == null)
                    return NotFound("Fav list item not found.");


                var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                if (userId != favListItem.UserId)
                    return Unauthorized("Cant delete something not yours.");

                var audit = new AuditLog(AuditActionType.Updated, favListItem);
                await _db.AddAsync(audit);

                favListItem.Update(action);
                _db.FavListItems.Update(favListItem);
                await _db.SaveChangesAsync();
                return Ok(new { favListItem, AuditId = audit.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }



        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFavListItem(Guid id)
        {
            try
            {
                var favListItem = await _db.FavListItems.FirstOrDefaultAsync(f => f.Id == id);

                if (favListItem == null)
                    return NotFound("Fav list item not found.");

                var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                if (userId != favListItem.UserId)
                    return Unauthorized("Cant delete something not yours.");


                var audit = new AuditLog(AuditActionType.Deleted, favListItem);
                await _db.AddAsync(audit);
                _db.FavListItems.Remove(favListItem);
                await _db.SaveChangesAsync();
                return Ok(audit.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }


    }
}
