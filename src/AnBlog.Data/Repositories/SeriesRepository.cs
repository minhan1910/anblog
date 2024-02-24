using AnBlog.Core.Domain.Content;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Content;
using AnBlog.Core.Repositories;
using AnBlog.Data.SeedWorks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AnBlog.Data.Repositories
{
    public class SeriesRepository : RepositoryBase<Series, Guid>, ISeriesRepository
    {
        private readonly AnBlogContext _context;
        private readonly IMapper _mapper;

        public SeriesRepository(AnBlogContext context, IMapper mapper) : base(context)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task AddPostToSeries(Guid seriesId, Guid postId, int sortOrder)
        {
            await _context.PostInSeries.AddAsync(new PostInSeries
            {
                PostId = postId,
                SeriesId = seriesId,
                DisplayOrder = sortOrder
            });
        }

        public async Task<bool> IsPostInSeries(Guid seriesId, Guid postId)
        {
            return await _context.PostInSeries.AnyAsync(x => x.SeriesId == seriesId && x.PostId == postId);
        }

        public async Task RemovePostToSeries(Guid seriesId, Guid postId)
        {
            var postInSeriesDb = await _context.PostInSeries.SingleOrDefaultAsync(x => x.SeriesId == seriesId && x.PostId == postId);

            if (postInSeriesDb is not null)
            {
                _context.PostInSeries.Remove(postInSeriesDb);
            }
        }

        public async Task<PagedResult<SeriesInListDto>> GetAllPaging(string? keyword, int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.Series.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword));
            }

            var totalRow = await query.CountAsync();

            var currentPage = pageIndex <= 0 ? 1 : pageIndex - 1;
            var skipRow = currentPage * pageSize;

            query = 
                query
                .OrderByDescending(x => x.DateCreated)
                .Skip(skipRow)
                .Take(pageSize);

            return new PagedResult<SeriesInListDto> 
            { 
                CurrentPage = currentPage,
                PageSize = pageSize,
                RowCount = totalRow,
                Results = await _mapper.ProjectTo<SeriesInListDto>(query).ToListAsync()
            };
        }

        public async Task<List<PostInListDto>> GetAllPostsInSeries(Guid seriesId)
        {
            var query = from pis in _context.PostInSeries
                        join p in _context.Posts
                        on pis.PostId equals p.Id
                        where pis.SeriesId == seriesId
                        select p;

            return await _mapper.ProjectTo<PostInListDto>(query).ToListAsync();
        }

    }
}