using AutoMapper;
using AnBlog.Core.Domain.Content;

namespace AnBlog.Core.Models.Content
{
    public class SeriesInListDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }                        
        public Guid AuthorUserId { get; set; }
        public DateTime DateCreated { get; set; }

        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Series, SeriesInListDto>();
            }
        }
    }
}