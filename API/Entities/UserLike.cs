namespace API.Entities
{
    // join entity
    public class UserLike
    {
        // user doing the liking
        public AppUser SourceUser { get; set; } = null!;

        public int SourceUserId { get; set; }

        // user being liked
        public AppUser TargetUser { get; set; } = null!;

        public int TargetUserId { get; set; }


    }
}
