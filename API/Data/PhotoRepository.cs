using API.DTO_s;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
    {

        public async Task<Photo> GetPhotoById(int id)
        {
            var photo = await context.Photos
                .IgnoreQueryFilters()
                .Where(x => x.Id == id)
                .FirstAsync();
            return photo;
        }

        public async Task<List<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            // map each to PhotoForApprovalDto - first set to IQueriable ?
            var unapprovedPhotos = await context.Photos
                .IgnoreQueryFilters()
                .Where(x => !x.IsApproved)
                .ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider)
                .ToListAsync();
            return unapprovedPhotos;
        }

        public async Task<bool> RemovePhoto(int id)
        {
            // need to get the specific users photo, access their Photos and remove
            var photo = await context.FindAsync<Photo>(id);
            var photoUser = await context.FindAsync<AppUser>(photo.AppUserId);
            photoUser.Photos.Remove(photo);

            if (!photoUser.Photos.Contains(photo)) return true;
            return false;
        }
    }
}
