using Microsoft.AspNetCore.Http;

namespace BLL.Services.Abstractions
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string directory);
        Task<List<string>> SaveFilesAsync(List<IFormFile> files, string directory);
        Task RemoveFileAsync(string file, string directory);
        Task RemoveFilesAsync(List<string> files, string directory);
    }
}
