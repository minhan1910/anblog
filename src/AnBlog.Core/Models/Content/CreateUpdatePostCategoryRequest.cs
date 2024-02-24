using AnBlog.Core.Domain.Content;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnBlog.Core.Models.Content
{
    public class CreateUpdatePostCategoryRequest
    {
        [MaxLength(250)]
        public required string Name { get; set; }

        [Column(TypeName = "varchar(250)")]
        public required string Slug { get; set; }

        public Guid? ParentId { get; set; }

        public bool IsActive { get; set; }

        public string? SeoKeywords { get; set; }

        public string? SeoDescription { get; set; }
        public int SortOrder { get; set; }

        public class AutoMapperProfiles : Profile
        {
            public AutoMapperProfiles() 
            {
                CreateMap<CreateUpdatePostCategoryRequest, PostCategory>();
            }
        }
    }
}