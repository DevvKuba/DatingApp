using API.DTO_s;
using API.Entities;
using API.Extensions;
using API.Helpers;
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
    public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        // specifices to look in query string => [FromQuery]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            userParams.CurrentUserName = User.GetUsername();
            var users = await userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users);

            return Ok(users);
        }

        // route parameter
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await userRepository.GetMemberAsync(username);

            if (user == null) { return NotFound(); }

            return user;


        }

        // put requests to update database
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {

            // user from datbase aquired by entity framework
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("Could not find user");

            mapper.Map(memberUpdateDto, user);


            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("Cannot update user");

            var result = await photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            // auto sets to main photo if its their first photo
            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);
            if (await userRepository.SaveAllAsync())
                return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
            // adds a location header of the user, to our response
            return BadRequest("Photos couldn't be added");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());
            // User.GetUsername() interprested as ClaimsPrincipleExtensions.GetUsername(User)

            if (user == null) return BadRequest("Could not find user");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain) return BadRequest("Cannot use this as a main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Probelm setting main photo");

        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("User not found");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleted photo");
        }

    }

}
