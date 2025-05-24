using API.DTO_s;
using API.Entities;
using API.Extensions;
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
    public class UsersController(IUserRepository userRepositary, IMapper mapper, IPhotoService photoService) : BaseApiController
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

            // user from datbase aquired by entity framework
            var user = await userRepositary.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("Could not find user");

            mapper.Map(memberUpdateDto, user);


            if (await userRepositary.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepositary.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("Cannot update user");

            var result = await photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            user.Photos.Add(photo);
            if (await userRepositary.SaveAllAsync()) return mapper.Map<PhotoDto>(photo);
            return BadRequest("Photos couldn't be added");
        }

    }
}
