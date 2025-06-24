namespace API.DTO_s
{
    public class PhotoForApprovalDto
    {
        public int Id { get; set; }

        public required string Url { get; set; }

        public string? Username { get; set; }

        public bool IsApprovedStatus { get; set; }

    }
}
