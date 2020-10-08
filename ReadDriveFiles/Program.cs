using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadDriveFiles
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";
        static DriveService service;
        static Dictionary<string, Google.Apis.Drive.v3.Data.File> files = new Dictionary<string, Google.Apis.Drive.v3.Data.File>();
        static void Main(string[] args)
        {

            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    // var absPath = AbsPath(file);
                    DownloadFile(service, file.Id);
                    Console.WriteLine("{0} ({1})", file.Name, file.Id);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }
            Console.Read();

        }


       private static string DownloadFile(DriveService service, string fileid)
        {
            string Folderpath = "DriveFiles";
            FilesResource.GetRequest request = service.Files.Get(fileid);
            string filename = request.Execute().Name;
            string filepath = System.IO.Path.Combine(Folderpath, filename);
            MemoryStream memorystream = new MemoryStream();
            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
              {
                  switch (progress.Status)
                  {
                      case DownloadStatus.Downloading:
                          {
                              break;
                          }
                      case DownloadStatus.Completed:
                          {
                              SaveStream(memorystream, filepath);
                              break;
                          }
                      case DownloadStatus.Failed:
                          {
                              break;
                          }

                  }
              };
            request.Download(memorystream);
            return filepath;
        }
        private static void SaveStream(MemoryStream stream,string filepath)
        {
            using (FileStream file=new FileStream(filepath,FileMode.Create,FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }
    }
}
   


