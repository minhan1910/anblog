using AnBlog.Core.Domain.Content;
using AnBlog.Core.Models.Content;
using AnBlog.Core.Models;
using AnBlog.Core.SeedWorks;

namespace AnBlog.Core.Repositories
{
    public interface ISeriesRepository : IRepository<Series, Guid>
    {
        Task<PagedResult<SeriesInListDto>> GetAllPaging(string? keyword, int pageIndex = 1, int pageSize = 10);

        Task AddPostToSeries(Guid seriesId, Guid postId, int sortOrder);

        Task RemovePostToSeries(Guid seriesId, Guid postId);

        Task<List<PostInListDto>> GetAllPostsInSeries(Guid seriesId);

        Task<bool> IsPostInSeries(Guid seriesId, Guid postId);
    }
}