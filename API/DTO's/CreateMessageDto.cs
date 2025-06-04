namespace API.DTO_s
{
    public class CreateMessageDto
    {
        public required string RecipientUsername { get; set; }

        public required string Content { get; set; }
    }
}
