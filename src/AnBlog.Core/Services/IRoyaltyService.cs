using AnBlog.Core.Models.Royalty;

namespace AnBlog.Core.Services
{
    public interface IRoyaltyService
    {
        Task<List<RoyaltyReportByMonthDto>> GetRoyaltyReportByMonthAsync(Guid? userId, int fromMonth, int fromYear, int toMonth, int toYear);

        Task<List<RoyaltyReportByUserDto>> GetRoyaltyReportByUserAsync(Guid? userId, int fromMonth, int fromYear, int toMonth, int toYear);

        Task PayRoyaltyForUserAsync(Guid fromUserId, Guid toUserId);
    }
}