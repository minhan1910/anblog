using AnBlog.Core.Domain.Content;
using AutoMapper;

namespace AnBlog.Core.Models.Content
{
    public class PostInListDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }
        public int ViewCount { get; set; }
        public DateTime DateCreated { get; set; }

        // du thua du lieu tranh join cho PostCategory
        public required string CategorySlug { get; set; }

        public required string CategoryName { get; set; }

        // du thua du lieu tranh join cho User
        public string? AuthorUserName { get; set; }

        public string? AuthorName { get; set; }

        public PostStatus Status { get; set; }
        public DateTime? PaidDate { get; set; }

        public class AutoMapperProfiles : Profile
        {
            public AutoMapperProfiles()
            {
                CreateMap<Post, PostInListDto>();
            }
        }
    }
}