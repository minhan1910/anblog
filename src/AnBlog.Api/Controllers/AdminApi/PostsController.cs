using AnBlog.Api.Extensions;
using AnBlog.Api.Filters;
using AnBlog.Core.Domain.Content;
using AnBlog.Core.Domain.Identity;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Content;
using AnBlog.Core.SeedWorks;
using AnBlog.Core.SeedWorks.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AnBlog.Api.Controllers.AdminApi
{
    public class PostController : AdminControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public PostController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] CreateUpdatePostRequest createUpdatePostRequest)
        {
            if (await _unitOfWork.Posts.IsSlugAlreadyExisted(createUpdatePostRequest.Slug, id))
            {
                return BadRequest("Slug has already existed.");
            }

            var post = await _unitOfWork.Posts.GetByIdAsync(id);

            if (post is null)
            {
                return NotFound("No post was found.");
            }

            // update new post category for current post 
            if (post.CategoryId != createUpdatePostRequest.CategoryId)
            {
                var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(createUpdatePostRequest.CategoryId);
                post.CategoryId = createUpdatePostRequest.CategoryId;
                post.CategoryName = postCategory.Name;
            }

            _mapper.Map(createUpdatePostRequest, post);
            
            var result = await _unitOfWork.CompleteAsync();

            return result > 0 ? Ok(post) : BadRequest();
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreatePost([FromBody] CreateUpdatePostRequest createUpdatePostRequest)
        {            

            if (await _unitOfWork.Posts.IsSlugAlreadyExisted(createUpdatePostRequest.Slug))
            {
                return BadRequest("Slug has already existed");
            }

            var post = _mapper.Map<Post>(createUpdatePostRequest);

            var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(post.CategoryId);

            if (postCategory is not null)
            {
                post.CategoryId = postCategory.Id;
                post.CategoryName = postCategory.Name;
                post.CategorySlug = postCategory.Slug;
            }

            var userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is not null)
            {
                post.AuthorName = user.GetFullName();
                post.AuthorUserName = user.UserName;
                post.AuthorUserId = user.Id;
            }
            
            _unitOfWork.Posts.Add(post);

            var result = await _unitOfWork.CompleteAsync();

            return result > 0 ? Ok() : BadRequest();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<PostDto>> GetPostById(Guid id)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);

            return post == null ? NotFound() : Ok(post);
        }

        [HttpGet("paging")]
        [Authorize(Permissions.Posts.View)]
        public async Task<ActionResult<PagedResult<PostInListDto>>> GetPostsPaging(string? keyword, Guid? categoryId,
            int pageIndex, int pageSize = 10)
        {

            var currentUserId = User.GetUserId();
            var result = await _unitOfWork.Posts.GetAllPaging(keyword, currentUserId, categoryId, pageIndex, pageSize);

            return Ok(result);
        }

        [HttpGet("series-belong/{id}")]        
        [Authorize(Permissions.Posts.View)]
        public async Task<ActionResult<List<SeriesInListDto>>> GetSeriesBelong(Guid id)
        {
            var postSeries = await _unitOfWork.Posts.GetAllSeries(id);

            return Ok(postSeries);
        }

        [HttpGet("approve/{id}")]
        [Authorize(Permissions.Posts.Approve)]
        public async Task<IActionResult> ApprovePost(Guid id)
        {
            await _unitOfWork.Posts.Approve(id, User.GetUserId());
            await _unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpGet("send-approve/{id}")]
        [Authorize(Permissions.Posts.Edit)]
        public async Task<IActionResult> SendToApprove(Guid id)
        {
            await _unitOfWork.Posts.SendToApprove(id, User.GetUserId());
            await _unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpPost("return-back/{id}")]
        [Authorize(Permissions.Posts.Approve)]
        public async Task<IActionResult> ReturnBack(Guid id, [FromBody] ReturnBackRequest model)
        {
            await _unitOfWork.Posts.ReturnBack(id, User.GetUserId(), model?.Reason ?? string.Empty);
            await _unitOfWork.CompleteAsync();

            return Ok();
        }

        [HttpGet("return-reason/{id}")]
        [Authorize(Permissions.Posts.Approve)]
        public async Task<IActionResult> GetReason(Guid id)
        {
            return Ok(await _unitOfWork.Posts.GetReturnReason(id));
        }

        [HttpGet("activity-logs/{id}")]
        [Authorize(Permissions.Posts.View)]
        public async Task<ActionResult<List<PostActivityLogDto>>> GetActivityLogs(Guid id)
        {
            var res = await _unitOfWork.Posts.GetActivityLogs(id);
            return Ok(res);
        }

        [HttpDelete]
        [Authorize(Permissions.Posts.Delete)]
        public async Task<ActionResult<List<PostActivityLogDto>>> DeletePosts (Guid[] ids)
        {
            foreach (var id in ids)
            {
                var post = await _unitOfWork.Posts.GetByIdAsync(id);

                if (post is null)
                {
                    return NotFound("No Post was found!");
                }

                _unitOfWork.Posts.Remove(post);
            }

            await _unitOfWork.CompleteAsync();

            return Ok("Delete Posts successfully!");
        }
    }
}
