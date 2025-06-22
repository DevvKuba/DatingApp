using API.DTO_s;
using API.Entities;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<List<PhotoForApprovalDto>> GetUnapprovedPhotos();

        Task<Photo> GetPhotoById(int id);

        Task<bool> RemovePhoto(int id);
    }
}
