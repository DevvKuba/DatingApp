namespace API.Entities;

// User Schema for the database
public class AppUser
{
    // Id default Primary key
    public int Id { get; set; }
    public required string UserName { get; set; }

    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];

    public DateOnly DateOfBirth { get; set; }

    public required string KnownAs { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    public required string Gender { get; set; }

    public string? Introduction { get; set; }

    public string? Interests { get; set; }

    public required string City { get; set; }

    public required string Country { get; set; }

    public List<Photo> Photos { get; set; } = [];


    // since we're using Get at the start of the method automapper knows to map to age property
    //public int GetAge()
    //{
    //    return DateOfBirth.CalculateAge();
    //}
}
