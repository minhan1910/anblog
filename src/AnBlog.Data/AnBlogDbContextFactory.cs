using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AnBlog.Data
{
    public class AnBlogDbContextFactory : IDesignTimeDbContextFactory<AnBlogContext>
    {
        public AnBlogContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            
            var builder = new DbContextOptionsBuilder<AnBlogContext>();
            builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new AnBlogContext(builder.Options);
        }
    }
}