using AnBlog.Core.Domain.Content;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Content;
using AnBlog.Core.Repositories;
using AnBlog.Data.SeedWorks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AnBlog.Data.Repositories
{
    public class PostCategoryRepository : RepositoryBase<PostCategory, Guid>, IPostCategoryRepository
    {
        private readonly AnBlogContext _context;
        private readonly IMapper _mapper;

        public PostCategoryRepository(AnBlogContext context, IMapper mapper) : base(context)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task<List<Post>> GetPopularPostAsync(int count)
            => await _context.Posts.OrderByDescending(post => post.ViewCount).Take(count).ToListAsync();

        public async Task<PagedResult<PostCategoryDto>> GetAllPaging(string? keyword, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.PostCategories.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(post => post.Name.Contains(keyword));
            }

            var totalRow = await query.CountAsync();
            
            var skipRow = (pageIndex - 1 < 0 ? 1 : pageIndex - 1) * pageIndex;
            
            var result =
                 query
                .Skip(skipRow)
                .Take(pageSize);

            return new PagedResult<PostCategoryDto>
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount = totalRow,
                Results = await _mapper.ProjectTo<PostCategoryDto>(query).ToListAsync(),
            };
        }
    }
}