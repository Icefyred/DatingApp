/**** Photo Management Challenge 9. ****/
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;

namespace API.Interfaces{
    public interface IPhotoRepository{
        Task<IEnumerable<PhotoApprovalDto>> GetUnapprovedPhotos();
        Task<Photo> GetPhotoById(int photoId);
        void RemovePhoto(Photo photo);
    }
}