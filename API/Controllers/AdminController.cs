using API.DTO_s;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    // figure out if needed
    //[Authorize]
    public class AdminController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IMapper mapper) : BaseApiController
    {
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await userManager.Users
                .OrderBy(x => x.UserName)
                .Select(x => new
                {
                    x.Id,
                    Username = x.UserName,
                    Roles = x.UserRoles.Select(r => r.Role.Name).ToList(),
                }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("you must select at least one role");

            var selectedRoles = roles.Split(",").ToArray();

            var user = await userManager.FindByNameAsync(username);

            if (user == null) return BadRequest("User not found");

            var userRoles = await userManager.GetRolesAsync(user);

            var results = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!results.Succeeded) return BadRequest("Failed to add to roles");

            results = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!results.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<List<PhotoForApprovalDto>> GetPhotosForApproval()
        {
            var photos = await unitOfWork.PhotoRepository.GetUnapprovedPhotos();
            return photos;
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{id}")]
        public async Task<ActionResult<PhotoForApprovalDto>> ApprovePhoto(int id)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoById(id);
            if (photo == null) return BadRequest("Photo to approve not found");
            photo.IsApproved = true;

            // user that is await the photo approval
            //var user = await userManager.FindByIdAsync(photo.AppUserId.ToString());
            var user = await unitOfWork.UserRepository.GetUserByPhotoIdAsync(id);
            if (user == null) return BadRequest("User for approved photo not found");

            if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

            if (await unitOfWork.Complete())
            {
                var photoApproved = mapper.Map<PhotoForApprovalDto>(photo);
                photoApproved.IsApprovedStatus = true;
                return Ok(photoApproved);
            }

            return BadRequest("Database was not updated after approving the photo");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{id}")]
        public async Task<ActionResult> RejectPhoto(int id)
        {
            bool PhotoDeletedStatus = await unitOfWork.PhotoRepository.RemovePhoto(id);
            if (PhotoDeletedStatus)
            {
                if (await unitOfWork.Complete()) return Ok();
                return BadRequest("Database was not updated after rejecting the photo");
            }
            return BadRequest("Photo was not deleted from Users photos");
        }


    }
}
