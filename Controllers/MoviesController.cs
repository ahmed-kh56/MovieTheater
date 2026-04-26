using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRatingApp.Extensions;
using MovieRatingApp.Models.Common;
using MovieRatingApp.Models.Movies;
using MovieRatingApp.Notifacationes.NewPhotoUploaded;
using MovieRatingApp.Requests;

namespace MovieRatingApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private static readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly long _maxFileSize = 4 * 1024 * 1024; // 4 MB

        private readonly MovieDbContext _context;
        public MoviesController(MovieDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] MovieQueryRequest request)
        {
            try
            {
                var query = _context.Movies
                    .Include(m => m.Series)
                    .Include(m=>m.MovieGenres)
                        .ThenInclude(g=>g.Genre)
                    .Include(m=>m.OldPhotos)
                    .ApplyFilters(request);

                var movies = await query
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        m.CreatedAt,
                        PhotoUrl = string.IsNullOrEmpty(m.PhotoShowUrl)
                               ? null
                               :m.PhotoShowUrl,
                        m.LastUpdate,
                        m.SeriesId,
                        SeriesName = m.Series.Name,
                        genres = m.MovieGenres
                            .Select(mg=>new 
                                {
                                    mg.Genre.Name,
                                    mg.Genre.Id
                                }
                            ).ToList()
                    })
                    .Skip(request.PageSize * request.Page)
                    .Take(request.PageSize)
                    .ToListAsync();

                return Ok(movies);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = "Invalid Filter Values", Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Server Error", Details = ex.Message });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromForm] CreateMovieRequest request
            )
        {
            try
            {
                string? photoUrl = null;
                if (request.Photo != null)
                {
                    photoUrl = await HandlePhoto(request.Photo);
                }

                var movie = new Movie(
                    request.Title,
                    request.Description,
                    photoUrl,
                    request.SeriesId,
                    request.OrderOnSeries
                );

                if(!string.IsNullOrWhiteSpace(photoUrl))
                {
                    var outbox = new EventsOutbox(new NewPhotoUploadedEvent(movie.Id, photoUrl));
                    await _context.AddAsync(outbox);
                }

                await _context.Movies.AddAsync(movie);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Movie created successfully!", movie });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = "Validation Failed", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal Server Error", Details = ex.Message });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateMovieRequest request)
        {
            try
            {
                var movie = await _context.Movies
                    .Include(m => m.OldPhotos)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (movie == null)
                    return NotFound(new { Message = $"Movie with ID {id} not found." });


                var audit = new AuditLog(AuditActionType.Updated, movie);
                await _context.AddAsync(audit);

                string? oldPhotoPath = movie.PhotoUrl;
                string? newPhotoUrl = null;
                if (request.Photo != null)
                {
                    newPhotoUrl = await HandlePhoto(request.Photo);
                    OldMoviePhoto oldPhoto = null;
                    if (!string.IsNullOrWhiteSpace(oldPhotoPath))
                    {
                        var deletedPhotoPath = await RenameOldPhotoToDeletedAsync(oldPhotoPath, audit.Id);
                        oldPhoto = new OldMoviePhoto(movie.Id, deletedPhotoPath, movie.PhotoShowUrl ?? "");
                    }
                    var outboxEvent = new EventsOutbox(new NewPhotoUploadedEvent(movie.Id, newPhotoUrl));
                     await _context.AddAsync(outboxEvent);
                }

                movie.Update(
                    title: request.Title,
                    description: request.Description,
                    photoUrl: newPhotoUrl
                );

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Movie updated successfully!", Movie = movie, AuditId = audit.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Code = "InternalServerError", Message = ex.Message });
            }
        }
        [Authorize(Roles ="Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie == null)
                    return NotFound(new { Message = "Movie not found" });

                var audit = new AuditLog(AuditActionType.Deleted, movie);
                await _context.Set<AuditLog>().AddAsync(audit);


                movie.MarkDeleted();
                _context.Movies.Update(movie);
                await _context.SaveChangesAsync();


                return Ok(new { Message = "Movie deleted successfully", AuditId = audit.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Server Error", Details = ex.Message });
            }
        }


        private async Task<string> HandlePhoto(IFormFile photo)
        {
            if (photo.Length > _maxFileSize)
                throw new ArgumentException($"File is too heavy! Max size allowed is {_maxFileSize / (1024 * 1024)}MB.");

            var extension = Path.GetExtension(photo.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"Invalid file type. Only {string.Join(", ", _allowedExtensions)} are allowed.");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "movies");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            return $"/uploads/movies/{fileName}";
        }
        private async Task<string?> RenameOldPhotoToDeletedAsync(string? photoUrl, Guid auditId)
        {
            if (string.IsNullOrWhiteSpace(photoUrl)) return null;

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var cleanPhotoUrl = photoUrl.TrimStart('/');
            var oldFullFilePath = Path.Combine(rootPath, cleanPhotoUrl);

            if (!System.IO.File.Exists(oldFullFilePath)) return photoUrl;

            var directoryPath = Path.GetDirectoryName(oldFullFilePath)!;
            var fileName = Path.GetFileName(oldFullFilePath);
            var newFileName = $"deleted_{auditId}_{fileName}";
            var newFullFilePath = Path.Combine(directoryPath, newFileName);

            await Task.Run(() => System.IO.File.Move(oldFullFilePath, newFullFilePath));

            var urlDirectory = Path.GetDirectoryName(photoUrl)?.Replace("\\", "/");
            var newUrl = Path.Combine(urlDirectory ?? "", newFileName).Replace("\\", "/");

            return newUrl.StartsWith("/") ? newUrl : "/" + newUrl;
        }
    }
}
