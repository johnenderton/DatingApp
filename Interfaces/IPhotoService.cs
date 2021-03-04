using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);

        // Each file upload to Cloudinary will be given a publicId
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}