using API.DTO_s;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // Primary constructor injecting DataContext
    // context or users endpoint


    // user needs to be autherized / logged in before utilising get requests
    [Authorize]
    public class UsersController(IUserRepository userRepositary) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await userRepositary.GetMembersAsync();

            return Ok(users);
        }

        // route parameter
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await userRepositary.GetMemberAsync(username);

            if (user == null) { return NotFound(); }

            return user;


        }
    }
}
