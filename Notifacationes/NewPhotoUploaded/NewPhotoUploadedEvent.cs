using MediatR;

namespace MovieRatingApp.Notifacationes.NewPhotoUploaded
{
    public record NewPhotoUploadedEvent(Guid MovieId, string LocalPhotoUrl) : INotification;
}
