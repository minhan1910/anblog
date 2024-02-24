using AnBlog.Core.ConfigOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace AnBlog.Api.Controllers.AdminApi
{
    public class MediaController : AdminControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnv;
        private readonly MediaSettings _settings;

        public MediaController(IWebHostEnvironment hostingEnv, IOptions<MediaSettings> settings)
        {
            _hostingEnv = hostingEnv;
            _settings = settings.Value;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult UploadImage(string type)
        {
            var allowdImageTypes = _settings.AllowImageFileTypes?.Split(",");
            var now = DateTime.Now;
            var files = Request.Form.Files;

            if (!files.Any())
            {
                return null;
            }
      
            var file = files[0];
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition)?.FileName?.Trim('"');

            if (allowdImageTypes?.Any(x => filename?.EndsWith(x, StringComparison.OrdinalIgnoreCase) == true) == false)
            {
                throw new Exception("Không cho phép tải lên file không phải ảnh.");
            }

            var imageFolder = $@"\{_settings.ImageFolder}\images\{type}\{now:MMyyyy}";
            var folder = _hostingEnv.WebRootPath + imageFolder;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var filePath = Path.Combine(folder, filename ?? string.Empty);

            using var fs = System.IO.File.Create(filePath);
            file.CopyTo(fs);
            fs.Flush();

            var path = Path.Combine(imageFolder, filename ?? string.Empty).Replace("\\", "/");

            return Ok(new { path });
        }

    }
}
