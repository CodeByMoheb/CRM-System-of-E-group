using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Services;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public UploadController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("image")]
        [Authorize(Roles = "Manager")] // keep consistent with existing auth
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            var result = await _cloudinaryService.UploadImageAsync(file, "gallery");
            if (!result.success)
                return BadRequest(new { error = result.error });

            return Ok(new { url = result.url, publicId = result.publicId });
        }
    }
}


