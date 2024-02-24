using AnBlog.Core.Domain.Identity;
using AnBlog.Core.Domain.Royalty;
using AnBlog.Core.Models.Royalty;
using AnBlog.Core.SeedWorks;
using AnBlog.Core.Services;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AnBlog.Data.Services
{
    public class RoyaltyService : IRoyaltyService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly string AppUsersTbl = $"{nameof(AppUser)}s";

        public RoyaltyService(UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<List<RoyaltyReportByMonthDto>> GetRoyaltyReportByMonthAsync(Guid? userId, int fromMonth, int fromYear, int toMonth, int toYear)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            if (conn.State == ConnectionState.Closed)
            {
                await conn.OpenAsync();
            }

            var coreSql = @"
            select
                datepart(month, p.DateCreated) as Month,
                datepart(year, p.DateCreated) as Year,
                sum(case when p.Status = 0 then 1 else 0 end) as NumberOfDraftPosts,
                sum(case when p.Status = 1 then 1 else 0 end) as NumberOfWaitingApprovalPosts,
                sum(case when p.Status = 2 then 1 else 0 end) as NumberOfRejectedPosts,
                sum(case when p.Status = 3 then 1 else 0 end) as NumberOfPublishedPosts,
                sum(case when p.Status = 3 and p.IsPaid = 1 then 1 else 0 end) as NumberOfPaidPublishedPosts,
                sum(case when p.Status = 3 and p.IsPaid = 0 then 1 else 0 end) as NumberOfUnpaidPublishedPosts
            from Posts p
            group by 
                datepart(month, p.DateCreated),
                datepart(year, p.DateCreated),
                p.AuthorUserId  
            having 
                (@fromMonth = 0 or datepart(month, p.DateCreated) >= @fromMonth)
                and (@fromYear = 0 or datepart(year, p.DateCreated) >= @fromYear)
                and (@toMonth = 0 or datepart(month, p.DateCreated) <= @toMonth)
                and (@toYear = 0 or datepart(year, p.DateCreated) <= @toYear)
                and (@userId is null or p.AuthorUserId = @userId)";

            var items = 
                await conn.QueryAsync<RoyaltyReportByMonthDto>(sql: coreSql, 
                                                               param: new
                                                               {
                                                                   fromMonth,
                                                                   fromYear,
                                                                   toMonth,
                                                                   toYear,
                                                                   userId
                                                               }, 
                                                               transaction: null, 
                                                               commandTimeout: 120, 
                                                               commandType: CommandType.Text);

            return items.AsList();
        }

        public async Task<List<RoyaltyReportByUserDto>> GetRoyaltyReportByUserAsync(Guid? userId, int fromMonth, int fromYear, int toMonth, int toYear)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            if (conn.State == ConnectionState.Closed)
            {
                await conn.OpenAsync();
            }

            var coreSql = @$"
            select
                u.Id as UserId,
                u.UserName as UserName,
                sum(case when p.Status = 0 then 1 else 0 end) as NumberOfDraftPosts,
                sum(case when p.Status = 1 then 1 else 0 end) as NumberOfWaitingApprovalPosts,
                sum(case when p.Status = 2 then 1 else 0 end) as NumberOfRejectedPosts,
                sum(case when p.Status = 3 then 1 else 0 end) as NumberOfPublishedPosts,
                sum(case when p.Status = 3 and p.IsPaid = 1 then 1 else 0 end) as NumberOfPaidPublishedPosts,
                sum(case when p.Status = 3 and p.IsPaid = 0 then 1 else 0 end) as NumberOfUnpaidPublishedPosts
            from Posts p
                join {AppUsersTbl} u on p.AuthorUserId = u.Id
            group by 
                datepart(month, p.DateCreated),
                datepart(year, p.DateCreated),
                p.AuthorUserId,
                u.Id,
                u.UserName
            having 
                (@fromMonth = 0 or datepart(month, p.DateCreated) >= @fromMonth)
                and (@fromYear = 0 or datepart(year, p.DateCreated) >= @fromYear)
                and (@toMonth = 0 or datepart(month, p.DateCreated) <= @toMonth)
                and (@toYear = 0 or datepart(year, p.DateCreated) <= @toYear)
                and (@userId is null or p.AuthorUserId = @userId)";

            var items =
                await conn.QueryAsync<RoyaltyReportByUserDto>(sql: coreSql,
                                                               param: new
                                                               {
                                                                   fromMonth,
                                                                   fromYear,
                                                                   toMonth,
                                                                   toYear,
                                                                   userId
                                                               },
                                                               transaction: null,
                                                               commandTimeout: 120,
                                                               commandType: CommandType.Text);

            return items.AsList();
        }

        public async Task PayRoyaltyForUserAsync(Guid fromUserId, Guid toUserId)
        {
            var fromUser = await _userManager.FindByIdAsync(fromUserId.ToString());

            if (fromUser == null)
            {
                throw new Exception($"User {fromUserId} not found");
            }

            var toUser = await _userManager.FindByIdAsync(toUserId.ToString());

            if (toUser == null)
            {
                throw new Exception($"User {toUserId} not found");
            }

            var totalUnpaidPosts = await _unitOfWork.Posts.GetListUnpaidPublishedPosts(toUserId);
            var totalRoaltyAmountPerPostsToPay = 0.0d;

            foreach (var unpaidPost in totalUnpaidPosts)
            {
                // Update from unpadid to paid status
                unpaidPost.IsPaid = true;
                unpaidPost.PaidDate = DateTime.Now;
                unpaidPost.RoyaltyAmount = toUser.RoyaltyAmountPerPost;

                // sum of the royalty amount to pay
                totalRoaltyAmountPerPostsToPay += toUser.RoyaltyAmountPerPost;
            }

            if (fromUser.CanBePayFor(totalRoaltyAmountPerPostsToPay))
            {
                fromUser.Balance -= totalRoaltyAmountPerPostsToPay;
                toUser.Balance += totalRoaltyAmountPerPostsToPay;
                await _userManager.UpdateAsync(toUser);
                await _userManager.UpdateAsync(fromUser);
            }
            else
            {
                throw new Exception(
                    $"From user with id: {fromUserId} can not enough balance to pay the roalty amount of all posts.");
            }

            _unitOfWork.Transactions.Add(new Transaction
            {
                FromUserName = fromUser.UserName ?? "From User",
                ToUserName = toUser.UserName ?? "To User",
                Amount = totalRoaltyAmountPerPostsToPay,
                DateCreated = DateTime.Now,
                FromUserId = fromUser.Id,
                ToUserId = toUser.Id,
                TransactionType = TransactionType.RoyaltyPay,
                Note = $"{fromUser.GetFullName()} paid the royalty amount for {toUser.GetFullName()}"
            });

            await _unitOfWork.CompleteAsync();
        }
    }
}