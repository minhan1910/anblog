using AnBlog.Core.Domain.Identity;
using AnBlog.Core.Repositories;
using AnBlog.Core.SeedWorks;
using AnBlog.Data.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace AnBlog.Data.SeedWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AnBlogContext _context;

        public UnitOfWork(AnBlogContext context, 
                          IMapper mapper, 
                          UserManager<AppUser> userManager, 
                          RoleManager<AppRole> roleManager)
        {
            _context = context;
            Posts = new PostRepository(context, userManager, mapper, roleManager);
            PostCategories = new PostCategoryRepository(context, mapper);   
            Series = new SeriesRepository(context, mapper);
            Transactions = new TransactionRepository(context, mapper);
        }

        public IPostRepository Posts { get; private set; }

        public IPostCategoryRepository PostCategories { get; private set; }

        public ISeriesRepository Series { get; private set; }

        public ITransactionRepository Transactions { get; private set; }

        public async Task<int> CompleteAsync() 
            => await _context.SaveChangesAsync();

        public void Dispose()
            => _context.Dispose();
    }
}