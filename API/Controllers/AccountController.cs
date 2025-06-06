using API.DTO_s;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers
{
    // Datacontent context -> database
    public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
    {
        [HttpPost("register")] // account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            // map into AppUser , map from registerDto
            var user = mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username.ToLower();

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,


            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // need to include photos As entity framework is lazy how it doesn't provide child data List<Photo> Photos
            var user = await userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.NormalizedUserName == loginDto.Username.ToUpper());

            if (user == null || user.UserName == null) return Unauthorized("Invalid username");

            // checks if that specific users password matches the password provided
            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result) return Unauthorized();

            return new UserDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Token = await tokenService.CreateToken(user),
                Gender = user.Gender,
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            };
        }


        private async Task<bool> UserExists(string username)
        {
            return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
        }
    }
}
