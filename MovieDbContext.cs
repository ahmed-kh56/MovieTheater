using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using MovieRatingApp.Models.Auth;
using MovieRatingApp.Models.Common;
using MovieRatingApp.Models.FavLists;
using MovieRatingApp.Models.Genres;
using MovieRatingApp.Models.Movies;

namespace MovieRatingApp
{
    public class MovieDbContext : DbContext
    {
        private readonly IDbContextTransaction _transaction;
        public MovieDbContext(DbContextOptions<MovieDbContext> options)
            : base(options)
        {
            _transaction = Database.BeginTransaction();
        }



        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var result = await base.SaveChangesAsync(cancellationToken);

            await _transaction.CommitAsync(cancellationToken);

            return result;
        }

        public override void Dispose()
        {
            _transaction?.Dispose();
            base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            if (_transaction != null) await _transaction.DisposeAsync();
            await base.DisposeAsync();
        }



        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<EventsOutbox> EventsOutbox { get; set; }
        public DbSet<OldMoviePhoto> OldPhotos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FilmSeries> FilmSeries { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<FavListItem> FavListItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // المناداة على الميثودز القديمة اللي انت كتبتها
            ConfigureMovieEntity(modelBuilder.Entity<Movie>());
            ConfigureGenreEntity(modelBuilder.Entity<Genre>());
            ConfigureOldMoviePhotoEntity(modelBuilder.Entity<OldMoviePhoto>());
            ConfigureEventsOutboxEntity(modelBuilder.Entity<EventsOutbox>());
            ConfigureAuditLogEntity(modelBuilder.Entity<AuditLog>());

            // المناداة على الميثودز الجديدة
            ConfigureUserEntity(modelBuilder.Entity<User>());
            ConfigureMovieGenreEntity(modelBuilder.Entity<MovieGenre>());
            ConfigureFilmSeriesEntity(modelBuilder.Entity<FilmSeries>());
            ConfigureFavListItemEntity(modelBuilder.Entity<FavListItem>());

            base.OnModelCreating(modelBuilder);
        }
        private void ConfigureMovieEntity(EntityTypeBuilder<Movie> modelBuilder)
        {
            modelBuilder
                .HasOne(m => m.Series)
                .WithMany(g => g.Movies)
                .HasForeignKey(m => m.SeriesId);
            modelBuilder
                .HasMany(m => m.OldPhotos)
                .WithOne(p => p.Movie)
                .HasForeignKey(p => p.MovieId);

            modelBuilder.Property(m => m.Title)
                .IsRequired()
                .HasMaxLength(300);
            modelBuilder.Property(m => m.Description)
                .HasMaxLength(1000);
            modelBuilder.Property(m => m.PhotoUrl)
                .HasMaxLength(2000);
            modelBuilder.Property(m => m.PhotoShowUrl)
                .HasMaxLength(2000);
        }
        private void ConfigureGenreEntity(EntityTypeBuilder<Genre> modelBuilder)
        {
            modelBuilder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
        private void ConfigureAuditLogEntity(EntityTypeBuilder<AuditLog> modelBuilder)
        {
            modelBuilder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Property(a => a.AuditAction)
                .IsRequired()
                .HasMaxLength(50);
            modelBuilder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Property(a => a.EntityId)
                .IsRequired();

        }
        private void ConfigureEventsOutboxEntity(EntityTypeBuilder<EventsOutbox> modelBuilder)
        {
            modelBuilder.Property(e => e.Id)
                .IsRequired();
            modelBuilder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(200);
            modelBuilder.Property(e => e.Notification)
                .HasMaxLength(5000)
                .IsRequired();
            modelBuilder.Property(e => e.IsHandled)
                .IsRequired();
        }
        private void ConfigureOldMoviePhotoEntity(EntityTypeBuilder<OldMoviePhoto> modelBuilder)
        {
            modelBuilder.Property(p => p.Id)
                .IsRequired();
            modelBuilder.Property(p => p.MovieId)
                .IsRequired();
            modelBuilder.Property(p => p.LocalPhotoUrl)
                .HasMaxLength(2000)
                .IsRequired();
            modelBuilder.Property(p => p.PhotoShowUrl)
                .HasMaxLength(2000);


        }

        private void ConfigureUserEntity(EntityTypeBuilder<User> modelBuilder)
        {
            modelBuilder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(500); // طول مناسب للـ Hashed Passwords

            modelBuilder.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.HasData(
                    new User("AmrMousv", "Password123", Role.Admin),
                    new User("AhmedZain", "Password123", Role.Admin)
                );

        }

        private void ConfigureMovieGenreEntity(EntityTypeBuilder<MovieGenre> modelBuilder)
        {
            // تعريف الـ Composite Key (مهم جداً لجدول الربط)
            modelBuilder.HasKey(mg => new { mg.MovieId, mg.GenreId });

            modelBuilder
                .HasOne(mg => mg.Movie)
                .WithMany() // لو مفيش Navigation Property في Movie للـ MovieGenres
                .HasForeignKey(mg => mg.MovieId);

            modelBuilder
                .HasOne(mg => mg.Genre)
                .WithMany()
                .HasForeignKey(mg => mg.GenreId);
        }

        private void ConfigureFilmSeriesEntity(EntityTypeBuilder<FilmSeries> modelBuilder)
        {
            modelBuilder.Property(fs => fs.Name)
                .IsRequired()
                .HasMaxLength(300);

            modelBuilder.HasMany(fs => fs.Movies)
                .WithOne(m => m.Series)
                .HasForeignKey(m => m.SeriesId);
        }

        private void ConfigureFavListItemEntity(EntityTypeBuilder<FavListItem> modelBuilder)
        {
            modelBuilder.HasOne(f => f.User)
                .WithMany(u => u.FavListItems)
                .HasForeignKey(f => f.UserId);

            modelBuilder.HasOne(f => f.Movie)
                .WithMany()
                .HasForeignKey(f => f.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.HasOne(f => f.Series)
                .WithMany()
                .HasForeignKey(f => f.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Property(f => f.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Property(f => f.Action)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Property(f => f.CreatedAt)
                .IsRequired();
        }
    }

}
