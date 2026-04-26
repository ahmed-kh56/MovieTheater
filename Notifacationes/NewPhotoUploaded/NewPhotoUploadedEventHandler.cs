using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MovieRatingApp.Notifacationes.NewPhotoUploaded
{
    public class NewPhotoUploadedEventHandler(Cloudinary _cloudinary, MovieDbContext _context, IWebHostEnvironment _env) : INotificationHandler<NewPhotoUploadedEvent>
    {

        public async Task Handle(NewPhotoUploadedEvent notification, CancellationToken cancellationToken)
        {
            var relativePath = notification.LocalPhotoUrl.TrimStart('/');
            var filePath = Path.Combine(_env.WebRootPath, relativePath);
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == notification.MovieId);

            if (!File.Exists(filePath)) return;

            try
            {
                var uploadResult = new ImageUploadResult();
                using (var stream = File.OpenRead(filePath))
                {
                    var filenameOverride = $"{Guid.NewGuid().ToString()}.{movie.Id}.{Path.GetFileName(filePath)}";

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(Path.GetFileName(filePath), stream),
                        Folder = "movies-images",
                        FilenameOverride = filenameOverride
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams, CancellationToken.None);
                }

                if (uploadResult.SecureUrl != null)
                {
                    if (movie != null)
                    {
                        movie.SetPhotoShowUrl(url: uploadResult.SecureUrl.ToString());

                        await _context.SaveChangesAsync(cancellationToken);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading to Cloudinary: {ex.Message}");
            }
        }
    }
}
