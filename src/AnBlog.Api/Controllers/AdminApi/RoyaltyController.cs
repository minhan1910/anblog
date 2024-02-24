using AnBlog.Api.Extensions;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Royalty;
using AnBlog.Core.SeedWorks;
using AnBlog.Core.SeedWorks.Constants;
using AnBlog.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnBlog.Api.Controllers.AdminApi
{
    public class RoyaltyController : AdminControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoyaltyService _royaltyService;
        public RoyaltyController(IUnitOfWork unitOfWork, IRoyaltyService royaltyService)
        {
            _unitOfWork = unitOfWork;
            _royaltyService = royaltyService;
        }

        [HttpGet("transaction-histories")]
        [Authorize(Permissions.Royalty.View)]
        public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactionHistory(string? keyword, 
                                                                                           int fromMonth,
                                                                                           int fromYear,
                                                                                           int toMonth,
                                                                                           int toYear,
                                                                                           int pageIndex,
                                                                                           int pageSize = 10)
        {
            var reuslt = await _unitOfWork.Transactions.GetAllPaging(keyword, fromMonth, fromYear, toMonth, toYear, pageIndex, pageSize);

            return Ok(reuslt);
        }

        [HttpGet("royalty-report-by-user")]
        [Authorize(Permissions.Royalty.View)]
        public async Task<ActionResult<List<RoyaltyReportByUserDto>>> GetRoyaltyReportByUser(Guid? userId,
                                                                                           int fromMonth,
                                                                                           int fromYear,
                                                                                           int toMonth,
                                                                                           int toYear)
        {
            var result = await _royaltyService.GetRoyaltyReportByUserAsync(userId, fromMonth, fromYear, toMonth, toYear);
            return Ok(result);
        }

        [HttpGet("royalty-report-by-month")]
        [Authorize(Permissions.Royalty.View)]
        public async Task<ActionResult<List<RoyaltyReportByMonthDto>>> GetRoyaltyReportByMonth(Guid? userId,
                                                                                               int fromMonth,
                                                                                               int fromYear,
                                                                                               int toMonth,
                                                                                               int toYear)
        {
            var result = await _royaltyService.GetRoyaltyReportByMonthAsync(userId, fromMonth, fromYear, toMonth, toYear);            
            return Ok(result);
        }

        [HttpPost("{userId}")]
        [Authorize(Permissions.Royalty.Pay)]
        public async Task<IActionResult> PayRoyalty(Guid userId)
        {
            var fromUserId = User.GetUserId();
            var toUserId = userId;

            await _royaltyService.PayRoyaltyForUserAsync(fromUserId, toUserId);
            return Ok();
        }

    }
}
