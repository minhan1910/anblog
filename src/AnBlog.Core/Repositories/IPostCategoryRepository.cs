using AnBlog.Core.Domain.Content;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Content;
using AnBlog.Core.SeedWorks;

namespace AnBlog.Core.Repositories
{
    public interface IPostCategoryRepository : IRepository<PostCategory, Guid>
    {
        Task<PagedResult<PostCategoryDto>> GetAllPaging(string? keyword, int pageIndex = 1, int pageSize = 10);
    }
}