using Microsoft.AspNetCore.Http;

namespace BLL.Services.Abstractions
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string directory);
        Task<List<string>> SaveFilesAsync(List<IFormFile> files, string directory);
        void RemoveFile(string file, string directory);
        void RemoveFiles(List<string> files, string directory);
    }
}
