using AnBlog.Core.Domain.Identity;
using AnBlog.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AnBlog.Api
{
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            using var anBlogContext = scope.ServiceProvider.GetRequiredService<AnBlogContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

            // applying update-database command
            anBlogContext.Database.Migrate();
            DataSeeder.SeedAsync(anBlogContext, roleManager).GetAwaiter().GetResult();

            return app;
        } 
    }
}
