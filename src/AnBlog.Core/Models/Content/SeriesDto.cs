using AnBlog.Core.Domain.Content;
using AutoMapper;

namespace AnBlog.Core.Models.Content
{
    public class SeriesDto : SeriesInListDto
    {
        public string? Thumbnail { get; set; }
        public string? SeoDescription { get; set; }
        public string? Content { get; set; }

        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Series, SeriesDto>();
            }
        }
    }
}