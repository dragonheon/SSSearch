using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using System.IO;
using Google.Apis.Auth.OAuth2;
// 일단 무시
namespace SmartSearchScreen
{
    class GoogleCloudStorageUploader
    {
        private readonly string _bucketName;
        private readonly StorageClient _storageClient;

        public GoogleCloudStorageUploader(string bucketName)
        {
            // 인증 설정
            string credential_path = @"C:\searchapp.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
            _bucketName = bucketName;
            _storageClient = StorageClient.Create();
        }

        public string UploadImage(string localFilePath)
        {
            try
            {
                if (!File.Exists(localFilePath))
                {
                    throw new FileNotFoundException($"File not found: {localFilePath}");
                }

                // 파일 이름 설정
                string fileName = Path.GetFileName(localFilePath);

                // Google Cloud Storage에 파일 업로드
                using (var fileStream = File.OpenRead(localFilePath))
                {
                    var objectOptions = new Google.Apis.Storage.v1.Data.Object
                    {
                        ContentType = "image/png" // 이미지 포맷에 맞게 설정 (예: image/png, image/jpeg 등)
                    };

                    _storageClient.UploadObject(_bucketName, fileName, objectOptions.ContentType, fileStream);
                }

                // 공개 URL 생성
                string publicUrl = $"https://storage.googleapis.com/{_bucketName}/{fileName}";
                return publicUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file: {ex.Message}", ex);
            }
        }
    }

}
