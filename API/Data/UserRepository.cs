using API.DTO_s;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API;

// where we aquire methods that interact with the database 
public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public async Task<MemberDto?> GetMemberAsync(string loggedInUsername, string username)
    {
        // if user is logged in then ignoreQueryFilter
        if (loggedInUsername == username)
        {
            return await context.Users
            .Where(x => x.UserName == username)
            .IgnoreQueryFilters()
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        return await context.Users
            // use normalisedUser == username.ToUpper() if need be
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = context.Users.AsQueryable();

        query = query.Where(x => x.UserName != userParams.CurrentUserName);

        if (userParams.Gender != null)
        {
            // was == previously
            query = query.Where(x => x.Gender == userParams.Gender);
        }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive),
        };

        return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
            userParams.PageNumber,
            userParams.PageSize);

    }

    public async Task<AppUser>? GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    // method created in order to find user via photoId but also include their photos
    public async Task<AppUser?> GetUserByPhotoIdAsync(int photoId)
    {
        return await context.Users
            .Include(x => x.Photos)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Photos.Any(x => x.Id == photoId));
    }

    public async Task<AppUser>? GetUserByUsernameAsync(string username)
    {
        return await context.Users
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await context.Users
            .Include(x => x.Photos)
            .ToListAsync();
    }

    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}
