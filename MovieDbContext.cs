using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MovieRatingApp.Models;

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

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);

                await _transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await _transaction.RollbackAsync(cancellationToken);
                throw;
            }
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
    }
}
