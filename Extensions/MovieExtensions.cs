using MovieRatingApp.Models;
using MovieRatingApp.Requests;

namespace MovieRatingApp.Extensions
{
    public static class MovieExtensions
    {
        public static IQueryable<Movie> ApplyFilters(this IQueryable<Movie> query, MovieQueryRequest req)
        {
            // 1. قفش العبط (Validation)
            if (req.FromDate.HasValue && req.ToDate.HasValue && req.FromDate > req.ToDate)
                throw new ArgumentException("Creation 'FromDate' can't be after 'ToDate'.");

            if (req.UpdateFromDate.HasValue && req.UpdateToDate.HasValue && req.UpdateFromDate > req.UpdateToDate)
                throw new ArgumentException("Update 'FromDate' can't be after 'ToDate'.");

            // 2. الفلترة الأساسية (نفس الكود بتاعك حرفياً)
            if (req.MovieId.HasValue) query = query.Where(m => m.Id == req.MovieId);
            if (req.GenreId.HasValue) query = query.Where(m => m.GenreId == req.GenreId);

            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            {
                var lowerSearch = req.SearchTerm.Trim().ToLower();
                query = query.Where(m => m.Title.ToLower().Contains(lowerSearch) ||
                                         m.Genre.Name.ToLower().Contains(lowerSearch));
            }

            // تواريخ الكرييت
            if (req.FromDate.HasValue) query = query.Where(m => m.CreatedAt >= req.FromDate);
            if (req.ToDate.HasValue) query = query.Where(m => m.CreatedAt <= req.ToDate);

            // تواريخ الابديت
            if (req.UpdateFromDate.HasValue) query = query.Where(m => m.LastUpdate >= req.UpdateFromDate);
            if (req.UpdateToDate.HasValue) query = query.Where(m => m.LastUpdate <= req.UpdateToDate);

            // 3. الترتيب (Switch Case)
            query = req.SortBy?.ToLower() switch
            {
                "title" => req.IsDescending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
                "genre" => req.IsDescending ? query.OrderByDescending(m => m.Genre.Name) : query.OrderBy(m => m.Genre.Name),
                "update" => req.IsDescending ? query.OrderByDescending(m => m.LastUpdate) : query.OrderBy(m => m.LastUpdate),
                _ => req.IsDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt)
            };

            return query;
        }
    }
}
