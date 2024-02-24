using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnBlog.Core.Domain.Content
{
    [Table("Posts")]
    [Index(nameof(Slug), IsUnique = true)]
    public class Post
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(250)]
        public required string Name { get; set; }

        [Required]        
        [Column(TypeName = "varchar(250)")]
        public required string Slug { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [MaxLength(500)]
        public string? Thumbnail { get; set; }

        [MaxLength(500)]
        public string? Content { get; set; }
        public Guid AuthorUserId { get; set; }

        [MaxLength(128)]
        public string? Source { get; set; }

        [MaxLength(250)]
        public string? Tags { get; set; }

        [MaxLength(160)]
        public string? SeoDescription { get; set; }

        public int ViewCount { get; set; } = default;
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }

        // Tien Nhuan But
        public double RoyaltyAmount { get; set; }
        public PostStatus Status { get; set; }

        // du thua du lieu tranh join cho PostCategory
        [Required]
        [Column(TypeName = "varchar(150)")]
        public required string CategorySlug { get; set; }

        [Required]
        [MaxLength(250)]
        public required string CategoryName { get; set; }

        // du thua du lieu tranh join cho User
        [MaxLength(250)]
        public string? AuthorUserName { get; set; }

        [MaxLength(250)]
        public string? AuthorName { get; set; }

        public bool Unpaid() => IsPaid == false;
        public bool Published() => Status == PostStatus.Published;
    }

    public enum PostStatus
    {
        Draft = 0,        
        WaitingForApproval,
        Rejected,        
        Published
    }
}