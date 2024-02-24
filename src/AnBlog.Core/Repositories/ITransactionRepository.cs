using AnBlog.Core.Domain.Royalty;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Royalty;
using AnBlog.Core.SeedWorks;

namespace AnBlog.Core.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction, Guid>
    {
        Task<PagedResult<TransactionDto>> GetAllPaging(string? userName,
                                                       int fromMonth,
                                                       int fromYear,
                                                       int toMonth,
                                                       int toYear,
                                                       int pageIndex = 1,
                                                       int pageSize = 10);
    }
}