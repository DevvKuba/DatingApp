using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API;

// primary constructor - part of the class
public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
}
