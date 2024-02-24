namespace AnBlog.Core.Models.System.Users
{
    public class ChangeMyPasswordRequest
    {
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}