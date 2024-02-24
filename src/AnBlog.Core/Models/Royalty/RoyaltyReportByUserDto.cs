namespace AnBlog.Core.Models.Royalty
{
    public class RoyaltyReportByUserDto
    {
        public Guid UserId { get; set; }
        public required string UserName { get; set; }
        public int NumberOfDraftPosts { get; set; }
        public int NumberOfWaitingApprovalPosts { get; set; }
        public int NumberOfRejectedPosts { get; set; }
        public int NumberOfPublishedPosts { get; set; }
        public int NumberOfPaidPublishedPosts { get; set; }
        public int NumberOfUnpaidPublishedPosts { get; set; }
    }
}