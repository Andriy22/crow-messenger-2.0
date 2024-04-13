using BLL.Common.Constants;
using BLL.Services.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BLL.Services.Implementations
{
    public class FileService : IFileService
    {
        public void RemoveFile(string fileName, string directory)
        {
            var filePath = Path.Combine(directory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void RemoveFiles(List<string> files, string directory)
        {
            foreach (var file in files)
            {
                var filePath = Path.Combine(directory, file);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        public async Task<string> SaveFileAsync(IFormFile file, string directory)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), FileConstants.StaticFilesFolder, directory);

            Directory.CreateDirectory(path);

            path = Path.Combine(path, fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fileName;
        }


        public async Task<List<string>> SaveFilesAsync(List<IFormFile> files, string directory)
        {
            if (files == null || files.Any() == false)
            {
                return new List<string>();
            }

            List<string> savedFilesPaths = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), FileConstants.StaticFilesFolder, directory);

                Directory.CreateDirectory(path);

                path = Path.Combine(path, fileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                    savedFilesPaths.Add(fileName);
                }
            }

            return savedFilesPaths;
        }
    }
}
