using AnBlog.Core.Domain.Content;
using AnBlog.Core.Domain.Identity;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Content;
using AnBlog.Core.Repositories;
using AnBlog.Core.SeedWorks.Constants;
using AnBlog.Data.SeedWorks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AnBlog.Data.Repositories
{
    public class PostRepository : RepositoryBase<Post, Guid>, IPostRepository
    {
        private readonly AnBlogContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;

        public PostRepository(AnBlogContext context,
                              UserManager<AppUser> userManager,
                              IMapper mapper,
                              RoleManager<AppRole> roleManager)
            : base(context)
        {
            this._context = context;
            this._mapper = mapper;
            this._userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<Post>> GetPopularPostAsync(int count)
            => await _context.Posts.OrderByDescending(post => post.ViewCount).Take(count).ToListAsync();

        private async Task<bool> IsProperRoleAndPostApprovedAsync(IEnumerable<string> roleNames)
        {
            if (roleNames.Contains(Roles.Admin))
            {
                return true;
            }
            
            var roles = new HashSet<string>();

            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);

                if (role is not null)
                {
                    roles.Add(role.Id.ToString());
                }
            }

            return await _context
                .RoleClaims
                .AnyAsync(rc => roles.Contains(rc.RoleId.ToString()) && 
                                rc.ClaimValue == Permissions.Posts.Approve);
        }

        public async Task<PagedResult<PostInListDto>> GetAllPaging(string? keyword, 
                                                                   Guid currentUserId, 
                                                                   Guid? categoryId, 
                                                                   int pageIndex = 1, 
                                                                   int pageSize = 10)
        {
            var user = await _userManager.FindByIdAsync(currentUserId.ToString());

            if (user is null)
            {
                throw new Exception("No user was found");
            }

            var roleNames = await _userManager.GetRolesAsync(user);

            var canApprove = await IsProperRoleAndPostApprovedAsync(roleNames);

            var query = _context.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(post => post.Name.Contains(keyword));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            if (!canApprove)
            {
                query = query.Where(x => x.AuthorUserId == currentUserId);
            }

            var totalRow = await query.CountAsync(); 

            var skipRow = (pageIndex - 1 < 0 ? 1 : pageIndex - 1) * pageIndex;

            var result =
                query
                .OrderByDescending(x => x.DateCreated)
                .Skip(skipRow)
                .Take(pageSize);

            return new PagedResult<PostInListDto>
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                RowCount = totalRow,
                Results = await _mapper.ProjectTo<PostInListDto>(query).ToListAsync(),
            };
        }

        public async Task<List<SeriesInListDto>> GetAllSeries(Guid postId)
        {
            var query =
                from pis in _context.PostInSeries
                join series in _context.Series
                on pis.SeriesId equals series.Id
                where pis.PostId == postId
                select series;

            return await _mapper.ProjectTo<SeriesInListDto>(query).ToListAsync();
        }

        public async Task<bool> IsSlugAlreadyExisted(string slug, Guid? currentId = null)
        {
            if (currentId.HasValue)
            {
                return await _context.Posts.AnyAsync(p => p.Slug == slug && p.Id != currentId.Value);
            }

            return await _context.Posts.AnyAsync(p => p.Slug == slug);
        }

        public async Task<List<PostActivityLogDto>> GetActivityLogs(Guid postId)
        {
            var query =
                _context
                .PostActivityLogs
                .Where(x => x.PostId == postId)
                .OrderByDescending(x => x.DateCreated);

            return await _mapper.ProjectTo<PostActivityLogDto>(query).ToListAsync();
        }

        public async Task Approve(Guid postId, Guid currentUserId)
        {
            var postInDb = await GetByIdAsync(postId);

            if (postInDb is null)
            {
                throw new Exception("Không tồn tại bài viết");
            }

            var user = await _context.Users.FindAsync(currentUserId);

            await _context.PostActivityLogs.AddAsync(new PostActivityLog
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = currentUserId,
                Username = user?.UserName,
                FromStatus = postInDb.Status, 
                ToStatus = PostStatus.Published,
                Note = $"{user?.UserName} duyệt bài"
            });

            postInDb.Status = PostStatus.Published;
            _context.Posts.Update(postInDb);
        }

        public async Task SendToApprove(Guid postId, Guid currentUserId)
        {
            var postInDb = await GetByIdAsync(postId);

            if (postInDb is null)
            {
                throw new Exception("Không tồn tại bài viết");
            }

            var user = await _context.Users.FindAsync(currentUserId);

            await _context.PostActivityLogs.AddAsync(new PostActivityLog
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = currentUserId,
                Username = user?.UserName,
                FromStatus = postInDb.Status,
                ToStatus = PostStatus.WaitingForApproval,
                Note = $"{user?.UserName} gửi bài chờ duyệt bài duyệt bài"
            });

            postInDb.Status = PostStatus.WaitingForApproval;
            _context.Posts.Update(postInDb);
        }

        public async Task ReturnBack(Guid postId, Guid currentUserId, string note)
        {
            var postInDb = await GetByIdAsync(postId);

            if (postInDb is null)
            {
                throw new Exception("Không tồn tại bài viết");
            }

            var user = await _context.Users.FindAsync(currentUserId);

            await _context.PostActivityLogs.AddAsync(new PostActivityLog
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = currentUserId,
                Username = user?.UserName,
                FromStatus = postInDb.Status,
                ToStatus = PostStatus.Rejected,
                Note = note
            });

            postInDb.Status = PostStatus.Rejected;
            _context.Posts.Update(postInDb);
        }

        public async Task<string> GetReturnReason(Guid postId)
        {
            var activity = 
                await _context
                .PostActivityLogs
                .Where(pal => pal.PostId == postId && pal.ToStatus == PostStatus.Rejected)
                .OrderByDescending(pal => pal.DateCreated)
                .SingleOrDefaultAsync();

            return activity?.Note ?? string.Empty;
        }

        public async Task<bool> HasPublishInLast(Guid postId)
        {
            var hasPublished =
              await _context
              .PostActivityLogs
              .CountAsync(x => x.PostId == postId && x.ToStatus == PostStatus.Published);

            return hasPublished > 0;
        }

        public async Task<List<Post>> GetListUnpaidPublishedPosts(Guid userId)
        {
            return
                await _context
                .Posts
                .Where(IsPostUnpaidAndPublishedOf(userId))
                .ToListAsync();

            static Expression<Func<Post, bool>> IsPostUnpaidAndPublishedOf(Guid userId)
              => post => post.Unpaid() &&
                 post.Published() &&
                 post.AuthorUserId == userId;
        }


    }
}