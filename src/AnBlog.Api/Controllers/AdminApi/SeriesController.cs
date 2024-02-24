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
    public class SeriesController : AdminControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SeriesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Permissions.Series.Create)]
        public async Task<IActionResult> CreateSeries([FromBody] CreateUpdateSeriesRequest request)
        {
            var series = _mapper.Map<Series>(request);

            _unitOfWork.Series.Add(series);
            await _unitOfWork.CompleteAsync();

            return Ok("Create series successfully!");
        }

        [HttpDelete]
        [Authorize(Permissions.Series.Delete)]
        public async Task<IActionResult> DeleteSeries([FromQuery] Guid[] ids)
        {
            foreach (var seriesId in ids)
            {
                var series = await _unitOfWork.Series.GetByIdAsync(seriesId);

                if (series is null)
                {
                    return NotFound($"No series with id {seriesId} was found.");
                }

                _unitOfWork.Series.Remove(series);
            }

            await _unitOfWork.CompleteAsync();

            return Ok("Delete series successfully!");
        }

        [HttpPut("{id}")]
        [Authorize(Permissions.Series.Edit)]
        public async Task<IActionResult> UpdateSeries(Guid id, [FromBody] CreateUpdateSeriesRequest request)
        {
            var seriesInDb = await _unitOfWork.Series.GetByIdAsync(id);

            if (seriesInDb is null)
            {
                return NotFound("No series was found");
            }

            _mapper.Map(request, seriesInDb);

            var result = await _unitOfWork.CompleteAsync();

            return result > 0 ? Ok("Update series successfully!") : BadRequest();
        }

        [HttpGet("{id}")]
        [Authorize(Permissions.Series.View)]
        public async Task<ActionResult<SeriesDto>> GetSeriesById(Guid id)
        {
            var seriesInDb = await _unitOfWork.Series.GetByIdAsync(id);

            if (seriesInDb is null)
            {
                return NotFound("No series was found");
            }

            return Ok(_mapper.Map<SeriesDto>(seriesInDb));
        }

        [HttpGet]
        [Authorize(Permissions.Series.View)]
        public async Task<ActionResult<List<SeriesDto>>> GetAllSeries()
        {
            var allSeriesInDb = await _unitOfWork.Series.GetAllAsync();

            return Ok(_mapper.Map<List<SeriesDto>>(allSeriesInDb));
        }

        [HttpGet]
        [Route("paging")]
        [Authorize(Permissions.Series.View)]
        public async Task<ActionResult<PagedResult<SeriesInListDto>>> GetSeriesPaging(string? keyword,
      int pageIndex, int pageSize = 10)
        {
            var result = await _unitOfWork.Series.GetAllPaging(keyword, pageIndex, pageSize);

            return Ok(result);
        }

        [HttpPost("post-series")]
        [Authorize(Permissions.Series.Edit)]
        public async Task<IActionResult> AddPostSeries([FromBody] AddPostSeriesRequest request)
        {
            var isExisted = await _unitOfWork.Series.IsPostInSeries(request.SeriesId, request.PostId);

            if (isExisted)
            {
                return BadRequest($"Bài viết này đã nằm trong loạt bài.");
            }

            await _unitOfWork.Series.AddPostToSeries(request.SeriesId, request.PostId, request.SortOrder);
            var result = await _unitOfWork.CompleteAsync();

            return result > 0 ? Ok() : BadRequest();
        }

        [HttpDelete("post-series")]
        [Authorize(Permissions.Series.Edit)]
        public async Task<IActionResult> DeletePostSeries([FromBody] AddPostSeriesRequest request)
        {
            var isExisted = await _unitOfWork.Series.IsPostInSeries(request.SeriesId, request.PostId);

            if (!isExisted)
            {
                return BadRequest($"Bài viết này đã nằm trong loạt bài.");
            }

            await _unitOfWork.Series.RemovePostToSeries(request.SeriesId, request.PostId);
            var result = await _unitOfWork.CompleteAsync();

            return result > 0 ? Ok() : BadRequest();
        }

        [HttpGet("post-series/{id}")]
        [Authorize(Permissions.Series.View)]
        public async Task<ActionResult<List<PostInListDto>>> GetPostsInSeries(Guid id)
        {
            var posts = await _unitOfWork.Series.GetAllPostsInSeries(id);

            return Ok(posts);
        }

    }
}
