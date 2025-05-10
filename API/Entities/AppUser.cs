namespace API.Entities;

// Schema for the database
public class AppUser
{
    // Id default Primary key
    public int Id { get; set; }
    public required string UserName { get; set; }

    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
}
