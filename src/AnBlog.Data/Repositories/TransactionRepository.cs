using AnBlog.Core.Domain.Royalty;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Royalty;
using AnBlog.Core.Repositories;
using AnBlog.Data.SeedWorks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AnBlog.Data.Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction, Guid>, ITransactionRepository
    {
        private readonly AnBlogContext _context;
        private readonly IMapper _mapper;

        public TransactionRepository(AnBlogContext context, IMapper mapper) : base(context)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task<PagedResult<TransactionDto>> GetAllPaging(string? userName,
                                                                    int fromMonth,
                                                                    int fromYear,
                                                                    int toMonth,
                                                                    int toYear,
                                                                    int pageIndex = 1,
                                                                    int pageSize = 10)
        {
            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = 
                    query
                    .Where(t => t.FromUserName.Contains(userName) 
                             || t.ToUserName.Contains(userName));
            }

            if (fromMonth > 0 && fromYear > 0)
            {
                query =
                    query
                    .Where(t => t.DateCreated.Month >= fromMonth && t.DateCreated.Year >= fromYear);
            }

            if (toMonth > 0 && toYear > 0)
            {
                query =
                    query
                    .Where(t => t.DateCreated.Month <= toMonth && t.DateCreated.Year <= toYear);
            }

            var totalRow = await query.CountAsync();

            var currentPageIndex = pageIndex == 0 ? 1 : pageIndex;
            var skipPage = (currentPageIndex - 1) * pageSize;

            query =
                query
                .OrderByDescending(t => t.DateCreated)
                .Skip(skipPage)
                .Take(pageSize);

            return new PagedResult<TransactionDto>
            {
                CurrentPage = currentPageIndex,
                PageSize = pageSize,
                Results = await _mapper.ProjectTo<TransactionDto>(query).ToListAsync(),
                RowCount = totalRow
            };
        }
    }
}