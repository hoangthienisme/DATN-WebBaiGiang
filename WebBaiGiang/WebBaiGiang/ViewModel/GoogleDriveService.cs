using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
//using Google.Apis.Drive.v3;
//using Google.Apis.Drive.v3.Data;
//using Google.Apis.Services;
//using Google.Apis.Upload;
using System;
using System.IO;
using System.Threading.Tasks;

public class GoogleDriveService
{
    private readonly DriveService _driveService;
    private readonly string _folderId = "1O1_d2qdVVll4RuMmT5yd44bIG98oAkFd"; // 📁 Folder ID trên Google Drive

    public GoogleDriveService(IWebHostEnvironment env)
    {
        var credentialPath = Path.Combine(env.ContentRootPath, "credentials.json");

        GoogleCredential credential;
        using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                                         .CreateScoped(DriveService.Scope.Drive);
        }

        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "WebBaiGiang"
        });
    }

    public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = fileName,
            Parents = new[] { _folderId }
        };

        var request = _driveService.Files.Create(fileMetadata, fileStream, contentType);
        request.Fields = "id";

        var result = await request.UploadAsync();

        if (result.Status != UploadStatus.Completed)
        {
            throw new Exception($"Upload failed: {result.Exception}");
        }

        var file = request.ResponseBody;

        // Chia sẻ file public (nếu muốn)
        await _driveService.Permissions.Create(new Permission
        {
            Type = "anyone",
            Role = "reader"
        }, file.Id).ExecuteAsync();

        return $"https://drive.google.com/file/d/{file.Id}/view";
    }
}