using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API
{
    [ApiController]
    [Route("api/[controller]")]

    // Primary constructor injecting DataContext
    // context or users endpoint
    public class UsersController(DataContext context) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            var users = await context.Users.ToListAsync();
            return users;
        }

        // route parameter
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {

            var user = await context.Users.FindAsync(id);
            if (user == null) { return NotFound(); }

            return user;


        }
    }
}
