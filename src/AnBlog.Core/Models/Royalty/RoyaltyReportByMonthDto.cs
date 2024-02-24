namespace AnBlog.Core.Models.Royalty
{
    public class RoyaltyReportByMonthDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int NumberOfDraftPosts { get; set; }
        public int NumberOfWaitingApprovalPosts { get; set; }
        public int NumberOfRejectedPosts { get; set; }
        public int NumberOfPublishedPosts { get; set; }
        public int NumberOfPaidPublishedPosts { get; set; }
        public int NumberOfUnpaidPublishedPosts { get; set; }
    }
}