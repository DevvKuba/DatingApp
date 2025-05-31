using System.Security.Cryptography;
using System.Text;
using API.DTO_s;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers
{
    // Datacontent context -> database
    public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : BaseApiController
    {
        [HttpPost("register")] // account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            // map into AppUser , map from registerDto
            var user = mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;


            context.Users.Add(user);
            // Entity Framework method to save changes
            await context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,


            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // need to include photos As entity framework is lazy how it doesn't provide child data List<Photo> Photos
            var user = await context.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
            if (user == null) return Unauthorized("Invalid username");

            // salt provided to recompute hash of password
            using var hmac = new HMACSHA512(user.PasswordSalt);

            // generated a hash byte array
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Token = tokenService.CreateToken(user),
                Gender = user.Gender,
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            };
        }



        private async Task<bool> UserExists(string username)
        {
            return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
        }
    }
}
