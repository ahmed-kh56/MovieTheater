using MovieRatingApp.Models.Movies;
using MovieRatingApp.Requests;

namespace MovieRatingApp.Extensions
{
    public static class MovieExtensions
    {
        public static IQueryable<Movie> ApplyFilters(this IQueryable<Movie> query, MovieQueryRequest req)
        {
            if (req.FromDate.HasValue && req.ToDate.HasValue && req.FromDate > req.ToDate)
                throw new ArgumentException("Creation 'FromDate' can't be after 'ToDate'.");

            if (req.UpdateFromDate.HasValue && req.UpdateToDate.HasValue && req.UpdateFromDate > req.UpdateToDate)
                throw new ArgumentException("Update 'FromDate' can't be after 'ToDate'.");

            if (req.MovieId.HasValue) query = query.Where(m => m.Id == req.MovieId);
            if (req.SeriesId.HasValue) query = query.Where(m => m.SeriesId == req.SeriesId);

            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            {
                var lowerSearch = req.SearchTerm.Trim().ToLower();
                query = query.Where(m => m.Title.ToLower().Contains(lowerSearch) ||
                                         m.Series.Name.ToLower().Contains(lowerSearch));
            }

            // تواريخ الكرييت
            if (req.FromDate.HasValue) query = query.Where(m => m.CreatedAt >= req.FromDate);
            if (req.ToDate.HasValue) query = query.Where(m => m.CreatedAt <= req.ToDate);

            if (req.UpdateFromDate.HasValue) query = query.Where(m => m.LastUpdate >= req.UpdateFromDate);
            if (req.UpdateToDate.HasValue) query = query.Where(m => m.LastUpdate <= req.UpdateToDate);

            query = req.SortBy?.ToLower() switch
            {
                "title" => req.IsDescending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
                "genre" => req.IsDescending ? query.OrderByDescending(m => m.Series.Name) : query.OrderBy(m => m.Series.Name),
                "update" => req.IsDescending ? query.OrderByDescending(m => m.LastUpdate) : query.OrderBy(m => m.LastUpdate),
                _ => req.IsDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt)
            };

            return query;
        }
    }
}
