using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Tests.AzureOpenAI.Base;

public class FileManager(string folderLocation) : IFileManagementClient
{
    private readonly string _folderLocation = folderLocation;

    public async Task<Stream> DownloadAsync(FileReference reference)
    {
        var path = Path.Combine(_folderLocation, "Input", reference.Name);
        var bytes = await File.ReadAllBytesAsync(path);
        return new MemoryStream(bytes);
    }

    public async Task<FileReference> UploadAsync(Stream stream, string contentType, string fileName)
    {
        var path = Path.Combine(_folderLocation, "Output", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        
        using (var fileStream = File.Create(path))
        {
            await stream.CopyToAsync(fileStream);
        }

        return new FileReference { Name = fileName };
    }
}
