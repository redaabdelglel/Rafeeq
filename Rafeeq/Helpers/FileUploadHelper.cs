using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rafeeq.Helpers
{
    public class FileUploadHelper
    {
        private readonly IWebHostEnvironment _environment;

        public FileUploadHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was uploaded");
            }

            // Determine base path (either webroot/uploads or content root/wwwroot/uploads)
            string uploadsBasePath;
            try
            {
                if (string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    // Use ContentRootPath as fallback
                    string contentRoot = _environment.ContentRootPath;
                    uploadsBasePath = Path.Combine(contentRoot, "wwwroot", "uploads");
                }
                else
                {
                    uploadsBasePath = Path.Combine(_environment.WebRootPath, "uploads");
                }

                // Ensure directory exists
                string uploadsFolder = Path.Combine(uploadsBasePath, folderName);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
            }
            catch (Exception)
            {
                // Fallback to temporary directory if we can't create the directory
                uploadsBasePath = Path.Combine(Path.GetTempPath(), "Rafeeq", "uploads");
                string uploadsFolder = Path.Combine(uploadsBasePath, folderName);

                // Try to create the fallback directory
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
            }

            // Create unique filename
            var fileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadsBasePath, folderName, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return relative URL path to the file
            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // Remove the leading slash if it exists
            if (filePath.StartsWith("/"))
                filePath = filePath.Substring(1);

            string fullPath;
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                // Use ContentRootPath as fallback
                fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", filePath);
            }
            else
            {
                fullPath = Path.Combine(_environment.WebRootPath, filePath);
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }
    }
}
