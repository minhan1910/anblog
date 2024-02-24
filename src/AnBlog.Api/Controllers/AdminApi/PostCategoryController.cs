using AnBlog.Api.Filters;
using AnBlog.Core.Domain.Content;
using AnBlog.Core.Models;
using AnBlog.Core.Models.Content;
using AnBlog.Core.SeedWorks;
using AnBlog.Core.SeedWorks.Constants;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnBlog.Api.Controllers.AdminApi
{
    public class PostCategoryController : AdminControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PostCategoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // CreatePostCategory
        [HttpPost]
        [ValidateModel]
        [Authorize(Permissions.PostCategories.Create)]
        public async Task<IActionResult> CreatePostCategory([FromBody] CreateUpdatePostCategoryRequest request)
        {
            var postCategory = _mapper.Map<PostCategory>(request);
            
            _unitOfWork.PostCategories.Add(postCategory);
            var result = await _unitOfWork.CompleteAsync();

            return  result > 0 ? Ok("Create new post category successfully!") : BadRequest();
        }

        // UpdatePostCategory
        [HttpPut("{id}")]
        [ValidateModel]
        [Authorize(Permissions.PostCategories.Edit)]
        public async Task<IActionResult> UpdatePostCategory(Guid id, [FromBody] CreateUpdatePostCategoryRequest request)
        {
            var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);

            if (postCategory is null)
            {
                return NotFound("Post category was not found.");
            }

            _mapper.Map(request, postCategory);
           var result = await _unitOfWork.CompleteAsync();

            return result > 0 ? Ok("Update post category successfully!") : BadRequest();
        }

        // GetAllPostsPaging
        [HttpGet("paging")]
        [Authorize(Permissions.PostCategories.View)]
        public async Task<ActionResult<PagedResult<PostCategoryDto>>> GetPostCategoriesPaging(string? keyword, int pageIndex, int pageSize = 10)
        {
            var postCategoriesPaging = await _unitOfWork.PostCategories.GetAllPaging(keyword, pageIndex, pageSize);

            if (postCategoriesPaging is null)
            {
                return NotFound("Post category is empty.");
            }

            return Ok(postCategoriesPaging);
        }

        // GetPostCategoryById 
        [HttpGet("{id}")]        
        [Authorize(Permissions.PostCategories.View)]
        public async Task<ActionResult<PostCategoryDto>> GetPostCategoryById(Guid id)
        {
            var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);

            if (postCategory is null)
            {
                return NotFound("Post category was not found.");
            }

            return Ok(_mapper.Map<PostCategoryDto>(postCategory));
        }

        [HttpGet]
        [Authorize(Permissions.PostCategories.View)]
        public async Task<ActionResult<List<PostCategoryDto>>> GetPostCategories()
        {
            var query = await _unitOfWork.PostCategories.GetAllAsync();
            var model = _mapper.Map<List<PostCategoryDto>>(query);
            return Ok(model);
        }

        // DeletePostCategory
        [HttpDelete]
        [Authorize(Permissions.PostCategories.Delete)]
        public async Task<IActionResult> DeletePostCategory([FromQuery] Guid[] ids) 
        {
            foreach (var id in ids)
            {
                var postCategory = await _unitOfWork.PostCategories.GetByIdAsync(id);

                if (postCategory is null)
                {
                    return NotFound("Post category with id = "+ id +" was not found.");
                }

                _unitOfWork.PostCategories.Remove(postCategory);
            }

            var result = await _unitOfWork.CompleteAsync();

            return result > 0 ? Ok("Delete post successfully!") : BadRequest();    
        }

    }
}
