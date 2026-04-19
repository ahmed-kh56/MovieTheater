using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRatingApp.Extensions;
using MovieRatingApp.Models;
using MovieRatingApp.Requests;

namespace MovieRatingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
                    .Include(m => m.Genre)
                    .ApplyFilters(request);

                var movies = await query
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        m.CreatedAt,
                        PhotoUrl = string.IsNullOrEmpty(m.PhotoUrl)
                               ? null
                               : $"{Request.Scheme}://{Request.Host}{m.PhotoUrl}",
                        m.LastUpdate,
                        m.GenreId,
                        GenreName = m.Genre.Name
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


        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateMovieRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie == null)
                    return NotFound(new { Message = $"Movie with ID {id} not found." });


                var audit = new AuditLog(AuditActionType.Updated, movie);
                await _context.Set<AuditLog>().AddAsync(audit);

                string? oldPhotoPath = movie.PhotoUrl;
                string? newPhotoUrl = null;

                if (request.Photo != null)
                {
                    newPhotoUrl = await HandlePhoto(request.Photo);

                    if (!string.IsNullOrWhiteSpace(oldPhotoPath))
                        RenameOldPhotoToDeleted(oldPhotoPath, audit.Id);
                }

                movie.Update(
                    title: request.Title,
                    description: request.Description,
                    photoUrl: newPhotoUrl
                );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Movie updated successfully!", Movie = movie, AuditId = audit.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Error = "Internal Server Error", Details = ex.Message });
            }
        }

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
                    request.GenreId,
                    request.Description,
                    photoUrl
                );

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


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie == null)
                    return NotFound(new { Message = "Movie not found" });

                var audit = new AuditLog(AuditActionType.Deleted, movie);
                await _context.Set<AuditLog>().AddAsync(audit);

                RenameOldPhotoToDeleted(movie.PhotoUrl, audit.Id);

                _context.Movies.Remove(movie);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Movie deleted successfully", AuditId = audit.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
        private void RenameOldPhotoToDeleted(string? photoUrl, Guid auditId)
        {
            if (string.IsNullOrEmpty(photoUrl)) return;

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var oldFilePath = Path.Combine(rootPath, photoUrl.TrimStart('/'));

            if (System.IO.File.Exists(oldFilePath))
            {
                try
                {
                    var directory = Path.GetDirectoryName(oldFilePath)!;
                    var fileName = Path.GetFileName(oldFilePath);
                    // الاسم الجديد: deleted_Guid_OriginalName
                    var newFileName = $"deleted_{auditId}_{fileName}";
                    var newFilePath = Path.Combine(directory, newFileName);

                    System.IO.File.Move(oldFilePath, newFilePath);
                }
                catch
                {
                }
            }
        }
    }
}
