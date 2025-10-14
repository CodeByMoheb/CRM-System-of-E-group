using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Services
{
    public interface ICloudinaryService
    {
        Task<(bool success, string? url, string? publicId, string? error)> UploadImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string publicId);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var cfg = options.Value;
            var account = new Account(cfg.CloudName, cfg.ApiKey, cfg.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(bool success, string? url, string? publicId, string? error)> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return (false, null, null, "No file provided");

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            try
            {
                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()))
                {
                    return (true, result.SecureUrl!.ToString(), result.PublicId, null);
                }
                return (false, null, null, result.Error?.Message ?? "Upload failed");
            }
            catch (Exception ex)
            {
                return (false, null, null, ex.Message);
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return true;
            var delParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };
            var result = await _cloudinary.DestroyAsync(delParams);
            return result.StatusCode == System.Net.HttpStatusCode.OK && result.Result == "ok";
        }
    }
}


