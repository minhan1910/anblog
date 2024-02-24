using AnBlog.Core.Domain.Content;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Content;
using AnBlog.Core.SeedWorks;

namespace AnBlog.Core.Repositories
{
    public interface IPostRepository : IRepository<Post, Guid>
    {
        Task<List<Post>> GetPopularPostAsync(int count);

        Task<PagedResult<PostInListDto>> GetAllPaging(string? keyword, Guid currentUserId, Guid? categoryId, int pageIndex = 1, int pageSize = 10);

        Task<List<SeriesInListDto>> GetAllSeries(Guid postId);

        Task<bool> IsSlugAlreadyExisted(string slug, Guid? currentId = null);

        Task<List<PostActivityLogDto>> GetActivityLogs(Guid postId);

        Task Approve(Guid postId, Guid currentUserId);

        Task SendToApprove(Guid postId, Guid currentUserId);

        Task ReturnBack(Guid postId, Guid currentUserId, string note);

        Task<string> GetReturnReason(Guid postId);

        Task<bool> HasPublishInLast(Guid postId);

        Task<List<Post>> GetListUnpaidPublishedPosts(Guid userId);
    }
}