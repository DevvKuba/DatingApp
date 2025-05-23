using System.Security.Claims;
using API.DTO_s;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // Primary constructor injecting DataContext
    // context or users endpoint


    // user needs to be autherized / logged in before utilising get requests
    [Authorize]
    public class UsersController(IUserRepository userRepositary, IMapper mapper) : BaseApiController
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

        // put requests to update database
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (username == null) return BadRequest("No username found in token");

            // user from datbase aquired by entity framework
            var user = await userRepositary.GetUserByUsernameAsync(username);

            if (user == null) return BadRequest("Could not find user");

            mapper.Map(memberUpdateDto, user);


            if (await userRepositary.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");



        }
    }
}
